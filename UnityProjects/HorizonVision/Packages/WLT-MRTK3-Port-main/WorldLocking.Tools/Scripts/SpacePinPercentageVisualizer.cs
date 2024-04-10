// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    public class SpacePinPercentageVisualizer : MonoBehaviour
    {
        public Text percentageNumberText;
        public Outline percentageNumberTextOutline;

        [Header("Percentage Colors")]
        public Color lowPercentageColor;
        public Color middlePercentageColor;
        public Color highPercentageColor;

        [Header("Percentage Outline Colors")]
        public Color lowPercentageOutlineColor;
        public Color middlePercentageOutlineColor;
        public Color highPercentageOutlineColor;

        private Camera mainCamera = null;

        public void UpdatePercentage(float percentage)
        {
            if (percentageNumberText != null)
            {
                percentageNumberText.text = (Mathf.RoundToInt(percentage)).ToString() + "%";

                float weight = percentage / 100.0f;
                UpdateColors(weight);
            }

        }

        public void SetVisibility(bool visibility)
        {
            if (percentageNumberText != null)
            {
                percentageNumberText.enabled = visibility;
            }
        }

        private void UpdateColors(float weight)
        {
            float lowWeight = Mathf.InverseLerp(0.5f, 0, weight);
            float middleWeight = 1 - (Mathf.Abs(0.5f - weight) * 2);
            float highWeight = Mathf.InverseLerp(0.5f, 1, weight);

            float sum = lowWeight + middleWeight + highWeight;

            lowWeight = lowWeight / sum;
            middleWeight = middleWeight / sum;
            highWeight = highWeight / sum;

            percentageNumberText.color = lowPercentageColor * lowWeight + middlePercentageColor * middleWeight + highPercentageColor * highWeight;
            percentageNumberTextOutline.effectColor = lowPercentageOutlineColor * lowWeight + middlePercentageOutlineColor * middleWeight + highPercentageOutlineColor * highWeight;
        }
        private void Awake()
        {
            mainCamera = Camera.main;
            SetVisibility(false);
        }

        private void Update()
        {
            if (mainCamera != null)
            {
                transform.LookAt(mainCamera.transform, Vector3.up);
            }
        }
    }
}
