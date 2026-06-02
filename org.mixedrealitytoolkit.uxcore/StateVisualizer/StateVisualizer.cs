// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Playables;

using WeightType = MixedReality.Toolkit.UX.IAnimationMixableEffect.WeightType;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// A high-performance Playables API-powered interaction feedback system with extensible effects.
    /// Requires <see cref="StatefulInteractable"/> and an <see cref="Animator"/>.
    /// </summary>
    [AddComponentMenu("MRTK/UX/State Visualizer")]
    [RequireComponent(typeof(Animator))]
    public class StateVisualizer : MonoBehaviour
    {
        // How long to wait after effects report they're done
        // before going to sleep. Some tolerance is needed as animators
        // can "lag" behind their intended motion in some instances.
        private const float keepAliveTime = 0.2f;

        // Number of wake-up events we subscribe to by default.
        // Used for nicer list initialization.
        private const int defaultWakeUpEventCount = 8;

        /// <summary>
        /// A container that holds a list of effects, as well as the
        /// current value of the state.
        /// </summary>
        /// <remarks>
        /// All effects in a state share the same state value. However,
        /// each effect can respond to the value in different ways.
        /// Consider using a more appropriate Effect rather than adjusting how
        /// the value is submitted.
        /// </remarks>
        [Serializable]
        protected internal class State
        {
            [SerializeReference]
            [Tooltip("The list of effects to apply.")]
            private List<IEffect> effects = new List<IEffect>();

            /// <summary>
            /// The list of effects to apply.
            /// </summary>
            public List<IEffect> Effects => effects;

            /// <summary>
            /// The value [0,1] that controls the effects within this state.
            /// </summary>
            /// <remarks>
            /// See the documentation of each <see cref="IEffect"/> for how
            /// they respond to the state value.
            /// </remarks>
            public float Value { get; set; }

            /// <summary>
            /// The value from last frame. Used to detect
            /// changes in the state's value between frames.
            /// </summary>
            public float PreviousValue { get; set; }

            [SerializeField]
            // Used internally to hint to the editor that this is a variable/float-based state.
            // Has no effect otherwise.
            private bool isVariable;

            /// <summary>
            /// Used internally to hint to the editor that this is a variable/float-based state.
            /// Has no effect otherwise.
            /// </summary>
            public bool IsVariable
            {
                get => isVariable;
                set => isVariable = value;
            }
        }

        /// <summary>
        /// The collection of feedback states that this <see cref="StateVisualizer"/> operates on.
        /// </summary>
        [SerializeField]
        protected internal SerializableDictionary<string, State> stateContainers = new SerializableDictionary<string, State>();

        // Default states that are written at validation + startup.
        private readonly Dictionary<string, State> defaultStates = new Dictionary<string, State>()
        {
            { "Disabled", new State() },
            { "PassiveHover", new State() },
            { "ActiveHover", new State() },
            { "Select", new State() { IsVariable = true } },
            { "Toggle", new State() }
        };

        [SerializeField]
        [Tooltip("The connected interactable.")]
        private StatefulInteractable interactable;

        /// <summary>
        /// The connected interactable.
        /// </summary>
        public StatefulInteractable Interactable
        {
            get
            {
                if (interactable == null)
                {
                    interactable = GetComponentInParent<StatefulInteractable>();
                }

                return interactable;
            }
            set => interactable = value;
        }

        [SerializeField]
        [Tooltip("The Animator to be used as the output for the Playable graph.")]
        private Animator animator;

        /// <summary>
        /// The Animator to be used as the output for the Playable graph.
        /// </summary>
        public Animator Animator
        {
            get => animator;
            set => animator = value;
        }

        // The PlayableGraph that injects animation data into the Animator.
        private PlayableGraph playableGraph;

        // The single animation mixer that all animation-based effects mix into.
        private AnimationLayerMixerPlayable animationMixerPlayable;

        // Set to keepAliveTime when awake. Ticked down towards
        // zero when sleep is requested.
        private float sleepTimer = 0;

        // We hold on to a list of actions we use to unsubscribe from the wake-up events.
        private List<UnityAction> unsubscribeActions = new List<UnityAction>(defaultWakeUpEventCount);

        // A runtime scratchpad for recording where each IMixableEffect is connected on the mixer.
        private Dictionary<IEffect, int> mixableIndices = new Dictionary<IEffect, int>();

        // A shared scratchpad for counting unique mixable effects during graph construction without allocating.
        private static readonly HashSet<IEffect> mixableEffectScratchpad = new HashSet<IEffect>();

        // Tracks which interactable we are currently subscribed to, to prevent redundant delegate allocations.
        private StatefulInteractable subscribedInteractable;

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            EnsureDefaultStates();
            if (interactable == null)
            {
                interactable = GetComponentInParent<StatefulInteractable>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private UnityAction Subscribe<T>(UnityEvent<T> genericEvent, UnityAction callback)
        {
            // Wrap callback in generic lambda
            UnityAction<T> wrapper = (_) => callback();
            genericEvent.AddListener(wrapper);

            // Return lambda that removes the listener.
            return () => genericEvent.RemoveListener(wrapper);
        }

        private UnityAction Subscribe(UnityEvent evt, UnityAction callback)
        {
            evt.AddListener(callback);
            return () => evt.RemoveListener(callback);
        }

        /// <summary>
        /// Tears down the current <see cref="PlayableGraph"/> and rebuilds it from the
        /// current state of <see cref="stateContainers"/>.
        /// </summary>
        /// <remarks>
        /// Call this after modifying the effect lists at runtime — for example, after a
        /// theme switch via <see cref="MixedReality.Toolkit.Theming.StateVisualizerEffectSetBinder"/>.
        /// Any effects added since the last <see cref="Start"/> call will not participate in
        /// the graph until <see cref="Rebuild"/> is called.
        /// </remarks>
        internal void Rebuild()
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
            mixableIndices.Clear();
            Start();
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
            // If the graph is already valid, Start() has already executed (e.g. manually invoked
            // by Rebuild() before Unity's natural Start lifecycle). Return early to prevent
            // memory leaks of unmanaged PlayableGraphs and duplicate event subscriptions.
            if (playableGraph.IsValid())
            {
                return;
            }

            OnValidate();

            if (interactable != null && interactable != subscribedInteractable)
            {
                // Unsubscribe from any previous interactable if we are hot-swapping
                foreach (UnityAction unsubscribe in unsubscribeActions)
                {
                    unsubscribe();
                }
                unsubscribeActions.Clear();

                unsubscribeActions.Add(Subscribe(interactable.hoverEntered, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.hoverExited, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.selectEntered, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.selectExited, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.IsToggled.OnEntered, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.IsToggled.OnExited, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.OnEnabled, WakeUp));
                unsubscribeActions.Add(Subscribe(interactable.OnDisabled, WakeUp));

                subscribedInteractable = interactable;
            }

            // Creates the graph, the mixer and binds them to the Animator.
            playableGraph = PlayableGraph.Create();

            // We can use a single animation output for all animation-based playables.
            var animationPlayableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

            // AnimationLayerMixerPlayable does not support dynamic resizing via SetInputCount well.
            // We must pre-calculate the required number of inputs before creation to prevent Playable exceptions.
            mixableEffectScratchpad.Clear();
            foreach (var kvp in stateContainers)
            {
                foreach (IEffect effect in kvp.Value.Effects)
                {
                    if (effect is IAnimationMixableEffect)
                    {
                        mixableEffectScratchpad.Add(effect);
                    }
                }
            }

            // We use a single master mixer for all animation-based playables.
            // Two-way animation playables mix into this mixer.
            // We start with at least 1 input to prevent Playable API issues with 0-input layer mixers.
            animationMixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph, Mathf.Max(1, mixableEffectScratchpad.Count));
            animationPlayableOutput.SetSourcePlayable(animationMixerPlayable);
            mixableEffectScratchpad.Clear();

            int currentSlot = 0;

            foreach (var kvp in stateContainers)
            {
                foreach (IEffect effect in kvp.Value.Effects)
                {
                    if (effect == null) { continue; }

                    effect.Setup(playableGraph, gameObject);

                    // If the effect uses Playables (not all do!)
                    // we connect it to the Mixer and set the relevant weights + settings.
                    if (effect is IPlayableEffect playableEffect)
                    {
                        // Connect all AnimationEffects to our single, reusable AnimationMixer.
                        if (effect is IAnimationMixableEffect mixableEffect)
                        {
                            // Guard against duplicate entries, which can occur if the same
                            // effect instance appears more than once across stateContainers.
                            if (mixableIndices.ContainsKey(mixableEffect))
                            {
                                Debug.LogWarning($"{nameof(StateVisualizer)}: Duplicate IAnimationMixableEffect instance detected in stateContainers ({effect.GetType().Name}). Skipping duplicate.");
                                continue;
                            }

                            // Configure the layer in the mixer, based on the effect's settings.
                            // For now, additive mixing is blocked by a Unity bug, described at the following links.
                            // AnimationLayerMixerPlayable writes defaults into the object's state upon being disabled,
                            // and then additively blends on top of what it was animating before it was disabled.
                            // https://forum.unity.com/threads/we-need-a-way-to-disable-write-defaults-in-the-playables-api.971643/
                            // https://forum.unity.com/threads/how-to-disable-writedefaults-in-custom-playable-graph-animator.717218/
                            animationMixerPlayable.SetLayerAdditive((uint)currentSlot, false);

                            playableGraph.Connect(playableEffect.Playable, 0, animationMixerPlayable, currentSlot);

                            // Record the index for later retrieval.
                            mixableIndices.Add(mixableEffect, currentSlot);

                            currentSlot++;
                        }
                        else
                        {
                            // If needed, we can build custom mixer support here. This was actually implemented
                            // in an earlier version of this system, but the complexity was unnecessary for current needs.
                            // In the future; IPlayableEffect can be expanded to specify a CreateMixer function that
                            // creates a generic mixer playable that would be in charge of combining the outputs from
                            // all participating PlayableBehaviours of a given type. These would be tracked and connected here.
                            ScriptPlayableOutput output = ScriptPlayableOutput.Create(playableGraph, kvp.Value + ":" + effect.GetType().Name);
                            output.SetSourcePlayable(playableEffect.Playable);
                        }
                    }
                }
            }

            // Update state values to match the current interactable state before setting weights.
            UpdateStateValues();

            // Snap the weights to the current state value initially so Rebuild() doesn't cause
            // a visual glitch by lerping from 0, or flashing the bind pose.
            foreach (var kvp in stateContainers)
            {
                foreach (IEffect effect in kvp.Value.Effects)
                {
                    if (effect is IAnimationMixableEffect mixableEffect && mixableIndices.TryGetValue(mixableEffect, out int slot))
                    {
                        animationMixerPlayable.SetInputWeight(slot, kvp.Value.Value);
                    }
                }
            }

            // Determine if we actually need to be awake.
            bool allEffectsDone = EvaluateEffects();

            if (allEffectsDone && interactable != null && !interactable.isSelected && !interactable.isHovered)
            {
                // Start asleep if nothing is happening. This prevents the Animator from
                // briefly waking up and applying bind poses with 0 weights, which overwrites
                // values set by other components (like theme binders).
                animator.enabled = false;
                playableGraph.Stop();
                enabled = false;
            }
            else
            {
                animator.enabled = true;
                playableGraph.Play();
                enabled = true;
                sleepTimer = keepAliveTime;
            }
        }

        /// <summary>
        /// Ensures that the default states are present.
        /// </summary>
        protected virtual void EnsureDefaultStates()
        {
            foreach (var kvp in defaultStates)
            {
                if (!stateContainers.ContainsKey(kvp.Key))
                {
                    stateContainers.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private void WakeUp()
        {
            enabled = true;
            sleepTimer = keepAliveTime;
        }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            bool valueChanged = UpdateStateValues();

            // If parameters have changed but the animator is currently disabled, wake up!
            if (valueChanged && !animator.enabled)
            {
                animator.enabled = true;
                playableGraph.Play();
                sleepTimer = keepAliveTime;
            }

            // If we're asleep, quit early.
            if (!animator.enabled)
            {
                enabled = false;
                return;
            }

            // Returns true if all effects are done playing.
            if (EvaluateEffects())
            {
                // Have we been "done" long enough to go to sleep?
#pragma warning disable UNT0004 // Using fixedDeltaTime to avoid going to sleep too early when frames hang (like at startup)
                sleepTimer -= Time.fixedDeltaTime;
#pragma warning restore UNT0004 // Using fixedDeltaTime to avoid going to sleep too early when frames hang (like at startup)

                // Only sleep if we're not currently selected or hovered.
                // This seems counter-intuitive, but we do this because an animation may need to be
                // kicked off when the float value of a state changes. We don't have a wake-up event
                // for a "value changed", so we just stay awake while we are hovered (or selected).
                if (sleepTimer <= 0 && interactable != null && !interactable.isSelected && !interactable.isHovered)
                {
                    // All effects are done, let's go to sleep.
                    animator.enabled = false;
                    playableGraph.Stop();
                    enabled = false;
                }
            }
            else
            {
                // We're not done, so reset the sleep timer.
                sleepTimer = keepAliveTime;
            }
        }

        /// <summary>
        /// A Unity event function that is called when the script component has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            foreach (var unsubscribeAction in unsubscribeActions)
            {
                unsubscribeAction();
            }

            // Destroys all Playables and Outputs created by the graph.
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }

        /// <summary>
        /// Sets the tint color on all <see cref="ITintEffect"/> effects within the named state.
        /// </summary>
        /// <param name="stateName">The name of the state whose tint effects should be updated.</param>
        /// <param name="color">The color to apply.</param>
        /// <returns>
        /// <see langword="true"/> if the state was found and at least one tint effect was updated,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool TrySetStateTintColor(string stateName, Color color)
        {
            if (!stateContainers.TryGetValue(stateName, out State state))
            {
                return false;
            }

            bool anyUpdated = false;
            foreach (IEffect effect in state.Effects)
            {
                if (effect is ITintEffect tintEffect)
                {
                    tintEffect.TintColor = color;
                    anyUpdated = true;
                }
            }

            return anyUpdated;
        }

        /// <summary>
        /// Sets the tint color on the <see cref="ITintEffect"/> within the named state whose
        /// <see cref="ITintEffect.HasTintable"/> returns <see langword="true"/> for
        /// <paramref name="tintTarget"/>.
        /// </summary>
        /// <remarks>
        /// This is the preferred overload when a single state contains multiple tint effects that
        /// need different colors (e.g. a background tint and an icon tint). Assign the
        /// <c>UnityEngine.Object</c> reference directly in the Inspector — it is rename-proof and
        /// breaks visibly (missing reference) rather than silently if the object is deleted.
        /// <para/>
        /// If <paramref name="tintTarget"/> is <see langword="null"/> this method behaves
        /// identically to <see cref="TrySetStateTintColor(string, Color)"/> and updates every
        /// tint effect in the state.
        /// </remarks>
        /// <param name="stateName">The name of the state whose tint effects should be updated.</param>
        /// <param name="tintTarget">A <see cref="UnityEngine.Object"/> that appears in the
        /// tintables list of the effect to update, or <see langword="null"/> to update all tint
        /// effects in the state.</param>
        /// <param name="color">The color to apply.</param>
        /// <returns>
        /// <see langword="true"/> if the state was found and at least one matching tint effect was
        /// updated, <see langword="false"/> otherwise.
        /// </returns>
        public bool TrySetStateTintColor(string stateName, UnityEngine.Object tintTarget, Color color)
        {
            if (tintTarget == null)
            {
                return TrySetStateTintColor(stateName, color);
            }

            if (!stateContainers.TryGetValue(stateName, out State state))
            {
                return false;
            }

            bool anyUpdated = false;
            foreach (IEffect effect in state.Effects)
            {
                if (effect is ITintEffect tintEffect &&
                    tintEffect.HasTintable(tintTarget))
                {
                    tintEffect.TintColor = color;
                    anyUpdated = true;
                }
            }

            return anyUpdated;
        }

        /// <summary>
        /// Adds the provided effect to the state with name <paramref name="stateName"/>.
        /// Creates the state if it doesn't exist.
        /// </summary>
        /// <param name="stateName">The name of the state to add the effect to.</param>
        /// <param name="effect">The effect to add.</param>
        internal void AddEffect(string stateName, IEffect effect)
        {
            if (!stateContainers.ContainsKey(stateName))
            {
                stateContainers.Add(stateName, new State());
            }

            stateContainers[stateName].Effects.Add(effect);
        }

        /// <summary>
        /// Removes the specified effect from the state with name <paramref name="stateName"/>.
        /// </summary>
        /// <param name="stateName">The name of the state to remove the effect from.</param>
        /// <param name="effect">The effect to remove.</param>
        /// <returns><see langword="true"/> if the effect was removed, <see langword="false"/> otherwise.</returns>
        internal bool RemoveEffect(string stateName, IEffect effect)
        {
            if (stateContainers.ContainsKey(stateName))
            {
                return stateContainers[stateName].Effects.Remove(effect);
            }
            return false;
        }

        private static readonly ProfilerMarker StateVisualizerUpdateStateValuesMarker =
            new ProfilerMarker("[MRTK] StateVisualizer.UpdateStateValues");

        // TODO: Custom states/effects should probably be able to set their own parameters.
        // Given that custom states can't yet be added to the StateVisualizer, this
        // is a non-issue for now.

        /// <summary>
        /// Sets the parameter value on each state.
        /// Override + extend this method to implement custom state parameters.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the parameter has changed, <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool UpdateStateValues()
        {
            if (interactable != null)
            {
                using (StateVisualizerUpdateStateValuesMarker.Auto())
                {
                    bool parameterChanged = false;
                    parameterChanged |= UpdateStateValue("Disabled", !interactable.enabled ? 1 : 0);
                    parameterChanged |= UpdateStateValue("PassiveHover", interactable.isHovered ? 1 : 0);
                    parameterChanged |= UpdateStateValue("ActiveHover", interactable.IsActiveHovered ? 1 : 0);
                    parameterChanged |= UpdateStateValue("Select", interactable.GetSelectionProgress());
                    parameterChanged |= UpdateStateValue("Toggle", interactable.IsToggled ? 1 : 0);
                    return parameterChanged;
                }
            }
            return false;
        }

        /// <summary>
        /// Manually sets a state to a given value.
        /// </summary>
        /// <param name="stateName">The name of the state to set.</param>
        /// <param name="newValue">The value to set the state to.</param>
        /// <returns>
        /// <see langword="true"/> if the parameter was changed this frame, <see langword="false"/> if it remained constant.
        /// </returns>
        protected internal bool UpdateStateValue(string stateName, float newValue)
        {
            if (stateContainers.TryGetValue(stateName, out var state))
            {
                state.Value = newValue;
                if (state.PreviousValue != newValue)
                {
                    state.PreviousValue = newValue;
                    return true; // The parameter changed!
                }
                state.PreviousValue = newValue;
            }

            return false;
        }

        private static readonly ProfilerMarker StateVisualizerEvaluateEffectsMarker =
            new ProfilerMarker("[MRTK] StateVisualizer.EvaluateEffects");

        /// <summary>
        /// Fires <see cref="IEffect.Evaluate"/> on all valid effects in every state.
        /// Uses the parameter currently set on the <see cref="StateVisualizer.State"/>.
        /// Call <see cref="StateVisualizer.UpdateStateValues"/> before calling this method.
        /// </summary>
        /// <remarks>
        /// The <see cref="StateVisualizer"/> and connected <see cref="Animator"/> will be put to
        /// sleep if this returns <see langword="true"/>.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if all effects are done playing, <see langword="false"/> otherwise.
        /// </returns>
        private bool EvaluateEffects()
        {
            using (StateVisualizerEvaluateEffectsMarker.Auto())
            {
                bool allEffectsDone = true;

                foreach (var kvp in stateContainers)
                {
                    foreach (IEffect effect in kvp.Value.Effects)
                    {
                        if (effect == null) { continue; }

                        // If it's a mixable effect, we need to update the weighting.
                        if (effect is IAnimationMixableEffect mixableEffect)
                        {
                            allEffectsDone &= UpdateWeight(mixableEffect, kvp.Value);
                        }

                        allEffectsDone &= effect.Evaluate(kvp.Value.Value);
                    }
                }

                return allEffectsDone;
            }
        }

        // Updates weights on mixable effects, based on transition settings (or lack thereof)
        // Assumes it will be called once per frame (for transition timing)
        // Returns true if all transitions are complete, false otherwise.
        private bool UpdateWeight(IAnimationMixableEffect mixableEffect, State state)
        {
            bool done = true;

            if (mixableEffect.WeightMode == WeightType.MatchStateValue)
            {
                // Set the playable's weight directly to the state's value.
                animationMixerPlayable.SetInputWeight(mixableEffect.Playable, state.Value);
            }
            else if (mixableEffect.WeightMode == WeightType.Transition)
            {
                // Grab the current weight, using our cached mixable indices.
                float currentWeight = animationMixerPlayable.GetInputWeight(mixableIndices[mixableEffect]);

                // Compute the direction of the transition.
                bool shouldBeActive = !Mathf.Approximately(state.Value, 0.0f);
                int transitionDirection = shouldBeActive ? 1 : -1;

                // Compute and set the new weight.
                float newWeight = Mathf.Clamp01(currentWeight + transitionDirection * (Time.deltaTime / mixableEffect.TransitionDuration));
                animationMixerPlayable.SetInputWeight(mixableEffect.Playable, newWeight);

                // If we're still transitioning, make sure we don't mark the effect as done.
                if ((shouldBeActive && !Mathf.Approximately(newWeight, 1.0f)) || (!shouldBeActive && !Mathf.Approximately(newWeight, 0.0f)))
                {
                    done = false;
                }
            }
            else
            {
                // WeightType.Constant is the only remaining option; the weight is always 1.0.
                animationMixerPlayable.SetInputWeight(mixableEffect.Playable, 1.0f);
            }

            return done;
        }
    }
}
