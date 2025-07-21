// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using TMPro;
using UnityEngine;
#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
using MixedReality.Toolkit.Input;
#endif
#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization;
#endif

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// When applied to a Unity game object along with a <see cref="PressableButton"/>, this component
    /// will enable a "see-it say-it" label if speech input is enabled within the application.
    /// </summary>
    /// <remarks>
    /// A "see-it say-it" label is used for accessibility and voice input. The label displays the keyword
    /// that can be spoken to active or click the associated <see cref="PressableButton"/>.
    ///
    /// This class will only enable a "see-it say-it" label if the application has included the MRTK input
    /// package, and has an active <see cref="SpeechInteractor"/> object when this component's
    /// <see cref="Start"/> method is invoked.
    /// </remarks>
    [RequireComponent(typeof(PressableButton))]
    [AddComponentMenu("MRTK/UX/See It Say It Label")]
    public class SeeItSayItLabelEnabler : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="PressableButton"/> present on the same GameObject.
        /// </summary>
        private PressableButton pressableButton;

        /// <summary>
        /// The <see cref="TMP_Text"/> used to display the label present in child.
        /// </summary>
        private TMP_Text labelText;

        [SerializeField]
        [Tooltip("The GameObject for the see-it say-it label to be enabled.")]
        private GameObject seeItSayItLabel;

        /// <summary>
        /// The <see cref="GameObject"/> for the see-it say-it label to be enabled.
        /// </summary>
        public GameObject SeeItSayItLabel
        {
            get => seeItSayItLabel;
            set => seeItSayItLabel = value;
        }

#if UNITY_LOCALIZATION_PRESENT
        [SerializeField]
        [Tooltip("The LocalizedString that define the label pattern. Use a smart string with one argument that will be replaced by the button's speech recognition keyword (e.g: \"Say '{0}'\").")]
        private LocalizedString localizedPattern;
#else
        [SerializeField]
        [Tooltip("The pattern for the see-it say-it label using string.Format()")]
        private string pattern = "Say '{0}'";

        /// <summary>
        /// The pattern for the see-it say-it label using string.Format()
        /// </summary>
        public string Pattern
        {
            get => pattern;
            set
            {
                pattern = value;
                if (pressableButton != null)
                {
                    UpdateLabel(pressableButton.SpeechRecognitionKeyword);
                }
            }
        }
#endif

        [SerializeField]
        [Tooltip("The Transform that the label will be dynamically positioned off of. Empty by default. If positioning a Canvas label, this must be a RectTransform.")]
        private Transform positionControl;

        /// <summary>
        /// The <see cref="Transform"/> that the label will be dynamically positioned off of. Empty by default. If positioning a Canvas label, this must be a <see cref="UnityEngine.RectTransform"/>.
        /// </summary>
        public Transform PositionControl
        {
            get => positionControl;
            set => positionControl = value;
        }

        private const float CanvasOffset = -10f;
        private const float NonCanvasOffset = -0.004f;

        protected virtual void Awake()
        {
            pressableButton = GetComponent<PressableButton>();
        }

        /// <summary>
        /// A Unity event function that is called on the frame when a script is enabled just before any of the update methods are called the first time.
        /// </summary>
        protected virtual void Start()
        {
            // Check if voice commands are enabled for this button
            if (pressableButton != null && pressableButton.AllowSelectByVoice)
            {
                // Check if input and speech packages are present
#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
                // If we can't find any active speech interactors, then do not enable the labels.
                if (!ComponentCache<SpeechInteractor>.FindFirstActiveInstance())
                {
                    return;
                }

                SeeItSayItLabel.SetActive(true);
                labelText = SeeItSayItLabel.GetComponentInChildren<TMP_Text>(true);
                pressableButton.OnSpeechRecognitionKeywordChanged.AddListener(UpdateLabel);

                // Children must be disabled so that they are not initially visible
                foreach (Transform child in SeeItSayItLabel.transform)
                {
                    child.gameObject.SetActive(false);
                }

                // Set the label text to reflect the speech recognition keyword
                UpdateLabel(pressableButton.SpeechRecognitionKeyword);

                // If a Transform is specified, use it to reposition the object dynamically
                if (positionControl != null)
                {
                    // The control RectTransform used to position the label's height
                    RectTransform controlRectTransform = PositionControl.gameObject.GetComponent<RectTransform>();

                    // If PositionControl is a RectTransform, reposition label relative to Canvas button
                    if (controlRectTransform != null && SeeItSayItLabel.transform.childCount > 0)
                    {
                        // The parent RectTransform used to center the label
                        RectTransform canvasTransform = SeeItSayItLabel.GetComponent<RectTransform>();

                        // The child RectTransform used to set the final position of the label
                        RectTransform labelTransform = SeeItSayItLabel.transform.GetChild(0).gameObject.GetComponent<RectTransform>();

                        if (labelTransform != null && canvasTransform != null)
                        {
                            labelTransform.anchoredPosition3D = new Vector3(canvasTransform.rect.width / 2f, canvasTransform.rect.height / 2f + (controlRectTransform.rect.height / 2f * -1) + CanvasOffset, CanvasOffset);
                        }
                    }
                    else
                    {
                        SeeItSayItLabel.transform.localPosition = new Vector3(PositionControl.localPosition.x, (PositionControl.lossyScale.y / 2f * -1) + NonCanvasOffset, PositionControl.localPosition.z + NonCanvasOffset);
                    }
                }

#if UNITY_LOCALIZATION_PRESENT
                if (!localizedPattern.IsEmpty)
                {
                    localizedPattern.StringChanged += OnLocalizedPatternChanged;
                }
#endif
#endif
            }
        }

        protected virtual void OnDestroy()
        {
#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (pressableButton != null)
            {
                pressableButton.OnSpeechRecognitionKeywordChanged.RemoveListener(UpdateLabel);
#if UNITY_LOCALIZATION_PRESENT
                if (!localizedPattern.IsEmpty)
                {
                    localizedPattern.StringChanged -= OnLocalizedPatternChanged;
                }
#endif
            }
#endif
        }

        protected virtual void UpdateLabel(string keyword)
        {
#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT
            if (!string.IsNullOrWhiteSpace(keyword) && labelText != null)
            {
#if UNITY_LOCALIZATION_PRESENT
                if (!localizedPattern.IsEmpty)
                {
                    labelText.text = localizedPattern.GetLocalizedString(keyword);
                }
                else
                {
                    labelText.text = $"Say '{keyword}'";
                }
#else
                labelText.text = string.Format(pattern, keyword);
#endif
            }
#endif
        }

#if MRTK_INPUT_PRESENT && MRTK_SPEECH_PRESENT && UNITY_LOCALIZATION_PRESENT
        protected virtual void OnLocalizedPatternChanged(string value)
        {
            UpdateLabel(pressableButton.SpeechRecognitionKeyword);
        }
#endif
    }
}
