// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_LOG_SAVE_LOAD
//#define WLT_EXTRA_LOGGING

#if WLT_DISABLE_LOGGING
#undef WLT_LOG_SAVE_LOAD
#undef WLT_EXTRA_LOGGING
#endif // WLT_DISABLE_LOGGING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Component helper for pinning the world locked space at a single reference point.
    /// </summary>
    /// <remarks>
    /// This component captures the initial pose of its gameObject, and then a second pose. It then
    /// adds that pair to the WorldLocking Alignment Manager. The manager then negotiates between all
    /// such added pins, based on the current head pose, to generate a frame-to-frame mapping aligning
    /// the Frozen Space, i.e. Unity's Global Space, such that the pins match up as well as possible.
    /// Another way to phrase this is:
    ///    Given an arbitrary pose (the "modeling pose"),
    ///    and a pose aligned somehow to the real world (the "world locked pose"),
    ///    apply a correction to the camera such that a virtual object with coordinates of the modeling pose
    ///    will appear overlaid on the real world at the position and orientation described by the locked pose.
    /// For this component, the locked pose must come in via one of the following three APIs:
    ///     <see cref="SetFrozenPose(Pose)"/> with input pose in Frozen Space, which includes pinning.
    ///     <see cref="SetSpongyPose(Pose)"/> with input pose in Spongy Space, which is the space of the camera's parent,
    ///         and is the same space the camera moves in, and that native APIs return values in (e.g. XR).
    ///     <see cref="SetLockedPose(Pose)"/> with input pose in Locked Space, which is the space stabilized by
    ///         the Frozen World engine DLL but excluding pinning.
    /// Note that since the Frozen Space is shifted by the AlignmentManager, calling SetFrozenPose(p) with the same Pose p
    /// twice is probably an error, since the Pose p would refer to different a location after the first call.
    /// </remarks>
    public class SpacePin : MonoBehaviour
    {
        #region Private members

        /// <summary>
        /// Choice of what to use for modeling position.
        /// </summary>
        /// <remarks>
        /// In general, the transform's global position is the preferred source of the model position.
        /// However, there are times when that is not practical. Specifically, if the model's transform has been "baked"
        /// into the model's vertices, leaving an identity transform, then while the transform's position is no longer
        /// meaningful, the renderer's world-space bounds and/or the collider's world-space bounds may still have a
        /// useful reference position.
        /// Also, it is very easy to offset the collider's bounds, when it might be more cumbersome to modify either
        /// the transform position or the renderer's bounds.
        /// Note that orientation **always** comes from transform, as renderer and collider bounds have no orientation.
        /// </remarks>
        public enum ModelPositionSourceEnum
        {
            Transform = 0,
            RendererBounds = 1,
            ColliderBounds = 2
        }

        [Tooltip("Where to find model space position on target. Transform is preferable, but if transforms are baked in, renderer or collider may be more appropriate.")]
        [SerializeField]
        private ModelPositionSourceEnum modelPositionSource = ModelPositionSourceEnum.Transform;

        /// <summary>
        /// Where to find model space position on target. Transform is preferable, but if transforms are baked in, renderer or collider may be more appropriate.
        /// </summary>
        /// <remarks>
        /// Note that orientation **always** comes from transform, as renderer and collider bounds have no orientation.
        /// </remarks>
        public ModelPositionSourceEnum ModelPositionSource { get { return modelPositionSource; } set { modelPositionSource = value; } }

        /// <summary>
        /// manager dependency is set exactly once in Start().
        /// </summary>
        private WorldLockingManager manager = null;

        /// <summary>
        /// Read only access to manager dependency from derived classes.
        /// </summary>
        protected WorldLockingManager Manager => manager;

        /// <summary>
        /// Overridable alignment manager. Defaults to WorldLockingManager.GetInstance().AlignmentManager;
        /// </summary>
        private IAlignmentManager alignmentManager = null;

        /// <summary>
        /// Accessor for overriding the AlignmentManager from script.
        /// </summary>
        public IAlignmentManager AlignmentManager
        {
            get { return alignmentManager; }
            set
            {
                if (alignmentManager != value)
                {
                    DebugLogSaveLoad($"Changing {name} pin's alignmentmanager {(value == WorldLockingManager.GetInstance().AlignmentManager ? "to global" : "from global")}");
                    Reset();
                    if (alignmentManager != null)
                    {
                        alignmentManager.UnregisterForLoad(RestoreOnLoad);
                    }
                    alignmentManager = value;
                    /// Register for post-loaded messages from the Alignment Manager.
                    /// When these come in check for the loading of the reference point
                    /// associated with this pin. Reference is by unique name.
                    alignmentManager.RegisterForLoad(RestoreOnLoad);
                }
            }
        }

        /// <summary>
        /// Unique identifier for the alignment data from this instance.
        /// </summary>
        private ulong ulAnchorId = (ulong)AnchorId.Unknown;

        /// <summary>
        /// This wrapper for the anchorId is because the anchorId has to be stored
        /// as a ulong, which is the base class for the AnchorId enum. Unity only
        /// supports int-based enums, so will complain on serialization etc. for
        /// the ulong based AnchorId.
        /// </summary>
        public AnchorId AnchorId
        {
            get { return (AnchorId)ulAnchorId; }
            private set { ulAnchorId = (ulong)value; }
        }

        /// <summary>
        /// Provide a unique anchor name. This is used for persistence.
        /// </summary>
        protected virtual string AnchorName { get { return gameObject.name + "SpacePin"; } }

        /// <summary>
        /// Whether this space pin is in active use pinning space
        /// </summary>
        public bool PinActive { get { return AnchorId.IsKnown(); } }

        /// <summary>
        /// modelingPoseLocal is the local Pose of the gameObject at startup.
        /// </summary>
        private Pose restorePoseLocal = Pose.identity;

        /// <summary>
        /// Pose to restore after manipulation (if any).
        /// </summary>
        protected Pose RestorePoseLocal
        {
            get { return restorePoseLocal; }
        }

        /// <summary>
        /// modelingPoseParent is the pose of the gameObject at startup (or after explicit capture with <see cref="ResetModelingPose"/> relative to its parent.
        /// </summary>
        private Pose modelingPoseParent = Pose.identity;

        /// <summary>
        /// First of the pair of poses submitted to alignment manager for alignment.
        /// </summary>
        public Pose ModelingPoseGlobal
        {
            get
            {
                Pose rescaledModelingPose = AddScale(modelingPoseParent, transform.lossyScale);
                return GlobalFromParent.Multiply(rescaledModelingPose);
            }
        }

        /// <summary>
        /// Second pose of pair submitted to alignment manager, always in Locked Space.
        /// </summary>
        private Pose lockedPose = Pose.identity;
        /// <summary>
        /// Accessor for world locked pose for derived classes.
        /// </summary>
        public Pose LockedPose
        {
            get { return lockedPose; }
            protected set { lockedPose = value; }
        }

        /// <summary>
        /// Return the Pose transforming from parent space to global space.
        /// </summary>
        /// <remarks>
        /// If the SpacePin has no parent, this will be the identity Pose.
        /// </remarks>
        protected Pose GlobalFromParent
        {
            get
            {
                Pose globalFromParent = Pose.identity;
                if (transform.parent != null)
                {
                    globalFromParent = transform.parent.GetGlobalPose();
                }
                return globalFromParent;
            }
        }

        /// <summary>
        /// Return the Pose transforming from global space to the parent's space.
        /// </summary>
        protected Pose ParentFromGlobal
        {
            get { return GlobalFromParent.Inverse(); }
        }


        /// <summary>
        /// Attachment point to react to refit operations.
        /// </summary>
        private IAttachmentPoint attachmentPoint = null;

        /// <summary>
        /// Id for fragment this pin belongs in.
        /// </summary>
        public FragmentId FragmentId
        {
            get
            {
                if (attachmentPoint != null)
                {
                    return attachmentPoint.FragmentId;
                }
                return FragmentId.Unknown;
            }
        }

        [System.Diagnostics.Conditional("WLT_LOG_SAVE_LOAD")]
        private void DebugLogSaveLoad(string message)
        {
            Debug.Log($"F={Time.frameCount} {message}");
        }

        [System.Diagnostics.Conditional("WLT_EXTRA_LOGGING")]
        private void DebugLogExtra(string message)
        {
            Debug.Log(message);
        }

        private void CheckDependencies()
        {
            /// Cache the WorldLockingManager as a dependency.
            manager = WorldLockingManager.GetInstance();

            if (AlignmentManager == null)
            {
                DebugLogSaveLoad($"Setting {name} pin's alignment manager to global because unset.");
                AlignmentManager = manager.AlignmentManager;
            }
        }
        #endregion Private members

        #region Unity members

        // Start is called before the first frame update
        protected virtual void Start()
        {
            /// Cache the initial pose.
            ResetModelingPose();

            CheckDependencies();
        }

        /// <summary>
        /// On destroy, unregister for the loaded event.
        /// </summary>
        protected virtual void OnDestroy()
        {
            AlignmentManager?.UnregisterForLoad(RestoreOnLoad);
        }

        #endregion Unity members

        #region Public APIs

        /// <summary>
        /// Transform pose to Locked Space and pass through.
        /// </summary>
        /// <param name="frozenPose">Pose in frozen space.</param>
        public void SetFrozenPose(Pose frozenPose)
        {
            SetLockedPose(manager.LockedFromFrozen.Multiply(frozenPose));
        }

        /// <summary>
        /// Transform pose to Locked Space and pass through.
        /// </summary>
        /// <param name="spongyPose">Pose in spongy space.</param>
        public void SetSpongyPose(Pose spongyPose)
        {
            SetLockedPose(manager.LockedFromSpongy.Multiply(spongyPose));
        }

        /// <summary>
        /// Record the locked pose and push data to the manager.
        /// </summary>
        /// <param name="lockedPose"></param>
        public virtual void SetLockedPose(Pose lockedPose)
        {
            this.lockedPose = lockedPose;

            DebugLogSaveLoad($"SetLockedPose {name}: mgr={(AlignmentManager == WorldLockingManager.GetInstance().AlignmentManager ? "global" : "local")}");

            PushAlignmentData(AlignmentManager);

            SendAlignmentData(AlignmentManager);
        }

        /// <summary>
        /// Reset the modeling pose to the current transform.
        /// </summary>
        /// <remarks>
        /// In normal usage, the modeling pose is the transform as set in Unity and as cached at start.
        /// In some circumstances, such as creation of pins from script, it may be convenient to set the 
        /// transform after Start(). In this case, the change of transform should be recorded by a
        /// call to ResetModelingPose().
        /// This must happen before the modeling pose is used implicitly by a call to set the 
        /// virtual pose, via SetFrozenPose, SetSpongyPose, or SetLockedPose.
        /// </remarks>
        public virtual void ResetModelingPose()
        {
            restorePoseLocal = transform.GetLocalPose();
            modelingPoseParent = ParentFromGlobal.Multiply(ExtractModelPose());
            // Undo any scale. This will be multiplied back in in ModelingPoseGlobal().
            modelingPoseParent = RemoveScale(modelingPoseParent, transform.lossyScale);
        }

        /// <summary>
        /// Go back to initial state, including removal of self-artifacts from alignment manager.
        /// </summary>
        public virtual void Reset()
        {
            if (PinActive)
            {
                AlignmentManager.RemoveAlignmentAnchor(AnchorId);
                AnchorId = AnchorId.Unknown;
                ReleaseAttachment();
                Debug.Assert(!PinActive);
                SendAlignmentData(AlignmentManager);
            }
        }
        #endregion Public APIs

        #region Internal

        #region Extract modelling pose

        protected Pose ExtractModelPose()
        {
            Pose modelPose = Pose.identity;
            switch (modelPositionSource)
            {
                case ModelPositionSourceEnum.Transform:
                    {
                        modelPose = ExtractModelPoseFromTransform();
                        DebugLogExtra($"Extracted pose from transform on {name}");
                    }
                    break;
                case ModelPositionSourceEnum.RendererBounds:
                    {
                        modelPose = ExtractModelPoseFromRenderer();
                        DebugLogExtra($"Extracted pose from renderer on {name}");
                    }
                    break;
                case ModelPositionSourceEnum.ColliderBounds:
                    {
                        modelPose = ExtractModelPoseFromCollider();
                        DebugLogExtra($"Extracted pose from collider on {name}");
                    }
                    break;
                default:
                    {
                        Debug.Assert(false, $"Unhandled model position source on {name}.");
                    }
                    break;
            }
            return modelPose;
        }
        protected Pose ExtractModelPoseFromTransform()
        {
            return transform.GetGlobalPose();
        }

        protected Pose GetModelPoseFromGlobalPosition(Vector3 globalPosition)
        {
            Pose modelPose = new Pose(globalPosition, transform.GetGlobalPose().rotation);

            return modelPose;
        }

        protected Pose ExtractModelPoseFromRenderer()
        {
            var rend = GetComponent<Renderer>();
            Debug.Assert(rend != null, $"Looking for Modeling pose on {name} renderer, but found no renderer.");
            return GetModelPoseFromGlobalPosition(rend.bounds.center);
        }

        protected Pose ExtractModelPoseFromCollider()
        {
            var collider = GetComponent<Collider>();
            Debug.Assert(collider != null, $"Looking for Modeling pose on {name} collider, but found no collider.");
            return GetModelPoseFromGlobalPosition(collider.bounds.center);
        }

        private static Pose RemoveScale(Pose pose, Vector3 scale)
        {
            Vector3 p = pose.position;
            p.Scale(new Vector3(1.0f / scale.x, 1.0f / scale.y, 1.0f / scale.z));
            pose.position = p;
            return pose;
        }

        private Pose AddScale(Pose pose, Vector3 scale)
        {
            Vector3 p = pose.position;
            p.Scale(scale);
            pose.position = p;
            return pose;
        }

        #endregion Extract modelling pose

        #region Alignment management

        /// <summary>
        /// Check if an attachment point is needed, if so then setup and make current.
        /// </summary>
        private void CheckAttachment()
        {
            if (!PinActive)
            {
                return;
            }
            ForceAttachment();
        }

        /// <summary>
        /// Ensure that there is an attachment, and it is positioned up to date.
        /// </summary>
        protected void ForceAttachment()
        {
            IAttachmentPointManager mgr = manager.AttachmentPointManager;
            if (attachmentPoint == null)
            {
                attachmentPoint = mgr.CreateAttachmentPoint(LockedPose.position, null, OnLocationUpdate, null);
            }
            else
            {
                mgr.TeleportAttachmentPoint(attachmentPoint, LockedPose.position, null);
            }
        }

        /// <summary>
        /// Dispose of any previously created attachment point.
        /// </summary>
        protected void ReleaseAttachment()
        {
            if (attachmentPoint != null)
            {
                manager.AttachmentPointManager.ReleaseAttachmentPoint(attachmentPoint);
                attachmentPoint = null;
            }
        }

        /// <summary>
        /// Callback for refit operations. Apply adjustment transform to locked pose.
        /// </summary>
        /// <param name="adjustment">Adjustment to apply.</param>
        protected virtual void OnLocationUpdate(Pose adjustment)
        {
            LockedPose = adjustment.Multiply(LockedPose);
        }

        /// <summary>
        /// Callback on notification of the alignment manager's database to check
        /// if this preset has been persisted, and restore it to operation if it has.
        /// </summary>
        protected virtual void RestoreOnLoad()
        {
            CheckDependencies();

            AnchorId = AlignmentManager.RestoreAlignmentAnchor(AnchorName, ModelingPoseGlobal);
            if (PinActive)
            {
                Pose restorePose;
                bool found = AlignmentManager.GetAlignmentPose(AnchorId, out restorePose);
                Debug.Assert(found);
                lockedPose = restorePose;
            }
            CheckAttachment();
        }

        /// <summary>
        /// Communicate the data from this point to the alignment manager.
        /// </summary>
        /// <param name="mgr"></param>
        protected void PushAlignmentData(IAlignmentManager mgr)
        {
            DebugLogExtra($"F{Time.frameCount} Push: {name}: MPG={ModelingPoseGlobal.ToString("F3")} GfP={GlobalFromParent.ToString("F3")} R={restorePoseLocal.ToString("F3")} MPP={WorldLockingManager.GetInstance().PinnedFromFrozen.Multiply(ModelingPoseGlobal)}");
            if (PinActive)
            {
                mgr.RemoveAlignmentAnchor(AnchorId);
            }
            AnchorId = mgr.AddAlignmentAnchor(AnchorName, ModelingPoseGlobal, lockedPose);
        }

        /// <summary>
        /// Notify the manager that all necessary updates have been submitted and
        /// are ready for processing.
        /// </summary>
        /// <param name="mgr"></param>
        protected void SendAlignmentData(IAlignmentManager mgr)
        {
            mgr.SendAlignmentAnchors();

            CheckAttachment();

            transform.SetLocalPose(RestorePoseLocal);
        }

        #endregion Alignment management

        #endregion Internal

    }
}