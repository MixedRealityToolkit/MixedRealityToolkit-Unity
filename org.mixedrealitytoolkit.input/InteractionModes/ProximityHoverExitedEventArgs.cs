// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverExitedEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityHoverExitedEventArgs.
        /// </summary>
        /// <param name="interactable">Interactable associated with the Proximity-Hover Exited event.</param>
        public ProximityHoverExitedEventArgs(IXRProximityInteractable interactable) : base(interactable)
        {
            //Empty on purpose
        }
    }
}
