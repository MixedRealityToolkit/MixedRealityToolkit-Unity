// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_2020_1_OR_NEWER

#if WLT_ARSUBSYSTEMS_PRESENT

#if WLT_MICROSOFT_OPENXR_PRESENT && UNITY_WSA
#define WLT_XR_PERSISTENCE
#endif // WLT_XR_PERSISTENCE

//#define WLT_EXTRA_LOGGING

#if WLT_DISABLE_LOGGING
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

#if WLT_XR_PERSISTENCE
//using Microsoft.MixedReality.ARSubsystems;
using Microsoft.MixedReality.OpenXR;
#endif // WLT_XR_PERSISTENCE

using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARSubsystems;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class AnchorManagerXR : AnchorManager
    {
        private bool openXRPersistence = false;

#if WLT_XR_PERSISTENCE
        private XRAnchorStore openXRAnchorStore = null;

        private async Task<XRAnchorStore> EnsureOpenXRAnchorStore()
        {
            if (openXRAnchorStore == null)
            {
                DebugLogExtra($"Getting new OpenXR XRAnchorStore.");
                //                openXRAnchorStore = await xrAnchorManager.LoadAnchorStoreAsync();
                openXRAnchorStore = await XRAnchorStore.LoadAsync(xrAnchorManager);
            }
            openXRPersistence = openXRAnchorStore != null;
            return openXRAnchorStore;
        }
#endif // WLT_XR_PERSISTENCE

        protected async Task SaveAnchorsOpenXR(List<SpongyAnchorWithId> spongyAnchors)
        {
            Debug.Assert(openXRPersistence, "Attempting to save via OpenXR when unsupported.");
#if WLT_XR_PERSISTENCE

            var anchorStore = await EnsureOpenXRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got OpenXR anchorStore for Save");

            foreach (var keyval in spongyAnchors)
            {
                var id = keyval.anchorId;
                var anchor = keyval.spongyAnchor;
                Debug.Assert(anchor.name == id.FormatStr());
                if (!anchor.IsSaved)
                {
                    var anchorXR = anchor as SpongyAnchorXR;
                    Debug.Assert(anchorXR != null);
                    anchorStore.UnpersistAnchor(anchor.name);
                    anchorStore.TryPersistAnchor(anchorXR.TrackableId, anchor.name);
                    anchor.IsSaved = true;
                }
            }

#else // WLT_XR_PERSISTENCE
            await Task.CompletedTask;
#endif // WLT_XR_PERSISTENCE
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
        protected async Task LoadAnchorsOpenXR(IPlugin plugin, AnchorId firstId, Transform parent, List<SpongyAnchorWithId> spongyAnchors)
        {
            Debug.Assert(openXRPersistence, "Attempting to save via OpenXR when unsupported.");
#if WLT_XR_PERSISTENCE
            DebugLogExtra($"Load enter: AnchorSubsystem is {xrAnchorManager.running}");

            var anchorStore = await EnsureOpenXRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got OpenXR anchorStore for Load");

            var anchorIds = plugin.GetFrozenAnchorIds();

            AnchorId maxId = firstId;

            foreach (var id in anchorIds)
            {
                var trackableId = anchorStore.LoadAnchor(id.FormatStr());
                if (trackableId != TrackableId.invalidId)
                {
                    DebugLogExtra($"LoadAnchor returns {trackableId}");
                    // We create the anchor here, but don't have a xrAnchor (XRAnchor) for it yet
                    var spongyAnchorXR = PrepAnchor(id, parent, trackableId, Pose.identity);
                    spongyAnchors.Add(new SpongyAnchorWithId()
                    {
                        anchorId = id,
                        spongyAnchor = spongyAnchorXR
                    });
                    spongyAnchorXR.IsSaved = true;
                }
                else
                {
                    plugin.RemoveFrozenAnchor(id);
                }
            }

            DebugLogExtra($"Load exit: AnchorSubsystem is {xrAnchorManager.running}");

#else // WLT_XR_PERSISTENCE
            await Task.CompletedTask;
#endif // WLT_XR_PERSISTENCE
        }
    }
}

#endif // WLT_ARSUBSYSTEMS_PRESENT

#endif // UNITY_2020_1_OR_NEWER

