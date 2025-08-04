// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    public class SliderTests : BaseRuntimeInputTests
    {
        // Slider/CanvasSlider.prefab
        private const string DefaultSliderPrefabGuid = "f64620d502cdf0f429efa27703913cb7";
        private static readonly string DefaultSliderPrefabPath = AssetDatabase.GUIDToAssetPath(DefaultSliderPrefabGuid);

        private const int HandMovementFrames = 10;

        private TestHand hand;

        public override IEnumerator Setup()
        {
            yield return base.Setup();
            hand = new TestHand(Handedness.Right);
        }

        [UnityTest]
        public IEnumerator TouchSlider_MoveRight_ValueIncreasesCorrectly([ValueSource(nameof(MoveRightTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();
            InputTestUtilities.SetHandAnchorPoint(Handedness.Left, Input.Simulation.ControllerAnchorPoint.IndexFinger);
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, Input.Simulation.ControllerAnchorPoint.IndexFinger);

            var testPrefab = InstantiateSlider(DefaultSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(Vector3.forward);

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(slider.HandleTransform.position, HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(slider.HandleTransform.position - new Vector3(-0.04f, 0, 0), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        [UnityTest]
        public IEnumerator GrabSlider_MoveRight_ValueIncreasesCorrectly([ValueSource(nameof(MoveRightTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();
            InputTestUtilities.SetHandAnchorPoint(Handedness.Left, Input.Simulation.ControllerAnchorPoint.Grab);
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, Input.Simulation.ControllerAnchorPoint.Grab);

            var testPrefab = InstantiateSlider(DefaultSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(Vector3.forward);

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.IsTouchable = false;
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(slider.HandleTransform.position, HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(slider.HandleTransform.position - new Vector3(-0.04f, 0, 0), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        [UnityTest]
        public IEnumerator TouchSlider_MoveLeft_ValueIncreasesCorrectly([ValueSource(nameof(MoveLeftTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();
            InputTestUtilities.SetHandAnchorPoint(Handedness.Left, Input.Simulation.ControllerAnchorPoint.IndexFinger);
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, Input.Simulation.ControllerAnchorPoint.IndexFinger);

            var testPrefab = InstantiateSlider(DefaultSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(Vector3.forward);

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(slider.HandleTransform.position, HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(slider.HandleTransform.position + new Vector3(-0.04f, 0, 0), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        [UnityTest]
        public IEnumerator GrabSlider_MoveLeft_ValueIncreasesCorrectly([ValueSource(nameof(MoveLeftTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();
            InputTestUtilities.SetHandAnchorPoint(Handedness.Left, Input.Simulation.ControllerAnchorPoint.Grab);
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, Input.Simulation.ControllerAnchorPoint.Grab);

            var testPrefab = InstantiateSlider(DefaultSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(Vector3.forward);

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.IsTouchable = false;
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(slider.HandleTransform.position, HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(slider.HandleTransform.position + new Vector3(-0.04f, 0, 0), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        public readonly struct TestCase
        {
            public float MinValue { get; }
            public float MaxValue { get; }
            public float Expected { get; }

            public TestCase(float minValue, float maxValue, float expected)
            {
                MinValue = minValue;
                MaxValue = maxValue;
                Expected = expected;
            }
        }

        private static IEnumerable MoveRightTestCases()
        {
            yield return new TestCase(minValue: 0, maxValue: 1, expected: 0.7f);
            yield return new TestCase(minValue: 0, maxValue: 10, expected: 7);
            yield return new TestCase(minValue: 0, maxValue: 0.1f, expected: 0.07f);
            yield return new TestCase(minValue: -1, maxValue: 1, expected: 0.4f);
            yield return new TestCase(minValue: -1, maxValue: 0, expected: -0.3f);
        }

        private static IEnumerable MoveLeftTestCases()
        {
            yield return new TestCase(minValue: 0, maxValue: 1, expected: 0.3f);
            yield return new TestCase(minValue: 0, maxValue: 10, expected: 3);
            yield return new TestCase(minValue: 0, maxValue: 0.1f, expected: 0.03f);
            yield return new TestCase(minValue: -1, maxValue: 1, expected: -0.4f);
            yield return new TestCase(minValue: -1, maxValue: 0, expected: -0.7f);
        }

        private IEnumerator ShowHand()
        {
            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.05f, 0.3f));
            yield return hand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        private GameObject InstantiateSlider(string prefabPath)
        {
            GameObject canvas = new GameObject("SliderParent", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvas.transform.localScale = Vector3.one * 0.001f;
            (canvas.transform as RectTransform).sizeDelta = Vector2.one * 200;
            GameObject slider = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            slider.transform.SetParent(canvas.transform, worldPositionStays: false);
            slider.transform.position = Vector3.zero;
            if (slider.transform is RectTransform rectTransform)
            {
                rectTransform.sizeDelta = new Vector2(200f, rectTransform.sizeDelta.y);
            }
            return canvas;
        }
    }
}
#pragma warning restore CS1591
