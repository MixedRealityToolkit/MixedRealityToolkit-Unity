// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WLT_ARCORE_SDK_INCLUDED
using GoogleARCore;
#endif // WLT_ARCORE_SDK_INCLUDED

// mafinc - installed Multiplayer HLAPI and Legacy Helpers for ARCore Examples, shouldn't need them when
// ARCore Examples is pruned out. Actually, Legacy Helpers is still needed (for InstantPreview), but MHLAPI shouldn't be.

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class AnchorManagerARCore : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return false; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorARCore.TrackingStartDelayTime; } }

        public static AnchorManagerARCore TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {

            AnchorManagerARCore anchorManagerARCore = null;

#if WLT_ARCORE_SDK_INCLUDED
            anchorManagerARCore = new AnchorManagerARCore(plugin, headTracker);
#endif // WLT_ARCORE_SDK_INCLUDED

            return anchorManagerARCore;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerARCore(IPlugin plugin, IHeadPoseTracker headTracker) 
            : base(plugin, headTracker)
        {
        }

        protected override bool IsTracking()
        {
#if WLT_ARCORE_SDK_INCLUDED
            return Session.Status == SessionStatus.Tracking;
#else // WLT_ARCORE_SDK_INCLUDED
            return false;
#endif // WLT_ARCORE_SDK_INCLUDED
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            var newAnchorObject = new GameObject(id.FormatStr());
            newAnchorObject.transform.parent = parent;
            newAnchorObject.transform.SetGlobalPose(initialPose);
            return newAnchorObject.AddComponent<SpongyAnchorARCore>();
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            if (spongyAnchor != null)
            {
                GameObject.Destroy(spongyAnchor.gameObject);
            }
            RemoveSpongyAnchorById(id);

            return null;
        }
  }
}