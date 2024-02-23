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
        /// <param name="interactor">Interactor associated with the Proximity Exited event.</param>
        public ProximityExitedEventArgs(Collider collider, XRBaseInteractor interactor) : base(collider, interactor)
        {
            //Empty on purpose
        }

        /// <summary>
        /// The Collider associated with the proximity exited event.
        /// </summary>
        public new Collider collider => base.Collider;

        /// <summary>
        /// The Interactor associated with the proximity exited event.
        /// </summary>
        public new XRBaseInteractor interactor => base.Interactor;
    }
}
