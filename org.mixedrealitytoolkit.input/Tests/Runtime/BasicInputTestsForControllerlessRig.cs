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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
        public const string MRTKSpeechName = "MRTK Speech";
        private const string MRTKInteractionManagerName = "MRTK Interaction Manager";
        private const string CameraOffsetName = "Camera Offset";
        private const string CanvasProxyInteractorName = "CanvasProxyInteractor";
        private const string MainCameraName = "Main Camera";
        private const string MRTKRightHandControllerName = "MRTK RightHand Controller";
        private const string MRTKLeftHandConrollerName = "MRTK LeftHand Controller";
        private const string MRTKGazeControllerName = "MRTK Gaze Controller";
        private const string GazeInteractorName = "GazeInteractor";
        private const string MRTKLeftHandDevicePositionName = "MRTK LeftHand/DevicePosition";
        private const string MRTKLeftHandDeviceRotationName = "MRTK LeftHand/DeviceRotation";
        private const string MRTKLeftHandTrackingStateName = "MRTK LeftHand/Tracking State";
        private const string MRTKRightHandDevicePositionName = "MRTK RightHand/DevicePosition";
        private const string MRTKRightHandDeviceRotationName = "MRTK RightHand/DeviceRotation";
        private const string MRTKRightHandTrackingStateName = "MRTK RightHand/Tracking State";
        private const string MRTKGazePositionName = "MRTK Gaze/Position";
        private const string MRTKGazeRotationName = "MRTK Gaze/Rotation";
        private const string MRTKGazeTrackingStateName = "MRTK Gaze/Tracking State";
        private const string MRTKGazeHeadGazePositionName = "MRTK Gaze/Head Gaze Position";
        private const string MRTKGazeHeadGazeRotationName = "MRTK Gaze/Head Gaze Rotation";
        private const string MRTKGazeHeadGazeTrackingStateName = "MRTK Gaze/Head Gaze Tracking State";
        private const string OpenXRLeftHandCloneName = "openxr_left_hand(Clone)";
        private const string OpenXRLeftHandName = "openxr_left_hand";
        private const string OpenXRRightHandCloneName = "openxr_right_hand(Clone)";
        private const string OpenXRRightHandName = "openxr_right_hand";
        private const string IndexTipPokeInteractorName = "IndexTip PokeInteractor";
        private const string FarRayName = "Far Ray";
        private const string GrabInteractorName = "GrabInteractor";
        private const string GazePinchInteractorName = "GazePinchInteractor";
        private const string MRTKLeftHandSelectValueName = "MRTK LeftHand/Select Value";
        private const string MRTKLeftHandSelectName = "MRTK LeftHand/Select";
        private const string MRTKLeftHandActivateName = "MRTK LeftHand/Activate";
        private const string MRTKLeftHandUIPressName = "MRTK LeftHand/UI Press";
        private const string MRTKRightHandSelectValueName = "MRTK RightHand/Select Value";
        private const string MRTKRightHandSelectName = "MRTK RightHand/Select";
        private const string MRTKRightHandActivateName = "MRTK RightHand/Activate";
        private const string MRTKRightHandUIPressName = "MRTK RightHand/UI Press";
        private HashSet<string> deprecatedXRControllerInputActions = new() { "Is Tracked", "Activate Action Value", "UI Press Action Value",
                                                                             "UI Scroll", "Haptic Device", "Directional Anchor Rotation",
                                                                             "Scale Toggle", "Scale Delta", "Select", "Select Action Value",
                                                                             "Activate", "Activate Action Value", "UI Press", "Rotate Anchor",
                                                                             "Directional Anchor Rotation", "Translate Anchor", "Scale Toggle" };

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
        /// however, the <see cref="TestHand"/> class inherits from XRSimulatedController for which there is no XRI3 equivalent and despite
        /// simulating tracking disabling the interactions are still initiated because the interactors use the new <see cref="TrackedPoseDriver">
        /// component.  Therefore, since there is no way to simulate tracking loss in XRI3 <see cref="TrackedPoseDriver"> (at least at the moment
        /// of this writing) then this test was repurposed to test interactions with normally tracked hands.
        /// Note: Next is a list of this of things tried to simulate loss of tracking with XRI3+ controllerless hands: disabling the
        ///       <see cref="TrackedPoseDriver"> component, destroying the <see cref="TrackedPoseDriver"> component, setting
        ///       <see cref="TrackedPoseDriver.ignoreTrackingState">, setting <see cref="TrackedPoseDriver.trackingStateInput"> to a new
        ///       <see cref="InputActionProperty"> (it is not nullable).  Unfortunately none worked for simulating tracking loss.  Therefore this
        ///       replacement test was repurposed to test tracked hands and ensure it initiates new interactions properly and that the interactable
        ///       IsGrabSelected property is updated properly.
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
            yield return rightHand.Show(InputTestUtilities.InFrontOfUser());
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

            // Otherwise, poke will conflict with grab.
            newCube.GetComponent<StatefulInteractable>().selectMode = InteractableSelectMode.Multiple;

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

        /// <summary>
        /// Checks that the Controllerless rig has the right components and that both pre-XRI3+ and XRI3+ input actions are properly referenced.
        /// The goal of this test is to prevent breaks when the Controllerless rig prefabs or its associated scripts are modified and prefabs are
        /// not updated properly.  This test also ensures that the Controllerless rig is shipped in the intended configuration.
        /// </summary>
        /// <remarks>
        /// This is an XRI3+ exclusive test.
        /// </remarks>
        [UnityTest]
        public IEnumerator ControllerlessRigSmokeTest()
        {
            // Check the Controllerless rig has the correct children.
            var controllerLessRig = InputTestUtilities.RigReference;
            List<GameObject> rigChildren = new List<GameObject>();
            controllerLessRig.GetChildGameObjects(rigChildren);

            Assert.AreEqual(4, rigChildren.Count);
            Assert.AreEqual(1, rigChildren.Where(c => c.name.Equals(MRTKInteractionManagerName)).Count());
            Assert.AreEqual(1, rigChildren.Where(c => c.name.Equals(CameraOffsetName)).Count());
            Assert.AreEqual(1, rigChildren.Where(c => c.name.Equals(MRTKSpeechName)).Count());
            Assert.AreEqual(1, rigChildren.Where(c => c.name.Equals(CanvasProxyInteractorName)).Count());

            // Check the rig's CameraOffset has the correct children
            var cameraOffset = rigChildren.Where(c => c.name.Equals(CameraOffsetName)).First();
            List<GameObject> cameraOffsetChildren = new List<GameObject>();
            cameraOffset.GetChildGameObjects(cameraOffsetChildren);

            Assert.AreEqual(5, cameraOffsetChildren.Count);
            Assert.AreEqual(1, cameraOffsetChildren.Where(c => c.name.Equals(MainCameraName)).Count());
            Assert.AreEqual(1, cameraOffsetChildren.Where(c => c.name.Equals(MRTKRightHandControllerName)).Count());
            Assert.AreEqual(1, cameraOffsetChildren.Where(c => c.name.Equals(MRTKLeftHandConrollerName)).Count());
            Assert.AreEqual(1, cameraOffsetChildren.Where(c => c.name.Equals(MRTKGazeControllerName)).Count());

            // Check the main camera has its TrackedPoseDriver component
            var mainCamera = cameraOffsetChildren.Where(c => c.name.Equals(MainCameraName));
            Assert.AreEqual(1, mainCamera.First().GetComponents<TrackedPoseDriver>().Length);

            GameObject leftHandGameObject = null;
            GameObject rightHandGameObject = null;
            GameObject gazeGameObject = null;
            GameObject gazeInteractorGameObject = null;

            // Check all controllers have an empty XRController Component
            var cameraOffsetControllers = cameraOffsetChildren.Where(c => c.name.Equals(MRTKRightHandControllerName) ||
                                                                          c.name.Equals(MRTKLeftHandConrollerName) ||
                                                                          c.name.Equals(MRTKGazeControllerName));
            foreach (GameObject controller in cameraOffsetControllers)
            {
                // Check the controller has the XRController component
#pragma warning disable CS0618 // ActionBasedController is obsolete
                var xrControllers = controller.GetComponents<ActionBasedController>();
#pragma warning restore CS0618 // ActionBasedController is obsolete
                Assert.AreEqual(1, xrControllers.Length);

                // Check the deprecated XRController does not have actions in it
                var xrControllerProperties = xrControllers[0].GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                                       .Where(p => p.PropertyType == typeof(InputActionProperty))
                                                                       .ToArray();
                foreach (PropertyInfo xrControllerPropertyInfo in xrControllerProperties)
                {
                    InputActionProperty inputActionProperty = (InputActionProperty)xrControllerPropertyInfo.GetValue(xrControllers[0]);
                    if (inputActionProperty.action != null)
                    {
                        Assert.IsNull(inputActionProperty.reference);
                        Assert.IsTrue(deprecatedXRControllerInputActions.Contains(inputActionProperty.action.name));
                    }
                    else
                    {
                        Assert.IsNull(inputActionProperty.reference);
                    }
                }

                // Check the deprecated XRController/Model and ModelPrefab properties are empty
                Assert.IsNull(xrControllers[0].model);
                Assert.IsNull(xrControllers[0].modelPrefab);

                // Hold a reference to the controllers for later easier testing
                if (controller.name.Equals(MRTKLeftHandConrollerName))
                {
                    leftHandGameObject = controller;
                }
                else if (controller.name.Equals(MRTKRightHandControllerName))
                {
                    rightHandGameObject = controller;
                }
                else if (controller.name.Equals(MRTKGazeControllerName))
                {
                    gazeGameObject = controller;
                    List<GameObject> gazeGameObjectChildren = new List<GameObject>();
                    gazeGameObject.GetChildGameObjects(gazeGameObjectChildren);

                    // Check the gaze GameObject has one and only one FuzzyGazeInteractor
                    var gazeInteractors = gazeGameObjectChildren.Where(c => c.name.Equals(GazeInteractorName)).ToArray();
                    Assert.AreEqual(1, gazeInteractors.Length);
                    gazeInteractorGameObject = gazeInteractors[0];
                }
                else
                {
                    Assert.Fail($"Controller '{controller.name}' is neither '{MRTKLeftHandConrollerName}', '{MRTKRightHandControllerName}', nor '{MRTKGazeControllerName}'");
                }
            }

            // Check that LeftHand and RightHand have their TrackedPoseDriver component and they are properly set
            var leftHandTrackedPoseDrivers = leftHandGameObject.GetComponents<TrackedPoseDriver>();
            Assert.AreEqual(1, leftHandTrackedPoseDrivers.Length);
            TrackedPoseDriver leftHandTrackedPoseDriver = leftHandTrackedPoseDrivers[0];
            Assert.AreEqual(TrackedPoseDriver.TrackingType.RotationAndPosition, leftHandTrackedPoseDriver.trackingType);
            Assert.AreEqual(TrackedPoseDriver.UpdateType.UpdateAndBeforeRender, leftHandTrackedPoseDriver.updateType);
            Assert.IsTrue(leftHandTrackedPoseDriver.positionInput.reference.name.Equals(MRTKLeftHandDevicePositionName));
            Assert.IsTrue(leftHandTrackedPoseDriver.rotationInput.reference.name.Equals(MRTKLeftHandDeviceRotationName));
            Assert.IsTrue(leftHandTrackedPoseDriver.trackingStateInput.reference.name.Equals(MRTKLeftHandTrackingStateName));

            var rightHandTrackedPoseDrivers = rightHandGameObject.GetComponents<TrackedPoseDriver>();
            Assert.AreEqual(1, rightHandTrackedPoseDrivers.Length);
            TrackedPoseDriver rightHandTrackedPoseDriver = rightHandTrackedPoseDrivers[0];
            Assert.AreEqual(TrackedPoseDriver.TrackingType.RotationAndPosition, rightHandTrackedPoseDriver.trackingType);
            Assert.AreEqual(TrackedPoseDriver.UpdateType.UpdateAndBeforeRender, rightHandTrackedPoseDriver.updateType);
            Assert.IsTrue(rightHandTrackedPoseDriver.positionInput.reference.name.Equals(MRTKRightHandDevicePositionName));
            Assert.IsTrue(rightHandTrackedPoseDriver.rotationInput.reference.name.Equals(MRTKRightHandDeviceRotationName));
            Assert.IsTrue(rightHandTrackedPoseDriver.trackingStateInput.reference.name.Equals(MRTKRightHandTrackingStateName));

            // Check that the GazeInteractor has its TrackedPoseDriverWithFallback component and that it is properly set
            var gazeInteractorTrackedPoseDriversWithFallback = gazeInteractorGameObject.GetComponents<TrackedPoseDriverWithFallback>();
            Assert.AreEqual(1, gazeInteractorTrackedPoseDriversWithFallback.Length);
            TrackedPoseDriverWithFallback gazeInteractorTrackedPoseDriverWithFallback = gazeInteractorTrackedPoseDriversWithFallback[0];
            Assert.AreEqual(TrackedPoseDriver.TrackingType.RotationAndPosition, gazeInteractorTrackedPoseDriverWithFallback.trackingType);
            Assert.AreEqual(TrackedPoseDriver.UpdateType.UpdateAndBeforeRender, gazeInteractorTrackedPoseDriverWithFallback.updateType);
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.positionInput.reference.name.Equals(MRTKGazePositionName));
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.rotationInput.reference.name.Equals(MRTKGazeRotationName));
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.trackingStateInput.reference.name.Equals(MRTKGazeTrackingStateName));
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.FallbackPositionAction.reference.name.Equals(MRTKGazeHeadGazePositionName));
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.FallbackRotationAction.reference.name.Equals(MRTKGazeHeadGazeRotationName));
            Assert.IsTrue(gazeInteractorTrackedPoseDriverWithFallback.FallbackTrackingStateAction.reference.name.Equals(MRTKGazeHeadGazeTrackingStateName));

            // Check LeftHand and RightHand have HandModel component and it is properly set
            var leftHandHandModels = leftHandGameObject.GetComponents<HandModel>();
            Assert.AreEqual(1, leftHandHandModels.Length);
            HandModel leftHandHandModel = leftHandHandModels[0];
            Assert.AreEqual(XRNode.LeftHand, leftHandHandModel.HandNode);
            Assert.IsTrue(leftHandHandModel.Model.name.Equals(OpenXRLeftHandCloneName));
            Assert.IsNull(leftHandHandModel.ModelParent);
            Assert.IsTrue(leftHandHandModel.ModelPrefab.name.Equals(OpenXRLeftHandName));

            var rigtHandHandModels = rightHandGameObject.GetComponents<HandModel>();
            Assert.AreEqual(1, rigtHandHandModels.Length);
            HandModel rightHandHandModel = rigtHandHandModels[0];
            Assert.AreEqual(XRNode.RightHand, rightHandHandModel.HandNode);
            Assert.IsTrue(rightHandHandModel.Model.name.Equals(OpenXRRightHandCloneName));
            Assert.IsNull(rightHandHandModel.ModelParent);
            Assert.IsTrue(rightHandHandModel.ModelPrefab.name.Equals(OpenXRRightHandName));

            // Now check the interactors

            // Check the Fuzzy Gaze Interactor has correct input configuration
            Assert.AreEqual(InteractorHandedness.None, gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>().handedness);
            Assert.IsNull(gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>().selectInput.inputActionReferenceValue);
            Assert.IsNull(gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>().selectInput.inputActionReferencePerformed);
            Assert.IsNull(gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>().activateInput.inputActionReferenceValue);
            Assert.IsNull(gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>().activateInput.inputActionReferencePerformed);

            // Check that the leftHand has one and only one of each type of interactors
            List<GameObject> leftHandChildren = new List<GameObject>();
            leftHandGameObject.GetChildGameObjects(leftHandChildren);

            var leftHandPokeInteractors = leftHandChildren.Where(c => c.name.Equals(IndexTipPokeInteractorName)).ToArray();
            var leftHandFarRays = leftHandChildren.Where(c => c.name.Equals(FarRayName)).ToArray();
            var leftHandGrabInteractors = leftHandChildren.Where(c => c.name.Equals(GrabInteractorName)).ToArray();
            var leftHandGazePinchInteractors = leftHandChildren.Where(c => c.name.Equals(GazePinchInteractorName)).ToArray();

            Assert.AreEqual(1, leftHandPokeInteractors.Length);
            Assert.AreEqual(1, leftHandFarRays.Length);
            Assert.AreEqual(1, leftHandGrabInteractors.Length);
            Assert.AreEqual(1, leftHandGazePinchInteractors.Length);

            // Check that the leftHand has one and only one of each type of interactors
            List<GameObject> rightHandChildren = new List<GameObject>();
            rightHandGameObject.GetChildGameObjects(rightHandChildren);

            var rightHandPokeInteractors = rightHandChildren.Where(c => c.name.Equals(IndexTipPokeInteractorName)).ToArray();
            var rightHandFarRays = rightHandChildren.Where(c => c.name.Equals(FarRayName)).ToArray();
            var rightHandGrabInteractors = rightHandChildren.Where(c => c.name.Equals(GrabInteractorName)).ToArray();
            var rightHandGazePinchInteractors = rightHandChildren.Where(c => c.name.Equals(GazePinchInteractorName)).ToArray();

            Assert.AreEqual(1, rightHandPokeInteractors.Length);
            Assert.AreEqual(1, rightHandFarRays.Length);
            Assert.AreEqual(1, rightHandGrabInteractors.Length);
            Assert.AreEqual(1, rightHandGazePinchInteractors.Length);

            // Check leftHand*Interactors
            // Check that leftHandPokeInteractor has correct input configuration
            GameObject leftHandPokeInteractor = leftHandPokeInteractors[0];
            Assert.AreEqual(InteractorHandedness.Left, leftHandPokeInteractor.GetComponent<PokeInteractor>().handedness);
            Assert.IsTrue(leftHandPokeInteractor.GetComponent<PokeInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKLeftHandSelectValueName));
            Assert.IsTrue(leftHandPokeInteractor.GetComponent<PokeInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandSelectName));
            Assert.IsTrue(leftHandPokeInteractor.GetComponent<PokeInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKLeftHandActivateName));
            Assert.IsTrue(leftHandPokeInteractor.GetComponent<PokeInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandActivateName));
            Assert.AreSame(leftHandPokeInteractor.GetComponent<PokeInteractor>().TrackedPoseDriver, leftHandTrackedPoseDriver);

            // Check that the leftHandMRTKRayInteractor has correct input configuration
            GameObject leftHandFarRay = leftHandFarRays[0];
            Assert.AreEqual(InteractorHandedness.Left, leftHandFarRay.GetComponent<MRTKRayInteractor>().handedness);
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKLeftHandSelectValueName));
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandSelectName));
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKLeftHandActivateName));
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandActivateName));
            Assert.AreSame(leftHandFarRay.GetComponent<MRTKRayInteractor>().TrackedPoseDriver, leftHandTrackedPoseDriver);
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().uiPressInput.inputActionReferenceValue.name.Equals(MRTKLeftHandUIPressName));
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().uiPressInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandUIPressName));
            Assert.IsTrue(leftHandFarRay.GetComponent<MRTKRayInteractor>().enableUIInteraction);

            // Check that leftHandGrabInteractor has correct input configuration
            GameObject leftHandGrabInteractor = leftHandGrabInteractors[0];
            Assert.AreEqual(InteractorHandedness.Left, leftHandGrabInteractor.GetComponent<GrabInteractor>().handedness);
            Assert.IsTrue(leftHandGrabInteractor.GetComponent<GrabInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKLeftHandSelectValueName));
            Assert.IsTrue(leftHandGrabInteractor.GetComponent<GrabInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandSelectName));
            Assert.IsTrue(leftHandGrabInteractor.GetComponent<GrabInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKLeftHandActivateName));
            Assert.IsTrue(leftHandGrabInteractor.GetComponent<GrabInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandActivateName));

            // Check that the leftHandMRTKRayInteractor has correct input configuration
            GameObject leftHandGazePinchInteractor = leftHandGazePinchInteractors[0];
            Assert.AreEqual(InteractorHandedness.Left, leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().handedness);
            Assert.IsTrue(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKLeftHandSelectValueName));
            Assert.IsTrue(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandSelectName));
            Assert.IsTrue(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKLeftHandActivateName));
            Assert.IsTrue(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKLeftHandActivateName));
            Assert.AreSame(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().TrackedPoseDriver, leftHandTrackedPoseDriver);
            Assert.AreSame(leftHandGazePinchInteractor.GetComponent<GazePinchInteractor>().DependentInteractor, gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>());

            // Check rightHand*Interactors
            // Check that leftHandPokeInteractor has correct input configuration
            GameObject leftandGazePincInteractor = rightHandPokeInteractors[0];
            Assert.AreEqual(InteractorHandedness.Right, leftandGazePincInteractor.GetComponent<PokeInteractor>().handedness);
            Assert.IsTrue(leftandGazePincInteractor.GetComponent<PokeInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKRightHandSelectValueName));
            Assert.IsTrue(leftandGazePincInteractor.GetComponent<PokeInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKRightHandSelectName));
            Assert.IsTrue(leftandGazePincInteractor.GetComponent<PokeInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKRightHandActivateName));
            Assert.IsTrue(leftandGazePincInteractor.GetComponent<PokeInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKRightHandActivateName));
            Assert.AreSame(leftandGazePincInteractor.GetComponent<PokeInteractor>().TrackedPoseDriver, rightHandTrackedPoseDriver);

            // Check that the leftHandMRTKRayInteractor has correct input configuration
            GameObject rigtHandFarRay = rightHandFarRays[0];
            Assert.AreEqual(InteractorHandedness.Right, rigtHandFarRay.GetComponent<MRTKRayInteractor>().handedness);
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKRightHandSelectValueName));
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKRightHandSelectName));
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKRightHandActivateName));
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKRightHandActivateName));
            Assert.AreSame(rigtHandFarRay.GetComponent<MRTKRayInteractor>().TrackedPoseDriver, rightHandTrackedPoseDriver);
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().uiPressInput.inputActionReferenceValue.name.Equals(MRTKRightHandUIPressName));
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().uiPressInput.inputActionReferencePerformed.name.Equals(MRTKRightHandUIPressName));
            Assert.IsTrue(rigtHandFarRay.GetComponent<MRTKRayInteractor>().enableUIInteraction);

            // Check that rightHandGrabInteractor has correct input configuration
            GameObject rightHandGrabInteractor = rightHandGrabInteractors[0];
            Assert.AreEqual(InteractorHandedness.Right, rightHandGrabInteractor.GetComponent<GrabInteractor>().handedness);
            Assert.IsTrue(rightHandGrabInteractor.GetComponent<GrabInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKRightHandSelectValueName));
            Assert.IsTrue(rightHandGrabInteractor.GetComponent<GrabInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKRightHandSelectName));
            Assert.IsTrue(rightHandGrabInteractor.GetComponent<GrabInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKRightHandActivateName));
            Assert.IsTrue(rightHandGrabInteractor.GetComponent<GrabInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKRightHandActivateName));

            // Check that the rightHandMRTKRayInteractor has correct input configuration
            GameObject rightHandGazePinchInteractor = rightHandGazePinchInteractors[0];
            Assert.AreEqual(InteractorHandedness.Right, rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().handedness);
            Assert.IsTrue(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().selectInput.inputActionReferenceValue.name.Equals(MRTKRightHandSelectValueName));
            Assert.IsTrue(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().selectInput.inputActionReferencePerformed.name.Equals(MRTKRightHandSelectName));
            Assert.IsTrue(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().activateInput.inputActionReferenceValue.name.Equals(MRTKRightHandActivateName));
            Assert.IsTrue(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().activateInput.inputActionReferencePerformed.name.Equals(MRTKRightHandActivateName));
            Assert.AreSame(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().TrackedPoseDriver, rightHandTrackedPoseDriver);
            Assert.AreSame(rightHandGazePinchInteractor.GetComponent<GazePinchInteractor>().DependentInteractor, gazeInteractorGameObject.GetComponent<FuzzyGazeInteractor>());

            yield return null;
        }
    }
}
#pragma warning restore CS1591
