// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX.Experimental;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the Canvas Touchable Non-Native keyboard prefab.
    /// </summary>
    public class TouchableNonNativeKeyboardTests : BaseRuntimeInputTests
    {
        // Keyboard/TouchableNonNativeKeyboard.prefab
        private const string NonNativeKeyboardGuid = "a22b6e5a0c5436743949403a3940f04b";
        private static readonly string NonNativeKeyboardPath = AssetDatabase.GUIDToAssetPath(NonNativeKeyboardGuid);

        private NonNativeKeyboard testKeyboard = null;
        private KeyboardPreview keyboardPreview = null;
        private Vector3 initialKeyboardPosition;

        public override IEnumerator Setup()
        {
            yield return base.Setup();
            testKeyboard = InstantiatePrefab(NonNativeKeyboardPath).GetComponent<NonNativeKeyboard>();
            keyboardPreview = testKeyboard.Preview;
            testKeyboard.Open();
            initialKeyboardPosition = testKeyboard.transform.position;
            // Give the keyboard a few seconds for its ReClickDelayTime to pass
            yield return new WaitForSeconds(2);
        }

        public override IEnumerator TearDown()
        {
            Object.Destroy(testKeyboard);
            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
            Assert.IsTrue(testKeyboard == null);

            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator TestNonNativeKeyboardInstantiate()
        {
            Assert.IsNotNull(testKeyboard.gameObject.GetComponent<NonNativeKeyboardTouchAdapter>(),
                $"{nameof(NonNativeKeyboardTouchAdapter)} component doesn't exist on prefab.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestTouchKey()
        {
            InputTestUtilities.SetHandAnchorPoint(Handedness.Right, Input.Simulation.ControllerAnchorPoint.IndexFinger);

            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(0.3f) + Vector3.up * 0.02f + Vector3.right * 0.01f;
            var handDelta = initialKeyboardPosition.z - initialHandPosition.z;
            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(initialHandPosition);
            // The keyboard needs a bit of time to initialize
            yield return RuntimeTestUtilities.WaitForUpdates(40);
            yield return rightHand.Move(Vector3.forward * handDelta, 5);
            Assert.AreEqual("j", keyboardPreview.Text, "Pressing key did not change text.");
        }

        private GameObject InstantiatePrefab(string prefabPath)
        {
            Object pressableButtonPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
            return Object.Instantiate(pressableButtonPrefab) as GameObject;
        }
    }
}
#pragma warning restore CS1591
