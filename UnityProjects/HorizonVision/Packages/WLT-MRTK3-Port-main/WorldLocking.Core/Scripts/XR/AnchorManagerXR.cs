// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_2020_1_OR_NEWER

#if WLT_MICROSOFT_OPENXR_PRESENT || WLT_MICROSOFT_WMR_XR_4_3_PRESENT
#define WLT_XR_PERSISTENCE
#endif // WLT_XR_PERSISTENCE

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

#if WLT_XR_MANAGEMENT_PRESENT
using UnityEngine.XR.Management;
#endif // WLT_XR_MANAGEMENT_PRESENT


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
    public partial class AnchorManagerXR : AnchorManager
    {
        /// <inheritdoc/>
        public override bool SupportsPersistence { get { return wmrPersistence || openXRPersistence; } }

        protected override float TrackingStartDelayTime { get { return SpongyAnchorXR.TrackingStartDelayTime; } }

        private readonly XRAnchorSubsystem xrAnchorManager;

        private readonly XRSessionSubsystem sessionSubsystem;

        private readonly Dictionary<TrackableId, SpongyAnchorXR> anchorsByTrackableId = new Dictionary<TrackableId, SpongyAnchorXR>();

        public static async Task<AnchorManagerXR> TryCreate(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            bool xrRunning = await CheckXRRunning();
            if (!xrRunning)
            {
                return null;
            }

            /// Try to find an XRAnchorManager (to be XRAnchorManager) here. 
            /// If we fail that,
            ///     give up. 
            /// Else 
            ///     pass the manager into AnchorManagerXR for its use.
            XRAnchorSubsystem xrAnchorManager = FindAnchorManager();

            if (xrAnchorManager == null)
            {
                return null;
            }
            if (!xrAnchorManager.running)
            {
                xrAnchorManager.Start();
            }

            var session = FindSessionSubsystem();

            AnchorManagerXR anchorManager = new AnchorManagerXR(plugin, headTracker, xrAnchorManager, session);

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
                DebugLogSetup($"F={Time.frameCount} waiting on XR startup.");
                await Task.Yield();
            }
            DebugLogSetup($"F={Time.frameCount} XR is running.");
#endif // WLT_XR_MANAGEMENT_PRESENT
            return true;
        }

        /// <summary>
        /// Find the correct AnchorManager for this session.
        /// </summary>
        /// <returns>Anchor manager for this session.</returns>
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
        private static XRAnchorSubsystem FindAnchorManager()
        {
            List<XRAnchorSubsystem> anchorSubsystems = new List<XRAnchorSubsystem>();
            SubsystemManager.GetInstances(anchorSubsystems);
            DebugLogSetup($"Found {anchorSubsystems.Count} anchor subsystems.");
            XRAnchorSubsystem activeSubsystem = null;
            int numFound = 0;
            foreach (var sub in anchorSubsystems)
            {
                if (sub.running)
                {
                    DebugLogSetup($"Found active anchor subsystem {sub.subsystemDescriptor.id}.");
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
                        DebugLogSetup($"Start changed anchor subsystem [{sub.subsystemDescriptor.id}] to running.");
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
        /// Find the session subsystem for this session.
        /// </summary>
        /// <returns>Session subsystem for this session in an active running state.</returns>
        /// <remarks>
        /// See remarks in <see cref="FindAnchorManager"/> above.
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
                    DebugLogSetup($"Found active session subsystem {session.subsystemDescriptor.id}");
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
                        DebugLogSetup($"Start changed session [{session.subsystemDescriptor.id}] to running.");
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
        private AnchorManagerXR(IPlugin plugin, IHeadPoseTracker headTracker, XRAnchorSubsystem xrAnchorManager, XRSessionSubsystem session)
            : base(plugin, headTracker)
        {
            this.xrAnchorManager = xrAnchorManager;
            this.sessionSubsystem = session;
            DebugLogSetup($"XR: Created AnchorManager XR, xrMgr={(this.xrAnchorManager != null ? "good" : "null")} session={(session != null ? "good" : "null")}");

            DebugLogSetup($"ActiveLoader name:[{XRGeneralSettings.Instance.Manager.activeLoader.name}] type:[{XRGeneralSettings.Instance.Manager.activeLoader.GetType().FullName}]");

#if WLT_XR_MANAGEMENT_PRESENT
            wmrPersistence = XRGeneralSettings.Instance.Manager.activeLoader.name.StartsWith("Windows MR");
            openXRPersistence = XRGeneralSettings.Instance.Manager.activeLoader.name.StartsWith("Open XR");
#endif // WLT_XR_MANAGEMENT_PRESENT
            DebugLogSetup($"XRSDK Persistence: WMR={wmrPersistence} OpenXR={openXRPersistence}");
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
            if (xrAnchorManager == null)
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
            DebugLogExtra($"UpdateTrackables {Time.frameCount} XRAnchorSubsystem is {xrAnchorManager.running}");
            TrackableChanges<XRAnchor> changes = xrAnchorManager.GetChanges(Unity.Collections.Allocator.Temp);
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
        private static void DebugOutExtra(string label, XRAnchor xrAnchor, SpongyAnchorXR tracker)
        {
            Debug.Assert(xrAnchor.trackableId == tracker.TrackableId);
            Vector3 tP = tracker.transform.position;
            Vector3 tR = tracker.transform.rotation.eulerAngles;
            Vector3 rP = xrAnchor.pose.position;
            Vector3 rR = xrAnchor.pose.rotation.eulerAngles;
            rR = new Vector3(1.0f, 2.0f, 3.0f);
            Debug.Log($"{label}{tracker.name}-{tracker.TrackableId}/{xrAnchor.trackingState}: {DebugVector3("tP=", tP)}|{DebugEuler("tR=", tR)} <=> {DebugVector3("rP=", rP)}|{DebugEuler("rR=", rR)}");
        }

        private static void UpdateTracker(string label, XRAnchor xrAnchor, Dictionary<TrackableId, SpongyAnchorXR> anchors)
        {
            SpongyAnchorXR tracker;
            if (anchors.TryGetValue(xrAnchor.trackableId, out tracker))
            {
                DebugOutExtra(label, xrAnchor, tracker);

                tracker.IsReliablyLocated = xrAnchor.trackingState != TrackingState.None;

                Pose repose = ExtractPose(xrAnchor);
                Vector3 delta = repose.position - tracker.transform.position;
                tracker.Delta = delta;
                tracker.transform.position = repose.position;
                tracker.transform.rotation = repose.rotation;
            }
            else
            {
                Debug.LogError($"Missing trackableId {xrAnchor.trackableId} from DB, ignoring.");
            }
        }

        private static Pose ExtractPose(XRAnchor xrAnchor)
        {
            return xrAnchor.pose;
        }

        private static bool CheckTracking(XRAnchor xrAnchor)
        {
            return xrAnchor.trackingState != TrackingState.None;
        }


        protected override bool IsTracking()
        {
            if (sessionSubsystem == null)
            {
                Debug.LogError($"Frame={Time.frameCount} have null sessionSubsystem.");
                return false;
            }
            DebugLogExtra($"AMXR F{Time.frameCount} session running={sessionSubsystem.running} state={sessionSubsystem.trackingState}");
            if (!sessionSubsystem.running)
            {
                // This is probably a catastrophic failure case.
                Debug.LogError($"Frame={Time.frameCount} LostTracking: Have session subsystem but not running.");
                return false;
            }
            return sessionSubsystem.notTrackingReason == NotTrackingReason.None;
        }

        protected override SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose)
        {
            SpongyAnchorXR spongyAnchorXR = null;
            if (IsTracking())
            {
                DebugLogExtra($"Creating anchor at initial ({initialPose.position.x:0.000}, {initialPose.position.y:0.000}, {initialPose.position.z:0.000})");
                XRAnchor xrAnchor;
                bool created = xrAnchorManager.TryAddAnchor(initialPose, out xrAnchor);
                if (created)
                {
                    spongyAnchorXR = PrepAnchor(id, parent, xrAnchor.trackableId, xrAnchor.pose);
                }
            }
            return spongyAnchorXR;
        }

        private SpongyAnchorXR PrepAnchor(AnchorId anchorId, Transform parent, TrackableId trackableId, Pose xrPose)
        {
            var newAnchorObject = new GameObject(anchorId.FormatStr());
            newAnchorObject.transform.parent = parent;
            newAnchorObject.transform.SetGlobalPose(xrPose);
            SpongyAnchorXR spongyAnchorXR = newAnchorObject.AddComponent<SpongyAnchorXR>();
            anchorsByTrackableId[trackableId] = spongyAnchorXR;
            spongyAnchorXR.TrackableId = trackableId;
            spongyAnchorXR.IsReliablyLocated = false;

            DebugLogExtra($"{anchorId} {DebugVector3("P=", xrPose.position)}, {DebugQuaternion("Q=", xrPose.rotation)}");

            return spongyAnchorXR;
        }

        protected override SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor)
        {
            SpongyAnchorXR spongyAnchorXR = spongyAnchor as SpongyAnchorXR;
            if (spongyAnchorXR != null)
            {
                Debug.Assert(anchorsByTrackableId[spongyAnchorXR.TrackableId] == spongyAnchorXR);
                anchorsByTrackableId.Remove(spongyAnchorXR.TrackableId);
                xrAnchorManager.TryRemoveAnchor(spongyAnchorXR.TrackableId);
                GameObject.Destroy(spongyAnchorXR.gameObject);
            }
            RemoveSpongyAnchorById(id);

            return null;
        }


        protected override async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
            DebugLogExtra($"SaveAnchors enter: persistence wmr={wmrPersistence} openXR={openXRPersistence} nAnchors={spongyAnchors.Count}");
            if (wmrPersistence)
            {
                await SaveAnchorsWMR(spongyAnchors);
            }
            // wmrPersistence might have turned false, if in trying to save it realized it couldn't.
            if (wmrPersistence)
            {
                openXRPersistence = false; // can only have one.
            }
            if (openXRPersistence)
            {
                await SaveAnchorsOpenXR(spongyAnchors);
            }
            DebugLogExtra($"SaveAnchors exit: persistence wmr={wmrPersistence} openXR={openXRPersistence}");
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
            DebugLogExtra($"LoadAnchors enter: persistence wmr={wmrPersistence} openXR={openXRPersistence}");
            if (wmrPersistence)
            {
                await LoadAnchorsWMR(plugin, firstId, parent, spongyAnchors);
            }
            // wmrPersistence might have turned false, if in trying to save it realized it couldn't.
            if (wmrPersistence)
            {
                openXRPersistence = false; // can only have one.
            }
            if (openXRPersistence)
            {
                await LoadAnchorsOpenXR(plugin, firstId, parent, spongyAnchors);
            }
            // THey might both be false, if we failed on both APIs.
            if (!wmrPersistence && !openXRPersistence)
            {
                /// Placeholder for consistency. If persistence is not supported, then 
                /// to be consistent with this APIs contract, we must clear all frozen anchors from the plugin.
                plugin.ClearFrozenAnchors();
            }
            DebugLogExtra($"LoadAnchors exit: persistence wmr={wmrPersistence} openXR={openXRPersistence} nAnchors={spongyAnchors.Count}");
        }
    }
}
#endif // WLT_ARSUBSYSTEMS_PRESENT

#endif // UNITY_2020_1_OR_NEWER
