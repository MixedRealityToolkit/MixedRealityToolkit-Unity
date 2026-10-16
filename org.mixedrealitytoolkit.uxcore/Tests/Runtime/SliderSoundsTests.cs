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
    /// Behavioral and regression tests for the <see cref="SliderSounds"/> component.
    /// </summary>
    public class SliderSoundsTests : BaseRuntimeTests
    {
        [UnityTest]
        public IEnumerator SliderSounds_CustomRange_NormalizedPitchAccurate()
        {
            GameObject go = new GameObject("SliderWithSounds");
            Slider slider = go.AddComponent<Slider>();
            slider.MinValue = 0f;
            slider.MaxValue = 100f;
            slider.Value = 0f;

            AudioSource audioSource = go.AddComponent<AudioSource>();
            SliderSounds sounds = go.AddComponent<SliderSounds>();
            sounds.AudioSource = audioSource;
            sounds.StartPitch = 0.5f;
            sounds.EndPitch = 1.5f;
            sounds.PlaySoundsOnlyOnInteract = false;
            sounds.PlayTickSounds = true;
            sounds.TickEvery = 0.01f;
            sounds.MinSecondsBetweenTicks = 0f;

            // Create a 1-sample dummy AudioClip so AudioSource has something to play
            AudioClip clip = AudioClip.Create("TestClick", 1, 1, 44100, false);
            sounds.PassNotchSound = clip;

            // Trigger value update to 50% (value = 50 in a 0..100 range)
            slider.Value = 50f;

            // In baseline, audioSource.pitch was set to Mathf.Lerp(0.5f, 1.5f, 50f) = 50.5f (wildly distorted!)
            // With fix, it should map normalized value 0.5f -> pitch 1.0f.
            Assert.AreEqual(1.0f, audioSource.pitch, 0.05f, "AudioSource pitch must normalize slider range instead of passing raw value to Lerp.");

            Object.Destroy(clip);
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SliderSounds_StepDivisionSafety_DoesNotThrow()
        {
            GameObject go = new GameObject("SliderWithStepSounds");
            Slider slider = go.AddComponent<Slider>();
            slider.MinValue = 0f;
            slider.MaxValue = 10f;
            slider.SliderStepDivisions = 0;
            slider.UseSliderStepDivisions = false;

            SliderSounds sounds = go.AddComponent<SliderSounds>();
            sounds.AudioSource = go.AddComponent<AudioSource>();
            sounds.PlayTickSounds = true;
            sounds.TickEvery = 0f;

            // Verifies no division by zero when TickEvery or step divisions are zero
            Assert.DoesNotThrow(() =>
            {
                sounds.OnValueUpdated(new SliderEventData(0f, 5f));
            });

            Object.Destroy(go);
            yield return null;
        }
    }
}
#pragma warning restore CS1591
