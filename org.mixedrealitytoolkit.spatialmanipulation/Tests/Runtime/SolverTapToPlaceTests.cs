// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MixedReality.Toolkit.Input;
using System.Collections;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.Core.Tests;

namespace MixedReality.Toolkit.SpatialManipulation.Runtime.Tests
{
    /// <summary>
    /// Tests for TapToPlace solver
    /// </summary>
    public class SolverTapToPlaceTests : BaseRuntimeInputTests
    {
        /// <summary>
        /// Override of the rig version to use for these tests. These tests validate that the old rig remain functional.
        /// The <see cref="SolverTapToPlaceTestsForControllerlessRig"/> will validate the new rig.
        /// </summary>
        protected override InputTestUtilities.RigVersion RigVersion => InputTestUtilities.RigVersion.Version1;

        /// <summary>
        /// Verify TapToPlace can move an object to the end of the right hand ray.
        /// </summary>
        [UnityTest]
#pragma warning disable CS0618 // Adding this pragma because all the encompassed tests depend on deprecated ControllerLookup
        public IEnumerator TapToPlaceFollowsRightHandRay()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();
            var solver = testObject.AddComponent<TapToPlace>();

            // Disable smoothing so moving happens instantly. This makes testing positions easier.
            solver.Smoothing = false;

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Both;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<ControllerLookup>();
            var leftInteractor = lookup.LeftHandController.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandController.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var rightHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.05f, 1f));
            var leftHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(-0.05f, -0.05f, 1f));

            testObject.transform.position = InputTestUtilities.InFrontOfUser(3.0f);

            yield return rightHand.Show(rightHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.AimAt(testObject.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return leftHand.Show(leftHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return leftHand.AimAt(testObject.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace starts without being in "placement" mode.
            Assert.IsFalse(solver.IsBeingPlaced, "TapToPlace should have starting without being in placement mode.");

            // Start placement and move hand.
            solver.StartPlacement();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace started.
            Assert.IsTrue(solver.IsBeingPlaced, "TapToPlace should have started.");
            var testObjectStartPosition = testObject.transform.position;

            // Aim hand and move object.
            yield return rightHand.AimAt(InputTestUtilities.InFrontOfUser(new Vector3(0.05f, 0.1f, 2.0f)));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Verify shape moved to placement
            var testObjectPlacementPosition = testObject.transform.position;
            Assert.AreNotEqual(testObjectStartPosition, testObjectPlacementPosition, $"Game object did not move");

            // Wait for solvers double click prevention timeout
            yield return new WaitForSeconds(0.5f + 0.1f);

            // Clicking with opposite hand should stop movement
            yield return leftHand.Click();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace stopped with pinch.
            Assert.IsFalse(solver.IsBeingPlaced, "TapToPlace should have stopped with left hand pinch.");

            // Aim hand
            yield return rightHand.AimAt(InputTestUtilities.InFrontOfUser(new Vector3(-0.05f, -0.1f, 2.0f)));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Verify shape did not moved 
            var testObjectFinalPosition = testObject.transform.position;
            Assert.AreEqual(testObjectPlacementPosition, testObjectFinalPosition, $"Game object should not have moved.");
        }

        /// <summary>
        /// Verify TapToPlace can move an object to the end of the left hand ray.
        /// </summary>
        [UnityTest]
        public IEnumerator TapToPlaceFollowsLeftHandRay()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();
            var solver = testObject.AddComponent<TapToPlace>();

            // Disable smoothing so moving happens instantly. This makes testing positions easier.
            solver.Smoothing = false;

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Both;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<ControllerLookup>();
            var leftInteractor = lookup.LeftHandController.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandController.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            var rightHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.05f, 1f));
            var leftHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(-0.05f, -0.05f, 1f));

            testObject.transform.position = InputTestUtilities.InFrontOfUser(3.0f);

            yield return leftHand.Show(leftHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return leftHand.AimAt(testObject.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            yield return rightHand.Show(rightHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return rightHand.AimAt(testObject.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace starts without being in "placement" mode.
            Assert.IsFalse(solver.IsBeingPlaced, "TapToPlace should have starting without being in placement mode.");

            // Start placement and move hand.
            solver.StartPlacement();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace started.
            Assert.IsTrue(solver.IsBeingPlaced, "TapToPlace should have started.");
            var testObjectStartPosition = testObject.transform.position;

            // Aim hand and move object.
            yield return leftHand.AimAt(InputTestUtilities.InFrontOfUser(new Vector3(-0.05f, 0.1f, 2.0f)));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Verify shape moved to placement
            var testObjectPlacementPosition = testObject.transform.position;
            Assert.AreNotEqual(testObjectStartPosition, testObjectPlacementPosition, $"Game object did not move");

            // Wait for solvers double click prevention timeout
            yield return new WaitForSeconds(0.5f + 0.1f);

            // Clicking with opposite hand should stop movement
            yield return rightHand.Click();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if TapToPlace stopped with pinch.
            Assert.IsFalse(solver.IsBeingPlaced, "TapToPlace should have stopped with right hand pinch.");

            // Aim hand
            yield return leftHand.AimAt(InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.1f, 2.0f)));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Verify shape did not moved 
            var testObjectFinalPosition = testObject.transform.position;
            Assert.AreEqual(testObjectPlacementPosition, testObjectFinalPosition, $"Game object should not have moved.");
        }

        /// <summary>
        /// Verify TapToPlace can start placement when method is called before its own Start.
        /// </summary>
        [UnityTest]
        public IEnumerator TapToPlaceIsBeingPlacedBeforeStart()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var solverHandler = testObject.AddComponent<SolverHandler>();
            var solver = testObject.AddComponent<TapToPlace>();

            // Disable smoothing so moving happens instantly. This makes testing positions easier.
            solver.Smoothing = false;

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Both;
            solverHandler.TrackedTargetType = TrackedObjectType.Interactor;
            var lookup = FindObjectUtility.FindAnyObjectByType<ControllerLookup>();
            var leftInteractor = lookup.LeftHandController.GetComponentInChildren<MRTKRayInteractor>();
            var rightInteractor = lookup.RightHandController.GetComponentInChildren<MRTKRayInteractor>();
            solverHandler.LeftInteractor = leftInteractor;
            solverHandler.RightInteractor = rightInteractor;

            int onPlacingStartedCount = 0;
            int onPlacingStoppedCount = 0;
            solver.OnPlacingStarted.AddListener(() => onPlacingStartedCount++);
            solver.OnPlacingStopped.AddListener(() => onPlacingStoppedCount++);

            // Call immediately, before the TapToPlace Start method has been called
            solver.StartPlacement();

            yield return RuntimeTestUtilities.WaitForUpdates(1);

            Assert.IsTrue(solver.IsBeingPlaced, "TapToPlace should have started.");
            Assert.AreEqual(1, onPlacingStartedCount, "TapToPlace should have invoked event OnPlacingStarted exactly 1 time.");
            Assert.AreEqual(0, onPlacingStoppedCount, "TapToPlace shouldn't have invoked event OnPlacingStopped.");

            // Call StartPlacement while it's already being placed
            solver.StartPlacement();

            yield return RuntimeTestUtilities.WaitForUpdates(1);

            Assert.IsTrue(solver.IsBeingPlaced, "TapToPlace should still being placed.");
            Assert.AreEqual(1, onPlacingStartedCount, "TapToPlace should have invoked event OnPlacingStarted exactly 1 time.");
            Assert.AreEqual(0, onPlacingStoppedCount, "TapToPlace shouldn't have invoked event OnPlacingStopped.");

            // Call StopPlacement too fast after StartPlacement
            solver.StopPlacement();

            yield return RuntimeTestUtilities.WaitForUpdates(1);

            Assert.IsTrue(solver.IsBeingPlaced, "TapToPlace should still being placed.");
            Assert.AreEqual(1, onPlacingStartedCount, "TapToPlace should have invoked event OnPlacingStarted exactly 1 time.");
            Assert.AreEqual(0, onPlacingStoppedCount, "TapToPlace shouldn't have invoked event OnPlacingStopped.");

            // Wait for solvers double click prevention timeout
            yield return new WaitForSeconds(0.5f + 0.1f);

            // Must call StopPlacement for following tests to not fail because rig keept reference to the TapToPlace
            solver.StopPlacement();

            yield return RuntimeTestUtilities.WaitForUpdates(1);

            Assert.IsFalse(solver.IsBeingPlaced, "TapToPlace should have stopped.");
            Assert.AreEqual(1, onPlacingStartedCount, "TapToPlace should have invoked event OnPlacingStarted exactly 1 time.");
            Assert.AreEqual(1, onPlacingStoppedCount, "TapToPlace should have invoked event OnPlacingStopped exactly 1 time.");
        }
#pragma warning restore CS0618 // Adding this pragma because all the encompassed tests depend on deprecated ControllerLookup
    }
}
#pragma warning restore CS1591
