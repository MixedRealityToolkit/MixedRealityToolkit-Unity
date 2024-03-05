// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverEnteredEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityHoverEnteredEventArgs.
        /// </summary>
        /// <param name="interactable">Interactable associated with the Proximity-Hover Entered event.</param>
        public ProximityHoverEnteredEventArgs(IXRProximityInteractable interactable) : base(interactable)
        {
            //Empty on purpose
        }
    }
}
