// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Represents an interactor whose parent pose is backed by a tracked input device.
    /// </summary>
    public interface ITrackedInteractor
    {
        /// <summary>
        /// Get the interactor's parent whose pose is backed by a tracked input device.
        /// </summary>
        public GameObject TrackedParent { get; }
    }
}
