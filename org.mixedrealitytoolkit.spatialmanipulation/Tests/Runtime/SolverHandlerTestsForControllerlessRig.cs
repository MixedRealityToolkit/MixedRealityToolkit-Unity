// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.Input;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.SpatialManipulation.Runtime.Tests
{
    /// <summary>
    /// Tests for SolverHandler for the XRI3+ controllerles MRTK rig.
    /// </summary>
    /// <remarks>
    /// These tests are equivalent to those in <see cref="SolverHandlerTests"/> but they test with the new MRTK Rig that was
    /// created for the XRI3 migration.  Eventually, this will replace the original <see cref="SolverHandlerTests"/> when
    /// the deprecated pre-XRI3 rig is removed in its entirety from MRTK3.
    /// Note:  This class contains only the tests that are specific to the XRI3+ rig.  Tests that are common to both rigs are in the
    ///        original <see cref="SolverHandlerTests"/>.  Once the XRI3 migration is completed by removing all the pre-XRI3
    ///        prefabs then those tests can be moved to this class.
    /// </remarks>
    public class SolverHandlerTestsForControllerlessRig : BaseRuntimeInputTests
    {
        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.SetupForControllerlessRig();
        }

        /// <summary>
        /// This checks if the SolverHandler can be configured to only track left hand only
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorLeftHandOnly"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorLeftHandOnly()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Left;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var initialHandPosition = InputTestUtilities.InFrontOfUser(0.5f);

            yield return rightHand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler did not start with target on right hand
            Assert.IsTrue(solverHandler.TransformTarget.position != solverHandler.RightInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            // Hide the right hand and make the left hand active at a new position
            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            var secondHandPosition = new Vector3(-0.05f, -0.05f, 1f);
            yield return leftHand.Show(secondHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target to the left hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.LeftInteractor.transform.position, $"Solver Handler did not start to track correct hand");

            // Repeat the test, but hide the left hand this time
            yield return leftHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            Vector3 finalPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, 0.05f, 0.5f));
            yield return rightHand.Show(finalPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler did not moves the target to the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position != solverHandler.RightInteractor.transform.position, $"Solver Handler switched to incorrect hand");
        }

        /// <summary>
        /// This checks if the SolverHandler moves with the active hand when tracking two interactors
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorMovesWithHand"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorMovesWithHand()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return new WaitForFixedUpdate();
            yield return null;

            TestHand rightHand = new TestHand(Handedness.Right);
            var initialHandPos = new Vector3(-0.05f, -0.05f, 1f);

            yield return rightHand.Show(initialHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler starts with target on right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            var finalHandPos = new Vector3(0.05f, 0.05f, 1f);
            yield return rightHand.MoveTo(finalHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that the SolverHandler keeps tracking the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler did not follow hand");
        }

        /// <summary>
        /// This checks if the SolverHandler starts tracking the preferred hand if both hands are view when tracking
        /// two interactors
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorPreferredHandedness"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorPreferredHandedness()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            yield return RuntimeTestUtilities.WaitForUpdates();
            // Set it to track interactors
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            // Set preferred tracked handedness to right
            solverHandler.PreferredTrackedHandedness = Handedness.Right;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var rightHandPos = new Vector3(-0.05f, -0.05f, 1f);
            var leftHandPos = new Vector3(0.05f, 0.05f, 1f);

            yield return rightHand.Show(rightHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return leftHand.Show(leftHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler tracks preferred hand if both are visible
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler not tracking preferred hand");
        }

        /// <summary>
        /// This checks if the SolverHandler can be configured to only track right hand only
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorRightHandOnly"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorRightHandOnly()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Right;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var initialHandPosition = InputTestUtilities.InFrontOfUser(0.5f);

            yield return leftHand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler did not start with target on left hand
            Assert.IsTrue(solverHandler.TransformTarget.position != solverHandler.LeftInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            // Hide the left hand and make the right hand active at a new position
            yield return leftHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            var secondHandPosition = new Vector3(-0.05f, -0.05f, 1f);
            yield return rightHand.Show(secondHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target to the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler did not start to track correct hand");

            // Repeat the test, but hide the right hand this time
            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            Vector3 finalPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, 0.05f, 0.5f));
            yield return leftHand.Show(finalPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler did not moves the target to the left hand
            Assert.IsTrue(solverHandler.TransformTarget.position != solverHandler.LeftInteractor.transform.position, $"Solver Handler switched to incorrect hand");
        }

        /// <summary>
        /// This checks if the SolverHandler correctly switches to the active hand when tracking
        /// two interactors
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorSwitchesToActiveHand"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorSwitchesToActiveHand()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Both;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var initialHandPosition = InputTestUtilities.InFrontOfUser(0.5f);

            yield return rightHand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler starts with target on right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            // Hide the right hand and make the left hand active at a new position
            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            var secondHandPosition = new Vector3(-0.05f, -0.05f, 1f);
            yield return leftHand.Show(secondHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target to the left hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.LeftInteractor.transform.position, $"Solver Handler did not switch to active hand");

            // Repeat the test, but hide the left hand this time
            yield return leftHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            Vector3 finalPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, 0.05f, 0.5f));
            yield return rightHand.Show(finalPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target back to the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler did not switch to final hand");
        }

        /// <summary>
        /// This checks if the SolverHandler correctly switches to the active hand when tracking
        /// two interactors, when the serialized `TrackedHandedness` value to set to Unity's
        /// Everything value, with is -1 or 0xFFFFFFFF. Everything can be set via Unity's
        /// inspector window.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorSwitchesToActiveHandWithEverythingValue"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorSwitchesToActiveHandWithEverythingValue()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = (Handedness)(-1);
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var initialHandPosition = InputTestUtilities.InFrontOfUser(0.5f);

            yield return rightHand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler starts with target on right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            // Hide the right hand and make the left hand active at a new position
            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            var secondHandPosition = new Vector3(-0.05f, -0.05f, 1f);
            yield return leftHand.Show(secondHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target to the left hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.LeftInteractor.transform.position, $"Solver Handler did not switch to active hand");

            // Repeat the test, but hide the left hand this time
            yield return leftHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();
            Vector3 finalPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, 0.05f, 0.5f));
            yield return rightHand.Show(finalPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if the SolverHandler moves the target back to the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler did not switch to final hand");
        }

        /// <summary>
        /// This checks if the SolverHandler keeps tracking the current active hand if another one comes
        /// in view when tracking two interactors
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ version of <see cref="SolverHandlerTests.SolverHandlerInteractorTracksInitialActiveHand"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator SolverHandlerInteractorTracksInitialActiveHand()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<TrackedPoseDriverLookup>();
            var leftInteractor = lookup.LeftHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return new WaitForFixedUpdate();
            yield return null;

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var rightHandPos = new Vector3(-0.05f, -0.05f, 1f);
            var leftHandPos = new Vector3(0.05f, 0.05f, 1f);

            yield return rightHand.Show(rightHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if SolverHandler starts with target on right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler started tracking incorrect hand");

            yield return leftHand.Show(leftHandPos);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that the SolverHandler keeps tracking the right hand
            Assert.IsTrue(solverHandler.TransformTarget.position == solverHandler.RightInteractor.transform.position, $"Solver Handler switched to wrong active hand");
        }
    }
}
#pragma warning restore CS1591
