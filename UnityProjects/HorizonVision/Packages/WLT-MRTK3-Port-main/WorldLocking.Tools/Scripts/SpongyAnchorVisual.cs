// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Component for controlling location, visual appearance and ID text of a spongy anchor visualization.
    /// </summary>
    /// <remarks>
    /// Spongy anchors are visualized by a concentric pair of an outer ring and an inner disc
    /// The outer ring of fixed size indicates the state of the spatial anchor by its color:
    /// 
    /// green: support(area of inner circle indicating relevance)
    /// red: support with zero relevance
    /// yellow: not a support 
    /// gray: anchor not located(i.e.currently not part of spongy world)
    /// 
    /// The inner disc indicates the relevance of the spongy anchor (0..100%) by its area.
    /// </remarks>
    public class SpongyAnchorVisual : MonoBehaviour
    {
        /// <summary>
        /// The SpongyAnchor on a separate GameObject for syncing this GameObject's localPose")]
        /// </summary>
        private SpongyAnchor spongyAnchor = null;

        [SerializeField]
        [Tooltip("The child object that will have its color controlled")]
        private Renderer ringObject = null;

        [SerializeField]
        [Tooltip("The child object that will have its scale controlled")]
        private GameObject discObject = null;

        [SerializeField]
        [Tooltip("The child Text object that will have its color and text controlled")]
        private TextMesh textObject = null;

        private Color color;

        /// <summary>
        /// Set in AnchorGraphVisual.
        /// </summary>
        private float verticalDisplacement = 0;

        /// <summary>
        /// Create a visualizer for a spongy anchor.
        /// </summary>
        /// <param name="parent">Coordinate space to create the visualizer in</param>
        /// <param name="spongyAnchor">The spongyAnchor component assigned to some other object that this object is supposed to sync with</param>
        /// <returns></returns>
        public SpongyAnchorVisual Instantiate(FrameVisual parent, SpongyAnchor spongyAnchor, float verticalDisplacement)
        {
            var res = Instantiate(this, parent.transform);
            res.name = spongyAnchor.name;
            if (res.textObject != null)
            {
                res.textObject.text = res.name;
            }
            res.spongyAnchor = spongyAnchor;
            res.color = Color.gray;
            res.verticalDisplacement = verticalDisplacement;
            return res;
        }

        private void Update()
        {
            var color = this.color;

            // Unity's implementation of spatial anchor adjusts its global transform to track the
            // SpatialAnchor coordinate system. Here we want to keep the local transform of the visualization
            // towards its parent to track the SpatialAnchor, so we copy the global pose of the anchor to the
            // local pose of the visualization object.
            // This is because the visualizations tree is rooted at the SpongyFrame, which also contains the camera (and MRTK Playspace).
            // The SpongyFrame is adjusted by FrozeWorld every frame. This means that giving a transform M relative to the SpongyFrame,
            // as done here, will put the object _relative to the camera_ in the same place as setting M as the world transform
            // if SpongyFrame wasn't there, i.e. Unity World Space.
            Pose localPose = spongyAnchor.SpongyPose;
            localPose.position = new Vector3(localPose.position.x, localPose.position.y + verticalDisplacement, localPose.position.z);
            transform.SetLocalPose(localPose);

            if (!spongyAnchor.IsLocated)
                color = Color.gray;

            if (ringObject != null)
            {
                ringObject.material.color = color;
            }
            if (textObject != null)
            {
                textObject.color = color;
            }
        }

        /// <summary>
        /// Set the relevance, which sets the color.
        /// </summary>
        /// <param name="relevance">The new relevance</param>
        public void SetSupportRelevance(float relevance)
        {
            if (relevance > 0.0f)
            {
                color = Color.green;
                var rad = (float)Math.Sqrt(relevance);
                if (discObject != null)
                {
                    discObject.SetActive(true);
                    discObject.transform.localScale = new Vector3(rad, 1.0f, rad);
                }
            }
            else
            {
                color = Color.red;
                if (discObject != null)
                {
                    discObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Declare as not being a support.
        /// </summary>
        public void SetNoSupport()
        {
            color = Color.yellow;
            if (discObject != null)
            {
                discObject.SetActive(false);
            }
        }
    }
}
