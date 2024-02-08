// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.Events;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with an interaction event between an Interactor and Interactable.
    /// </summary>
    public abstract partial class BaseProximityHoverInteractionEventArgs
    {
        /// <summary>
        /// The Interactable associated with the interaction event.
        /// </summary>
        public IXRProximityInteractable proximityHoverInteractableObject { get; set; }
    }

    /// <summary>
    /// The event that is called only when the first Interactor begins a proximity hover
    /// over this Interactable. Subsequent Interactors that begin a proximity over this Interactable
    /// will not cause this event to be invoked as long as any others are still proximity hovering.
    /// </summary>
    [Serializable]
    public sealed class ProximityHoverEnterEvent : UnityEvent<ProximityHoverEnterEventArgs>
    {
        //Empty on purpose
    }

    /// <summary>
    /// Event data associated with the event when an Interactor initiates a proximity hover over an Interactable.
    /// </summary>
    public class ProximityHoverEnterEventArgs : BaseProximityHoverInteractionEventArgs
    {
        /// <summary>
        /// The Interactable associated with the interaction event.
        /// </summary>
        public new IXRProximityInteractable proximityHoverInteractableObject
        {
            get => base.proximityHoverInteractableObject;
            set => base.proximityHoverInteractableObject = value;
        }
    }

    /// <summary>
    /// The event that is called only when the last remaining proximity hovering Interactor
    /// ends proximity hovering over this Interactable.
    /// </summary>
    [Serializable]
    public sealed class ProximityHoverExitEvent : UnityEvent<ProximityHoverExitEventArgs>
    {
        //Empty on purpose
    }

    /// <summary>
    /// Event data associated with the event when an Interactor initiates a proximity hover over an Interactable.
    /// </summary>
    public class ProximityHoverExitEventArgs : BaseProximityHoverInteractionEventArgs
    {
        /// <summary>
        /// The Interactable associated with the interaction event.
        /// </summary>
        public new IXRProximityInteractable proximityHoverInteractableObject
        {
            get => base.proximityHoverInteractableObject;
            set => base.proximityHoverInteractableObject = value;
        }
    }
}
