// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.TestTools;

using HandshapeId = MixedReality.Toolkit.Input.HandshapeTypes.HandshapeId;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests to evaluate the functionality of the StateVisualizer modular visual driver.
    /// </summary>
    public class StateVisualizerTests : BaseRuntimeInputTests
    {
        /// <summary>
        /// Tests to see if StateVisualizer can correctly evaluate/execute the SetTargetActiveEffect.
        /// Adds/removes the SetTargetActiveEffect from a few different states to make sure everything works.
        /// </summary>
        [UnityTest]
        public IEnumerator TestSetTargetsActiveEffect()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube.AddComponent<StatefulInteractable>() as StatefulInteractable;
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            cube.AddComponent<Animator>();
            StateVisualizer sv = cube.AddComponent<StateVisualizer>() as StateVisualizer;

            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.1f, 0.1f, 1));
            cube.transform.localScale = Vector3.one * 0.1f;

            GameObject cubeToToggle = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Attach a toggle effect to the Select state.
            SetTargetsActiveEffect toggleEffect = new SetTargetsActiveEffect(new List<GameObject>() { cubeToToggle });
            sv.AddEffect("Select", toggleEffect);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "The cube should be immediately toggled off when the effect is added.");

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "Nothing should have happened on hover.");
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsTrue(cubeToToggle.activeSelf, "The toggle effect should have turned on the cube on selection.");
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "The toggle effect should have turned off the cube on deselection.");

            // Detach the effect from the state.
            sv.RemoveEffect("Select", toggleEffect);
            yield return rightHand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "Nothing should have happened on hover.");
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "The toggle effect should have been removed from the StateVisualizer!");
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Attach the same toggle effect to the ActiveHover state instead.
            sv.AddEffect("ActiveHover", toggleEffect);
            yield return rightHand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsTrue(cubeToToggle.activeSelf, "The toggle effect should have turned on the cube on active hover.");
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsTrue(cubeToToggle.activeSelf, "The cube should have stayed toggled.");
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Detach the effect and attach it to the Toggle state instead.
            sv.RemoveEffect("ActiveHover", toggleEffect);
            sv.AddEffect("Toggle", toggleEffect);
            yield return rightHand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Have to turn on toggle mode on the interactable, or else toggles won't toggle :)
            interactable.ToggleMode = StatefulInteractable.ToggleType.Toggle;

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "Nothing should have happened on hover.");
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "Nothing should have happened on selection.");
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsTrue(cubeToToggle.activeSelf, "The toggle effect should have turned on the cube on IsToggled = true.");

            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsFalse(cubeToToggle.activeSelf, "Cube should be toggled off again.");
        }

        /// <summary>
        /// This test effect scales the object up by the parameter value.
        /// </summary>
        private class CustomTestEffect : IEffect
        {
            public Playable Playable => Playable.Null;
            private Vector3 initialLocalScale;
            private GameObject owner;

            // Lets us check the parameter passed in for testing.
            public float LastSetParameter = 0;

            public void Setup(PlayableGraph graph, GameObject owner)
            {
                initialLocalScale = owner.transform.localScale;
                this.owner = owner;
            }

            public bool Evaluate(float parameter)
            {
                owner.transform.localScale = initialLocalScale * (1.0f + parameter);
                LastSetParameter = parameter;
                return true;
            }
        }

        /// <summary>
        /// A minimal mixable effect to verify PlayableGraph connections.
        /// </summary>
        private class TestMixableEffect : IAnimationMixableEffect, IPlayableEffect
        {
            public Playable Playable { get; private set; }

            public float TransitionDuration => 0f;

            public IAnimationMixableEffect.WeightType WeightMode => IAnimationMixableEffect.WeightType.MatchStateValue;

            public void Setup(PlayableGraph graph, GameObject owner)
            {
                AnimationClip clip = new AnimationClip();
                clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.Constant(0f, 1f, 42f));
                Playable = AnimationClipPlayable.Create(graph, clip);
            }

            public bool Evaluate(float parameter) => true;
        }

        /// <summary>
        /// Tests to see whether StateVisualizer can properly evaluate/execute a custom effect,
        /// defined above (CustomTestEffect).
        /// </summary>
        [UnityTest]
        public IEnumerator TestCustomEffect()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube.AddComponent<StatefulInteractable>() as StatefulInteractable;
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            cube.AddComponent<Animator>();
            StateVisualizer sv = cube.AddComponent<StateVisualizer>() as StateVisualizer;

            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.1f, 0.1f, 1));
            cube.transform.localScale = Vector3.one * 0.1f;

            // Attach a toggle effect to the Select state.
            CustomTestEffect customEffect = new CustomTestEffect();
            sv.AddEffect("Select", customEffect);

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(cube.transform.localScale.z, 0.1f, 0.00001f, "Nothing should have happened on hover.");
            Assert.AreEqual(customEffect.LastSetParameter, 0.0f, 0.00001f, "The custom effect should have received the parameter value of 0.0f.");
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(cube.transform.localScale.z, 0.2f, 0.00001f, "The custom effect should have made the cube grow in size on selection.");
            Assert.AreEqual(customEffect.LastSetParameter, 1.0f, 0.00001f, "The custom effect should have received the parameter value of 1.0f.");
            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(cube.transform.localScale.z, 0.1f, 0.00001f, "The custom effect should have returned the cube to its original size on deselection.");
            Assert.AreEqual(customEffect.LastSetParameter, 0.0f, 0.00001f, "The custom effect should have received the parameter value of 0.0f.");
        }

        /// <summary>
        /// Makes sure StateVisualizer is going to sleep/waking up correctly to save resources.
        /// </summary>
        [UnityTest]
        public IEnumerator TestSleepWakeBehavior()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube.AddComponent<StatefulInteractable>() as StatefulInteractable;
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            cube.AddComponent<Animator>();
            StateVisualizer sv = cube.AddComponent<StateVisualizer>() as StateVisualizer;

            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.5f, 0.5f, 1));
            cube.transform.localScale = Vector3.one * 0.1f;

            // Attach a test effect to the Select state.
            CustomTestEffect customEffect = new CustomTestEffect();
            sv.AddEffect("Select", customEffect);

            Assert.IsTrue(sv.Animator.enabled, "The animator should be enabled by default.");

            // Wait for the keepAliveTime
            yield return new WaitForSeconds(0.11f);

            Assert.IsFalse(sv.Animator.enabled, "The animator should be disabled after the keepAliveTime has elapsed.");
            Assert.IsFalse(sv.enabled, "The StateVisualizer should be disabled after the keepAliveTime has elapsed.");

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(sv.Animator.enabled, "The animator should have woken up when hovered.");

            yield return new WaitForSeconds(0.5f);

            Assert.IsTrue(sv.Animator.enabled, "The animator should remain awake throughout a hover.");

            yield return rightHand.MoveTo(Vector3.zero);

            Assert.IsTrue(sv.Animator.enabled, "The animator should still be enabled after a short delay.");

            // Wait for the rest of the keepAliveTime
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(sv.Animator.enabled, "The animator should be disabled after the keepAliveTime has elapsed.");
        }

        /// <summary>
        /// This test effect does not report that it is "done" until the parameter returns to zero.
        /// Used to test the "keep awake" behavior.
        /// </summary>
        private class TestEffectThatJustKeepsGoing : IEffect
        {
            public Playable Playable => Playable.Null;

            public void Setup(PlayableGraph graph, GameObject owner) { }

            public bool Evaluate(float parameter)
            {
                // This effect will only be done once the parameter returns to zero.
                // StateVisualizer will stay awake, waiting for this to be done.
                return parameter == 0.0f;
            }
        }

        /// <summary>
        /// Uses TestEffectThatJustKeepsGoing to see if the StateVisualizer will stay awake
        /// waiting for the effect to be done.
        /// </summary>
        [UnityTest]
        public IEnumerator TestWaitForLongRunningEffect()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube.AddComponent<StatefulInteractable>() as StatefulInteractable;
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            cube.AddComponent<Animator>();
            StateVisualizer sv = cube.AddComponent<StateVisualizer>() as StateVisualizer;

            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.1f, 0.1f, 1));
            cube.transform.localScale = Vector3.one * 0.1f;

            // Attach a test effect to the Select state.
            TestEffectThatJustKeepsGoing customEffect = new TestEffectThatJustKeepsGoing();
            sv.AddEffect("Select", customEffect);

            Assert.IsTrue(sv.Animator.enabled, "The animator should be enabled by default.");

            // Wait for the keepAliveTime
            yield return new WaitForSeconds(0.12f);

            Assert.IsFalse(sv.Animator.enabled, "The animator should be disabled after the keepAliveTime has elapsed.");

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.MoveTo(cube.transform.position);
            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(sv.Animator.enabled, "The animator should have woken up when hovered.");

            yield return new WaitForSeconds(0.5f);

            Assert.IsTrue(sv.Animator.enabled, "The animator should remain awake throughout a hover.");

            yield return rightHand.MoveTo(Vector3.zero);

            Assert.IsTrue(sv.Animator.enabled, "The animator should still be enabled after a short delay.");

            // Wait for the keepAliveTime
            yield return new WaitForSeconds(0.12f);

            Assert.IsFalse(sv.Animator.enabled, "The animator should be disabled after the keepAliveTime has elapsed.");

            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(interactable.isSelected && interactable.IsGrabSelected, "Interactable wasn't selected");
            Assert.IsTrue(sv.Animator.enabled, "The animator should have woken up when selected.");

            // Wait for far longer than the keepAliveTime.
            // The StateVisualizer should remain awake, waiting for our TestEffectThatJustKeepsGoing to be done.
            // However, our test effect will never be done, until we un-hover.
            yield return new WaitForSeconds(0.5f);

            Assert.IsTrue(sv.Animator.enabled, "StateVisualizer did not wait for the effect to be done!");

            yield return rightHand.MoveTo(Vector3.zero);

            // Wait for longer than the keepAliveTime. The hand is still selecting, and so the stateviz shouldn't go back to sleep.
            yield return new WaitForSeconds(0.5f);

            Assert.IsTrue(sv.Animator.enabled, "The animator should remain awake throughout a select.");

            yield return rightHand.SetHandshape(HandshapeId.Open);

            // Wait for longer than the keepAliveTime. The effect should report "done" and the StateVisualizer should go back to sleep.
            yield return new WaitForSeconds(0.25f);

            Assert.IsFalse(sv.Animator.enabled, "StateVisualizer did not go back to sleep!");
        }

        /// <summary>
        /// Makes sure an Animator component is added when necessary.
        /// </summary>
        [UnityTest]
        public IEnumerator TestAnimatorMissing()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.AddComponent<StatefulInteractable>();
            cube.AddComponent<StateVisualizer>();

            yield return null;

            Assert.IsTrue(cube.GetComponent<Animator>() != null, "An animator wasn't automatically added when it was missing!");
        }

        /// <summary>
        /// Tests that setting a new Interactable safely unsubscribes from the old and subscribes to the new.
        /// </summary>
        [UnityTest]
        public IEnumerator TestInteractableHotSwap()
        {
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable1 = cube1.AddComponent<StatefulInteractable>();
            interactable1.DisableInteractorType(typeof(IPokeInteractor));
            cube1.AddComponent<Animator>();
            StateVisualizer sv = cube1.AddComponent<StateVisualizer>();

            cube1.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(-0.2f, 0.1f, 1));
            cube1.transform.localScale = Vector3.one * 0.1f;

            GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable2 = cube2.AddComponent<StatefulInteractable>();
            interactable2.DisableInteractorType(typeof(IPokeInteractor));
            cube2.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.2f, 0.1f, 1));
            cube2.transform.localScale = Vector3.one * 0.1f;

            CustomTestEffect customEffect = new CustomTestEffect();
            sv.AddEffect("Select", customEffect);

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Interact with cube 1
            yield return rightHand.MoveTo(cube1.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(1.0f, customEffect.LastSetParameter, 0.00001f, "Effect should trigger on interactable 1.");

            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(0.0f, customEffect.LastSetParameter, 0.00001f, "Effect should reset.");

            // Swap interactable
            sv.Interactable = interactable2;
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Interact with cube 1 again (should NOT trigger)
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(0.0f, customEffect.LastSetParameter, 0.00001f, "Effect should NOT trigger on interactable 1 after hot-swap.");

            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Interact with cube 2
            yield return rightHand.MoveTo(cube2.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(1.0f, customEffect.LastSetParameter, 0.00001f, "Effect should trigger on interactable 2 after hot-swap.");
        }

        /// <summary>
        /// Tests that setting a new Animator at runtime successfully triggers a Rebuild and updates the target.
        /// </summary>
        [UnityTest]
        public IEnumerator TestAnimatorHotSwap()
        {
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube1.AddComponent<StatefulInteractable>();
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            Animator anim1 = cube1.AddComponent<Animator>();
            StateVisualizer sv = cube1.AddComponent<StateVisualizer>();

            cube1.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.1f, 0.1f, 1));
            cube1.transform.localScale = Vector3.one * 0.1f;

            // Add the effect BEFORE Start() so we don't need to manually Rebuild() to pick it up.
            CustomTestEffect customEffect = new CustomTestEffect();
            sv.AddEffect("Select", customEffect);

            TestMixableEffect mixableEffect = new TestMixableEffect();
            sv.AddEffect("Select", mixableEffect);

            yield return RuntimeTestUtilities.WaitForUpdates();

            GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Animator anim2 = cube2.AddComponent<Animator>();

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(cube1.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(anim1, sv.Animator, "SV should default to Animator on the same GameObject.");
            Assert.IsTrue(anim1.enabled, "The initial Animator should be enabled because the StateVisualizer is hovered.");

            // Swap animator while awake
            sv.Animator = anim2;

            Assert.IsFalse(anim1.enabled, "The old Animator should be disabled immediately after hot-swap.");
            Assert.IsTrue(anim2.enabled, "The new Animator should instantly inherit the playing state from the PlayableGraph.");
            Assert.AreEqual(anim2, sv.Animator, "SV Animator should be updated.");

            // Wait for it to go to sleep
            yield return rightHand.MoveTo(Vector3.zero);
            yield return new WaitForSeconds(0.25f);
            Assert.IsFalse(anim2.enabled, "The new Animator should have been put to sleep by the visualizer after keepAliveTime elapsed.");

            // Hot swap while asleep
            GameObject cube3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Animator anim3 = cube3.AddComponent<Animator>();
            sv.Animator = anim3;

            Assert.IsFalse(anim2.enabled, "The old Animator should still be disabled after hot-swap.");
            Assert.IsFalse(anim3.enabled, "The newly swapped Animator should remain asleep because the PlayableGraph is stopped.");

            yield return rightHand.MoveTo(cube1.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(anim3.enabled, "The new Animator should be enabled when the StateVisualizer wakes up.");

            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(1.0f, customEffect.LastSetParameter, 0.00001f, "Effect should trigger after Animator hot-swap.");
            Assert.AreEqual(42f, cube3.transform.localPosition.y, 0.001f, "The mixable effect should have animated the new Animator's GameObject.");

            // Verify destruction doesn't cause PlayableGraph leaks/errors
            Object.Destroy(cube1);
            Object.Destroy(cube2);
            Object.Destroy(cube3);
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        /// <summary>
        /// Tests that calling Rebuild() multiple times doesn't cause crashes, leaks, or break existing effects.
        /// </summary>
        [UnityTest]
        public IEnumerator TestRebuild()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            StatefulInteractable interactable = cube.AddComponent<StatefulInteractable>();
            interactable.DisableInteractorType(typeof(IPokeInteractor));
            cube.AddComponent<Animator>();
            StateVisualizer sv = cube.AddComponent<StateVisualizer>();
            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.1f, 0.1f, 1));
            cube.transform.localScale = Vector3.one * 0.1f;
            yield return RuntimeTestUtilities.WaitForUpdates();

            CustomTestEffect customEffect = new CustomTestEffect();
            sv.AddEffect("Select", customEffect);

            // Call rebuild multiple times to ensure no crashes or obvious leaks.
            sv.Rebuild();
            sv.Rebuild();
            sv.Rebuild();

            yield return RuntimeTestUtilities.WaitForUpdates();

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.AreEqual(1.0f, customEffect.LastSetParameter, 0.00001f, "Effect should trigger after multiple Rebuilds.");
        }
    }
}
#pragma warning restore CS1591
