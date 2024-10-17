// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    using Core.Tests;
    using Input;
    using System;
    using System.Collections;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

    public class SliderTests : BaseRuntimeInputTests
    {
        private const string TouchSliderPrefabPath =
            "Packages/org.mixedrealitytoolkit.uxcore/Tests/Runtime/Prefabs/TouchSliderTest.prefab";

        private const string GrabSliderPrefabPath =
            "Packages/org.mixedrealitytoolkit.uxcore/Tests/Runtime/Prefabs/GrabSliderTest.prefab";

        private const int HandMovementFrames = 10;

        private TestHand hand;

        [SetUp]
        public void SetUp()
        {
            hand = new TestHand(Handedness.Right);
        }

        [UnityTest]
        public IEnumerator TouchSlider_MoveRight_ValueIncreasesCorrectly([ValueSource(nameof(MoveRightTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();

            var testPrefab = InstantiateSlider(TouchSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0, 0, 1));

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(0, 0, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(-0.04f, 0f, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        [UnityTest]
        public IEnumerator GrabSlider_MoveRight_ValueIncreasesCorrectly([ValueSource(nameof(MoveRightTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();

            var testPrefab = InstantiateSlider(GrabSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0, 0, 1));

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(0, 0, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(-0.04f, 0f, 0.00f), HandMovementFrames);
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

            var testPrefab = InstantiateSlider(TouchSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0, 0, 1));

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(0, 0, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(testPrefab.transform.position + new Vector3(-0.04f, 0f, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        [UnityTest]
        public IEnumerator GrabSlider_MoveLeft_ValueIncreasesCorrectly([ValueSource(nameof(MoveLeftTestCases))] TestCase testCase)
        {
            InputTestUtilities.InitializeCameraToOriginAndForward();

            var testPrefab = InstantiateSlider(GrabSliderPrefabPath);
            testPrefab.transform.position = InputTestUtilities.InFrontOfUser(new Vector3(0, 0, 1));

            var slider = testPrefab.GetComponentInChildren<Slider>();
            slider.MinValue = testCase.MinValue;
            slider.MaxValue = testCase.MaxValue;
            slider.Value = ((testCase.MaxValue - testCase.MinValue) / 2) + testCase.MinValue;

            yield return ShowHand();
            yield return hand.MoveTo(testPrefab.transform.position - new Vector3(0, 0, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Grab);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.MoveTo(testPrefab.transform.position + new Vector3(-0.04f, 0f, 0.00f), HandMovementFrames);
            yield return RuntimeTestUtilities.WaitForUpdates();
            yield return hand.SetHandshape(HandshapeTypes.HandshapeId.Open);
            yield return RuntimeTestUtilities.WaitForUpdates();

            Assert.That(slider.Value, Is.EqualTo(testCase.Expected).Within(0.00001f));

            Object.Destroy(testPrefab);
        }

        public struct TestCase
        {
            public float MinValue;
            public float MaxValue;
            public float Expected;
        }

        private static IEnumerable MoveRightTestCases()
        {
            yield return new TestCase { MinValue = 0, MaxValue = 1, Expected = 0.7f };
            yield return new TestCase { MinValue = 0, MaxValue = 10, Expected = 7 };
            yield return new TestCase { MinValue = 0, MaxValue = 0.1f, Expected = 0.07f };
            yield return new TestCase { MinValue = -1, MaxValue = 1, Expected = 0.4f };
            yield return new TestCase { MinValue = -1, MaxValue = 0, Expected = -0.3f };
        }

        private static IEnumerable MoveLeftTestCases()
        {
            yield return new TestCase { MinValue = 0, MaxValue = 1, Expected = 0.3f };
            yield return new TestCase { MinValue = 0, MaxValue = 10, Expected = 3 };
            yield return new TestCase { MinValue = 0, MaxValue = 0.1f, Expected = 0.03f };
            yield return new TestCase { MinValue = -1, MaxValue = 1, Expected = -0.4f };
            yield return new TestCase { MinValue = -1, MaxValue = 0, Expected = -0.7f };
        }

        private IEnumerator ShowHand()
        {
            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(new Vector3(0.05f, -0.05f, 0.3f));
            yield return hand.Show(initialHandPosition);
            yield return RuntimeTestUtilities.WaitForUpdates();
        }

        private GameObject InstantiateSlider(string prefabPath)
        {

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }
}
