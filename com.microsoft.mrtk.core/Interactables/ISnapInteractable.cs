// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Interactables that represent a handle-like affordance should implement this
    /// interface, such that interactors can snap their visuals/ray/etc directly to the
    /// affordance instead of using the typical local offset.
    /// </summary>
    public interface ISnapInteractable
    {
        /// <summary>
        /// Called by interactors to query which exact transform on an interactable
        /// should be considered the snappable affordance.
        /// </summary>
        /// <remarks>
        /// For example, sliders return the sliding handle transform.
        /// </remarks>
        Transform HandleTransform { get; }
    }
}