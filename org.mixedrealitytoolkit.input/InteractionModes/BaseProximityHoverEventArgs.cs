// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity-hover events triggered by an Interactable.
    /// </summary>
    public abstract class BaseProximityHoverEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for BaseProximityHoverEventArgs.
        /// </summary>
        /// <param name="interactable">IXRProximityInteractable that triggers proximity-hover event.</param>
        public BaseProximityHoverEventArgs(IXRProximityInteractable interactable)
        {
            Interactable = interactable;
        }

        /// <summary>
        /// The interactable associated with the proximity-hover interaction event.
        /// </summary>
        public IXRProximityInteractable Interactable { get; private set; }
    }
}
