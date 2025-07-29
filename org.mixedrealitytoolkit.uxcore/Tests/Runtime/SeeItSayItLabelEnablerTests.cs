// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using System.Text.RegularExpressions;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Input.Tests;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the See-It Say-It label generator
    /// </summary>
    public class SeeItSayItLabelEnablerTests : BaseRuntimeInputTests
    {
        [UnityTest]
        public IEnumerator TestEnableAndSetLabel()
        {
            GameObject testButton = SetUpButton(true, Control.None);
            Transform label = testButton.transform.GetChild(0);

#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (Application.isBatchMode)
            {
                Debug.Log("Did not run SeeItSayItLabelEnablerTests, since speech is not available in batch mode.");
            }
            else
            {
                SpeechInteractor interactor = FindObjectUtility.FindAnyObjectByType<SpeechInteractor>(true);
                interactor.gameObject.SetActive(true);
                yield return null;

                if (Application.isBatchMode)
                {
                    LogAssert.Expect(LogType.Exception, new Regex("Speech recognition is not supported on this machine"));
                }

                Transform sublabel = label.transform.GetChild(0);
                Assert.IsTrue(label.gameObject.activeSelf, "Label is enabled");
                Assert.IsTrue(!sublabel.gameObject.activeSelf, "Child objects are disabled");
                TMP_Text text = label.gameObject.GetComponentInChildren<TMP_Text>(true);
                Assert.AreEqual(text.text, "Say 'test'", "Label text was set to voice command keyword.");
                Object.Destroy(testButton);
            }
#else
            Debug.Log("Did not run SeeItSayItLabelEnablerTests, since speech is not present.");
#endif

            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestAutoUpdateLabel()
        {
            GameObject testButton = SetUpButton(true, Control.None);
            Transform label = testButton.transform.GetChild(0);

#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (Application.isBatchMode)
            {
                Debug.Log("Did not run SeeItSayItLabelEnablerTests, since speech is not available in batch mode.");
            }
            else
            {
                SpeechInteractor interactor = FindObjectUtility.FindAnyObjectByType<SpeechInteractor>(true);
                interactor.gameObject.SetActive(true);
                yield return null;

                if (Application.isBatchMode)
                {
                    LogAssert.Expect(LogType.Exception, new Regex("Speech recognition is not supported on this machine"));
                }

                Transform sublabel = label.transform.GetChild(0);
                TMP_Text text = label.gameObject.GetComponentInChildren<TMP_Text>(true);
                Assert.AreEqual(text.text, "Say 'test'", "Label text was set to voice command keyword.");

                testButton.GetComponent<PressableButton>().SpeechRecognitionKeyword = "hello world";

                Assert.AreEqual(text.text, "Say 'hello world'", "Label text was updated according to voice command keyword.");
            }
#else
            Assert.IsTrue(!label.gameObject.activeSelf, "Did not enable label because voice commands unavailable.");
#endif

            Object.Destroy(testButton);
            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
            // The speech recognition keyword change will trigger this exception at next update when speech recognition is not supported
            if (Application.isBatchMode)
            {
                LogAssert.Expect(LogType.Exception, new Regex("Speech recognition is not supported on this machine"));
            }
        }

        [UnityTest]
        public IEnumerator TestVoiceCommandsUnavailable()
        {
            GameObject testButton = SetUpButton(false, Control.None);
            yield return null;

            Transform label = testButton.transform.GetChild(0);
            Assert.IsTrue(!label.gameObject.activeSelf, "Did not enable label because voice commands unavailable.");

            Object.Destroy(testButton);
            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPositionCanvasLabel()
        {
            GameObject testButton = SetUpButton(true, Control.Canvas);
            Transform label = testButton.transform.GetChild(0);

#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (Application.isBatchMode)
            {
                Debug.Log("Did not run TestPositionCanvasLabel, since speech is not available in batch mode.");
            }
            else
            {
                SpeechInteractor interactor = FindObjectUtility.FindAnyObjectByType<SpeechInteractor>(true);
                interactor.gameObject.SetActive(true);
                yield return null;

                if (Application.isBatchMode)
                {
                    LogAssert.Expect(LogType.Exception, new Regex("Speech recognition is not supported on this machine"));
                }

                RectTransform sublabel = label.transform.GetChild(0) as RectTransform;
                Assert.AreEqual(sublabel.anchoredPosition3D, new Vector3(10, -30, -10), "Label is positioned correctly");
                Object.Destroy(testButton);
            }
#else
            Debug.Log("Did not run TestPositionCanvasLabel, since speech is not present.");
#endif

            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPositionNonCanvasLabel()
        {
            GameObject testButton = SetUpButton(true, Control.NonCanvas);
            Transform label = testButton.transform.GetChild(0);

#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (Application.isBatchMode)
            {
                Debug.Log("Did not run TestPositionNonCanvasLabel, since speech is not available in batch mode.");
            }
            else
            {
                SpeechInteractor interactor = FindObjectUtility.FindAnyObjectByType<SpeechInteractor>(true);
                interactor.gameObject.SetActive(true);
                yield return null;

                if (Application.isBatchMode)
                {
                    LogAssert.Expect(LogType.Exception, new Regex("Speech recognition is not supported on this machine"));
                }

                Assert.AreEqual(label.transform.localPosition, new Vector3(10f, -.504f, -.004f), "Label is positioned correctly");
                Object.Destroy(testButton);
            }
#else
            Debug.Log("Did not run TestPositionNonCanvasLabel, since speech is not present.");
#endif

            // Wait for a frame to give Unity a change to actually destroy the object
            yield return null;
        }

        private GameObject SetUpButton(bool allowSelectByVoice, Control control)
        {
            // Create a PressableButton to add SeeItSayItLabelCreator script to
            GameObject testButton = new GameObject("Button");
            PressableButton pressablebutton = testButton.AddComponent<PressableButton>();
            pressablebutton.AllowSelectByVoice = allowSelectByVoice;
            pressablebutton.SpeechRecognitionKeyword = "test";

            // Create a label GameObject to generate
            GameObject label = new GameObject("Label");
            label.transform.SetParent(testButton.transform, false);
            label.SetActive(false);
            GameObject subLabel = new GameObject("SubLabel");
            subLabel.transform.SetParent(label.transform, false);
            subLabel.AddComponent<TextMeshProUGUI>();
            subLabel.SetActive(true);

            // Set positions as necessary to test Canvas and NonCanvas positioning
            Transform positionControl = null;
            switch (control)
            {
                case Control.Canvas:
                    RectTransform buttonRectTransform = testButton.AddComponent<RectTransform>();
                    buttonRectTransform.offsetMin = new Vector2(-30, -30);
                    buttonRectTransform.offsetMax = new Vector2(30, 30);
                    RectTransform labelRectTransform = label.AddComponent<RectTransform>();
                    labelRectTransform.offsetMin = new Vector2(-10, -10);
                    labelRectTransform.offsetMax = new Vector2(10, 10);
                    positionControl = buttonRectTransform;
                    break;
                case Control.NonCanvas:
                    testButton.transform.localPosition = new Vector3(10f, 10f, 0f);
                    positionControl = testButton.transform;
                    break;
                default:
                    break;
            }

            // Set up SeeItSayItCreatorLabel script
            SeeItSayItLabelEnabler enabler = testButton.AddComponent<SeeItSayItLabelEnabler>();
            enabler.SeeItSayItLabel = label;
            enabler.PositionControl = positionControl;

            return testButton;
        }

        private enum Control
        {
            None,
            Canvas,
            NonCanvas
        }
    }
}
#pragma warning restore CS1591
