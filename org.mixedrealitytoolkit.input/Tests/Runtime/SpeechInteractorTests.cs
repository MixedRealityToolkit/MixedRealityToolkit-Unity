// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests for verifying the behavior of <see cref="SpeechInteractor"/> and its hover requirements.
    /// </summary>
    internal class SpeechInteractorTests : BaseRuntimeTests
    {
        private GameObject managerGo;
        private XRInteractionManager interactionManager;

        private GameObject speechGo;
        private SpeechInteractor speechInteractor;

        private GameObject gazeInteractorGo;
        private TestGazeInteractor gazeInteractor;

        private GameObject rayInteractorGo;
        private TestRayInteractor rayInteractor;

        private List<GameObject> spawnedObjects = new List<GameObject>();

        private const string TestKeyword = "select";

        /// <summary>
        /// Mock interactor implementing <see cref="IGazeInteractor"/> to simulate gaze hover.
        /// </summary>
        private class TestGazeInteractor : XRBaseInteractor, IGazeInteractor
        {
            public List<IXRInteractable> validTargets = new List<IXRInteractable>();

            public override void GetValidTargets(List<IXRInteractable> targets)
            {
                targets.Clear();
                targets.AddRange(validTargets);
            }

            public override bool CanHover(IXRHoverInteractable interactable) => true;
            public override bool isHoverActive => true;
        }

        /// <summary>
        /// Mock interactor implementing <see cref="IRayInteractor"/> to simulate active ray hover.
        /// </summary>
        private class TestRayInteractor : XRBaseInteractor, IRayInteractor
        {
            public List<IXRInteractable> validTargets = new List<IXRInteractable>();

            public override void GetValidTargets(List<IXRInteractable> targets)
            {
                targets.Clear();
                targets.AddRange(validTargets);
            }

            public override bool CanHover(IXRHoverInteractable interactable) => true;
            public override bool isHoverActive => true;
        }

        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.Setup();

            managerGo = new GameObject("InteractionManager");
            interactionManager = managerGo.AddComponent<XRInteractionManager>();

            speechGo = new GameObject("SpeechInteractor");
            speechInteractor = speechGo.AddComponent<SpeechInteractor>();
            speechInteractor.interactionManager = interactionManager;

            gazeInteractorGo = new GameObject("GazeInteractor");
            gazeInteractor = gazeInteractorGo.AddComponent<TestGazeInteractor>();
            gazeInteractor.interactionManager = interactionManager;

            rayInteractorGo = new GameObject("RayInteractor");
            rayInteractor = rayInteractorGo.AddComponent<TestRayInteractor>();
            rayInteractor.interactionManager = interactionManager;

            yield return null;
        }

        [UnityTearDown]
        public override IEnumerator TearDown()
        {
            foreach (var go in spawnedObjects)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }
            spawnedObjects.Clear();

            Object.Destroy(speechGo);
            Object.Destroy(gazeInteractorGo);
            Object.Destroy(rayInteractorGo);
            Object.Destroy(managerGo);

            yield return base.TearDown();
        }

        private StatefulInteractable CreateInteractable(bool voiceRequiresFocus = true, string keyword = TestKeyword)
        {
            var go = new GameObject("TestInteractable");
            go.AddComponent<BoxCollider>();
            var stateful = go.AddComponent<StatefulInteractable>();
            stateful.VoiceRequiresFocus = voiceRequiresFocus;
            stateful.SpeechRecognitionKeyword = keyword;

            spawnedObjects.Add(go);

            LogAssert.Expect(LogType.Warning, "Failed to retrieve a running KeywordRecognitionSubsystem while registering an interactable. " +
                "Please make sure the subsystem is correctly set up for this platform or disable this speech interactor if it's unused.");
            stateful.interactionManager = interactionManager;

            return stateful;
        }

        private void HoverWithGaze(StatefulInteractable target)
        {
            gazeInteractor.validTargets.Add(target);
            interactionManager.HoverEnter(gazeInteractor, target);
        }

        private void UnhoverWithGaze(StatefulInteractable target)
        {
            gazeInteractor.validTargets.Remove(target);
            interactionManager.HoverExit(gazeInteractor, target);
        }

        private void HoverWithRay(StatefulInteractable target)
        {
            rayInteractor.validTargets.Add(target);
            interactionManager.HoverEnter(rayInteractor, target);
        }

        private void UnhoverWithRay(StatefulInteractable target)
        {
            rayInteractor.validTargets.Remove(target);
            interactionManager.HoverExit(rayInteractor, target);
        }

        /// <summary>
        /// Ensures default HoverMode is Any, which allows both gaze and active hover to satisfy focus.
        /// </summary>
        [UnityTest]
        public IEnumerator TestDefaultHoverModeIsAny()
        {
            Assert.AreEqual(SpeechHoverMode.Any, speechInteractor.HoverMode, "Default HoverMode should be Any.");

            var target = CreateInteractable(voiceRequiresFocus: true);
            yield return null;

            // 1. Without hover: keyword should not select
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsFalse(target.isSelected, "Should not select without hover when VoiceRequiresFocus is true.");

            // 2. With gaze hover: keyword should select
            HoverWithGaze(target);
            Assert.IsTrue(target.isHovered, "Interactable should be hovered.");
            Assert.IsTrue(target.IsGazeHovered.Active, "Interactable IsGazeHovered should be active.");

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Gaze hover should satisfy focus in Any mode.");
            UnhoverWithGaze(target);

            // Wait for speech interactor trigger time to expire
            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
            Assert.IsFalse(target.isSelected);

            // 3. With active (ray) hover: keyword should select
            HoverWithRay(target);
            Assert.IsTrue(target.isHovered, "Interactable should be hovered.");
            Assert.IsTrue(target.IsActiveHovered.Active, "Interactable IsActiveHovered should be active.");

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Active hover should satisfy focus in Any mode.");
            UnhoverWithRay(target);

            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
        }

        /// <summary>
        /// Ensures that when HoverMode is Gaze, only gaze hover enables voice selection.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHoverModeGaze()
        {
            speechInteractor.HoverMode = SpeechHoverMode.Gaze;
            Assert.AreEqual(SpeechHoverMode.Gaze, speechInteractor.HoverMode);

            var target = CreateInteractable(voiceRequiresFocus: true);
            yield return null;

            // 1. Without hover: should not select
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsFalse(target.isSelected);

            // 2. With active (ray) hover: should NOT select in Gaze mode
            HoverWithRay(target);
            Assert.IsTrue(target.isHovered);
            Assert.IsTrue(target.IsActiveHovered.Active);
            Assert.IsFalse(target.IsGazeHovered.Active);

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsFalse(target.isSelected, "Active hover should not satisfy Gaze hover requirement.");
            UnhoverWithRay(target);
            yield return null;

            // 3. With gaze hover: should select!
            HoverWithGaze(target);
            Assert.IsTrue(target.IsGazeHovered.Active);

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Gaze hover should satisfy Gaze hover requirement.");
            UnhoverWithGaze(target);

            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
        }

        /// <summary>
        /// Ensures that when HoverMode is Active, only active hover (ray, poke, grab) enables voice selection.
        /// </summary>
        [UnityTest]
        public IEnumerator TestHoverModeActive()
        {
            speechInteractor.HoverMode = SpeechHoverMode.Active;
            Assert.AreEqual(SpeechHoverMode.Active, speechInteractor.HoverMode);

            var target = CreateInteractable(voiceRequiresFocus: true);
            yield return null;

            // 1. Without hover: should not select
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsFalse(target.isSelected);

            // 2. With gaze hover: should NOT select in Active mode
            HoverWithGaze(target);
            Assert.IsTrue(target.isHovered);
            Assert.IsTrue(target.IsGazeHovered.Active);
            Assert.IsFalse(target.IsActiveHovered.Active);

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsFalse(target.isSelected, "Gaze hover should not satisfy Active hover requirement.");
            UnhoverWithGaze(target);
            yield return null;

            // 3. With active (ray) hover: should select!
            HoverWithRay(target);
            Assert.IsTrue(target.IsActiveHovered.Active);

            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Active hover should satisfy Active hover requirement.");
            UnhoverWithRay(target);

            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
        }

        /// <summary>
        /// Ensures that when VoiceRequiresFocus is false, voice selection occurs regardless of hover state.
        /// </summary>
        [UnityTest]
        public IEnumerator TestVoiceRequiresFocusFalse()
        {
            var target = CreateInteractable(voiceRequiresFocus: false);
            yield return null;

            // In Any mode without hover: should select
            speechInteractor.HoverMode = SpeechHoverMode.Any;
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Should select in Any mode without hover when VoiceRequiresFocus is false.");
            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);

            // In Gaze mode without hover: should select
            speechInteractor.HoverMode = SpeechHoverMode.Gaze;
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Should select in Gaze mode without hover when VoiceRequiresFocus is false.");
            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);

            // In Active mode without hover: should select
            speechInteractor.HoverMode = SpeechHoverMode.Active;
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Should select in Active mode without hover when VoiceRequiresFocus is false.");
            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
        }

        /// <summary>
        /// Ensures that registering an interactable multiple times with the same keyword
        /// does not result in duplicate selection or exit events.
        /// </summary>
        [UnityTest]
        public IEnumerator TestDuplicateRegistrationDoesNotCauseDuplicateSelectOrExit()
        {
            var target = CreateInteractable(voiceRequiresFocus: false);
            yield return null;

            // Explicitly register the same interactable again with the same keyword
            speechInteractor.RegisterInteractable(target, TestKeyword);

            // Trigger voice command
            speechInteractor.OnKeywordRecognized(TestKeyword);
            Assert.IsTrue(target.isSelected, "Target should be selected.");

            // Wait for voice command duration to expire
            yield return new WaitForSeconds(speechInteractor.VoiceCommandTriggerTime + 0.1f);
            Assert.IsFalse(target.isSelected, "Target should no longer be selected after trigger time expires.");
        }
    }
}
