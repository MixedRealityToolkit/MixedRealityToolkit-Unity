// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using MixedReality.Toolkit.Input.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using HandshapeId = MixedReality.Toolkit.Input.HandshapeTypes.HandshapeId;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Basic tests for verifying user input and basic interactions for the XRI3+ controllerless MRTK rig.
    /// </summary>
    /// <remarks>
    /// This tests are equivalent to those in <see cref="BasicInputTests"/> but they test with the new MRTK Rig that was
    /// created for the XRI3 migration.  Eventually, this will replace the original <see cref="BasicInputTests"/> when
    /// the deprecated pre-XRI3 rig is removed in its entirety from MRTK3.
    /// Note:  This class contains only the tests that are specific to the XRI3+ rig.  Tests that are common to both rigs are in the
    ///        original <see cref="BasicInputTests"/>.  Once the XRI3 migration is completed by removing all the pre-XRI3
    ///        prefabs then those tests can be moved to this class.
    /// </remarks>
    public class BasicInputTestsForControllerlessRig : BaseRuntimeInputTests
    {
        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.SetupForControllerlessRig();
        }

        /// <summary>
        /// Ensure the simulated input devices bind to the hands and gaze on the rig.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="BasicInputTests.InputBindingSmoketest"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator InputBindingSmoketest()
        {
            var TrackedPoseDrivers = new[] {
                CachedTrackedPoseDriverLookup.LeftHandTrackedPoseDriver,
                CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver,
                CachedTrackedPoseDriverLookup.GazeTrackedPoseDriver
            };

            foreach (var trackedPoseDriver in TrackedPoseDrivers)
            {
                Assert.That(trackedPoseDriver, Is.Not.Null);
                Assert.That(trackedPoseDriver, Is.AssignableTo(typeof(TrackedPoseDriver)));

                TrackedPoseDriver actionBasedController = trackedPoseDriver;
                Assert.That(actionBasedController.positionAction.controls, Has.Count.GreaterThanOrEqualTo(1));
            }

            yield return null;
        }

        /// <summary>
        /// Ensure the simulated input device actually makes the rig's hands move/actuate.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="BasicInputTests.HandMovingSmoketest"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator HandMovingSmoketest()
        {
            var trackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;

            var testHand = new TestHand(Handedness.Right);
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, ControllerAnchorPoint.Device);

            yield return testHand.Show(Vector3.forward);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.That(trackedPoseDriver.transform.position.x, Is.EqualTo(0.0f).Within(0.05f));

            yield return testHand.Move(Vector3.right * 0.5f, 60);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Debug.Log("Input system update mode: " + InputSystem.settings.updateMode);

            Assert.That(trackedPoseDriver.positionAction.controls, Has.Count.GreaterThanOrEqualTo(1));
            Assert.That(trackedPoseDriver.positionAction.activeControl, Is.Not.Null);
            Assert.That(trackedPoseDriver.positionAction.ReadValue<Vector3>().x, Is.EqualTo(0.5f).Within(0.01f));

            Assert.That(trackedPoseDriver.transform.position.x, Is.EqualTo(0.5f).Within(0.05f));

            yield return null;
        }

        /// <summary>
        /// Tests whether disabling an interactable mid-interaction will
        /// break XRDirectInteractor. Repro test for ADO#1582/1581.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="BasicInputTests.InteractableDisabledDuringInteraction"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator InteractableDisabledDuringInteraction()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(1.0f, 0.1f, 1.0f));
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.AddComponent<StatefulInteractable>();

            // Otherwise, poke will conflict with grab.
            cube.GetComponent<StatefulInteractable>().selectMode = InteractableSelectMode.Multiple;

            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());

            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(cube.GetComponent<StatefulInteractable>().IsGrabSelected,
                          "StatefulInteractable did not get GrabSelected.");

            cube.SetActive(false);

            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(cube.GetComponent<StatefulInteractable>().IsGrabSelected,
                           "StatefulInteractable did not get un-GrabSelected.");

            yield return rightHand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            cube.SetActive(true);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(BasicInputTests.AnyProximityDetectorsTriggered(),
                           "ProximityInteractor was still hovering after re-enabling faraway object.");

            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            Assert.IsTrue(rightHandTrackedPoseDriver != null, "No TrackedPoseDriver found for right hand.");

            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>().enabled, "Ray didn't reactivate");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<GazePinchInteractor>().enabled, "GazePinch didn't reactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<PokeInteractor>().enabled, "Poke didn't deactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<GrabInteractor>().enabled, "Grab didn't deactivate");

            yield return null;
        }

        /// <summary>
        /// Tests whether spawning an interactable on top of a hand will cause problems with the proximity detector.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="BasicInputTests.SpawnInteractableOnHand"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SpawnInteractableOnHand()
        {
            // Spawn our hand.
            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Prox detector should start out un-triggered.
            Assert.IsFalse(BasicInputTests.AnyProximityDetectorsTriggered(), "Prox detector started out triggered, when it shouldn't be (no cube yet!)");

            // Rays should start enabled
            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            Assert.IsTrue(rightHandTrackedPoseDriver != null, "No TrackedPoseDriver found for right hand.");

            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>().enabled, "Ray didn't start active");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<GazePinchInteractor>().enabled, "GazePinch didn't start active");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<PokeInteractor>().enabled, "Poke started active, when it shouldn't");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<GrabInteractor>().enabled, "Grab started active, when it shouldn't");

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser();
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.AddComponent<StatefulInteractable>();
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(BasicInputTests.AnyProximityDetectorsTriggered(), "Prox detector should see it!");

            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>().enabled, "Ray didn't disable on proximity");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<GazePinchInteractor>().enabled, "GazePinch disable on proximity");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<PokeInteractor>().enabled, "Poke didn't activate on proximity");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<GrabInteractor>().enabled, "Grab didn't activate on proximity");

            // Move hand far away.
            yield return rightHand.MoveTo(new Vector3(2, 2, 2));
            yield return RuntimeTestUtilities.WaitForUpdates(frameCount:240);

            Assert.IsFalse(BasicInputTests.AnyProximityDetectorsTriggered(), "Prox detectors should no longer be triggered.");

            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>().enabled, "Ray didn't reactivate");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<GazePinchInteractor>().enabled, "GazePinch didn't reactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<PokeInteractor>().enabled, "Poke didn't deactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<GrabInteractor>().enabled, "Grab didn't deactivate");

            yield return null;
        }

        /// <summary>
        /// Tests that tracked hands initiate new interactions correctly and interactable IsGrabSelected property updates correctly.
        /// </summary>
        /// <remarks>
        /// This test was originally meant to be the XRI3+ equivalent of <see cref="BasicInputTests.UntrackedControllerNearInteractions"/>,
        /// however, the TestHand class inherits from XRSimualtedController for which there is no XRI3 equivalent and despite simulating
        /// tracking disabling the interactions are still initiated because the interactors use the new TrackedPoseDriver component.
        /// Therefore, since there is no way to simulate tracking loss in XRI3 TrackedPoseDriver (at least at the moment of this writing) then
        /// this test was repurposed to test interactions with normally tracked hands.
        /// Note: Next is a list of this of things tried to simulate loss of tracking with XRI3+ controllerless hands: disabling the TrackedPoseDriver
        ///       component, destroying the TrackedPoseDriver component, setting TrackedPoseDriver.ignoreTrackingState, setting
        ///       TrackedPoserDriver.trackingStateInput to a new InputActionProperty (it is not nullable).  Unfortunately none worked for simulating
        ///       tracking loss.  Therefore this replacement test was repurposed to test tracked hands and ensure it initiates new interactions
        ///       properly and that the interactable IsGrabSelected property is updated properly.
        /// </remarks>
        [UnityTest]
        public IEnumerator TrackedHandNearInteractions()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(1.0f, 0.1f, 1.0f));
            cube.transform.localScale = Vector3.one * 0.1f;
            cube.AddComponent<StatefulInteractable>();

            // Otherwise, poke will conflict with grab.
            cube.GetComponent<StatefulInteractable>().selectMode = InteractableSelectMode.Multiple;

            var rightHand = new TestHand(Handedness.Right);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();
            // First ensure that the interactor can interact with a cube normally
            yield return rightHand.MoveTo(cube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(cube.GetComponent<StatefulInteractable>().IsGrabSelected,
                           "StatefulInteractable is no longer GrabSelected.");

            // Make sure state is maintained even if the hand gameobject moves
            yield return rightHand.Move(Vector3.left);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.IsTrue(cube.GetComponent<StatefulInteractable>().IsGrabSelected,
                           "StatefulInteractable is no longer GrabSelected.");

            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(cube.GetComponent<StatefulInteractable>().IsGrabSelected,
                           "StatefulInteractable did not get un-GrabSelected.");

            // Check that the hand cannot interact with any new interactables
            var newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newCube.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(-3.0f, 0.1f, 1.0f));
            newCube.transform.localScale = Vector3.one * 0.1f;
            newCube.AddComponent<StatefulInteractable>();

            yield return rightHand.MoveTo(newCube.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.SetHandshape(HandshapeId.Pinch);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(newCube.GetComponent<StatefulInteractable>().IsGrabSelected,
                          "The interactor grabbed the new cube");

            yield return rightHand.SetHandshape(HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Finish
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.Show();
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(BasicInputTests.AnyProximityDetectorsTriggered(),
                           "ProximityInteractor was still hovering after re-enabling faraway object.");

            TrackedPoseDriver rightHandTrackedPoseDriver = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver;
            Assert.IsTrue(rightHandTrackedPoseDriver != null, "No TrackedPoseDriver found for right hand.");

            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>().enabled, "Ray didn't reactivate");
            Assert.IsTrue(rightHandTrackedPoseDriver.GetComponentInChildren<GazePinchInteractor>().enabled, "GazePinch didn't reactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<PokeInteractor>().enabled, "Poke didn't deactivate");
            Assert.IsFalse(rightHandTrackedPoseDriver.GetComponentInChildren<GrabInteractor>().enabled, "Grab didn't deactivate");

            yield return null;
        }
    }
}
#pragma warning restore CS1591
