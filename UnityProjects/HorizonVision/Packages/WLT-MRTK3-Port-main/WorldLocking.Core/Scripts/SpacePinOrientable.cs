// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// A component derived from <see cref="SpacePin"/> which differs only in that,
    /// rather than using an explicit rotation passed in, an implicit rotation is calculated
    /// based on the relative positions of all active <see cref="SpacePinOrientable"/>s.
    /// </summary>
    /// <remarks>
    /// The implementation of <see cref="IOrientable"/> allows it to provide an input position
    /// and receive an output rotation from the managing <see cref="IOrienter"/>.
    /// 
    /// </remarks>
    public class SpacePinOrientable : SpacePin, IOrientable
    {
        #region Internal members
        [SerializeField]
        [Tooltip("Input dependency of the managing Orienter which will arbitrate individual rotations.")]
        private Orienter orienter = null;

        /// <summary>
        /// Reference to managing interface.
        /// </summary>
        /// <remarks>
        /// If it is non-null, the reference set in the Inspector for the Orienter field will be used.
        /// It may be overridden at any time using the property accessor. 
        /// </remarks>
        private IOrienter iorienter = null;

        /// <summary>
        /// Input dependency of the managing Orienter which will arbitrate individual rotations.
        /// </summary>
        /// <remarks>
        /// Access to the orienter is strictly by interface IOrienter. The type of the orienter member is Orienter
        /// to allow it to be set explicitly in the Inspector (see notes in <see cref="Orienter"/>), but any
        /// object implementing IOrienter can be used by explicit setting through <see cref="SetOrienter(IOrienter)"/>. 
        /// </remarks>
        public IOrienter Orienter { get { return iorienter; } set { SetOrienter(value); } }

        #endregion Internal members

        #region IOrientable implmentation

        /// <summary>
        /// The fragment this belongs to. Public property to satisfy IOrientable interface.
        /// </summary>
        /// <remarks>
        /// Only elements within the same fragment are allowed to interact with each other, because by definition
        /// the relationship between coordinates of elements of differing fragments are undefined.
        /// </remarks>
        public new FragmentId FragmentId
        {
            get
            {
                return base.FragmentId;
            }
        }

        /// <inheritdocs />
        public Vector3 ModelPosition { get { return ModelingPoseGlobal.position; } }

        /// <inheritdocs />
        public Quaternion ModelRotation { get { return ModelingPoseGlobal.rotation; } }

        /// <inheritdocs />
        public Vector3 LockedPosition { get { return LockedPose.position; } }

        /// <inheritdocs />
        public Quaternion LockedRotation { get { return LockedPose.rotation; } }

        /// <summary>
        /// Accept the rotation as computed by the IOrienter.
        /// </summary>
        /// <param name="mgr">The alignment manager which needs to receive the updated Pose.</param>
        /// <param name="lockedRotation">The new world locked rotation to adopt.</param>
        public void PushRotation(IAlignmentManager mgr, Quaternion lockedRotation)
        {
            //Debug.Log($"PushRotation {name}: mgr={(mgr == WorldLockingManager.GetInstance().AlignmentManager ? "global" : "local")}");
            /// Append the modeling pose rotation. This will cancel out when computing the 
            /// pinnedFromLocked transform, so that the computed rotation gets applied as is.
            LockedPose = new Pose(LockedPose.position, lockedRotation * ModelingPoseGlobal.rotation);
            PushAlignmentData(mgr);
        }

        #endregion IOrientable implmentation

        #region Public APIs added
        /// <summary>
        /// Set the position in frozen space. Rotation not needed since it is computed based on relative positions.
        /// </summary>
        /// <param name="frozenPosition">Position in frozen space.</param>
        public void SetFrozenPosition(Vector3 frozenPosition)
        {
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Vector3 lockedPosition = wltMgr.LockedFromFrozen.Multiply(frozenPosition);
            SetLockedPose(new Pose(lockedPosition, LockedRotation));
        }

        /// <summary>
        /// Set the position in spongy space. Rotation not needed since it is computed based on relative positions.
        /// </summary>
        /// <param name="spongyPosition">Position in spongyt space.</param>
        public void SetSpongyPosition(Vector3 spongyPosition)
        {
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Vector3 lockedPosition = wltMgr.LockedFromSpongy.Multiply(spongyPosition);
            SetLockedPose(new Pose(lockedPosition, LockedRotation));
        }

        /// <summary>
        /// Set the position in world locked space. Rotation not needed since it is computed based on relative positions.
        /// </summary>
        /// <param name="lockedPosition">Position in locked space.</param>
        public void SetLockedPosition(Vector3 lockedPosition)
        {
            SetLockedPose(new Pose(lockedPosition, LockedRotation));
        }

        #endregion Public APIs added

        #region SpacePin overrides

        /// <summary>
        /// Adopt the Inspector set Orienter as the interface iorienter.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (orienter != null)
            {
                iorienter = orienter;
            }
        }

        /// <summary>
        /// Override of base SetLockedPose to allow insertion of the computation of rotation.
        /// </summary>
        /// <param name="lockedPose">The new pose in world locked space.</param>
        /// <remarks>
        /// Note that base class implementation is not invoked here, but rather this override 
        /// performs the same steps but with additional computations (for the rotation)
        /// interleaved.
        /// </remarks>
        public override void SetLockedPose(Pose lockedPose)
        {
            this.LockedPose = lockedPose;

            /// World locked space pose is meaningless outside the context of the current fragment.
            /// Record that fragment id now.
            /// Note that fragment id may change in the course of refit operations, but that's
            /// okay because the locked pose will be world locked then too.
            if (EnsureRegistered())
            {
                IAlignmentManager mgr = AlignmentManager;

                Debug.Assert(Orienter != null, "Registration with orienter should not succeed with null orienter.");
                Orienter.Reorient(FragmentId, mgr);

                SendAlignmentData(mgr);
            }

        }

        /// <summary>
        /// Reset and unregister from the IOrienter.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            EnsureUnregistered();
        }

        /// <summary>
        /// Callback for refit operations.
        /// </summary>
        /// <param name="adjustment">Adjustment transform to apply.</param>
        /// <remarks>
        /// Note that the FragmentId may change here. 
        /// </remarks>
        protected override void OnLocationUpdate(Pose adjustment)
        {
            base.OnLocationUpdate(adjustment);
        }

        /// <summary>
        /// If base restore on load succeeds, register with orienter for further manipulation.
        /// </summary>
        protected override void RestoreOnLoad()
        {
            base.RestoreOnLoad();
            if (PinActive)
            {
                EnsureRegistered();
            }
        }

        #endregion SpacePin overrides

        #region IOrienter access

        /// <summary>
        /// Register with the IOrienter if there is one.
        /// </summary>
        /// <returns>True if registered.</returns>
        private bool EnsureRegistered()
        {
            if (Orienter == null)
            {
                return false;
            }

            /// Check that the AttachmentPoint is made. It's FragmentId is
            /// used in isolating which other orientables to infer orientation by.
            ForceAttachment();
            Orienter.Register(this);
            return true;
        }

        /// <summary>
        /// Unregister from the IOrienter if registered.
        /// </summary>
        private void EnsureUnregistered()
        {
            if (Orienter != null)
            {
                Orienter.Unregister(this);
            }
        }

        /// <summary>
        /// Explicitly set the managing IOrienter, overriding any setting from the Inspector.
        /// </summary>
        /// <param name="iorenter"></param>
        /// <remarks>
        /// The Orienter is nominally a completely internal artifact. The public setter is to allow
        /// construction from script.
        /// </remarks>
        public void SetOrienter(IOrienter iorienter)
        {
            this.iorienter = iorienter;
            // If of an appropriate type, assign to the serialized orienter field as well.
            if (iorienter is Orienter)
            {
                orienter = iorienter as Orienter;
            }
        }
        #endregion IOrienter access
    }
}