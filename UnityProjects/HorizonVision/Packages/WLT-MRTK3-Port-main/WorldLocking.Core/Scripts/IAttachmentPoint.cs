// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// The states an attachment point can be in.
    /// </summary>
    public enum AttachmentPointStateType
    {
        Invalid = 0,    // Doesn't exist
        Pending,        // Exists, but is still under construction
        Normal,         // Exists, and is active and valid
        Unconnected,    // Exists, but is disconnected from the active fragment. Location data unreliable.
        Released,       // Existed, but has been released. Is now garbage.
    }

    /// <summary>
    /// Notification from the system that the state of the fragment containing the attachment point has changed.
    /// The client can take action to hide objects in disconnected space if desired.
    /// </summary>
    /// <param name="state">The new state of the containing fragment</param>
    public delegate void AdjustStateDelegate(AttachmentPointStateType state);

    /// <summary>
    /// Notification that a correction in the world locked space has been computed and
    /// should be applied to this object.
    /// </summary>
    /// <param name="adjustment">The adjustment to apply</param>
    public delegate void AdjustLocationDelegate(Pose adjustment);

    /// <summary>
    /// Opaque handle to an attachment point. Create one of these to enable
    /// WorldLocking to adjust an attached object as corrections to the world locked space 
    /// optimization are made. 
    /// </summary>
    /// <remarks>
    /// The attachment point gives an interface for notifying the system that you have moved
    /// the attached object, and the system indicates that it has computed an adjustment
    /// for the object through the callbacks passed into the creation routine.
    /// Alternatively, polling is also supported through the State and ObjectAdjustment accessors.
    /// </remarks>
    public interface IAttachmentPoint
    {
        /// <summary>
        /// Name is auto-populated on create with something unique, but can
        /// be renamed to anything useful and convenient. It is only used as
        /// a label, so can be anything (including empty or null).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Associated anchor id
        /// </summary>
        AnchorId AnchorId { get; }

        /// <summary>
        /// Associated fragment id
        /// </summary>
        FragmentId FragmentId { get; }

        /// <summary>
        /// Position of attachment point in anchor point's space.
        /// </summary>
        Vector3 LocationFromAnchor { get; }

        /// <summary>
        /// Internal history cache.
        /// </summary>
        Vector3 CachedPosition { get; }

        /// <summary>
        /// Current state of this attachment point. 
        /// </summary>
        /// <remarks>
        /// Positioning information is only valid when state is Normal. See <see cref="AttachmentPointStateType"/>
        /// </remarks>
        AttachmentPointStateType State { get; }

        /// <summary>
        /// The position of object(s) bound to this attachment point.
        /// </summary>
        Vector3 ObjectPosition { get; }

        /// <summary>
        /// Cumulative transform adjustment for object(s) bound to this attachment point.
        /// </summary>
        Pose ObjectAdjustment { get; }

        /// <summary>
        /// Notify attachment point that it has moved incrementally to a new position.
        /// </summary>
        /// <remarks>
        /// This should be used for conceptually continuous motion. For discontinuous motion (i.e. teleport),
        /// use <see cref="TeleportTo"/>. This is equivalent to <see cref="IAttachmentPointManager.MoveAttachmentPoint"/>
        /// </remarks>
        /// <param name="manager">The mananger</param>
        /// <param name="newFrozenPosition">The new position</param>
        void MoveTo(IAttachmentPointManager manager, Vector3 newFrozenPosition);

        /// <summary>
        /// Notify attachment point that it has teleported to a new position.
        /// </summary>
        /// <remarks>
        /// This should be used for discontinuous movement, i.e. teleporting. For continuous motion, use <see cref="MoveTo"/>.
        /// This is equivalent to <see cref="IAttachmentPointManager.TeleportAttachmentPoint"/>
        /// </remarks>
        /// <param name="manager">The manager</param>
        /// <param name="newFrozenPosition">The new position</param>
        /// <param name="context">The context into which to teleport. Can be null. <see cref="IAttachmentPointManager.CreateAttachmentPoint"/></param>
        void TeleportTo(IAttachmentPointManager manager, Vector3 newFrozenPosition, IAttachmentPoint context);
    }
}
