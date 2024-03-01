// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    public class ProximityExitedEventArgs : BaseProximityEventArgs
    {
        /// <summary>
        /// Constructor for ProximityExitedArgs.
        /// </summary>
        /// <param name="sender">Source of event.</param>
        /// <param name="collider">Collider associated with the Proximity Exited event.</param>
        /// <param name="interactable">Interactable associated with the Proximity Exited event.</param>
        public ProximityExitedEventArgs(Collider collider, XRBaseInteractable interactable) : base(collider, interactable)
        {
            //Empty on purpose
        }
    }
}
