// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for sample. While nice to have, this documentation is not required for samples.
#pragma warning disable CS1591

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using MixedReality.Toolkit.Subsystems;

namespace MixedReality.Toolkit.Examples
{
    /// <summary>
    /// Sample for binding speech recognition keywords to Unity Event functions.
    /// </summary>
    public class KeywordRecognitionHandler : MonoBehaviour
    {
        [Serializable]
        public struct KeywordEvent
        {
            [SerializeField]
            public string Keyword;

            [SerializeField]
            public UnityEvent Event;
        }

        [Tooltip("List of speech recognition keywords and events to trigger.")]
        [SerializeField]
        private List<KeywordEvent> keywords = new List<KeywordEvent>();

        public List<KeywordEvent> Keywords
        {
            get => keywords;
            set
            {
                keywords = value;
                UpdateKeywords();
            }
        }

        private IKeywordRecognitionSubsystem keywordRecognitionSubsystem;

        private void Start()
        {
            keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<IKeywordRecognitionSubsystem>();
            if (keywordRecognitionSubsystem == null)
            {
                Debug.LogWarning("No keyword subsystem detected.");
            }

            UpdateKeywords();
        }

        private void UpdateKeywords()
        {
            foreach (var data in keywords)
            {
                keywordRecognitionSubsystem.CreateOrGetEventForKeyword(data.Keyword).AddListener(() =>
                {
                    data.Event?.Invoke();
                });
            }
        }
    }
}

#pragma warning restore CS1591
