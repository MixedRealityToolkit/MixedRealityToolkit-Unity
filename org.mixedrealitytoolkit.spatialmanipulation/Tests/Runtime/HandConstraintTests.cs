// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.SpatialManipulation.Runtime.Tests
{
    /// <summary>
    /// Tests for <see cref="HandConstraint"/>.
    /// </summary>
    public class HandConstraintTests : BaseRuntimeInputTests
    {
        /// <summary>
        /// This checks if the HandConstraint events properly fire when the tracked handedness is set to a single hand.
        /// </summary>
        [UnityTest]
        public IEnumerator HandConstraintEventsOneHanded()
        {
            // Disable gaze interactions for this unit test
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            GameObject testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SolverHandler solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Right;
            solverHandler.TrackedTargetType = TrackedObjectType.HandJoint;
            solverHandler.TrackedHandJoint = TrackedHandJoint.Palm;

            // Set up GameObject with a HandConstraint
            HandConstraint handConstraint = testObject.AddComponent<HandConstraint>();

            bool onFirstHandDetected = false;
            bool onHandActivate = false;
            bool onHandDeactivate = false;
            bool onLastHandLost = false;

            handConstraint.OnFirstHandDetected.AddListener(() => onFirstHandDetected = true);
            handConstraint.OnHandActivate.AddListener(() => onHandActivate = true);
            handConstraint.OnHandDeactivate.AddListener(() => onHandDeactivate = true);
            handConstraint.OnLastHandLost.AddListener(() => onLastHandLost = true);

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);

            yield return rightHand.Show(InputTestUtilities.InFrontOfUser(0.5f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onFirstHandDetected, "OnFirstHandDetected wasn't successfully sent.");
            Assert.IsTrue(onHandActivate, "OnHandActivate wasn't successfully sent.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onHandDeactivate, "OnHandDeactivate wasn't successfully sent.");
            Assert.IsTrue(onLastHandLost, "OnLastHandLost wasn't successfully sent.");

            // Reset our state for the second iteration
            onFirstHandDetected = false;
            onHandActivate = false;
            onHandDeactivate = false;
            onLastHandLost = false;

            yield return rightHand.Show(new Vector3(-0.05f, -0.05f, 1f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onFirstHandDetected, "OnFirstHandDetected wasn't successfully sent.");
            Assert.IsTrue(onHandActivate, "OnHandActivate wasn't successfully sent.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onHandDeactivate, "OnHandDeactivate wasn't successfully sent.");
            Assert.IsTrue(onLastHandLost, "OnLastHandLost wasn't successfully sent.");
        }

        /// <summary>
        /// This checks if the HandConstraint events properly fire when the tracked handedness is set to both hands.
        /// </summary>
        [UnityTest]
        public IEnumerator HandConstraintEventsBothHanded()
        {
            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // Set up GameObject with a SolverHandler
            GameObject testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SolverHandler solverHandler = testObject.AddComponent<SolverHandler>();

            // Set it to track interactors
            solverHandler.TrackedHandedness = Handedness.Both;
            solverHandler.TrackedTargetType = TrackedObjectType.HandJoint;
            solverHandler.TrackedHandJoint = TrackedHandJoint.Palm;

            // Set up GameObject with a HandConstraint
            HandConstraint handConstraint = testObject.AddComponent<HandConstraint>();

            bool onFirstHandDetected = false;
            bool onHandActivate = false;
            bool onHandDeactivate = false;
            bool onLastHandLost = false;

            handConstraint.OnFirstHandDetected.AddListener(() => onFirstHandDetected = true);
            handConstraint.OnHandActivate.AddListener(() => onHandActivate = true);
            handConstraint.OnHandDeactivate.AddListener(() => onHandDeactivate = true);
            handConstraint.OnLastHandLost.AddListener(() => onLastHandLost = true);

            yield return RuntimeTestUtilities.WaitForUpdates();

            TestHand rightHand = new TestHand(Handedness.Right);
            TestHand leftHand = new TestHand(Handedness.Left);
            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(0.5f);

            yield return rightHand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onFirstHandDetected, "OnFirstHandDetected wasn't successfully sent.");
            Assert.IsTrue(onHandActivate, "OnHandActivate wasn't successfully sent.");

            onFirstHandDetected = false;
            onHandActivate = false;

            yield return leftHand.Show(new Vector3(-0.05f, -0.05f, 1f));
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent (or not)
            Assert.IsFalse(onFirstHandDetected, "OnFirstHandDetected should not have been sent.");
            Assert.IsFalse(onHandActivate, "OnHandActivate should not have been sent.");

            yield return rightHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsFalse(onFirstHandDetected, "OnFirstHandDetected should not have been sent.");
            Assert.IsTrue(onHandDeactivate, "OnHandDeactivate wasn't successfully sent.");
            Assert.IsTrue(onHandActivate, "OnHandActivate wasn't successfully sent.");
            Assert.IsFalse(onLastHandLost, "OnLastHandLost should not have been sent.");

            onHandDeactivate = false;
            onHandActivate = false;

            yield return leftHand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check if corresponding events were sent
            Assert.IsTrue(onHandDeactivate, "OnHandDeactivate wasn't successfully sent.");
            Assert.IsTrue(onLastHandLost, "OnLastHandLost wasn't successfully sent.");
        }
    }
}
#pragma warning restore CS1591
