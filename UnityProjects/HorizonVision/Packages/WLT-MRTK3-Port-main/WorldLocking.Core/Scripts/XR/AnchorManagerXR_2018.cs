// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if !UNITY_2020_1_OR_NEWER

//#define WLT_EXTRA_LOGGING
#define WLT_LOG_SETUP

#if WLT_DISABLE_LOGGING
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

#if WLT_ARSUBSYSTEMS_PRESENT
using UnityEngine.SpatialTracking;
using UnityEngine.XR.ARSubsystems;

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
    public class AnchorManagerXR : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return false; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorXR.TrackingStartDelayTime; } }

        private readonly XRReferencePointSubsystem xrReferencePointManager;

        private readonly XRSessionSubsystem sessionSubsystem;

        private readonly Dictionary<TrackableId, SpongyAnchorXR> anchorsByTrackableId = new Dictionary<TrackableId, SpongyAnchorXR>();

        public static async Task<AnchorManagerXR> TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            bool xrRunning = await CheckXRRunning();
            if (!xrRunning)
            {
                return null;
            }

            /// Try to find an XRReferencePointManager (to be XRAnchorManager) here. 
            /// If we fail that,
            ///     give up. 
            /// Else 
            ///     pass the manager into AnchorManagerXR for its use.
            XRReferencePointSubsystem xrReferencePointManager = FindReferencePointManager();

            if (xrReferencePointManager == null)
            {
                return null;
            }
            if (!xrReferencePointManager.running)
            {
                xrReferencePointManager.Start();
            }

            var session = FindSessionSubsystem();
            if (session == null)
            {
                return null;
            }

            AnchorManagerXR anchorManager = new AnchorManagerXR(plugin, headTracker, xrReferencePointManager, session);

            return anchorManager;
        }

        private static async Task<bool> CheckXRRunning()
        {
#if WLT_XR_MANAGEMENT_PRESENT
            DebugLogSetup($"F={Time.frameCount} checking that XR is running.");
            // Wait for XR initialization before initializing the anchor subsystem to ensure that any pending Remoting connection has been established first.
            while (UnityEngine.XR.Management.XRGeneralSettings.Instance == null ||
                   UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager == null ||
                   UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                if ((Time.frameCount / 100) * 100 == Time.frameCount)
                {
                    DebugLogSetup($"F={Time.frameCount} waiting on XR startup.");
                }
                await Task.Yield();
            }
            DebugLogSetup($"F={Time.frameCount} XR is running.");
#endif // WLT_XR_MANAGEMENT_PRESENT
            return true;
        }



        /// <summary>
        /// Find the correct ReferencePointManager for this session.
        /// </summary>
        /// <returns>Reference point manager for this session.</returns>
        /// <remarks>
        /// For HoloLens, we are looking for the _active_ anchor subsystem. There may be multiple
        /// anchor subsystems (e.g. OpenXR Plugin and WMR XR Plugin), but only one will be active.
        /// However, on Android, no anchor subsystem is active until we make it active by calling Start() on it.
        /// So if we still don't have an active subsystem, try calling start on any that are available and 
        /// if that makes them active (running), then take that one. 
        /// Note that this would be bad on HoloLens, as it would start up a subsystem that is installed but not currently selected.
        /// I believe that iOS works as HoloLens does, but I don't want to rely on undocumented behavior. The algorithm here
        /// should work on either.
        /// </remarks>
        private static XRReferencePointSubsystem FindReferencePointManager()
        {
            List<XRReferencePointSubsystem> anchorSubsystems = new List<XRReferencePointSubsystem>();
            SubsystemManager.GetInstances(anchorSubsystems);
            DebugLogSetup($"Found {anchorSubsystems.Count} anchor subsystems.");
            XRReferencePointSubsystem activeSubsystem = null;
            int numFound = 0;
            foreach (var sub in anchorSubsystems)
            {
                if (sub.running)
                {
                    DebugLogSetup($"Found active anchor subsystem.");
                    activeSubsystem = sub;
                    ++numFound;
                }
            }
            if (activeSubsystem == null)
            {
                DebugLogSetup($"Found no anchor subsystem running, will try starting one.");
                foreach (var sub in anchorSubsystems)
                {
                    sub.Start();
                    if (sub.running)
                    {
                        activeSubsystem = sub;
                        ++numFound;
                        DebugLogSetup($"Start changed an anchor subsystem [{sub.SubsystemDescriptor.id}] to running.");
                    }
                }
            }
            if (numFound != 1)
            {
                Debug.LogError($"Found {numFound} active anchor subsystems, expected exactly one.");
            }
            return activeSubsystem;
        }

        /// <summary>
        /// Find and return the correct XRSessionSubsystem.
        /// </summary>
        /// <returns>The XRSessionSubsystem.</returns>
        /// <remarks>
        /// See remarks in <see cref="FindReferencePointManager"/> above.
        /// </remarks>
        private static XRSessionSubsystem FindSessionSubsystem()
        {
            List<XRSessionSubsystem> sessionSubsystems = new List<XRSessionSubsystem>();
            SubsystemManager.GetInstances(sessionSubsystems);
            DebugLogSetup($"Found {sessionSubsystems.Count} session subsystems");
            XRSessionSubsystem activeSession = null;
            int numFound = 0;
            foreach (var session in sessionSubsystems)
            {
                if (session.running)
                {
                    DebugLogSetup($"Found active session subsystem");
                    activeSession = session;
                    ++numFound;
                }
            }
            if (activeSession == null)
            {
                DebugLogSetup($"Found no active session subsystem, will try starting one.");
                foreach (var session in sessionSubsystems)
                {
                    session.Start();
                    if (session.running)
                    {
                        activeSession = session;
                        ++numFound;
                        DebugLogSetup($"Start changed session [{session.SubsystemDescriptor.id}] to running.");
                    }
                }
            }
            if (numFound != 1)
            {
                Debug.LogError($"Found {numFound} active session subsystems, expected exactly one.");
            }    
            return activeSession;
        }

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        private AnchorManagerXR(IPlugin plugin, IHeadPoseTracker headTracker, XRReferencePointSubsystem xrReferencePointManager, XRSessionSubsystem session)
            : base(plugin, headTracker)
        {
            this.xrReferencePointManager = xrReferencePointManager;
            DebugLogSetup($"XR: Created AnchorManager XR, xrMgr={(this.xrReferencePointManager != null ? "good" : "null")}");
            this.sessionSubsystem = session;
        }

        public override bool Update()
        {
            if (!UpdateTrackables())
            {
                return false;
            }
            return base.Update();
        }

        private bool UpdateTrackables()
        {
            if (xrReferencePointManager == null || !xrReferencePointManager.running)
            {
                return false;
            }
#if !UNITY_ANDROID && !UNITY_IOS
            if (sessionSubsystem != null)
            {
                sessionSubsystem.Update(new XRSessionUpdateParams
                {
                    screenOrientation = Screen.orientation,
                    screenDimensions = new Vector2Int(Screen.width, Screen.height)
                });
            }
#endif // !UNITY_ANDROID && !UNITY_IOS            
            TrackableChanges<XRReferencePoint> changes = xrReferencePointManager.GetChanges(Unity.Collections.Allocator.Temp);
            if (changes.isCreated && (changes.added.Length + changes.updated.Length + changes.removed.Length > 0))
            {
                DebugLogExtra($"Changes Fr{Time.frameCount:0000}: isCreated={changes.isCreated} Added={changes.added.Length}, Updated={changes.updated.Length} Removed={changes.removed.Length}");
                for (int i = 0; i < changes.added.Length; ++i)
                {
                    UpdateTracker("Added::", changes.added[i], anchorsByTrackableId);
                }
                for (int i = 0; i < changes.updated.Length; ++i)
                {
                    UpdateTracker("Updated::", changes.updated[i], anchorsByTrackableId);
                }
                for (int i = 0; i < changes.removed.Length; i++)
                {
                    RemoveTracker(changes.removed[i], anchorsByTrackableId);
                }
            }
            changes.Dispose();
            return true;
        }
        private static bool RemoveTracker(TrackableId trackableId, Dictionary<TrackableId, SpongyAnchorXR> anchors)
        {
            DebugLogExtra($"Removed:: id={trackableId}");

            return anchors.Remove(trackableId);
        }

        private static float DebugNormAngleDeg(float deg)
        {
            while (deg > 180.0f)
            {
                deg -= 360.0f;
            }
            return deg;
        }
        private static Vector3 DebugNormRot(Vector3 euler)
        {
            euler.x = DebugNormAngleDeg(euler.x);
            euler.y = DebugNormAngleDeg(euler.y);
            euler.z = DebugNormAngleDeg(euler.z);
            return euler;
        }
        public static string DebugEuler(string label, Vector3 euler)
        {
            euler = DebugNormRot(euler);
            //            return $"{label}{euler}";
            return DebugVector3(label, euler);
        }
        public static string DebugQuaternion(string label, Quaternion q)
        {
            return $"{label}({q.x:0.00},{q.y:0.00},{q.z:0.00},{q.w:0.00})";
        }
        public static string DebugVector3(string label, Vector3 p)
        {
            return $"{label}({p.x:0.000},{p.y:0.000},{p.z:0.000})";
        }

        [System.Diagnostics.Conditional("WLT_EXTRA_LOGGING")]
        private static void DebugOutExtra(string label, XRReferencePoint referencePoint, SpongyAnchorXR tracker)
        {
            Debug.Assert(referencePoint.trackableId == tracker.TrackableId);
            Vector3 tP = tracker.transform.position;
            Vector3 tR = tracker.transform.rotation.eulerAngles;
            Vector3 rP = referencePoint.pose.position;
            Vector3 rR = referencePoint.pose.rotation.eulerAngles;
            rR = new Vector3(1.0f, 2.0f, 3.0f);
            DebugLogSetup($"{label}{tracker.name}-{tracker.TrackableId}/{referencePoint.trackingState}: {DebugVector3("tP=", tP)}|{DebugEuler("tR=", tR)} <=> {DebugVector3("rP=", rP)}|{DebugEuler("rR=", rR)}");
        }

        private static void UpdateTracker(string label, XRReferencePoint referencePoint, Dictionary<TrackableId, SpongyAnchorXR> anchors)
        {
            SpongyAnchorXR tracker;
            if (anchors.TryGetValue(referencePoint.trackableId, out tracker))
            {
                DebugOutExtra(label, referencePoint, tracker);

                tracker.IsReliablyLocated = referencePoint.trackingState != TrackingState.None;

                Pose repose = ExtractPose(referencePoint);
                Vector3 delta = repose.position - tracker.transform.position;
                tracker.Delta = delta;
                tracker.transform.position = repose.position;
                tracker.transform.rotation = repose.rotation;
            }
            else
            {
                Debug.LogError($"Missing trackableId {referencePoint.trackableId} from DB.");
            }
        }

        private static Pose ExtractPose(XRReferencePoint referencePoint)
        {
            return referencePoint.pose;
        }

        private static bool CheckTracking(XRReferencePoint referencePoint)
        {
            return referencePoint.trackingState != TrackingState.None;
        }


        protected override bool IsTracking()
        {
            return sessionSubsystem != null
                && sessionSubsystem.running
                && sessionSubsystem.trackingState != TrackingState.None;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            SpongyAnchorXR spongyAnchorXR = null;
            if (IsTracking())
            {
                DebugLogExtra($"Creating refPt at initial ({initialPose.position.x:0.000}, {initialPose.position.y:0.000}, {initialPose.position.z:0.000})");
                XRReferencePoint xrReferencePoint;
                bool created = xrReferencePointManager.TryAddReferencePoint(initialPose, out xrReferencePoint);
                if (created)
                {
                    Pose xrPose = xrReferencePoint.pose;
                    DebugLogExtra($"Created refPt {id} at ({xrPose.position.x:0.000}, {xrPose.position.y:0.000}, {xrPose.position.z:0.000}) is {xrReferencePoint.trackingState}");
                    var newAnchorObject = new GameObject(id.FormatStr());
                    newAnchorObject.transform.parent = parent;
                    newAnchorObject.transform.SetGlobalPose(initialPose);
                    spongyAnchorXR = newAnchorObject.AddComponent<SpongyAnchorXR>();
                    anchorsByTrackableId[xrReferencePoint.trackableId] = spongyAnchorXR;
                    spongyAnchorXR.TrackableId = xrReferencePoint.trackableId;

                    DebugLogExtra($"{id} {DebugVector3("P=", initialPose.position)}, {DebugQuaternion("Q=", initialPose.rotation)}");
                }
            }
            return spongyAnchorXR;
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            SpongyAnchorXR spongyAnchorXR = spongyAnchor as SpongyAnchorXR;
            if (spongyAnchorXR != null)
            {
                Debug.Assert(anchorsByTrackableId[spongyAnchorXR.TrackableId] == spongyAnchorXR);
                anchorsByTrackableId.Remove(spongyAnchorXR.TrackableId);
                xrReferencePointManager.TryRemoveReferencePoint(spongyAnchorXR.TrackableId);
                GameObject.Destroy(spongyAnchorXR.gameObject);
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
            /// Placeholder for consistency. Persistence not implemented for XR, so
            /// to be consistent with this APIs contract, we must clear all frozen anchors from the plugin.
            plugin.ClearFrozenAnchors();

            await Task.CompletedTask;
        }
    }
}
#endif // WLT_ARSUBSYSTEMS_PRESENT

#endif // !UNITY_2020_1_OR_NEWER