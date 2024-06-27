// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests to ensure the proper behavior of the interaction mode manager.
    /// </summary>
    /// <remarks>
    /// This tests are equivalent to those in <see cref="InteractionModeManagerTests"/> but they test with the new MRTK Rig that was
    /// created for the XRI3 migration.  Eventually, this will replace the original <see cref="InteractionModeManagerTests"/> when
    /// the deprecated pre-XRI3 rig is removed in its entirety from MRTK3.
    /// Note:  This class contains only the tests that are specific to the XRI3+ rig.  Tests that are common to both rigs are in the
    ///        original <see cref="InteractionModeManagerTests"/>.  Once the XRI3 migration is completed by removing all the pre-XRI3
    ///        prefabs then those tests can be moved to this class.
    /// </remarks>
    public class InteractionModeManagerTestsForControllerlessRig : BaseRuntimeInputTests
    {
        /// <summary>
        /// Tests that the proximity detector detects when to change the hand's interaction mode and properly toggles the associated interactors.
        /// Also checks that the proximity detector doesn't trigger hovers on other objects
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="InteractionModeManagerTests.ProximityDetectorTest"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator ProximityDetectorTest()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(1.0f, 0.1f, 1.0f);
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.AddComponent<StatefulInteractable>();

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(new Vector3(0, 0, 0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            Assert.IsTrue(rightHandTrackedPoseDriver != null, "Right hand TrackedPoseDriver was not found.");

            // Magic number is tuned for a prox detector on the index tip with
            // a radius (collider) of 0.12. This is so that the prox detector should
            // overlap with the cube, but none of the interactors will.
            yield return rightHand.MoveTo(cube.transform.position + Vector3.back * 0.12f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(cube.GetComponent<StatefulInteractable>().isHovered,
                          "Interactable was hovered when it shouldn't have been. Was the radius of any of the interactors changed, or is a proximity detector firing hovers?");

            Assert.IsTrue(InteractionModeManagerTests.AnyProximityDetectorsTriggered(),
                           "The proximity detector should have detected the cube. Was the detector's radius changed, or is it broken?");

            InteractionMode currentMode = rightHandTrackedPoseDriver.transform.parent.GetComponentInChildren<ProximityDetector>().ModeOnDetection;
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, currentMode);

            yield return null;
        }

        /// <summary>
        /// Tests the basic Interaction detector. The hand should enter one mode during hover, another during select, and fall back to the default mode during neither
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="InteractionModeManagerTests.InteractionDetectorTest"/>.
        /// </remarks>
        [UnityTest]
        public IEnumerator InteractionDetectorTest()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser(1.5f);
            cube.transform.localScale = Vector3.one * 0.2f;
            cube.AddComponent<StatefulInteractable>();

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());
            yield return RuntimeTestUtilities.WaitForUpdates();

            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            InteractionDetector rightHandInteractionDetector = rightHandTrackedPoseDriver.transform.parent.GetComponentInChildren<MRTKRayInteractor>().GetComponent<InteractionDetector>();

            // Moving the hand to a position where it's far ray is hovering over the cube
            yield return rightHand.AimAt(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionMode currentMode = rightHandInteractionDetector.ModeOnHover;
            Assert.AreEqual(currentMode, rightHandInteractionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, currentMode);

            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            currentMode = rightHandInteractionDetector.ModeOnSelect;
            Assert.AreEqual(currentMode, rightHandInteractionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, currentMode);

            // move the hand far away and validate that we are in the default mode
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position + new Vector3(3.0f,0,0));
            yield return RuntimeTestUtilities.WaitForUpdates();

            currentMode = InteractionModeManager.Instance.DefaultMode;
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, currentMode);
        }

        /// <summary>
        /// Tests that mode mediation works properly. 
        /// </summary>
        /// <remarks>
        /// The interaction mode with the higher priority should be the valid one which affects the hand.
        /// This test operates on the basic assumption that the priority order is <c>FarRayHover</c> &lt; <c>Near</c> &lt; <c>GrabSelect</c>.
        /// This test is the XRI3+ equivalent of <see cref="InteractionModeManagerTests.ModeMediationTest"/>.
        /// </remarks>
        [UnityTest]
        public IEnumerator ModeMediationTest()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser(1.5f);
            cube.transform.localScale = Vector3.one * 0.2f;
            cube.AddComponent<StatefulInteractable>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Otherwise, poke will conflict with grab.
            cube.GetComponent<StatefulInteractable>().selectMode = InteractableSelectMode.Multiple;

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());
            yield return RuntimeTestUtilities.WaitForUpdates();

            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            Assert.IsTrue(rightHandTrackedPoseDriver != null, "Right hand TrackedPoseDriver was not found.");

            // Grab stabilization == ray stabilization
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, ControllerAnchorPoint.Device);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Moving the hand to a position where it's far ray is hovering over the cube
            yield return rightHand.AimAt(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionDetector rightHandInteractionDetector = rightHandTrackedPoseDriver.transform.parent.GetComponentInChildren<MRTKRayInteractor>().GetComponent<InteractionDetector>();

            InteractionMode farRayMode = rightHandInteractionDetector.ModeOnHover;
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(farRayMode, rightHandInteractionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, farRayMode);

            // Now move the hand in range for the proximity detector
            yield return rightHand.MoveTo(cube.transform.position - Vector3.forward * 0.09f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionMode nearMode = rightHandTrackedPoseDriver.transform.parent.GetComponentInChildren<ProximityDetector>().ModeOnDetection;
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, nearMode);
            Assert.IsTrue(nearMode.Priority > farRayMode.Priority);

            // Finally move in for a grab
            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();

            rightHandInteractionDetector = rightHandTrackedPoseDriver.transform.parent.GetComponentInChildren<GrabInteractor>().GetComponent<InteractionDetector>();
            InteractionMode grabMode = rightHandInteractionDetector.ModeOnSelect;
            Assert.AreEqual(grabMode, rightHandInteractionDetector.ModeOnDetection);
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandTrackedPoseDriver, grabMode);
            Assert.IsTrue(grabMode.Priority > nearMode.Priority);

            // Run it all in reverse and make sure the interaction stack is in order
            // Now move the hand in range for the proximity detector
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position - Vector3.forward * 0.09f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            ValidateInteractionModeActive(rightHandTrackedPoseDriver, nearMode);

            // Moving the hand to a position where it's far ray is hovering over the cube
            yield return rightHand.MoveTo(cube.transform.position + new Vector3(0.02f, -0.1f, -0.8f));
            yield return RuntimeTestUtilities.WaitForUpdates(frameCount:120);

            ValidateInteractionModeActive(rightHandTrackedPoseDriver, farRayMode);
        }

        /// <summary>
        /// Validates that an interaction mode is active for the specified hand
        /// </summary>
        /// <remarks>
        /// This method is the XRI3+ equivalent of <see cref="InteractionModeManagerTests.ValidateInteractionModeActive(XRBaseController, InteractionMode)"/>
        /// </remarks>
        /// <param name="handTrackedPoseDriver">The <see cref="TrackedPoseDriver"/> hand we are checking</param>
        /// <param name="currentMode">The interaction mode we expect to be active for the hand</param>
        private void ValidateInteractionModeActive(TrackedPoseDriver handTrackedPoseDriver, InteractionMode currentMode)
        {
            // We construct the list of managed interactor types manually because we don't want to expose the internal mapping implementation to even internal use, since
            // we don't want any other class to be able to modify those collections without going through the Mode Manager or its in-editor inspector.
            HashSet<System.Type> managedInteractorTypes = new HashSet<System.Type>(InteractionModeManager.Instance.PrioritizedInteractionModes.SelectMany(x => x.AssociatedTypes));
            HashSet<System.Type> activeInteractorTypes = InteractionModeManager.Instance.PrioritizedInteractionModes.Find(x => x.ModeName == currentMode.Name).AssociatedTypes;

            // Ensure the prox detector has actually had the desired effect of enabling/disabling interactors.
            foreach (System.Type interactorType in managedInteractorTypes)
            {
                XRBaseInteractor interactor = handTrackedPoseDriver.GetComponentInChildren(interactorType) as XRBaseInputInteractor;
                if (interactor != null)
                {
                    Assert.AreEqual(activeInteractorTypes.Contains(interactorType), interactor.enabled);
                }
            }
        }
    }
}
#pragma warning restore CS1591
