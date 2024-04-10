// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_EXTRA_LOGGING

#if WLT_DISABLE_LOGGING
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

#if UNITY_2020_1_OR_NEWER

#if UNITY_2020_3_OR_NEWER
#define WLT_ADD_ANCHOR_COMPONENT
#endif // UNITY_2020_3_OR_NEWER

#if WLT_ARFOUNDATION_PRESENT

#if WLT_MICROSOFT_OPENXR_PRESENT && UNITY_WSA
#define WLT_XR_PERSISTENCE
#endif // WLT_XR_PERSISTENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

#if WLT_XR_MANAGEMENT_PRESENT
using UnityEngine.XR.Management;
#endif // WLT_XR_MANAGEMENT_PRESENT

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
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return openXRPersistence; } }

        /// <inheritdoc/>
        public override Pose AnchorFromSpongy 
        { 
            get 
            { 
                return xrOrigin.transform.GetGlobalPose(); 
            } 
        }

        private readonly ARSession arSession;
        private readonly XROrigin xrOrigin;

        private readonly ARAnchorManager arAnchorManager;

        private readonly Dictionary<TrackableId, SpongyAnchorARF> anchorsByTrackableId = new Dictionary<TrackableId, SpongyAnchorARF>();

        protected override float TrackingStartDelayTime { get { return SpongyAnchorARF.TrackingStartDelayTime; } }

        public static async Task<AnchorManagerARF> TryCreate(IPlugin plugin, IHeadPoseTracker headTracker, 
            GameObject arSessionSource,
            GameObject xrOriginSource)
        {
            bool xrRunning = await CheckXRRunning();
            if (!xrRunning)
            {
                Debug.LogError($"Error checking that XR is up and running.");
                return null;
            }
            if (arSessionSource == null)
            {
                Debug.LogError("Trying to create an AR Foundation anchor manager with null session source holder GameObject.");
                return null;
            }
            if (xrOriginSource == null)
            {
                Debug.LogError("Trying to create an AR Foundation anchor manager with null session origin source holder GameObject.");
                return null;
            }
            ARSession arSession = arSessionSource.GetComponent<ARSession>();
            if (arSession == null)
            {
                DebugLogSetup($"Adding AR session to {arSessionSource.name}");
                arSession = arSessionSource.AddComponent<ARSession>();
            }
            if (arSession == null)
            {
                Debug.LogError($"Failure acquiring ARSession component from {arSessionSource.name}, can't create AnchorManagerARF");
                return null;
            }
            XROrigin xrOrigin = xrOriginSource.GetComponent<XROrigin>();
            if (xrOrigin == null)
            {
                DebugLogSetup($"Adding AR session origin to {xrOriginSource.name}");
                xrOrigin = xrOriginSource.AddComponent<XROrigin>();
            }
            if (xrOrigin == null)
            {
                Debug.LogError($"Failure acquiring XROrigin from {xrOriginSource.name}, can't create AnchorManagerARF");
            }
            AnchorManagerARF anchorManager = new AnchorManagerARF(plugin, headTracker, arSession, xrOrigin);

            return anchorManager;
        }

                /// <summary>
        /// Wait to make sure XR is up and running before proceeding. This is important when using Holographic Remoting,
        /// during which the delay can be significant.
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> CheckXRRunning()
        {
#if WLT_XR_MANAGEMENT_PRESENT
            DebugLogSetup($"F={Time.frameCount} checking that XR is running.");
            // Wait for XR initialization before initializing the anchor subsystem to ensure that any pending Remoting connection has been established first.
            while (UnityEngine.XR.Management.XRGeneralSettings.Instance == null ||
                   UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager == null ||
                   UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                DebugLogSetup($"F={Time.frameCount} waiting on XR startup.");
                await Task.Yield();
            }
            DebugLogSetup($"F={Time.frameCount} XR is running.");
#endif // WLT_XR_MANAGEMENT_PRESENT
            return true;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerARF(IPlugin plugin, IHeadPoseTracker headTracker, ARSession arSession, XROrigin xrOrigin) 
            : base(plugin, headTracker)
        {
            DebugLogSetup($"ARF: Creating AnchorManagerARF with {arSession.name} and {xrOrigin.name}");
            this.arSession = arSession;
            this.xrOrigin = xrOrigin;

#if WLT_XR_MANAGEMENT_PRESENT
            openXRPersistence = XRGeneralSettings.Instance.Manager.activeLoader.name.StartsWith("Open XR");
#endif // WLT_XR_MANAGEMENT_PRESENT

            this.arAnchorManager = xrOrigin.gameObject.GetComponent<ARAnchorManager>();
            if (this.arAnchorManager == null)
            {
                DebugLogSetup($"Adding AR reference point manager to {xrOrigin.name}");
                this.arAnchorManager = xrOrigin.gameObject.AddComponent<ARAnchorManager>();
            }
            DebugLogSetup($"ARF: Created AnchorManager ARF");
#if WLT_XR_PERSISTENCE
            // See notes at OnAnchorsChanged definition.
            this.arAnchorManager.anchorsChanged += OnAnchorsChanged;
#endif // WLT_XR_PERSISTENCE
        }

#if WLT_XR_PERSISTENCE
        /// <summary>
        /// Callback when the set of active spatial anchors changes.
        /// </summary>
        /// <param name="obj">Callback arguments</param>
        /// <remarks>
        /// The only anchor change event we care about is when anchors, which are in the "waitOnLoading" dictionary,
        /// are added. Those are anchors which have been loaded but now need to be finalized.
        /// We ignore other anchor added/changed/removed events. 
        /// And in fact, when anchor persistence isn't supported, we don't even bother registering for this event.
        /// </remarks>
        private void OnAnchorsChanged(ARAnchorsChangedEventArgs obj)
        {
            foreach (var added in obj.added)
            {
                DebugLogExtra($"Process Added={added.trackableId}");
                TrackableId trackableId = added.trackableId;
                AnchorId anchorId;
                if (waitOnLoading.TryGetValue(trackableId, out anchorId))
                {
                    Debug.Assert(SpongyAnchors.FindIndex(x => x.anchorId == anchorId) < 0, $"OnAnchorsChanged: {anchorId.FormatStr()} in waiting list, but already in SpongyAnchors");
                    added.gameObject.name = anchorId.FormatStr();
                    SpongyAnchorARF spongyARF = WrapARAnchor(anchorId, trackableId, added.gameObject);
                    // Adding through this path means it was just loaded from anchorStore, which means it's already saved.
                    spongyARF.IsSaved = true;
                    SpongyAnchors.Add(new SpongyAnchorWithId()
                    {
                        anchorId = anchorId,
                        spongyAnchor = spongyARF
                    }
                    );
                    waitOnLoading.Remove(trackableId);
                }
                else
                {
                    DebugLogExtra($"Incoming trackableId={trackableId} but not in waiting list of {waitOnLoading.Count} pending anchors");
                }
            }
        }
#endif // WLT_XR_PERSISTENCE

        protected override bool IsTracking()
        {
            Debug.Assert(arSession != null);
            return ARSession.notTrackingReason == UnityEngine.XR.ARSubsystems.NotTrackingReason.None;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            DebugLogExtra($"Creating anchor {id.FormatStr()}");
            initialPose = AnchorFromSpongy.Multiply(initialPose);
            SpongyAnchorARF newAnchor = null;
#if WLT_ADD_ANCHOR_COMPONENT
            GameObject go = new GameObject(id.FormatStr());
            go.transform.SetParent(parent);
            go.transform.SetGlobalPose(initialPose);
            ARAnchor arAnchor = go.AddComponent<ARAnchor>();
            newAnchor = WrapARAnchor(id, arAnchor.trackableId, go);
#else // WLT_ADD_ANCHOR_COMPONENT
            var arAnchor = arAnchorManager.AddAnchor(initialPose);
            if (arAnchor == null)
            {
                Debug.LogError($"ARAnchorManager failed to create ARAnchor {id}");
                return null;
            }
            arAnchor.gameObject.name = id.FormatStr();
            newAnchor = WrapARAnchor(id, arAnchor.trackableId, arAnchor.gameObject);
#endif // WLT_ADD_ANCHOR_COMPONENT
            Debug.Assert(newAnchor != null);
            return newAnchor;
        }

        private SpongyAnchorARF WrapARAnchor(AnchorId anchorId, TrackableId trackableId, GameObject newAnchorObject)
        {
            SpongyAnchorARF spongyAnchorARF = newAnchorObject.AddComponent<SpongyAnchorARF>();
            anchorsByTrackableId[trackableId] = spongyAnchorARF;
            spongyAnchorARF.TrackableId = trackableId;

            return spongyAnchorARF;
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            if (spongyAnchor is SpongyAnchorARF spongyARF)
            {
                spongyARF.Cleanup(arAnchorManager);
            }
            RemoveSpongyAnchorById(id);

            return null;
        }

        [System.Diagnostics.Conditional("WLT_EXTRA_LOGGING")]
        private static void DebugOutExtra(string label, ARAnchor arAnchor, SpongyAnchorARF tracker)
        {
            Debug.Assert(arAnchor.trackableId == tracker.TrackableId);
            Debug.Log($"{label}{tracker.name}-{tracker.TrackableId}/{arAnchor.trackingState}: T={tracker.transform.GetGlobalPose().ToString("F3")} AR={arAnchor.transform.GetGlobalPose().ToString("F3")}");
        }

        protected override async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
            if (openXRPersistence)
            {
                DebugLogSetup($"Saving {spongyAnchors.Count} spongyAnchors.");
                await SaveAnchorsOpenXR(spongyAnchors);
            }
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
            if (openXRPersistence)
            {
                await LoadAnchorsOpenXR(plugin, firstId, parent, spongyAnchors);
            }
        }
    }
}
#endif // WLT_ARFOUNDATION_PRESENT

#endif // UNITY_2020_1_OR_NEWER
