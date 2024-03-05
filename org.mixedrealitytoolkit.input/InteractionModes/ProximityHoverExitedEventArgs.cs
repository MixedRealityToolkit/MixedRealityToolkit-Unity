// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverExitedEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityExitedArgs.
        /// </summary>
        /// <param name="collider">Collider associated with the Proximity-Hover Exited event.</param>
        /// <param name="interactable">Interactable associated with the Proximity-Hover Exited event.</param>
        public ProximityHoverExitedEventArgs(Collider collider, IXRProximityInteractable interactable) : base(collider, interactable)
        {
            //Empty on purpose
        }
    }
}
