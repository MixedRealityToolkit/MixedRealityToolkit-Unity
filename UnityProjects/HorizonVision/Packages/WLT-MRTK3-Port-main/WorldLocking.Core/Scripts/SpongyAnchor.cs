// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity spatial anchors, facilitating creation and persistence.
    /// </summary>
    public abstract class SpongyAnchor : MonoBehaviour
    {

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public abstract bool IsLocated { get; }

        /// <summary>
        /// Return the anchor's pose in spongy space.
        /// </summary>
        public abstract Pose SpongyPose { get; }

        /// <summary>
        /// Whether the underlying spatial anchor is known to be in the local anchor store.
        /// </summary>
        /// <remarks>
        /// Note that the anchor might be in the anchor store but isn't known to be, so IsSaved  == false.
        /// In particular, a different anchor might be stored under the same name, in which case saving
        /// this anchor probably requires deleting the old anchor first.
        /// </remarks>
        public virtual bool IsSaved { get; set; } = false;

        /// <summary>
        /// Diagnostic only - to be removed.
        /// </summary>
        public virtual Vector3 Delta { get; set; }
    }
}
