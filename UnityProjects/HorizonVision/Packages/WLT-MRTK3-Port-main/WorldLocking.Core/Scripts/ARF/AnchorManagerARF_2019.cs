// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !UNITY_2020_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

#if WLT_ARFOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;

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
    public class AnchorManagerARF : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return false; } }

        /// <inheritdoc/>
        public override Pose AnchorFromSpongy 
        { 
            get 
            { 
                return axrOrigin.transform.GetGlobalPose(); 
            } 
        }

        private readonly ARSession arSession;
        private readonly XROrigin xrOrigin;

        private readonly ARReferencePointManager arReferencePointManager;

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

            this.arReferencePointManager = xrOrigin.gameObject.GetComponent<ARReferencePointManager>();
            if (this.arReferencePointManager == null)
            {
                DebugLogSetup($"Adding AR reference point manager to {xrOrigin.name}");
                this.arReferencePointManager = xrOrigin.gameObject.AddComponent<ARReferencePointManager>();
            }
            DebugLogSetup($"ARF: Created AnchorManager ARF");
        }

        protected override bool IsTracking()
        {
            Debug.Assert(arSession != null);

            return ARSession.notTrackingReason == UnityEngine.XR.ARSubsystems.NotTrackingReason.None;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
#if WLT_EXTRA_LOGGING
            Debug.Log($"Creating anchor {id.FormatStr()}");
#endif // WLT_EXTRA_LOGGING
            initialPose = AnchorFromSpongy.Multiply(initialPose);
            var arAnchor = arReferencePointManager.AddReferencePoint(initialPose);
            if (arAnchor == null)
            {
                Debug.LogError($"ARReferencePoinManager failed to create ARAnchor {id}");
                return null;                
            }
            arAnchor.gameObject.name = id.FormatStr();
            SpongyAnchorARF newAnchor =  arAnchor.gameObject.AddComponent<SpongyAnchorARF>();
            return newAnchor;
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            if (spongyAnchor is SpongyAnchorARF spongyARF)
            {
                spongyARF.Cleanup(arReferencePointManager);
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
            /// Placeholder for consistency. Persistence not implemented for ARF, so
            /// to be consistent with this APIs contract, we must clear all frozen anchors from the plugin.
            plugin.ClearFrozenAnchors();

            await Task.CompletedTask;
        }
    }
}
#endif // WLT_ARFOUNDATION_PRESENT

#endif // !UNITY_2020_1_OR_NEWER
