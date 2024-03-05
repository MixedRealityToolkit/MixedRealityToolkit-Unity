// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity-hover events triggered by a Collider and an Interactable combo.
    /// </summary>
    public abstract class BaseProximityHoverEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for BaseProximityEventArgs.
        /// </summary>
        /// <param name="collider">Collider that triggers proximity event.</param>
        /// <param name="interactable">IXRProximityInteractable that triggers proximity-hover event.</param>
        public BaseProximityHoverEventArgs(Collider collider, IXRProximityInteractable interactable)
        {
            Collider = collider;
            Interactable = interactable;
        }

        /// <summary>
        /// The collider associated with the proximity-hover interaction event.
        /// </summary>
        public Collider Collider { get; private set; }

        /// <summary>
        /// The interactable associated with the proximity-hover interaction event.
        /// </summary>
        public IXRProximityInteractable Interactable { get; private set; }
    }
}
