// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Implementation of the IAttachmentPoint interface. Provides implementations, as well
    /// as a binding to the update delegates.
    /// </summary>
    public class AttachmentPoint : IAttachmentPoint
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public AnchorId AnchorId { get; private set; }

        /// <inheritdoc />
        public FragmentId FragmentId { get; private set; }

        /// <inheritdoc />
        public Vector3 LocationFromAnchor { get; set; } = Vector3.zero;

        /// <inheritdoc />
        public Vector3 CachedPosition { get; set; }

        /// <inheritdoc />
        public AttachmentPointStateType State { get; set; }

        /// <inheritdoc />
        public Pose ObjectAdjustment { get; set; }

        /// <inheritdoc />
        public Vector3 ObjectPosition { get; set; }

        /// <summary>
        /// Handler for system positional adjustments. May be null
        /// </summary>
        public AdjustLocationDelegate LocationHandler { get; private set; }

        /// <summary>
        /// Handler for system connectivity adjustments. May be null.
        /// </summary>
        public AdjustStateDelegate StateHandler { get; private set; }

        /// <summary>
        /// Constructor, sets handlers
        /// </summary>
        /// <param name="locationHandler">Handler for positional adjustments, may be null.</param>
        /// <param name="stateHandler">Handler for connectivity adjustments, may be null.</param>
        public AttachmentPoint(AdjustLocationDelegate locationHandler, AdjustStateDelegate stateHandler)
        {
            this.LocationHandler = locationHandler;
            this.StateHandler = stateHandler;
        }

        /// <inheritdoc />
        public void MoveTo(IAttachmentPointManager manager, Vector3 newFrozenPosition)
        {
            manager.MoveAttachmentPoint(this, newFrozenPosition);
        }

        /// <inheritdoc />
        public void TeleportTo(IAttachmentPointManager manager, Vector3 newFrozenPosition, IAttachmentPoint parent)
        {
            manager.TeleportAttachmentPoint(this, newFrozenPosition, parent);
        }

        /// <summary>
        /// If state has changed, record the new state and pass on to client handler (if any).
        /// </summary>
        /// <param name="newState">The state to change to.</param>
        public void HandleStateChange(AttachmentPointStateType newState)
        {
            if (newState != State)
            {
                State = newState;
                StateHandler?.Invoke(newState);
            }
        }

        /// <summary>
        /// Keep track of cumulative transform adjustment, and pass on to client adjustment handler (if any).
        /// </summary>
        /// <remarks>
        /// See <see cref="IAttachmentPoint.ObjectAdjustment"/> and <see cref="AdjustLocationDelegate"/>
        /// </remarks>
        /// <param name="adjustment"></param>
        public void HandlePoseAdjustment(Pose adjustment)
        {
            ObjectPosition = adjustment.Multiply(ObjectPosition);
            ObjectAdjustment = ObjectAdjustment.Multiply(adjustment);
            LocationHandler?.Invoke(adjustment);
        }

        /// <summary>
        /// Set internals of attachment point to new values.
        /// </summary>
        /// <param name="fragmentId">New fragment</param>
        /// <param name="cachedPosition">Cache last position moved to.</param>
        /// <param name="anchorId">New anchor id</param>
        /// <param name="locationFromAnchor">New displacement from anchor</param>
        public void Set(FragmentId fragmentId, Vector3 cachedPosition, AnchorId anchorId, Vector3 locationFromAnchor)
        {
            this.AnchorId = anchorId;
            this.FragmentId = fragmentId;
            this.CachedPosition = cachedPosition;
            this.LocationFromAnchor = locationFromAnchor;
        }

    }

}