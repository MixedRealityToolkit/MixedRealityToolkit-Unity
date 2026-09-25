// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// PlayMode runtime tests verifying CanvasSliderVisuals and Slider runtime behavior.
    /// </summary>
    public class CanvasSliderVisualsRuntimeTests : BaseRuntimeInputTests
    {
        private GameObject canvasObject;
        private GameObject sliderObject;
        private Slider slider;
        private CanvasSliderVisuals visuals;
        private RectTransform sliderStart;
        private RectTransform sliderEnd;
        private RectTransform trackArea;
        private RectTransform handle;
        private RectTransform fillVisual;

        public override IEnumerator Setup()
        {
            yield return base.Setup();

            canvasObject = new GameObject("SliderParent", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.localScale = Vector3.one * 0.001f;
            (canvasObject.transform as RectTransform).sizeDelta = Vector2.one * 200;

            sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.SetActive(false);
            sliderObject.transform.SetParent(canvasObject.transform, false);
            sliderObject.transform.position = Vector3.zero;
            (sliderObject.transform as RectTransform).sizeDelta = new Vector2(200f, 10f);

            slider = sliderObject.GetComponent<Slider>();

            GameObject startGo = new GameObject("SliderStart", typeof(RectTransform));
            startGo.transform.SetParent(sliderObject.transform, false);
            sliderStart = startGo.GetComponent<RectTransform>();

            GameObject endGo = new GameObject("SliderEnd", typeof(RectTransform));
            endGo.transform.SetParent(sliderObject.transform, false);
            sliderEnd = endGo.GetComponent<RectTransform>();

            GameObject trackGo = new GameObject("TrackArea", typeof(RectTransform));
            trackGo.transform.SetParent(sliderObject.transform, false);
            trackArea = trackGo.GetComponent<RectTransform>();
            trackArea.sizeDelta = new Vector2(200f, 10f);

            GameObject fillGo = new GameObject("FillVisual", typeof(RectTransform));
            fillGo.transform.SetParent(trackGo.transform, false);
            fillVisual = fillGo.GetComponent<RectTransform>();

            GameObject handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(trackGo.transform, false);
            handle = handleGo.GetComponent<RectTransform>();

            BoxCollider trackCollider = trackGo.AddComponent<BoxCollider>();
            slider.TrackCollider = trackCollider;
            typeof(Slider).GetField("handleTransform", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(slider, handle.transform);
            slider.SliderStart = sliderStart;
            slider.SliderEnd = sliderEnd;
            slider.MinValue = 0f;
            slider.MaxValue = 1f;
            slider.Value = 0f;

            visuals = sliderObject.AddComponent<CanvasSliderVisuals>();
            visuals.TrackArea = trackArea;
            visuals.Handle = handle;
            visuals.FillVisual = fillVisual;
            sliderObject.SetActive(true);

            yield return null;
        }

        public override IEnumerator TearDown()
        {
            if (canvasObject != null)
            {
                Object.Destroy(canvasObject);
            }
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator CanvasSliderVisuals_PlayMode_RespondsToSliderValueUpdate()
        {
            slider.Value = 0.75f;
            yield return null;

            Assert.AreEqual(0.75f, handle.anchorMin.x, 0.001f, "Handle anchorMin.x should update to reflect 0.75 in PlayMode");
            Assert.AreEqual(0.75f, handle.anchorMax.x, 0.001f, "Handle anchorMax.x should update to reflect 0.75 in PlayMode");
            Assert.AreEqual(0.75f, fillVisual.anchorMax.x, 0.001f, "FillVisual anchorMax.x should update to reflect 0.75 in PlayMode");
        }

        [UnityTest]
        public IEnumerator CanvasSliderVisuals_PlayMode_StepDivisionsSnapValueAndVisuals()
        {
            slider.MinValue = 0f;
            slider.MaxValue = 10f;
            slider.Value = 3.4f;

            slider.SliderStepDivisions = 10;
            slider.UseSliderStepDivisions = true;
            yield return null;

            Assert.AreEqual(3.0f, slider.Value, 0.0001f, "Slider value should snap to step division when enabled");
            Assert.AreEqual(0.3f, handle.anchorMin.x, 0.001f, "Handle should reflect snapped normalized value");
        }

        [UnityTest]
        public IEnumerator CanvasSliderVisuals_PlayMode_SliderDirectionUpdatesLayout()
        {
            slider.Value = 0.5f;

            visuals.SliderDirection = CanvasSliderVisuals.Direction.RightToLeft;
            yield return null;
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);

            visuals.SliderDirection = CanvasSliderVisuals.Direction.BottomToTop;
            yield return null;
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            visuals.SliderDirection = CanvasSliderVisuals.Direction.TopToBottom;
            yield return null;
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            visuals.SliderDirection = CanvasSliderVisuals.Direction.LeftToRight;
            yield return null;
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);
        }
    }
}
#pragma warning restore CS1591
