// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Component for adjusting color and description text of visual origin markers in a frame (coordinate system axes).
    /// </summary>
    public class FrameVisual : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Visual marker to be colored")]
        private Renderer originMarker = null;

        [SerializeField]
        [Tooltip("Text object to be colored and set to the name of this GameObject")]
        private TextMesh originText = null;

        /// <summary>
        /// Text and axes color
        /// </summary>
        public Color color
        {
            get { return originText.color; }
            set
            {
                originText.color = value;
                for (int i = 0; i < originMarker.materials.Length; i++)
                {
                    originMarker.materials[i].color = value;
                }
            }
        }

        private void Start()
        {
            originText.text = name;
        }
    }
}
