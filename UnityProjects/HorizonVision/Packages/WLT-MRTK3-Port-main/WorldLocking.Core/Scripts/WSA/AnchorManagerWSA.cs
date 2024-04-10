// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
#define WLT_ENABLE_LEGACY_WSA
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
#if WLT_ENABLE_LEGACY_WSA
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
#endif // WLT_ENABLE_LEGACY_WSA

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Encapsulation of spongy world (raw input) state. Its primary duty is the creation and maintenance
    /// of the graph of (spongy) anchors built up over the space traversed by the camera.
    /// </summary>
    /// <remarks>
    /// Anchor and Edge creation algorithm:
    /// 
    /// Goal: a simple and robust algorithm that guarantees an even distribution of anchors, fully connected by
    /// edges between nearest neighbors with a minimum of redundant edges
    ///
    /// For simplicity, the algorithm should be stateless between time steps
    ///
    /// Rules
    /// * two parameters define spheres MIN and MAX around current position
    /// * whenever MIN does not contain any anchors, a new anchor is created
    /// * when a new anchor is created is is linked by edges to all anchors within MAX
    /// * the MAX radius is 20cm larger than MIN radius which would require 12 m/s beyond world record sprinting speed to cover in one frame
    /// * whenever MIN contains more than one anchor, the anchor closest to current position is connected to all others within MIN 
    /// </remarks>
    public class AnchorManagerWSA : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return true; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorWSA.TrackingStartDelayTime; } }

        public static AnchorManagerWSA TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            if (!UnityEngine.XR.XRSettings.enabled)
            {
                Debug.LogWarning($"Warning: Legacy WSA AnchorManager selected but legacy WSA not enabled. Check Player Settings/XR.");
            }

            AnchorManagerWSA anchorManagerWSA = new AnchorManagerWSA(plugin, headTracker);

            return anchorManagerWSA;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerWSA (IPlugin plugin, IHeadPoseTracker headTracker) : base(plugin, headTracker)
        {
        }

        protected override bool IsTracking()
        {
#if WLT_ENABLE_LEGACY_WSA
            return UnityEngine.XR.WSA.WorldManager.state == UnityEngine.XR.WSA.PositionalLocatorState.Active;
#else // WLT_ENABLE_LEGACY_WSA
            return true;
#endif // WLT_ENABLE_LEGACY_WSA
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            var newAnchorObject = new GameObject(id.FormatStr());
            newAnchorObject.transform.parent = parent;
            newAnchorObject.transform.SetGlobalPose(initialPose);
            return newAnchorObject.AddComponent<SpongyAnchorWSA>();
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

#if WLT_ENABLE_LEGACY_WSA
        /// <summary>
        /// Convert WorldAnchorStore.GetAsync call into a modern C# async call
        /// </summary>
        /// <returns>Result from WorldAnchorStore.GetAsync</returns>
        private static async Task<UnityEngine.XR.WSA.Persistence.WorldAnchorStore> getWorldAnchorStoreAsync()
        {
            var tcs = new TaskCompletionSource<UnityEngine.XR.WSA.Persistence.WorldAnchorStore>();
            UnityEngine.XR.WSA.Persistence.WorldAnchorStore.GetAsync(store =>
            {
                tcs.SetResult(store);
            });
            return await tcs.Task;
        }
#endif // WLT_ENABLE_LEGACY_WSA

        protected override async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
#if WLT_ENABLE_LEGACY_WSA

            var worldAnchorStore = await getWorldAnchorStoreAsync();
            foreach (var keyval in spongyAnchors)
            {
                var id = keyval.anchorId;
                var anchor = keyval.spongyAnchor;
                Debug.Assert(anchor.name == id.FormatStr());
                var wsaAnchor = anchor as SpongyAnchorWSA;
                if (wsaAnchor != null)
                {
                    wsaAnchor.Save(worldAnchorStore);
                }
            }
#else // WLT_ENABLE_LEGACY_WSA
            await Task.CompletedTask;
#endif // WLT_ENABLE_LEGACY_WSA
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
#if WLT_ENABLE_LEGACY_WSA
            var worldAnchorStore = await getWorldAnchorStoreAsync();

            var anchorIds = plugin.GetFrozenAnchorIds();

            AnchorId maxId = firstId;

            foreach (var id in anchorIds)
            {
                var spongyAnchor = CreateAnchor(id, parent, Pose.identity);
                var wsaAnchor = spongyAnchor as SpongyAnchorWSA;
                bool success = false;
                if (wsaAnchor != null)
                {
                    success = wsaAnchor.Load(worldAnchorStore);
                }
                if (success)
                {
                    spongyAnchors.Add(new SpongyAnchorWithId()
                    {
                        anchorId = id,
                        spongyAnchor = spongyAnchor
                    });
                    if (maxId <= id)
                    {
                        maxId = id + 1;
                    }
                }
                else
                {
                    DestroyAnchor(AnchorId.Invalid, spongyAnchor);
                    plugin.RemoveFrozenAnchor(id);
                }
            }

#else // WLT_ENABLE_LEGACY_WSA
            await Task.CompletedTask;
#endif // WLT_ENABLE_LEGACY_WSA
        }
    }
}
