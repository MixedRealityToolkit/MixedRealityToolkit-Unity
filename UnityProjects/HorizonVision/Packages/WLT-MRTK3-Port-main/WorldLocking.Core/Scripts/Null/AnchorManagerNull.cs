// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class AnchorManagerNull : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return false; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorNull.TrackingStartDelayTime; } }

        public static AnchorManagerNull TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            AnchorManagerNull anchorManager = new AnchorManagerNull(plugin, headTracker);

            return anchorManager;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerNull(IPlugin plugin, IHeadPoseTracker headTracker)
            : base(plugin, headTracker)
        {
            DebugLogSetup($"Null: Creating AnchorManagerNull");
        }

        protected override bool IsTracking()
        {
            return true;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            var newAnchorObject = new GameObject(id.FormatStr());
            newAnchorObject.transform.parent = parent;
            newAnchorObject.transform.SetGlobalPose(initialPose);
            return newAnchorObject.AddComponent<SpongyAnchorNull>();
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

        protected override async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
            await Task.CompletedTask;
        }


        /// <summary>
        /// Load the spongy anchors from persistent storage
        /// </summary>
        /// <remarks>
        /// The set of spongy anchors loaded by this routine is defined by the frozen anchors
        /// previously loaded into the plugin.
        /// 
        /// Likewise, when a spongy anchor fails to load, this routine will delete its frozen
        /// counterpart from the plugin.
        /// </remarks>
        protected override async Task LoadAnchors(IPlugin plugin, AnchorId firstId, Transform parent, List<SpongyAnchorWithId> spongyAnchors)
        {
            /// Placeholder for consistency. Persistence not implemented for Null, so
            /// to be consistent with this APIs contract, we must clear all frozen anchors from the plugin.
            plugin.ClearFrozenAnchors();

            await Task.CompletedTask;
        }
    }
}
