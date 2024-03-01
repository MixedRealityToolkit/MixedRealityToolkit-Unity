// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity events triggered by a Collider and an Interactable combo.
    /// </summary>
    public abstract class BaseProximityEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for BaseProximityEventArgs.
        /// </summary>
        /// <param name="sender">Source of event.</param>
        /// <param name="collider">Collider that triggers proximity event.</param>
        /// <param name="interactor">XRBaseInteractable that triggers proximity event.</param>
        public BaseProximityEventArgs(Collider collider, XRBaseInteractable interactable)
        {
            Collider = collider;
            Interactable = interactable;
        }

        /// <summary>
        /// The collider associated with the interaction event.
        /// </summary>
        public Collider Collider { get; private set; }

        /// <summary>
        /// The interactable associated with the interaction event.
        /// </summary>
        public XRBaseInteractable Interactable { get; private set; }
    }
}
