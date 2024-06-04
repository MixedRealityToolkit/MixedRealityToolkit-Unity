// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests for verifying the behavior of FuzzyGazeInteractor for the XRI3+ controllerless MRTK rig.
    /// </summary>
    /// <remarks>
    /// These tests are equivalent to those in <see cref="FuzzyGazeInteractorTests"/> but they test with the new MRTK Rig that was
    /// created for the XRI3 migration.  Eventually, this will replace the original <see cref="FuzzyGazeInteractorTests"/> when
    /// the deprecated pre-XRI3 rig is removed in its entirety from MRTK3.
    /// Note:  This class contains only the tests that are specific to the XRI3+ rig.  Tests that are common to both rigs are in the
    ///        original <see cref="FuzzyGazeInteractorTests"/>.  Once the XRI3 migration is completed by removing all the pre-XRI3
    ///        prefabs then those tests can be moved to this class.
    /// </remarks>
    public class FuzzyGazeInteractorTestsForControllerlessRig : BaseRuntimeInputTests
    {
        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.SetupForControllerlessRig();
        }

        /// <summary>
        /// Test that eye-gaze works as expected.
        /// </summary>
        /// <remarks>
        /// This test was meant to be the XRI3+ equivalent of <see cref="MRTKRayInteractorVisualsTests.ReticleAndLineVisualActiveTest"/>, however, since our
        /// <see cref="InputTestUtilities"/> cannot simulate the loss of tracking for XRI3+ <see cref="TrackedPoseDriver"/> because it still uses controllers
        /// (as of the moment of this writing) then this test was repurposed to test that eye-gazing works for XRI3+ as expected without simulating
        /// eye-gaze tracking loss.
        /// </remarks>
        [UnityTest]
        public IEnumerator EyeGazeWorksAsExpectedTest()
        {
            // Confirm a FuzzyGazeInteractor is active in the scene
            FuzzyGazeInteractor fuzzyGazeInteractor = FindObjectUtility.FindFirstObjectByType<FuzzyGazeInteractor>();
            Assert.IsNotNull(fuzzyGazeInteractor, "There is no active FuzzyGazeInteractor found in the scene.");

            // Instantiate two foregound cubes and one background cube for testing
            GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube1.GetComponent<MeshRenderer>().material.color = Color.red;
            cube1.AddComponent<StatefulInteractable>();
            cube1.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0.07f, 0.2f, 1));
            cube1.transform.localScale = Vector3.one * 0.1f;

            GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube2.GetComponent<MeshRenderer>().material.color = Color.blue;
            cube2.AddComponent<StatefulInteractable>();
            cube2.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(-0.05f, 0.2f, 1));
            cube2.transform.localScale = Vector3.one * 0.1f;

            GameObject backgroundCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backgroundCube.AddComponent<StatefulInteractable>();
            backgroundCube.transform.position = InputTestUtilities.InFrontOfUser(1.6f);
            backgroundCube.transform.localScale = Vector3.one;

            yield return RuntimeTestUtilities.WaitForUpdates();

            // No foreground cube should be hovered at their starting positions
            Assert.IsFalse(cube1.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Cube 1'sStatefulInteractable was already hovered.");
            Assert.IsFalse(cube2.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Cube 2's StatefulInteractable was already hovered.");
            Assert.IsTrue(backgroundCube.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Background's StatefulInteractable was not hovered by FuzzyGazeInteractor.");

            // Point camera (HMD) at cube 1
            yield return InputTestUtilities.RotateCameraToTarget(cube1.transform.position);

            // Point eyes at cube 2
            yield return InputTestUtilities.RotateEyesToTarget(cube2.transform.position);

            // The eyes gaze should have focused cube 2
            Assert.IsFalse(cube1.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Cube 1's StatefulInteractable was hovered, perhaps by head gaze. Expected eye gaze to hover different object.");
            Assert.IsTrue(cube2.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Cube 2's StatefulInteractable should have been hovered by eye gaze.");
            Assert.IsFalse(backgroundCube.GetComponent<StatefulInteractable>().IsGazeHovered,
                           "Background's StatefulInteractable was unexpectedly hovered.");
        }
    }
}
#pragma warning restore CS1591
