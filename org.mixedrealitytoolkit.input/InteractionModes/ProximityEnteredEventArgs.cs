// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    public class ProximityEnteredEventArgs : BaseProximityEventArgs
    {
        /// <summary>
        /// Constructor for ProximityEnteredArgs.
        /// </summary>
        /// <param name="sender">Source of event.</param>
        /// <param name="collider">Collider associated with the Proximity Entered event.</param>
        /// <param name="interactor">Interactor associated with the Proximity Entered event.</param>
        public ProximityEnteredEventArgs(Collider collider, XRBaseInteractor interactor) : base(collider, interactor)
        {
            //Empty on purpose
        }
    }
}
