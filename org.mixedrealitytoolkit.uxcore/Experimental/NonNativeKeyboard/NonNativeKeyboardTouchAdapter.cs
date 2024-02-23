// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clauseusing System;

using Microsoft.MixedReality.GraphicsTools;
using UnityEngine;
using UnityEngine.UI;

namespace MixedReality.Toolkit.UX.Experimental
{
    public class NonNativeKeyboardTouchAdapter : MonoBehaviour
    {
        private void Awake()
        {
            var defaultAudioComponent = GetComponent<AudioSource>();
            defaultAudioComponent.playOnAwake = false;
            defaultAudioComponent.spatialize = false;
        }

        private void Start()
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