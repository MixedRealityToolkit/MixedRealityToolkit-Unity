// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_2020_1_OR_NEWER

#if WLT_ARSUBSYSTEMS_PRESENT

#if WLT_MICROSOFT_WMR_XR_4_3_PRESENT && UNITY_WSA
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
using UnityEngine.XR.WindowsMR;
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

        private bool wmrPersistence = false;

#if WLT_XR_PERSISTENCE
        private XRAnchorStore wmrAnchorStore = null;

        private async Task<XRAnchorStore> EnsureWMRAnchorStore()
        {
            if (wmrAnchorStore == null)
            {
                DebugLogExtra($"Getting new WMR XRAnchorStore.");
                wmrAnchorStore = await xrAnchorManager.TryGetAnchorStoreAsync();
            }
            wmrPersistence = wmrAnchorStore != null;
            return wmrAnchorStore;
        }
#endif // WLT_XR_PERSISTENCE

        protected async Task SaveAnchorsWMR(List<SpongyAnchorWithId> spongyAnchors)
        {
            Debug.Assert(wmrPersistence, "Trying to save WMR anchors when unsupported.");
#if WLT_XR_PERSISTENCE
            var anchorStore = await EnsureWMRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got WMR anchorStore for Save");

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
        protected async Task LoadAnchorsWMR(IPlugin plugin, AnchorId firstId, Transform parent, List<SpongyAnchorWithId> spongyAnchors)
        {
            Debug.Assert(wmrPersistence, "Trying to save WMR anchors when unsupported.");
#if WLT_XR_PERSISTENCE
            DebugLogExtra($"Load enter: AnchorSubsystem is {xrAnchorManager.running}");

            var anchorStore = await EnsureWMRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got WMR anchorStore for Load");

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
