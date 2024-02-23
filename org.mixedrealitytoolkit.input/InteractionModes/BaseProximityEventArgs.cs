// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity events triggered by a Collider and an Interactor combo.
    /// </summary>
    public abstract partial class BaseProximityEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for BaseProximityEventArgs.
        /// </summary>
        /// <param name="sender">Source of event.</param>
        /// <param name="collider">Collider that triggers proximity event.</param>
        /// <param name="interactor">XRBaseInteractor that triggers proximity event.</param>
        public BaseProximityEventArgs(object sender, Collider collider, XRBaseInteractor interactor)
        {
            this.sender = sender;
            this.collider = collider;
            this.interactor = interactor;
        }

        /// <summary>
        /// The object that triggered the proximity event.
        /// </summary>
        public object sender { get; private set; }

        /// <summary>
        /// The collider associated with the interaction event.
        /// </summary>
        public Collider collider { get; private set; }

        /// <summary>
        /// The interactor associated with the interaction event.
        /// </summary>
        public XRBaseInteractor interactor { get; private set; }
    }
}
