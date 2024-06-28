// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests for verifying the behavior of visuals related to the MRTKRayInteractor for the XRI3+ controllerless MRTK rig.
    /// </summary>
    /// <remarks>
    /// These tests are equivalent to those in <see cref="MRTKRayInteractorVisualsTests"/> but they test with the new MRTK Rig that was
    /// created for the XRI3 migration.  Eventually, this will replace the original <see cref="MRTKRayInteractorVisualsTests"/> when
    /// the deprecated pre-XRI3 rig is removed in its entirety from MRTK3.
    /// Note:  This class contains only the tests that are specific to the XRI3+ rig.  Tests that are common to both rigs are in the
    ///        original <see cref="MRTKRayInteractorVisualsTests"/>.  Once the XRI3 migration is completed by removing all the pre-XRI3
    ///        prefabs then those tests can be moved to this class.
    /// </remarks>
    public class MRTKRayInteractorVisualsTestsForControllerlessRig : BaseRuntimeInputTests
    {
        /// <summary>
        /// Ensure that far ray interactor visuals are set active/inactive appropriately.
        /// </summary>
        /// <remarks>
        /// This test is the XRI3+ equivalent of <see cref="MRTKRayInteractorVisualsTests.ReticleAndLineVisualActiveTest"/>
        /// </remarks>
        [UnityTest]
        public IEnumerator ReticleAndLineVisualActiveTest()
        {
            // Because many of our visual scripts rely on OnBeforeRender, exit early if this test
            // is being run in batchmode (which does not render)
            if (Application.isBatchMode)
            {
                Debug.Log("Skipping test ReticleAndLineVisualActiveTest, as it does not work in batch mode settings");
                yield break;
            }

            // Disable gaze interactions for this unit test;
            InputTestUtilities.DisableGazeInteractor();

            // set up cube with manipulation handler
            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            Vector3 initialObjectPosition = InputTestUtilities.InFrontOfUser(1f);
            testObject.transform.position = initialObjectPosition;
            testObject.AddComponent<StatefulInteractable>();

            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that our components are enabled
            var lineVisual = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKLineVisual>();
            var reticleVisual = CachedTrackedPoseDriverLookup.RightHandTrackedPoseDriver.GetComponentInChildren<MRTKRayReticleVisual>();
            Assert.IsTrue(lineVisual.enabled);
            Assert.IsTrue(reticleVisual.enabled);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that the ray is active and the reticle is not
            Assert.IsTrue(lineVisual.GetComponentInChildren<LineRenderer>().enabled);
            Assert.IsFalse(reticleVisual.Reticle.activeSelf);

            Vector3 hoverPosition = InputTestUtilities.InFrontOfUser(0.6f);
            Quaternion hoverRotation = Quaternion.identity;

            yield return hand.MoveTo(hoverPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.RotateTo(hoverRotation);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that both are active
            Assert.IsTrue(lineVisual.GetComponentInChildren<LineRenderer>().enabled);
            Assert.IsTrue(reticleVisual.Reticle.activeSelf);

            // disable the components and check that all visuals are disabled
            lineVisual.enabled = false;
            reticleVisual.enabled = false;
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that both are disabled
            Assert.IsFalse(lineVisual.GetComponentInChildren<LineRenderer>().enabled);
            Assert.IsFalse(reticleVisual.Reticle.activeSelf);

            // Make sure they are still disabled after moving the hand back to the inital position
            yield return hand.MoveTo(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.RotateTo(Quaternion.identity);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that both are disabled
            Assert.IsFalse(lineVisual.GetComponentInChildren<LineRenderer>().enabled);
            Assert.IsFalse(reticleVisual.Reticle.activeSelf);

            // Make sure we are back in the correct visibility state after reactivating the visuals
            lineVisual.enabled = true;
            reticleVisual.enabled = true;
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(lineVisual.GetComponentInChildren<LineRenderer>().enabled);
            Assert.IsFalse(reticleVisual.Reticle.activeSelf);
        }
    }
}
#pragma warning restore CS1591
