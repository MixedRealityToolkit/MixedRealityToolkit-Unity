// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using MixedReality.Toolkit.Core.Tests;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX.Experimental;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using HandshapeId = MixedReality.Toolkit.Input.HandshapeTypes.HandshapeId;

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

		[SetUp]
		public void Init()
		{
			testKeyboard = InstantiatePrefab(NonNativeKeyboardPath).GetComponent<NonNativeKeyboard>();
            keyboardPreview = testKeyboard.Preview;
            testKeyboard.Open();
            initialKeyboardPosition = testKeyboard.transform.position;
		}

		[TearDown]
		public void Teardown()
		{
			Object.Destroy(testKeyboard);
			// Wait for a frame to give Unity a change to actually destroy the object
		}

		[UnityTest]
		public IEnumerator TestNonNativeKeyboardInstantiate()
		{
            Assert.IsNotNull(testKeyboard.gameObject.GetComponent<NonNativeKeyboardTouchAdapter>(),
                "NonNativeKeyboardTouchAdapter component exists on prefab");
			yield return null;
		}

        [UnityTest]
        public IEnumerator TestTouchKey()
        {
            Vector3 initialHandPosition = InputTestUtilities.InFrontOfUser(0.3f) + Vector3.up * 0.02f + Vector3.right * 0.01f;
            var handDelta = initialKeyboardPosition.z - initialHandPosition.z;
            var rightHand = new TestHand(Handedness.Right);
            yield return rightHand.Show(initialHandPosition);
            // The keyboard needs a bit of time to initialize
            yield return RuntimeTestUtilities.WaitForUpdates(40);
            yield return rightHand.Move(Vector3.forward * handDelta,5);
            Assert.AreEqual("j", keyboardPreview.Text, "j", "Pressing key did not change text.");
        }

		private GameObject InstantiatePrefab(string prefabPath)
		{
			Object pressableButtonPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));
			GameObject testGO = Object.Instantiate(pressableButtonPrefab) as GameObject;

			return testGO;
		}
	}
}
#pragma warning restore CS1591
