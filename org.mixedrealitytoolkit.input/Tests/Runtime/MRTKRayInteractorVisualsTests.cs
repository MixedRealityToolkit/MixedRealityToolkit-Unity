// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace MixedReality.Toolkit.Input.Tests
{
    /// <summary>
    /// Tests for verifying the behavior of visuals related to the MRTKRayInteractor
    /// </summary>
    public class MRTKRayInteractorVisualsTests : BaseRuntimeInputTests
    {
        /// <summary>
        /// Ensure that far ray interactor visuals are set active/inactive appropriately.
        /// </summary>
        [UnityTest]
        public IEnumerator ReticleAndLineVisualActiveTest()
        {
            // Because many of our visual scripts rely on OnBeforeRender, exit early if this test
            // is being run in batchmode (which does not rendering)
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
            var lineVisual = CachedLookup.RightHandController.GetComponentInChildren<MRTKLineVisual>();
            var reticleVisual = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayReticleVisual>();
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

        /// <summary>
        /// Ensure that far ray reticle visual is hidden when tracking is lost while hovering a WorldSpace UI Canvas with TrackedDeviceGraphicRaycaster.
        /// </summary>
        [UnityTest]
        public IEnumerator ReticleHiddenWhenTrackingLostOnUITest()
        {
            // Disable gaze interactions for this unit test
            InputTestUtilities.DisableGazeInteractor();

            var canvasGo = new GameObject("TestCanvas");
            canvasGo.transform.position = InputTestUtilities.InFrontOfUser(1f);
            canvasGo.transform.rotation = Quaternion.identity;

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();

            var rectTransform = canvasGo.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 500);

            var imageGo = new GameObject("Image", typeof(RectTransform));
            imageGo.transform.SetParent(canvasGo.transform, false);
            var image = imageGo.AddComponent<Image>();
            image.raycastTarget = true;
            var imageRect = imageGo.GetComponent<RectTransform>();
            imageRect.sizeDelta = new Vector2(400, 400);

            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            var rayInteractor = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayInteractor>();
            var reticleVisual = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayReticleVisual>();

            Vector3 hoverPosition = InputTestUtilities.InFrontOfUser(0.6f);
            yield return hand.MoveTo(hoverPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.RotateTo(Quaternion.identity);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that the UI is hovered and the reticle is active
            Assert.IsTrue(rayInteractor.TryGetCurrentUIRaycastResult(out _), "Ray should hit UI canvas");
            Assert.IsTrue(reticleVisual.Reticle.activeSelf, "Reticle should be active while hovering UI");

            // Hide the hand (simulate tracking loss)
            yield return hand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Reticle and UI hover should be inactive after tracking is lost
            Assert.IsFalse(reticleVisual.Reticle.activeSelf, "Reticle should NOT stay active when hand tracking is lost");
            Assert.IsFalse(rayInteractor.HasUIHover, "HasUIHover should be false when hand tracking is lost");
            Assert.IsFalse(rayInteractor.TryGetCurrentUIRaycastResult(out _), "UIRaycastResult should be invalid when hand tracking is lost");

            // Show hand again to ensure tracking restoration brings back the reticle
            yield return hand.Show();
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(reticleVisual.Reticle.activeSelf, "Reticle should reactivate when hand tracking is restored");
        }

        /// <summary>
        /// Ensure that far ray reticle visual is hidden when tracking is lost even if VisibilitySettings is AllValidSurfaces.
        /// </summary>
        [UnityTest]
        public IEnumerator ReticleHiddenWhenTrackingLostAllValidSurfacesTest()
        {
            // Disable gaze interactions for this unit test
            InputTestUtilities.DisableGazeInteractor();

            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one;

            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Vector3 hoverPosition = InputTestUtilities.InFrontOfUser(0.6f);
            testObject.transform.position = hoverPosition + Vector3.forward * 0.5f;

            var reticleVisual = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayReticleVisual>();
            var originalVisibility = reticleVisual.VisibilitySettings;
            reticleVisual.VisibilitySettings = MRTKRayReticleVisual.ReticleVisibilitySettings.AllValidSurfaces;

            try
            {
                yield return hand.MoveTo(hoverPosition);
                yield return RuntimeTestUtilities.WaitForUpdates();
                yield return hand.AimAt(testObject.transform.position);
                yield return RuntimeTestUtilities.WaitForUpdates();

                Assert.IsTrue(reticleVisual.Reticle.activeSelf, "Reticle should be active on valid surface");

                yield return hand.Hide();
                yield return RuntimeTestUtilities.WaitForUpdates();

                Assert.IsFalse(reticleVisual.Reticle.activeSelf, "Reticle should be hidden when hand tracking is lost even with AllValidSurfaces");
            }
            finally
            {
                reticleVisual.VisibilitySettings = originalVisibility;
            }
        }

        /// <summary>
        /// Ensure that far ray reticle visual is hidden when tracking is lost while hovering a 3D interactable,
        /// and that hasHover and isHoverActive both become false.
        /// </summary>
        [UnityTest]
        public IEnumerator ReticleHiddenWhenTrackingLostOnInteractableTest()
        {
            // Disable gaze interactions for this unit test
            InputTestUtilities.DisableGazeInteractor();

            var testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testObject.transform.localScale = Vector3.one * 0.2f;
            testObject.transform.position = InputTestUtilities.InFrontOfUser(1f);
            testObject.AddComponent<StatefulInteractable>();

            TestHand hand = new TestHand(Handedness.Right);
            yield return hand.Show(Vector3.zero);
            yield return RuntimeTestUtilities.WaitForUpdates();

            var rayInteractor = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayInteractor>();
            var reticleVisual = CachedLookup.RightHandController.GetComponentInChildren<MRTKRayReticleVisual>();

            Vector3 hoverPosition = InputTestUtilities.InFrontOfUser(0.6f);
            yield return hand.MoveTo(hoverPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.AimAt(testObject.transform.position);
            yield return RuntimeTestUtilities.WaitForUpdates();

            // Check that the 3D interactable is hovered, isHoverActive is true, and the reticle is active
            Assert.IsTrue(rayInteractor.isHoverActive, "isHoverActive should be true while pointing forward and tracked");
            Assert.IsTrue(rayInteractor.hasHover, "hasHover should be true while aiming at the 3D interactable");
            Assert.IsTrue(reticleVisual.Reticle.activeSelf, "Reticle should be active while hovering the 3D interactable");

            // Hide the hand (simulate tracking loss)
            yield return hand.Hide();
            yield return RuntimeTestUtilities.WaitForUpdates();

            // isHoverActive, hasHover, and reticle should all be false after tracking is lost
            Assert.IsFalse(rayInteractor.isHoverActive, "isHoverActive should be false when tracking is lost");
            Assert.IsFalse(rayInteractor.hasHover, "hasHover should be false when tracking is lost");
            Assert.IsFalse(reticleVisual.Reticle.activeSelf, "Reticle should NOT stay active when hand tracking is lost");

            // Show hand again to ensure tracking restoration brings back hover and reticle
            yield return hand.Show();
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.IsTrue(rayInteractor.isHoverActive, "isHoverActive should be true when tracking is restored");
            Assert.IsTrue(rayInteractor.hasHover, "hasHover should be true when tracking is restored");
            Assert.IsTrue(reticleVisual.Reticle.activeSelf, "Reticle should reactivate when hand tracking is restored");
        }
    }
}
#pragma warning restore CS1591