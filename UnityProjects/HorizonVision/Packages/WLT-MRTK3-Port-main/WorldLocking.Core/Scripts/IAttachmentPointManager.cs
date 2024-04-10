// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Interface for application creation and manipulation of attachment points. In particular, the creation and release
    /// of attachment points must be conducted through the IAttachmentPointManager.
    /// </summary>
    /// <remarks>
    /// Obtain access to the attachment point manager through the WorldLockingManager.
    /// </remarks>
    public interface IAttachmentPointManager
    {
        /// <summary>
        /// Create and register a new attachment point.
        /// </summary>
        /// <remarks>
        /// The attachment point itself is a fairly opaque handle. Its effects are propagated to the client via the
        /// two handlers associated with it.
        /// The context interface is optional. It should be given if the new attachment point is conceptually
        /// spawned from an existing attachment point (or its target object).
        /// If null, then conceptually the new attachment point was spawned from the current camera.
        /// The attachment point itself is a fairly opaque handle. The actual adjustments are made via
        /// notifications through the two delegates passed into the creation.
        /// The locationHandler is strictly to notify of adjustments when refitting (Merge or Refreeze).
        /// The stateHandler notifies whether this attachment point is "connected" with the current fragment.
        /// Both handlers are optional and may be null.
        /// </remarks>
        /// <param name="frozenPosition">The position in the frozen space at which to start the attachment point</param>
        /// <param name="context">The optional context into which to create the attachment point (may be null)</param>
        /// <param name="locationHandler">Delegate to handle Frozen World engine system adjustments to position</param>
        /// <param name="stateHandler">Delegate to handle Frozen World engine connectivity changes</param>
        /// <returns>The new attachment point interface.</returns>
        IAttachmentPoint CreateAttachmentPoint(Vector3 frozenPosition, IAttachmentPoint context,
                AdjustLocationDelegate locationHandler, AdjustStateDelegate stateHandler);

        /// <summary>
        /// Release an attachment point for disposal. The attachment point is no longer valid after this call.
        /// This also un-registers the handlers (if any) given when it was created.
        /// </summary>
        /// <remarks>
        /// In the unlikely circumstance that another attachment point has been spawned from this one
        /// but has not yet been processed (is still in the pending queue),
        /// that relationship is broken on release of this one, and when the other attachment point is
        /// finally processed, it will be as if it was created with a null context.
        /// </remarks>
        /// <param name="attachPointIface">The attachment point to release.</param>
        void ReleaseAttachmentPoint(IAttachmentPoint attachPointIface);

        /// <summary>
        /// Move (as opposed to Teleport) means that the object is meant to have traversed 
        /// frozen space from its old position to the given new position on some continuous path.
        /// </summary>
        /// <remarks>
        /// Not to be used for automatic (i.e. FrozenWorld Engine instigated) moves.
        /// Use this for continuous movement through space. For discontinuous movement (i.e. teleportation), use <see cref="TeleportAttachmentPoint"/>
        /// </remarks>
        /// <param name="attachPoint">Attachment point to move</param>
        /// <param name="newFrozenPosition">The new position in frozen space</param>
        void MoveAttachmentPoint(IAttachmentPoint attachPointIface, Vector3 newFrozenPosition);

        /// <summary>
        /// Teleport (as opposed to Move) means that the object is meant to have disappeared at its old position 
        /// and instantaneously reappeared at its new position in frozen space without traversing the space in between.
        /// </summary>
        /// <remarks>
        /// Use this for discontinuous movement through space (i.e. teleportation). For continuous movement, use <see cref="MoveAttachmentPoint"/>.
        /// This is equivalent to releasing the attachment point (<see cref="ReleaseAttachmentPoint"/>) and creating it (<see cref="CreateAttachmentPoint"/>) 
        /// at the new location in the given context, except that using Teleport allows the reference to the existing attachment point to remains valid.
        /// </remarks>
        /// <param name="attachPointIface">The attachment point to teleport</param>
        /// <param name="newFrozenPosition">The position to teleport to.</param>
        /// <param name="context">The optional context.</param>
        void TeleportAttachmentPoint(IAttachmentPoint attachPointIface, Vector3 newFrozenPosition, IAttachmentPoint context);

    }
}