// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Component for controlling color and ID text of anchor visualizations.
    /// </summary>
    /// <remarks>
    /// The text will be set to the AnchorID.
    /// The color will be kept in sync with the parent frame.
    /// </remarks>
    public class FrozenAnchorVisual : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The anchor visualization icon that will have its color adjusted")]
        private Renderer iconObject = null;

        [SerializeField]
        [Tooltip("The anchor visualization ID text that will have its text&color adjusted")]
        private TextMesh textObject = null;

        private FrameVisual parent;

        /// <summary>
        /// Create an instance of a frame visualizer
        /// </summary>
        /// <param name="name">The name of the anchor to be displayed</param>
        /// <param name="parent">The frame visualization object that defines the color of this anchor</param>
        /// <returns></returns>
        public FrozenAnchorVisual Instantiate(string name, FrameVisual parent)
        {
            var res = Instantiate(this, parent.transform);
            res.parent = parent;
            res.name = name;
            if (res.textObject != null)
            {
                res.textObject.text = name;
            }
            return res;
        }

        private void Update()
        {
            var color = parent.color;

            if (iconObject != null)
            {
                iconObject.material.color = color;
            }
            if (textObject != null)
            {
                textObject.color = color;
            }
        }
    }
}
