// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.UX;
using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.UX.Tests.EditMode
{
    public class CanvasSliderVisualsTests
    {
        private GameObject sliderObject;
        private Slider slider;
        private CanvasSliderVisuals visuals;
        private RectTransform sliderStart;
        private RectTransform sliderEnd;
        private RectTransform trackArea;
        private RectTransform handle;
        private RectTransform fillVisual;

        [SetUp]
        public void Setup()
        {
            sliderObject = new GameObject("TestSlider", typeof(RectTransform), typeof(Slider));
            sliderObject.SetActive(false);
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
            trackArea.sizeDelta = new Vector2(100f, 10f);

            GameObject fillGo = new GameObject("FillVisual", typeof(RectTransform));
            fillGo.transform.SetParent(trackGo.transform, false);
            fillVisual = fillGo.GetComponent<RectTransform>();

            GameObject handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(trackGo.transform, false);
            handle = handleGo.GetComponent<RectTransform>();

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
        }

        [TearDown]
        public void TearDown()
        {
            if (sliderObject != null)
            {
                Object.DestroyImmediate(sliderObject);
            }
        }

        private static void InvokeOnValidate(Object target)
        {
            MethodInfo onValidate = target.GetType().GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (onValidate != null)
            {
                onValidate.Invoke(target, null);
            }
        }

        [Test]
        public void Slider_OnValidate_ClampsAndSnapsValue_AndFiresOnValueUpdated()
        {
            slider.MinValue = 0f;
            slider.MaxValue = 10f;
            slider.UseSliderStepDivisions = true;
            slider.SliderStepDivisions = 10;

            // Set raw serialized value to out-of-range un-snapped value
            SerializedObject so = new SerializedObject(slider);
            so.FindProperty("value").floatValue = 12.3f;
            so.ApplyModifiedProperties();

            bool eventFired = false;
            float reportedValue = -1f;
            slider.OnValueUpdated.AddListener(data =>
            {
                eventFired = true;
                reportedValue = data.NewValue;
            });

            InvokeOnValidate(slider);

            Assert.IsTrue(eventFired, "OnValueUpdated should have been fired by Slider.OnValidate()");
            Assert.AreEqual(10f, slider.Value, 0.0001f, "Slider.Value should be clamped to MaxValue");
            Assert.AreEqual(10f, reportedValue, 0.0001f, "SliderEventData should report the clamped value");
        }

        [Test]
        public void CanvasSliderVisuals_EditMode_RespondsToSliderValueUpdate()
        {
            // Because CanvasSliderVisuals is [ExecuteAlways], in EditMode it listens to OnValueUpdated.
            slider.Value = 0.75f;

            Assert.AreEqual(0.75f, handle.anchorMin.x, 0.001f, "Handle anchorMin.x should update to reflect 0.75");
            Assert.AreEqual(0.75f, handle.anchorMax.x, 0.001f, "Handle anchorMax.x should update to reflect 0.75");
            Assert.AreEqual(0.75f, fillVisual.anchorMax.x, 0.001f, "FillVisual anchorMax.x should update to reflect 0.75");
        }

        [Test]
        public void CanvasSliderVisuals_EditMode_SliderOnValidateSyncsVisuals()
        {
            // Simulate changing slider value in inspector during Edit Mode
            SerializedObject so = new SerializedObject(slider);
            so.FindProperty("value").floatValue = 0.4f;
            so.ApplyModifiedProperties();

            InvokeOnValidate(slider);

            Assert.AreEqual(0.4f, handle.anchorMin.x, 0.001f, "Handle anchorMin.x should update when Slider.OnValidate runs");
            Assert.AreEqual(0.4f, fillVisual.anchorMax.x, 0.001f, "FillVisual anchorMax.x should update when Slider.OnValidate runs");
        }

        [Test]
        public void CanvasSliderVisuals_SliderDirectionProperty_AppliesLayoutAndHandlePosition()
        {
            slider.Value = 0.5f;

            // Test RightToLeft
            visuals.SliderDirection = CanvasSliderVisuals.Direction.RightToLeft;
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);

            // Test BottomToTop
            visuals.SliderDirection = CanvasSliderVisuals.Direction.BottomToTop;
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            // Test TopToBottom
            visuals.SliderDirection = CanvasSliderVisuals.Direction.TopToBottom;
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            // Test LeftToRight
            visuals.SliderDirection = CanvasSliderVisuals.Direction.LeftToRight;
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);
        }

        [Test]
        public void CanvasSliderVisuals_OnValidate_AppliesLayoutAndHandlePosition()
        {
            slider.Value = 0.5f;
            SerializedObject so = new SerializedObject(visuals);
            SerializedProperty dirProp = so.FindProperty("sliderDirection");

            // Test RightToLeft
            dirProp.enumValueIndex = (int)CanvasSliderVisuals.Direction.RightToLeft;
            so.ApplyModifiedProperties();
            InvokeOnValidate(visuals);
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);

            // Test BottomToTop
            dirProp.enumValueIndex = (int)CanvasSliderVisuals.Direction.BottomToTop;
            so.ApplyModifiedProperties();
            InvokeOnValidate(visuals);
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            // Test TopToBottom
            dirProp.enumValueIndex = (int)CanvasSliderVisuals.Direction.TopToBottom;
            so.ApplyModifiedProperties();
            InvokeOnValidate(visuals);
            Assert.AreEqual(new Vector2(0.5f, 1f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.y, 0.001f);

            // Test LeftToRight
            dirProp.enumValueIndex = (int)CanvasSliderVisuals.Direction.LeftToRight;
            so.ApplyModifiedProperties();
            InvokeOnValidate(visuals);
            Assert.AreEqual(new Vector2(0f, 0.5f), sliderStart.anchorMin);
            Assert.AreEqual(new Vector2(1f, 0.5f), sliderEnd.anchorMin);
            Assert.AreEqual(0.5f, handle.anchorMin.x, 0.001f);
        }
    }
}
#pragma warning restore CS1591
