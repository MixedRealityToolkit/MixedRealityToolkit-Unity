// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// Binds a <see cref="ColorThemeItemData"/> theme item to a single tint effect on a
    /// <see cref="StateVisualizer"/> component.
    /// </summary>
    /// <remarks>
    /// Add one of these binders per state (or per state+effect combination) to a
    /// <see cref="ThemeBinding"/> component. Each binder reads a <c>Color</c> from the
    /// active theme and routes it to the matching tint effect via
    /// <see cref="StateVisualizer.TrySetStateTintColor(string, UnityEngine.Object, Color)"/>.
    /// <para/>
    /// Set <see cref="StateName"/> to one of the built-in state names (<c>Disabled</c>,
    /// <c>PassiveHover</c>, <c>ActiveHover</c>, <c>Select</c>, <c>Toggle</c>) or any
    /// custom state you have added.
    /// <para/>
    /// Leave <see cref="TintTarget"/> unassigned to repaint every <c>ITintEffect</c> in that
    /// state. Assign it to the specific <c>Graphic</c> or <c>SpriteRenderer</c> that appears
    /// in the tintables list of the effect you want to target — this is rename-proof and
    /// breaks visibly (missing reference) rather than silently if the object is deleted.
    /// This is the preferred approach when one state contains multiple tint effects that need
    /// different colors (e.g. a background tint and an icon tint).
    /// </remarks>
    [System.Serializable]
    public class StateVisualizerEffectColorBinder : BaseThemeBinder<Color, StateVisualizer>
    {
        [SerializeField]
        [Tooltip("The name of the StateVisualizer state to target (e.g. 'PassiveHover', 'Toggle').")]
        private string stateName = "PassiveHover";

        /// <summary>The name of the state whose tint effect(s) will be updated.</summary>
        public string StateName
        {
            get => stateName;
            set => stateName = value;
        }

        [SerializeField]
        [Tooltip("The specific Graphic or SpriteRenderer that appears in the tintables list of " +
                 "the effect to target. Leave unassigned to update every tint effect in the state.")]
        private UnityEngine.Object tintTarget;

        /// <summary>
        /// The specific tintable component used to identify which <c>ITintEffect</c> to update,
        /// or <see langword="null"/> to update all tint effects in the state.
        /// </summary>
        /// <remarks>
        /// Assign the same <c>Graphic</c> or <c>SpriteRenderer</c> that is listed in the
        /// effect's <c>Tintables</c> array in the Inspector. This is rename-proof and breaks
        /// visibly (missing reference) rather than silently if the object is deleted.
        /// </remarks>
        public UnityEngine.Object TintTarget
        {
            get => tintTarget;
            set => tintTarget = value;
        }

        /// <inheritdoc />
        protected override void Apply(BaseThemeItemData<Color> themeItemData)
        {
            if (Target == null)
            {
                Debug.LogWarning($"{nameof(StateVisualizerEffectColorBinder)}: Target {nameof(StateVisualizer)} is null. Skipping Apply.");
                return;
            }

            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogWarning($"{nameof(StateVisualizerEffectColorBinder)}: {nameof(StateName)} is not set. Skipping Apply.");
                return;
            }

            Target.TrySetStateTintColor(stateName, tintTarget, themeItemData.Value);
        }
    }
}
