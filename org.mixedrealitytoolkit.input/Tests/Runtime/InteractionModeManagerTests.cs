// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Simulation;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests to ensure the proper behavior of the interaction mode manager.
    /// </summary>
    [Obsolete("This has been replaced by InteractionModeManagerTestsForControllerlessRig.")]
    public class InteractionModeManagerTests : BaseRuntimeInputTests
    {
        /// <summary>
        /// Override of the rig version to use for these tests. These tests validate that the old rig remain functional.
        /// The <see cref="InteractionModeManagerTestsForControllerlessRig"/> will validate the new rig.
        /// </summary>
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        /// <summary>
        /// Tests that the proximity detector detects when to change the controllers interaction mode and properly toggles the associated interactors.
        /// Also checks that the proximity detector doesn't trigger hovers on other objects
        /// </summary>
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

            XRBaseController rightHandController = CachedLookup.RightHandController;
            Assert.IsTrue(rightHandController != null, "No controllers found for right hand.");

            // Magic number is tuned for a prox detector on the index tip with
            // a radius (collider) of 0.1. This is so that the prox detector should
            // overlap with the cube, but none of the interactors will.
            yield return rightHand.MoveTo(cube.transform.position + Vector3.back * 0.12f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(cube.GetComponent<StatefulInteractable>().isHovered,
                          "Interactable was hovered when it shouldn't have been. Was the radius of any of the interactors changed, or is a proximity detector firing hovers?");

            Assert.IsTrue(InteractionModeManagerTestsForControllerlessRig.AnyProximityDetectorsTriggered(),
                           "The proximity detector should have detected the cube. Was the detector's radius changed, or is it broken?");

            InteractionMode currentMode = rightHandController.GetComponentInChildren<ProximityDetector>().ModeOnDetection;
            ValidateInteractionModeActive(rightHandController, currentMode);

            yield return null;
        }

        /// <summary>
        /// Tests the basic Interaction detector. The controller should enter one mode during hover, another during select, and fall back to the default mode during neither.
        /// </summary>
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

            XRBaseController rightHandController = CachedLookup.RightHandController;
            Assert.IsTrue(rightHandController != null, "No controllers found for right hand.");

            // Moving the hand to a position where it's far ray is hovering over the cube
            yield return rightHand.AimAt(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionDetector interactionDetector = rightHandController.GetComponentInChildren<MRTKRayInteractor>().GetComponent<InteractionDetector>();

            InteractionMode expectedMode = interactionDetector.ModeOnHover;
            Assert.AreEqual(expectedMode, interactionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandController, expectedMode);

            // Select the cube and check that we're in the correct mode
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            expectedMode = interactionDetector.ModeOnSelect;
            Assert.AreEqual(expectedMode, interactionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandController, expectedMode);

            // Release the selection and move the hand far away and validate that we are in the default mode
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position + new Vector3(3.0f, 0, 0));
            yield return RuntimeTestUtilities.WaitForUpdates();
            expectedMode = InteractionModeManager.Instance.DefaultMode;
            ValidateInteractionModeActive(rightHandController, expectedMode);

            // Put the hand into a grab state and validate that we are in the default mode, since we're not selecting an object
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandController, expectedMode);

            // Release the grab state and validate that we are in the default mode
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandController, expectedMode);
        }

        /// <summary>
        /// Tests that mode mediation works properly.
        /// </summary>
        /// <remarks>
        /// The interaction mode with the higher priority should be the valid one which affects the controller.
        /// This test operates on the basic assumption that the priority order is <c>FarRayHover</c> &lt; <c>Near</c> &lt; <c>GrabSelect</c>.
        /// </remarks>
        [UnityTest]
        public IEnumerator ModeMediationTest()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser(1.5f);
            cube.transform.localScale = Vector3.one * 0.2f;
            cube.AddComponent<StatefulInteractable>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());
            yield return RuntimeTestUtilities.WaitForUpdates();

            XRBaseController rightHandController = CachedLookup.RightHandController;
            Assert.IsTrue(rightHandController != null, "No controllers found for right hand.");

            // Grab stabilization == ray stabilization
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, ControllerAnchorPoint.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionDetector rayInteractionDetector = rightHandController.GetComponentInChildren<MRTKRayInteractor>().GetComponent<InteractionDetector>();

            // Moving the hand to a position where its far ray is hovering over the cube
            yield return rightHand.AimAt(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            InteractionMode farRayMode = rayInteractionDetector.ModeOnHover;
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.AreEqual(farRayMode, rayInteractionDetector.ModeOnDetection);
            ValidateInteractionModeActive(rightHandController, farRayMode);

            // Now move the hand in range for the proximity detector
            yield return rightHand.MoveTo(cube.transform.position - Vector3.forward * 0.09f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionMode nearMode = rightHandController.GetComponentInChildren<ProximityDetector>().ModeOnDetection;
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandController, nearMode);
            Assert.IsTrue(nearMode.Priority > farRayMode.Priority);

            // Finally move in for a grab
            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();

            InteractionDetector grabInteractionDetector = rightHandController.GetComponentInChildren<GrabInteractor>().GetComponent<InteractionDetector>();

            InteractionMode grabMode = grabInteractionDetector.ModeOnSelect;
            Assert.AreEqual(grabMode, grabInteractionDetector.ModeOnDetection);
            yield return RuntimeTestUtilities.WaitForUpdates();
            ValidateInteractionModeActive(rightHandController, grabMode);
            Assert.IsTrue(grabMode.Priority > nearMode.Priority);

            // Run it all in reverse and make sure the interaction stack is in order
            // Now move the hand in range for the proximity detector
            yield return rightHand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position - Vector3.forward * 0.09f);
            yield return RuntimeTestUtilities.WaitForUpdates();

            ValidateInteractionModeActive(rightHandController, nearMode);

            // Moving the hand to a position where it's far ray is hovering over the cube
            yield return rightHand.MoveTo(cube.transform.position + new Vector3(0.02f, -0.1f, -0.8f));
            yield return RuntimeTestUtilities.WaitForUpdates(frameCount: 120);

            ValidateInteractionModeActive(rightHandController, farRayMode);
        }

        /// <summary>
        /// Validates that an interaction mode is active for the specified controller
        /// </summary>
        /// <param name="controller">The controller we are checking</param>
        /// <param name="currentMode">The interaction mode we expect to be active for the controller</param>
        private void ValidateInteractionModeActive(XRBaseController controller, InteractionMode currentMode)
        {
            // We construct the list of managed interactor types manually because we don't want to expose the internal controller mapping implementation to even internal use, since
            // we don't want any other class to be able to modify those collections without going through the Mode Manager or its in-editor inspector.
            HashSet<Type> managedInteractorTypes = new HashSet<System.Type>(InteractionModeManager.Instance.PrioritizedInteractionModes.SelectMany(x => x.AssociatedTypes));
            HashSet<Type> activeInteractorTypes = InteractionModeManager.Instance.PrioritizedInteractionModes.Find(x => x.ModeName == currentMode.Name).AssociatedTypes;

            // Ensure the prox detector has actually had the desired effect of enabling/disabling interactors.
            foreach (Type interactorType in managedInteractorTypes)
            {
                if (controller.GetComponentInChildren(interactorType) is XRBaseInputInteractor interactor && interactor != null)
                {
                    Assert.AreEqual(activeInteractorTypes.Contains(interactorType), interactor.enabled);
                }
            }
        }
    }
}
#pragma warning restore CS1591
