// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#define WLT_LOG_SETUP

#if WLT_DISABLE_LOGGING
#undef WLT_LOG_SETUP
#endif // WLT_DISABLE_LOGGING

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Ultimate manager of World Locking.
    /// WorldLockingManager supplies access to the sub-managers, <see cref="IAnchorManager"/>, <see cref="IFragmentManager"/>, and <see cref="IAttachmentPointManager"/>.
    /// </summary>
    public class WorldLockingManager
    {

        #region Shared settings

        private SharedManagerSettings shared = new SharedManagerSettings();

        #endregion Shared settings


        #region Public accessors

        /// <summary>
        /// The version of this release. This will be displayed in the WorldLockingContext component in the Unity Inspector,
        /// allowing quick visual verification of the version of World Locking Tools for Unity currently installed.
        /// It has no effect in code, but serves only as a label.
        /// </summary>
        public static string Version => "1.5.9";

        /// <summary>
        /// The configuration settings may only be set as a block.
        /// Get returns a snapshot of current settings, and set copies entire block.
        /// </summary>
        /// <remarks>
        /// To change an individual field in the settings, retrieve the entire settings block,
        /// change the desired field(s), then set the entire block. E.g.
        /// var settings = mgr.Settings;
        /// settings.AutoLoad = false;
        /// settings.AutoSave = true;
        /// mgr.Settings = settings;
        /// </remarks>
        public ManagerSettings Settings
        {
            get { return shared.settings; }
            set
            {
                shared.settings = value;
                ApplyNewSettings();
            }
        }

        /// <summary>
        /// Access to linkage settings. 
        /// </summary>
        public LinkageSettings LinkageSettings
        {
            get { return shared.linkageSettings; }
            set
            {
                shared.linkageSettings = value;
            }
        }

        /// <summary>
        /// Access to anchor management settings. 
        /// </summary>
        /// <remarks>
        /// Use <see cref="ResetAnchorManager"/> to change the type of the anchor manager after startup, or just rebuild it from scratch.
        /// </remarks>
        public AnchorSettings AnchorSettings
        {
            get { return shared.anchorSettings; }
            set
            {
                bool changedSubsystem = shared.anchorSettings.anchorSubsystem != value.anchorSubsystem;
                shared.anchorSettings = value;
                if (changedSubsystem)
                {
                    ResetAnchorManager();
                }
            }
        }

        /// <summary>
        /// Get a copy of the shared diagnostics configuration settings, or set the
        /// shared settings to a copy of the input.
        /// </summary>
        public DiagnosticsSettings DiagnosticsSettings
        {
            get { return DiagnosticRecordings.SharedSettings.settings; }
            set { DiagnosticRecordings.SharedSettings.settings = value; }
        }

        /// <summary>
        /// The transform at which to apply the camera adjustment. This can't be the camera node, as its
        /// transform is overwritten every frame with head pose data. But the camera should be an attached
        /// descendant of this node.
        /// </summary>
        public Transform AdjustmentFrame { get; set; }

        /// <summary>
        /// The camera parent node defines the "spongy frame of reference". All raw head based data,
        /// such as the spatial mapping, gesture events, and XR head pose data, are relative to this
        /// transform.
        /// </summary>
        public Transform CameraParent { get; set; }

        /// <summary>
        /// Whether the system is currently active and stabilizing space.
        /// </summary>
        public bool Enabled => shared.settings.Enabled;

        /// <summary>
        /// Automatically trigger a fragment merge whenever the FrozenWorld engine indicates that
        /// one would be appropriate.
        /// </summary>
        public bool AutoMerge => shared.settings.AutoMerge;

        /// <summary>
        /// Automatically trigger a refreeze whenever the FrozenWorld engine indicates that
        /// one would be appropriate.
        /// </summary>
        public bool AutoRefreeze => shared.settings.AutoRefreeze;

        /// <summary>
        /// Automatically load the WorldLocking state from disk at startup.
        /// </summary>
        public bool AutoLoad => shared.settings.AutoLoad;

        /// <summary>
        /// Periodically save the WorldLocking state to disk.
        /// </summary>
        public bool AutoSave => shared.settings.AutoSave;

        /// <summary>
        /// Apply the computed adjustment via the AdjustmentFrame transform.
        /// </summary>
        /// <remarks>
        /// If ApplyAdjustment is false, then WLT does the same computations, but it is up to the application to apply the computed transforms
        /// correctly, either in the camera hierarchy, or elsewhere in the scene hierarchy.
        /// </remarks>
        public bool ApplyAdjustment => shared.linkageSettings.ApplyAdjustment;

        /// <summary>
        /// Zero out pitch and roll from FrozenWorldEngine's computed correction.
        /// </summary>
        /// <remarks>
        /// This does not affect pitch and roll from the AlignmentManager (SpacePins).
        /// </remarks>
        public bool NoPitchAndRoll => shared.linkageSettings.NoPitchAndRoll;

        /// <summary>
        /// Direct interface to the plugin. It is not generally necessary or desired to 
        /// directly manipulate the plugin, but may be useful for manual override
        /// of some plugin inputs, outputs, or controls.
        /// </summary>
        public readonly IPlugin Plugin;

        private IHeadPoseTracker headPoseTracker = null;

        private IAnchorManager anchorManager;

        /// <summary>
        /// Interface to the Anchor Manager. 
        /// </summary>
        public IAnchorManager AnchorManager => anchorManager;

        private readonly IFragmentManager fragmentManager;

        /// <summary>
        /// Interface to the fragment manager.
        /// </summary>
        public IFragmentManager FragmentManager => fragmentManager;

        private readonly IAttachmentPointManager attachmentPointManager;

        /// <summary>
        /// Interface to the attachment point manager. Use for creating and manipulating attachment points.
        /// </summary>
        public IAttachmentPointManager AttachmentPointManager => attachmentPointManager;

        private IAlignmentManager alignmentManager;

        public IAlignmentManager AlignmentManager => alignmentManager;

        /// <summary>
        /// Indicator for the FrozenWorld engine internal heuristics of whether a merge should be performed
        /// </summary>
        public bool MergeIndicated => Plugin.Metrics.RefitMergeIndicated;

        /// <summary>
        /// Indicator for the FrozenWorld engine internal heuristics of whether a refreeze should be performed
        /// </summary>
        public bool RefreezeIndicated => Plugin.Metrics.RefitRefreezeIndicated;

        /// <summary>
        /// The current error status of the WorldLockingManager
        /// </summary>
        /// <remark>
        /// This rudimentary ad-hoc error reporting mechanism works nicely to inform expert users about problems.
        /// The flexibility is helpful in the early phase of the project. Once the set of possible errors is known and
        /// stable, a more systematic approach for error reporting should be found.
        ///
        /// It is important to note that this is intended to communicate an error *state* that persists for some time,
        /// not a one-time error *event* as it would be reported by a standard exception mechanism.
        /// </remark>
        public string ErrorStatus { get; private set; } = "";

        /// <summary>
        /// Transform from spongy space to frozen space. Spongy space is that native to
        /// XR interfaces. Frozen is Unity's global coordinate space. 
        /// Transform includes the WorldLocking adjustment to the camera, as
        /// well as any other transforms applied to the camera (e.g. teleport).
        /// </summary>
        public Pose FrozenFromSpongy { get { return FrozenFromLocked.Multiply(LockedFromSpongy); } }

        /// <summary>
        /// Transform from frozen space to XR native spongy space, including other
        /// transforms accumulated in the camera's ancestors (e.g. teleport).
        /// </summary>
        public Pose SpongyFromFrozen { get { return FrozenFromSpongy.Inverse(); } }

        public Pose LockedFromSpongy { get { return LockedFromPlayspace.Multiply(PlayspaceFromSpongy); } }

        public Pose SpongyFromLocked { get { return LockedFromSpongy.Inverse(); } }

        public Pose FrozenFromLocked { get { return FrozenFromPinned.Multiply(PinnedFromLocked); } }

        public Pose LockedFromFrozen { get { return FrozenFromLocked.Inverse(); } }

        /// <summary>
        /// Any application applied transform above the adjustment node.
        /// </summary>
        public Pose FrozenFromPinned
        {
            get
            {
                if (AdjustmentFrame != null && AdjustmentFrame.parent != null)
                {
                    return AdjustmentFrame.parent.transform.GetGlobalPose();
                }
                return Pose.identity;
            }
        }

        /// <summary>
        /// Transform from application's frozen space back to space computed by WorldLocking.
        /// </summary>
        public Pose PinnedFromFrozen
        {
            get { return FrozenFromPinned.Inverse(); }
        }

        /// <summary>
        /// Transform from the world locked space computed by WorldLocking to the space pinned in place.
        /// </summary>
        public Pose PinnedFromLocked { get; set; } = Pose.identity;

        /// <summary>
        /// From pinned space back to the world-locked space.
        /// </summary>
        public Pose LockedFromPinned { get { return PinnedFromLocked.Inverse(); } }

        /// <summary>
        /// Adjustment transform to world-lock the coordinate space.
        /// </summary>
        public Pose LockedFromPlayspace { get; set; } = Pose.identity;

        /// <summary>
        /// Inverse of adjustment transform to world-lock the coordinate space.
        /// </summary>
        public Pose PlayspaceFromLocked { get { return LockedFromPlayspace.Inverse(); } }

        /// <summary>
        /// Transform applied by (optional) camera parent node (e.g. for teleport).
        /// </summary>
        public Pose PlayspaceFromSpongy
        {
            get
            {
                if (CameraParent != null)
                {
                    return CameraParent.GetLocalPose();
                }
                return Pose.identity;
            }
        }

        /// <summary>
        /// Inverse of transform applied by (optional) camera parent node (e.g. for teleport).
        /// </summary>
        public Pose SpongyFromPlayspace { get { return PlayspaceFromSpongy.Inverse(); } }

        /// <summary>
        ///  The camera transform (parent from camera).
        /// </summary>
        public Pose SpongyFromCamera { get; set; } = Pose.identity;

        /// <summary>
        ///  Inverse of the camera transform (camera from parent).
        /// </summary>
        public Pose CameraFromSpongy { get { return SpongyFromCamera.Inverse(); } }

        /// <summary>
        /// Whether the manager is currently asynchronously loading or saving state.
        /// </summary>
        /// <remarks>
        /// Any attempt to manually initiate a Save or Load while HasPendingIO is true will quietly fail.
        /// </remarks>
        public bool HasPendingIO { get { return hasPendingLoadTask || hasPendingSaveTask; } }

        /// <summary>
        /// Filename at which to Save subsequent FrozenWorldEngine state to, and from which to Load it.
        /// </summary>
        /// <remarks>
        /// Some error checking for common mistakes is made, but some common sense should prevail.
        /// Use valid, normal filenames.
        /// A subpath may be introduced, but the entire path must be relative.
        /// Some examples:
        /// Good: 'myfile.myext', 'mypath/myfile.myext'
        /// Bad: null, '/myfile.myext'
        /// The actual final full path name used will be off of Application.persistentDataPath, which is platform dependent.
        /// </remarks>
        public string FrozenWorldFileName
        {
            get { return frozenWorldFile; }
            set
            {
                if (Path.IsPathRooted(value))
                {
                    Debug.LogWarning($"Invalid FrozenWorldFileName '{value}', must be relative path (no leading '/').");
                    value = Path.GetFileName(value);
                    Debug.LogWarning($"Reset input FrozenWorldFileName to '{value}'");
                }
                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogError("Invalid FrozenWorldFileName, null or empty not allowed, ignoring.");
                    return;
                }
                // Note that changing frozenWorldFile changes stateFileNameBase.
                frozenWorldFile = value;
                string stateFilePath = Path.GetDirectoryName(stateFileNameBase);
                if (!Directory.Exists(stateFilePath))
                {
                    Directory.CreateDirectory(stateFilePath);
                }
#if !WLT_DISABLE_LOGGING
                if (AutoSave)
                {
                    Debug.LogWarning($"Changing FrozenWorldFileName to '{frozenWorldFile}' with AutoSave enabled is risky, consider taking manual control over Save & Load.");
                }
#endif // !WLT_DISABLE_LOGGING
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// While loading, any WorldLocking updates are prohibited.
        /// </summary>
        private bool hasPendingLoadTask = false;

        /// <summary>
        /// While saving, WorldLocking updates are still possible, only Load/Save are prohibited.
        /// A simple bool used as locking mechanism for load/save tasks happening in a background task.
        /// </summary>
        private bool hasPendingSaveTask = false;

        /// <summary>
        /// Keep track of whether one-time initializations have been performed yet.
        /// </summary>
        private enum InitializationState { Uninitialized, Starting, Running };
        private InitializationState initializationState = InitializationState.Uninitialized;

        /// <summary>
        /// A handle of the class offering the optional feature of periodically logging the FrozenWorld engine state to disk
        /// </summary>
        private Diagnostics DiagnosticRecordings = new Diagnostics();

        /// <summary>
        /// The manager instance.
        /// </summary>
        private static WorldLockingManager managerInstance = null;

        /// <summary>
        /// GameObject to hold the proxy which supplies the update kick every frame.
        /// </summary>
        private static GameObject updateProxyNode = null;

        #endregion

        #region Update proxy

        /// <summary>
        /// Internal component to pass Unity Update pass on to the WorldLockingManager.
        /// Note that as a subclass, it will not be exposed for users to add in Unity editor.
        /// </summary>
        private class UpdateProxy : MonoBehaviour
        {
            /// <summary>
            /// Create an update proxy with 
            /// </summary>
            /// <returns>GameObject wrapper for the update proxy.</returns>
            public static GameObject CreateUpdateWrapper()
            {
                GameObject go = new GameObject("WorldLockingUpdater");
                UpdateProxy updater = go.AddComponent<UpdateProxy>();
                GameObject.DontDestroyOnLoad(go);
                return go;
            }

            /// <summary>
            /// Pass the update on to the manager. 
            /// </summary>
            private void Update()
            {
                var manager = WorldLockingManager.GetInstance();
                if (manager != null)
                {
                    manager.Update();
                }
            }
        }

        /// <summary>
        /// Create update proxy node if it hasn't already been created.
        /// </summary>
        private void CreateUpdaterNode()
        {
            if (updateProxyNode == null)
            {
                updateProxyNode = UpdateProxy.CreateUpdateWrapper();
            }
        }

        #endregion Update proxy

        #region Startup and settings refresh

        [System.Diagnostics.Conditional("WLT_LOG_SETUP")]
        private static void DebugLogSetup(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// Start using shared settings from given context.
        /// </summary>
        /// <param name="context">The context supplying the new shared settings.</param>
        public void SetContext(WorldLockingContext context)
        {
            shared = context.SharedSettings;
            DiagnosticRecordings.SharedSettings = context.DiagnosticsSettings;

            if (initializationState == InitializationState.Uninitialized)
            {
                ResetAnchorManager();
            }

            ApplyNewSettings();

            DebugLogSetup($"Context {context.name} set, Adjustment={(AdjustmentFrame == null ? "Null" : AdjustmentFrame.name)}");
        }

        /// <summary>
        /// Perform any initialization only appropriate once. This is called after
        /// giving the caller a chance to change settings.
        /// </summary>
        public async void ResetAnchorManager()
        {
            initializationState = InitializationState.Starting;

            if (anchorManager != null)
            {
                anchorManager.Dispose();
            }
            anchorManager = await SelectAnchorManager(Plugin, headPoseTracker);

            if (AutoLoad)
            {
                Load();
            }
            else
            {
                Reset();
            }

            initializationState = InitializationState.Running;
        }

        private async Task<IAnchorManager> SelectAnchorManager(IPlugin plugin, IHeadPoseTracker headTracker)
        {
#if false
            DebugLogSetup($"Select {shared.anchorSettings.anchorSubsystem} anchor manager.");
            if (AnchorManager != null)
            {
                DebugLogSetup("Creating new anchor manager, but have old one. Reseting it before replacing.");
                AnchorManager.Reset();
            }
            var anchorSettings = shared.anchorSettings;
#else
            if (AnchorManager != null)
            {
                DebugLogSetup("Creating new anchor manager, but have old one. Reseting it before replacing.");
                AnchorManager.Reset();
            }
            var anchorSettings = shared.anchorSettings;
#if UNITY_EDITOR
            if (anchorSettings.NullSubsystemInEditor)
            {
                DebugLogSetup($"Switching from {anchorSettings.anchorSubsystem} to AnchorSubsystem.Null because running in editor.");
                anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.Null;
            }
#endif // UNITY_EDITOR
            DebugLogSetup($"Select {anchorSettings.anchorSubsystem} anchor manager.");
#endif
#if WLT_ARFOUNDATION_PRESENT
            if (anchorSettings.anchorSubsystem == AnchorSettings.AnchorSubsystem.ARFoundation)
            {
                DebugLogSetup($"Trying to create ARF anchor manager on {anchorSettings.ARSessionSource.name} and {anchorSettings.XROriginSource.name}");
                AnchorManagerARF arfAnchorManager = await AnchorManagerARF.TryCreate(plugin, headTracker,
                    anchorSettings.ARSessionSource, anchorSettings.XROriginSource);
                if (arfAnchorManager != null)
                {
                    DebugLogSetup("Success creating ARF anchor manager");
                    return arfAnchorManager;
                }
                Debug.LogError("Failed to create requested AR Foundation anchor manager!");
            }
#endif // WLT_ARFOUNDATION_PRESENT
#if WLT_ARSUBSYSTEMS_PRESENT
            if (anchorSettings.anchorSubsystem == AnchorSettings.AnchorSubsystem.XRSDK)
            {
                DebugLogSetup($"Trying to create XR anchor manager");
                AnchorManagerXR xrAnchorManager = await AnchorManagerXR.TryCreate(plugin, headTracker);
                if (xrAnchorManager != null)
                {
                    DebugLogSetup("Success creating XR anchor manager");
                    return xrAnchorManager;
                }
                Debug.LogError("Failed to create requested XR SDK anchor manager!");
            }
#endif // WLT_ARSUBSYSTEMS_PRESENT
#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
            if (anchorSettings.anchorSubsystem == AnchorSettings.AnchorSubsystem.WSA)
            {
                AnchorManagerWSA wsaAnchorManager = AnchorManagerWSA.TryCreate(plugin, headTracker);
                if (wsaAnchorManager != null)
                {
                    DebugLogSetup("Success creating WSA anchor manager");
                    return wsaAnchorManager;
                }
                Debug.LogError("Failed to create requested WSA anchor manager!");
            }
#endif // UNITY_WSA
#if WLT_ARCORE_SDK_INCLUDED
            if (anchorSettings.anchorSubsystem == AnchorSettings.AnchorSubsystem.ARCore)
            {
                AnchorManagerARCore arCoreAnchorManager = AnchorManagerARCore.TryCreate(plugin, headTracker);
                if (arCoreAnchorManager != null)
                {
                    DebugLogSetup("Success creating ARCore anchor manager");
                    return arCoreAnchorManager;
                }
                Debug.LogError("Failed to create requested ARCore anchor manager!");
            }
#endif // WLT_ARCORE_SDK_INCLUDED
            if (anchorSettings.anchorSubsystem != AnchorSettings.AnchorSubsystem.Null)
            {
                DebugLogSetup("Failure creating useful anchor manager of any type. Creating null manager");
                anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.Null;
                shared.anchorSettings = anchorSettings;
            }
            AnchorManagerNull nullAnchorManager = AnchorManagerNull.TryCreate(plugin, headTracker);
            Debug.Assert(nullAnchorManager != null, "Creation of Null anchor manager should never fail.");
            /// No-op await here to suppress warnings if no anchor manager system which requires asynchronous startup is compiled in.
            await Task.CompletedTask;
            return nullAnchorManager;
        }

        /// <summary>
        /// Push the current anchor maintenance settings to the AnchorManager.
        /// </summary>
        private void ApplyAnchorSettings()
        {
            if (!shared.anchorSettings.IsValid)
            {
                Debug.LogError("Invalid anchor management settings detected!");
            }
            AnchorManager.MinNewAnchorDistance = shared.anchorSettings.MinNewAnchorDistance;
            AnchorManager.MaxAnchorEdgeLength = shared.anchorSettings.MaxAnchorEdgeLength;
            AnchorManager.MaxLocalAnchors = shared.anchorSettings.MaxLocalAnchors;
        }

        /// <summary>
        /// Make sure any new settings have a chance to be applied. 
        /// </summary>
        private void ApplyNewSettings()
        {
            ApplyAnchorSettings();

            if (!shared.linkageSettings.UseExisting)
            {
                CameraParent = shared.linkageSettings.CameraParent;
                AdjustmentFrame = shared.linkageSettings.AdjustmentFrame;
            }
            bool useDefaultFrame = false;
            if (CameraParent == null)
            {
                if (Camera.main != null)
                {
                    string parentName = Camera.main.transform.parent != null ? Camera.main.transform.parent.name : "null";
                    Debug.LogWarning($"No camera parent set on WorldLockingManager, using parent {parentName} of scene's main camera.");
                    CameraParent = Camera.main.transform.parent;
                }
                else if (ApplyAdjustment)
                {
                    Debug.LogError("No CameraParent set on WorldLockingManager, and no main camera to infer parent from.");
                }
                useDefaultFrame = true;
            }
            if (AdjustmentFrame == null)
            {
                if (CameraParent != null && CameraParent.parent != null)
                {
                    Debug.LogWarning($"No Adjustment Frame set on WorldLockingManager, using Transform {CameraParent.parent.gameObject.name} from scene's main camera hierarchy.");
                    AdjustmentFrame = CameraParent.parent;
                }
                else if (CameraParent != null)
                {
                    Debug.LogWarning($"No Adjustment Frame set on WorldLockingManager, using root Transform {CameraParent.transform.root.gameObject.name} from scene's main camera.");
                    AdjustmentFrame = CameraParent.transform.root;
                }
                else if (ApplyAdjustment)
                {
                    Debug.LogError("No Adjustment Frame set and no main camera to infer node from.");
                }
                useDefaultFrame = true;
            }
            Debug.Assert(AdjustmentFrame != Camera.main.transform, "ERROR: AdjustmentFrame can't be camera, adjustments will be overwritten. Add parent to camera");
            if (shared.linkageSettings.ApplyAdjustment)
            {
                if (useDefaultFrame && CameraParent == null)
                {
                    Debug.LogWarning($"Warning! Camera {Camera.main.gameObject.name} needs at least one parent for applying adjustments!");
                }
                if (AdjustmentFrame == CameraParent)
                {
                    Debug.LogWarning($"Warning! Camera needs at least parent and grandparent for teleport and manual camera movement to work.");
                }
            }
        }

        /// <summary>
        /// Update is called by the update proxy.
        /// </summary>
        private void Update()
        {
            ErrorStatus = "";

            if (initializationState != InitializationState.Running)
            {
                ErrorStatus = $"Init: F={Time.frameCount} - {initializationState}";
                return;
            }
            if (hasPendingLoadTask)
            {
                ErrorStatus = "pending background load task";
                return;
            }
            if (ApplyAdjustment && (AdjustmentFrame == null))
            {
                Debug.LogWarning($"F={Time.frameCount}: No WLM update because no adjustment frame set");
                ErrorStatus = "no adjustment frame";
                return;
            }

            // AnchorManager.Update takes care of creating anchors&edges and feeding the up-to-date state
            // into the FrozenWorld engine
            bool hasSpongyAnchors = AnchorManager.Update();

            if (!hasSpongyAnchors)
            {
                // IFragmentManager.Pause() will set all fragments to disconnected.
                ErrorStatus = AnchorManager.ErrorStatus;
                FragmentManager.Pause();
                return;
            }

            try
            {
                DiagnosticRecordings.Update();
            }
            catch (Exception exception)
            {
                Debug.LogErrorFormat("Error writing WorldLocking diagnostics record: {0}", exception);
            }

            // The basic output from the FrozenWorld engine (current fragment and its alignment)
            // are applied to the unity scene
            FragmentManager.Update(AutoRefreeze, AutoMerge);

            /// The following assumes a camera hierarchy like this:
            /// Nodes_A => AdjustmentFrame => Nodes_B => camera
            /// The cumulative effect of Nodes_B is to transform from Spongy space to playspace.
            /// Spongy space is the space that the camera moves about in, and is the space that
            /// coordinates coming from scene agnostic APIs like XR are in.
            /// (Note the MRTK APIs are in Unity's global space, not Spongy space.
            /// The internal structure of that graph is inconsequential here, the only dependency
            /// is on the cumulative transform, PlayspaceFromSpongy.
            /// Likewise, the cumulative effect of Nodes_A is to transform from alignment space (described below)
            /// to Unity's global space, referred to here as FrozenSpace.
            /// The AdjustmentFrame's transform is composed of two transforms. 
            /// The first comes from the FrozenWorld engine DLL as the inverse of Plugin.GetAlignment(), 
            /// and transforms from Playspace to the base stable world locked space, labeled as
            /// LockedFromPlayspace.
            /// The second transforms from this stable but arbitrary space to a space locked
            /// to a finite set of real world markers. This transform is labeled PinnedFromLocked.
            /// The transform chain equivalent of the above camera hierarchy is:
            /// FrozenFromPinned * [PinnedFromLocked * LockedFromPlayspace] * PlayspaceFromSpongy * SpongyFromCamera
            /// 
            /// FrozenFromSpongy and its inverse are useful for converting between the coordinates of scene agnostic APIs (e.g. XR)
            /// and Frozen coordinates, i.e. Unity's global space.
            /// FrozenFromLocked is convenient for converting between the "frozen" coordinates of the FrozenWorld engine DLL
            /// and Unity's global space, i.e. Frozen coordinate.
            if (Enabled)
            {
                Pose playspaceFromLocked = Plugin.GetAlignment();
                if (NoPitchAndRoll)
                {
                    playspaceFromLocked.rotation = Quaternion.Euler(0f, playspaceFromLocked.rotation.eulerAngles.y, 0f); // Zero out X and Z rotation from frozen world engine
                }
                LockedFromPlayspace = playspaceFromLocked.Inverse();

                SpongyFromCamera = Plugin.GetSpongyHead();

                Pose lockedHeadPose = LockedFromPlayspace.Multiply(PlayspaceFromSpongy.Multiply(SpongyFromCamera));
                alignmentManager.ComputePinnedPose(lockedHeadPose);
                PinnedFromLocked = alignmentManager.PinnedFromLocked;
            }
            else
            {
                SpongyFromCamera = Camera.main.transform.GetLocalPose();
                /// Note leave adjustment and pinning transforms alone, to facilitate
                /// comparison of behavior when toggling FW enabled.
            }

            if (AdjustmentFrame != null && ApplyAdjustment)
            {
                AdjustmentFrame.SetLocalPose(PinnedFromLocked.Multiply(LockedFromPlayspace));
            }

#if false && WLT_ARSUBSYSTEMS_PRESENT
            if ((AdjustmentFrame.GetGlobalPose().position != Vector3.zero) || (AdjustmentFrame.GetGlobalPose().rotation != Quaternion.identity))
            {
                Debug.Log($"WLT: Adj{AnchorManagerXR.DebugVector3("O=", AdjustmentFrame.GetGlobalPose().position)}, {AnchorManagerXR.DebugEuler("R=", AdjustmentFrame.GetGlobalPose().rotation.eulerAngles)}");
            }
#endif // WLT_ARSUBSYSTEMS_PRESENT

            AutoSaveTriggerHook();
        }

        private WorldLockingManager()
        {
            CreateUpdaterNode();

            /// It might look nicer to pull these off into internal setup functions,
            /// but by leaving them in the constructor, these fields can be marked "readonly", 
            /// which they conceptually are.
            if (Core.Plugin.HasEngine())
            {
                Plugin = new Plugin();
            }
            else
            {
                Plugin = new PluginNoop();
            }
            DiagnosticRecordings.Start(Plugin);

            headPoseTracker = new HeadPoseTrackerCamera();
            /// This should never fail. It's a null-manager.
            anchorManager = AnchorManagerNull.TryCreate(Plugin, headPoseTracker);
            Debug.Assert(anchorManager != null, "Null manager creation should never fail");

            var fm = new FragmentManager(Plugin);
            fragmentManager = fm;
            attachmentPointManager = fm;
            /// Note the alignmentManager accesses the FragmentManager in its constructor
            /// to register for refit notifications. Either FragmentManager needs to be fully
            /// setup before constructing AlignmentManager, or that registration needs to be deferred.
            alignmentManager = new AlignmentManager(this);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        ~WorldLockingManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        private void Dispose(bool disposing)
        {
            DiagnosticRecordings.Dispose();

            if (Plugin != null)
            {
                Plugin.Dispose();
            }
            if (updateProxyNode != null)
            {
                GameObject.Destroy(updateProxyNode);
                updateProxyNode = null;
            }
        }

        #endregion

        #region Public APIs

        /// <summary>
        /// Get the WorldLockingManager instance. This may be called at any time in program execution, 
        /// but if called during load its settings may not have been loaded from a new scene yet.
        /// </summary>
        /// <returns>The WorldLockingManager</returns>
        public static WorldLockingManager GetInstance()
        {
            if (managerInstance == null)
            {
                managerInstance = new WorldLockingManager();
            }
            return managerInstance;
        }

        /// <summary>
        /// Bring WorldLocking to a well-defined, empty state
        /// </summary>
        public void Reset()
        {
            AnchorManager.Reset();
            FragmentManager.Reset();
            AlignmentManager.ClearAlignmentAnchors();
            AlignmentManager.SendAlignmentAnchors();

            Plugin.ClearFrozenAnchors();
            Plugin.ResetAlignment(Pose.identity);
        }

        /// <summary>
        /// Manually trigger a save operation for the WorldLocking state
        /// </summary>
        public void Save()
        {
            WrapErrors(saveAsync());
        }

        /// <summary>
        /// Manually trigger a load operation for the WorldLocking state
        /// </summary>
        public void Load()
        {
            WrapErrors(loadAsync());
        }

        #endregion

        #region Load and Save

        private string frozenWorldFile = "frozenWorldState.hkfw";

        private string stateFileNameBase => $"{Application.persistentDataPath}/{frozenWorldFile}";

        private float lastSavingTime = float.NegativeInfinity;

        private const float AutoSaveInterval = 10f;

        /// <summary>
        /// Save WorldLocking state in a background task
        /// </summary>
        private async Task saveAsync()
        {
            if (HasPendingIO)
            {
                return;
            }

            hasPendingSaveTask = true;

            try
            {
                // mafinc - future work might include making an incremental save,
                // appending deltas, rather than new file every time.
                if (File.Exists(stateFileNameBase + ".new"))
                {
                    File.Delete(stateFileNameBase + ".new");
                }

                await AnchorManager.SaveAnchors();

                if (AnchorManager.SupportsPersistence)
                {
                    alignmentManager.Save();

                    using (var file = File.Create(stateFileNameBase + ".new"))
                    {
                        using (var ps = Plugin.CreateSerializer())
                        {
                            ps.IncludePersistent = true;
                            ps.IncludeTransient = false;
                            ps.GatherRecord();
                            await ps.WriteRecordToAsync(file);
                        }
                    }

                    if (File.Exists(stateFileNameBase + ".old"))
                    {
                        File.Delete(stateFileNameBase + ".old");
                    }
                    if (File.Exists(stateFileNameBase))
                    {
                        File.Move(stateFileNameBase, stateFileNameBase + ".old");
                    }
                    File.Move(stateFileNameBase + ".new", stateFileNameBase);

                    lastSavingTime = Time.unscaledTime;
                }
            }
            finally
            {
                hasPendingSaveTask = false;
            }
        }

        /// <summary>
        /// Load the WorldLocking state in a background task
        /// </summary>
        private async Task loadAsync()
        {
            if (HasPendingIO)
            {
                return;
            }

            hasPendingLoadTask = true;

            try
            {
                // reset in any case to guarantee clean state even if no files have been read successfully
                Reset();

                string[] tryFileNames = { stateFileNameBase, stateFileNameBase + ".old" };

                foreach (var fileName in tryFileNames)
                {
                    if (File.Exists(fileName))
                    {
                        using (var file = File.OpenRead(fileName))
                        {
                            using (var pds = Plugin.CreateDeserializer())
                            {
                                pds.IncludePersistent = true;
                                pds.IncludeTransient = false;
                                await pds.ReadRecordFromAsync(file);
                                pds.ApplyRecord();
                            }
                        }
                        await AnchorManager.LoadAnchors();
                        if (AnchorManager.SupportsPersistence)
                        {
                            AlignmentManager.Load();
                        }

                        // finish when reading was successful
                        return;
                    }
                }
            }
            finally
            {
                hasPendingLoadTask = false;
            }
        }

        /// <summary>
        /// Periodically trigger an auto-save in the background
        /// </summary>
        private void AutoSaveTriggerHook()
        {
            if (AnchorManager.SupportsPersistence)
            {
                /// Persistence currently only supported on HoloLens
                if (AutoSave && Time.unscaledTime >= lastSavingTime + AutoSaveInterval)
                {
                    WrapErrors(saveAsync());
                }
            }
        }

        /// <summary>
        /// Explicitly wrap an Async function to be called from sync code
        /// for details, see http://www.stevevermeulen.com/index.php/2017/09/using-async-await-in-unity3d-2017/
        /// </summary>
        /// <param name="task">return value from an async routine</param>
        private static async void WrapErrors(Task task)
        {
            await task;
        }

        #endregion Load and Save

    }
}