// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Behavioral and regression tests for the <see cref="Slider"/> component.
    /// </summary>
    public class SliderTests : BaseRuntimeTests
    {
        [UnityTest]
        public IEnumerator Slider_ValueChange_DeliversEventsAndUpdatesValue()
        {
            GameObject go = new GameObject("SliderTest");
            Slider slider = go.AddComponent<Slider>();
            slider.MinValue = 0f;
            slider.MaxValue = 100f;
            slider.Value = 10f;

            float receivedOld = -1f;
            float receivedNew = -1f;
            int callCount = 0;

            slider.OnValueUpdated.AddListener((SliderEventData data) =>
            {
                receivedOld = data.OldValue;
                receivedNew = data.NewValue;
                callCount++;
            });

            slider.Value = 25f;
            Assert.AreEqual(10f, receivedOld, 1e-4f);
            Assert.AreEqual(25f, receivedNew, 1e-4f);
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(0.25f, slider.NormalizedValue, 1e-4f);

            // Test clamping
            slider.Value = 150f;
            Assert.AreEqual(100f, slider.Value, 1e-4f);
            Assert.AreEqual(1.0f, slider.NormalizedValue, 1e-4f);

            slider.Value = -50f;
            Assert.AreEqual(0f, slider.Value, 1e-4f);
            Assert.AreEqual(0.0f, slider.NormalizedValue, 1e-4f);

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Slider_MathAndDivisionSafety_DoesNotNaNOrThrow()
        {
            GameObject go = new GameObject("DegenerateSlider");
            Slider slider = go.AddComponent<Slider>();

            // Degenerate case 1: MinValue == MaxValue
            slider.MaxValue = 5f;
            slider.MinValue = 5f;
            slider.Value = 5f;

            Assert.IsFalse(float.IsNaN(slider.NormalizedValue), "NormalizedValue must not be NaN when MinValue == MaxValue.");
            Assert.AreEqual(0f, slider.NormalizedValue, 1e-4f);

            // Degenerate case 2: Track endpoints coincide (zero track length)
            GameObject startObj = new GameObject("Start");
            startObj.transform.SetParent(go.transform);
            startObj.transform.position = Vector3.zero;

            GameObject endObj = new GameObject("End");
            endObj.transform.SetParent(go.transform);
            endObj.transform.position = Vector3.zero;

            slider.SliderStart = startObj.transform;
            slider.SliderEnd = endObj.transform;

            Assert.AreEqual(0f, slider.SliderTrackDirection.sqrMagnitude, 1e-6f);

            // Trigger internal UpdateSliderValue directly
            Assert.DoesNotThrow(() =>
            {
                slider.UpdateSliderValue();
            }, "UpdateSliderValue must safely handle zero-length tracks without division-by-zero or throwing.");

            Assert.IsFalse(float.IsNaN(slider.Value), "Slider.Value must remain non-NaN after zero-length track evaluation.");

            // Degenerate case 3: sliderStepDivisions <= 0
            slider.SliderStepDivisions = 0;
            slider.UseSliderStepDivisions = true;
            float snapped = slider.SnapSliderToStepPositions(2.5f);
            Assert.IsFalse(float.IsNaN(snapped), "SnapSliderToStepPositions must not return NaN when step divisions are 0.");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Slider_StepPositions_SnapsRelativeToCustomRange()
        {
            GameObject go = new GameObject("StepSliderTest");
            Slider slider = go.AddComponent<Slider>();
            slider.MaxValue = 5f;
            slider.MinValue = 1f;
            slider.SliderStepDivisions = 2; // Steps: 1, 3, 5 (stepVal = 2)
            slider.UseSliderStepDivisions = true;

            // Value at 1.2f should snap to 1.0f
            slider.Value = 1.2f;
            Assert.AreEqual(1.0f, slider.Value, 1e-4f);

            // Value at 2.4f should snap to 3.0f
            slider.Value = 2.4f;
            Assert.AreEqual(3.0f, slider.Value, 1e-4f);

            // Value at 4.6f should snap to 5.0f
            slider.Value = 4.6f;
            Assert.AreEqual(5.0f, slider.Value, 1e-4f);

            Object.Destroy(go);
            yield return null;
        }
    }
}
#pragma warning restore CS1591
