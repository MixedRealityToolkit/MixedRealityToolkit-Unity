// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverEnteredEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityEnteredArgs.
        /// </summary>
        /// <param name="collider">Collider associated with the Proximity-Hover Entered event.</param>
        /// <param name="interactable">Interactable associated with the Proximity-Hover Entered event.</param>
        public ProximityHoverEnteredEventArgs(Collider collider, IXRProximityInteractable interactable) : base(collider, interactable)
        {
            //Empty on purpose
        }
    }
}
