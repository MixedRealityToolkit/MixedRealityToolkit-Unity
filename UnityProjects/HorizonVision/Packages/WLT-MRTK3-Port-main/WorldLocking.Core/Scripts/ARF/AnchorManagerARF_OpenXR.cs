// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_EXTRA_LOGGING

#if WLT_DISABLE_LOGGING
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

#if UNITY_2020_1_OR_NEWER

#if UNITY_2020_4_OR_NEWER
#define WLT_ADD_ANCHOR_COMPONENT
#endif // UNITY_2020_4_OR_NEWER

#if WLT_ARFOUNDATION_PRESENT

#if WLT_MICROSOFT_OPENXR_PRESENT && UNITY_WSA
#define WLT_XR_PERSISTENCE
#endif // WLT_XR_PERSISTENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

using UnityEngine.XR.ARFoundation;

#if WLT_XR_PERSISTENCE
using Microsoft.MixedReality.OpenXR;
using UnityEngine.XR.ARSubsystems;
#endif // WLT_XR_PERSISTENCE


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
    public partial class AnchorManagerARF : AnchorManager
    {
        private bool openXRPersistence = false;

#if WLT_XR_PERSISTENCE
        private XRAnchorStore openXRAnchorStore = null;

        private async Task<XRAnchorStore> EnsureOpenXRAnchorStore()
        {
            Debug.Assert(arAnchorManager != null, "Trying to open XRAnchorStore before creating ARAnchorManager.");
            if (openXRAnchorStore == null)
            {
                DebugLogExtra($"Getting new OpenXR XRAnchorStore.");
                //                openXRAnchorStore = await arAnchorManager.LoadAnchorStoreAsync();
                openXRAnchorStore = await XRAnchorStore.LoadAsync(arAnchorManager.subsystem);
            }
            openXRPersistence = openXRAnchorStore != null;
            return openXRAnchorStore;
        }
#endif // WLT_XR_PERSISTENCE

#if false
        /// <summary>
        /// Steps to show problem with XRAnchorStore.Clear()
        /// </summary>
        /// <param name="anchorStore">An empty XRAnchorStore</param>
        /// <param name="anchorTrackableId">TrackableId for an ARAnchor</param>
        private static void TestAnchorStore(XRAnchorStore anchorStore, TrackableId anchorTrackableId)
        {
            string anchorName = "Anchor0";

            // Add the anchor to the anchorStore. Since anchorStore is empty, this succeeds.
            anchorStore.TryPersistAnchor(anchorTrackableId, anchorName);

            // Add the anchor again. Since it is already in the anchorStore, this fails with error message emitted.
            // This is correct and expected.
            anchorStore.TryPersistAnchor(anchorTrackableId, anchorName);
            // [XR] [Mixed Reality OpenXR Anchor]: Could not persist anchor "Anchor0"; name already used in anchor store.

            // Clear the anchorStore.
            anchorStore.Clear();

            // It seems to have cleared everything, because the anchorStore.PersistedAnchorNames.Count == 0 now.
            // But attempting to persist Anchor0 again will still result in the same error.
            // INCORRECT HERE.
            anchorStore.TryPersistAnchor(anchorTrackableId, anchorName);
            // [XR] [Mixed Reality OpenXR Anchor]: Could not persist anchor "Anchor0"; name already used in anchor store.

            // Now Un-persist the anchor.
            anchorStore.UnpersistAnchor(anchorName);

            // Persisting it now seems to succeed (or at least generates no error message.)
            anchorStore.TryPersistAnchor(anchorTrackableId, anchorName);
        }
#endif

        protected async Task SaveAnchorsOpenXR(List<SpongyAnchorWithId> spongyAnchors)
        {
            Debug.Assert(openXRPersistence, "Attempting to save via OpenXR when unsupported.");
#if WLT_XR_PERSISTENCE

            var anchorStore = await EnsureOpenXRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got OpenXR anchorStore for Save with {anchorStore.PersistedAnchorNames.Count} previously saved anchors.");

            foreach (var keyval in spongyAnchors)
            {
                DebugLogExtra($"key={keyval.anchorId.FormatStr()}, val={keyval.spongyAnchor.name}");
                var id = keyval.anchorId;
                var anchor = keyval.spongyAnchor;
                Debug.Assert(anchor.name == id.FormatStr(), $"anchor.name={anchor.name} != id={id.FormatStr()}");
                var anchorARF = anchor as SpongyAnchorARF;
                Debug.Assert(anchorARF != null);
                if (!anchorARF.IsSaved)
                {
                    DebugLogExtra($"Unpersist: {anchorARF.TrackableId} - {anchor.name}");
                    anchorStore.UnpersistAnchor(anchor.name);
                    DebugLogExtra($"TryPersist: {anchorARF.TrackableId} - {anchor.name}");
                    anchorStore.TryPersistAnchor(anchorARF.TrackableId, anchor.name);
                    anchorARF.IsSaved = true;
                }
                else
                {
                    DebugLogExtra($"Skip save of {anchorARF.name} because it is already saved.");
                }
            }

#else // WLT_XR_PERSISTENCE
            await Task.CompletedTask;
#endif // WLT_XR_PERSISTENCE
        }

#if WLT_XR_PERSISTENCE
        private readonly Dictionary<TrackableId, AnchorId> waitOnLoading = new Dictionary<TrackableId, AnchorId>();
#endif // WLT_XR_PERSISTENCE

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
            DebugLogExtra($"Load enter: AnchorSubsystem is {arAnchorManager.subsystem.running}");

            var anchorStore = await EnsureOpenXRAnchorStore();
            if (anchorStore == null)
            {
                return;
            }
            DebugLogExtra($"Got OpenXR anchorStore for Load {anchorStore.PersistedAnchorNames.Count}");
#if WLT_EXTRA_LOGGING
            foreach (var name in anchorStore.PersistedAnchorNames)
            {
                Debug.Log($"PERSISTED: {name}");
            }
#endif // WLT_EXTRA_LOGGING

            var anchorIds = plugin.GetFrozenAnchorIds();
            DebugLogExtra($"Got {anchorIds.Length} ids from plugin.");

            AnchorId maxId = firstId;

            foreach (var id in anchorIds)
            {
                var trackableId = anchorStore.LoadAnchor(id.FormatStr());
                if (trackableId != TrackableId.invalidId)
                {
                    DebugLogExtra($"LoadAnchor returns {trackableId}");
                    // We have the trackableId for this anchorId, but don't have an ARAnchor yet.
                    waitOnLoading[trackableId] = id;
                }
                else
                {
                    DebugLogExtra($"Failed to load id={id.FormatStr()} trackableId={trackableId}, deleting");
                    plugin.RemoveFrozenAnchor(id);
                }
            }

            while (waitOnLoading.Count > 0)
            {
                DebugLogExtra($"Waiting for load of {waitOnLoading.Count} more anchors at frame {Time.frameCount}");
                await Task.Yield();
            }

            DebugLogExtra($"Load exit: AnchorSubsystem is {arAnchorManager.subsystem.running}");

#else // WLT_XR_PERSISTENCE
            await Task.CompletedTask;
#endif // WLT_XR_PERSISTENCE
        }
    }

}

#endif // WLT_ARFOUNDATION_PRESENT

#endif // UNITY_2020_1_OR_NEWER
