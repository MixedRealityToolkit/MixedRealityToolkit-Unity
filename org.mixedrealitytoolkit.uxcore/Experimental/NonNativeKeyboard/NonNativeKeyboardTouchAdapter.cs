// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clauseusing System;

using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX.Experimental
{
    /// <summary>
    /// Adds touch interaction to every button on the non-native keyboard
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class NonNativeKeyboardTouchAdapter : MonoBehaviour
    {
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            var defaultAudioComponent = GetComponent<AudioSource>();
            defaultAudioComponent.playOnAwake = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            var buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                // The search box has an incorrect collider and should not act as a button anyway
                if (button.gameObject.name != "search")
                {
                    button.gameObject.EnsureComponent<NonNativeKeyTouchAdapter>();
                }
            }
        }
    }
}
