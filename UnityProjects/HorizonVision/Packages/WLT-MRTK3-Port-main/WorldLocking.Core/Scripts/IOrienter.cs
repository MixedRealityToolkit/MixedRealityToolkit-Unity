// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Base class for a thing whose orientation can be inferred 
    /// from the positions of a collection of IOrientables.
    /// </summary>
    public interface IOrientable
    {
        /// <summary>
        /// What fragment the object belongs in. Objects in different fragments don't affect each other.
        /// </summary>
        FragmentId FragmentId { get; }

        /// <summary>
        /// The position of the object in Modeling space.
        /// </summary>
        Vector3 ModelPosition { get; }

        /// <summary>
        /// The orientation of the object in Modeling space.
        /// </summary>
        Quaternion ModelRotation { get; }

        /// <summary>
        /// The desired position of the object in world locked space.
        /// </summary>
        Vector3 LockedPosition { get; }

        /// <summary>
        /// The desired rotation of the object in world locked space.
        /// </summary>
        Quaternion LockedRotation { get; }

        /// <summary>
        /// Accept a rotation computed externally (by an <see cref="IOrienter"/>).
        /// </summary>
        /// <param name="mgr">The Alignment Manager in charge.</param>
        /// <param name="lockedRotation">The rotation to apply, in world locked space.</param>
        void PushRotation(IAlignmentManager mgr, Quaternion lockedRotation);
    }

    /// <summary>
    /// An object capable of computing self-consistent rotations for IOrientables based on their positions.
    /// </summary>
    public interface IOrienter
    {
        /// <summary>
        /// Optional subtree alignment manager. 
        /// </summary>
        /// <remarks>
        /// If unset, will use global alignment manager, ie WorldLockingManager.GetInstance().AlignmentManager.
        /// </remarks>
        IAlignmentManager AlignmentManager { get; set; }

        /// <summary>
        /// Add this orientable to the list to be both source of rotation computation, and targets to apply the computed rotation.
        /// </summary>
        /// <param name="orientable">The object to start maintining the orientation of.</param>
        void Register(IOrientable orientable);

        /// <summary>
        /// Stop managing orientation for this object, and release all references to it.
        /// </summary>
        /// <param name="orientable">The object to forget.</param>
        void Unregister(IOrientable orientable);

        /// <summary>
        /// Compute a consistent orientation for all registered IOrientables in the given fragment.
        /// </summary>
        /// <param name="fragmentId">The fragment to selectively apply to.</param>
        /// <param name="mgr">The manager governing the process.</param>
        void Reorient(FragmentId fragmentId, IAlignmentManager mgr);

    }
}