// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;


namespace MixedReality.Toolkit
{
    /// <summary>
    /// The abstract interaction events that all proxy interactors support.
    /// Proxy interactors are used to map foreign interaction systems (like UnityUI)
    /// onto XRI interaction primitives.
    /// </summary>
    /// <remarks>
    /// Generally, input shims will call these functions to request the proxy to
    /// hover/select/etc the object on which the shim is placed.
    /// </remarks>
    public interface IProxyInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor, UnityEngine.XR.Interaction.Toolkit.Interactors.IXRHoverInteractor
    {
        /// <summary>
        /// Begin hovering the interactable. The interactable will receive
        /// <c>OnHoverEntering</c> and <c>OnHoverEntered</c> events, and the proxy interactor will include it in
        /// its list of valid targets.
        /// </summary>
        void StartHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable interactable);

        /// <summary>
        /// Begin hovering the interactable. The interactable will receive
        /// <c>OnHoverEntering</c> and <c>OnHoverEntered</c> events, and the proxy interactor will include it in
        /// its list of valid targets. Also includes the worldPosition of the pointer.
        /// </summary>
        void StartHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable interactable, Vector3 worldPosition);

        /// <summary>
        /// End hovering the interactable. The interactable will receive
        /// <c>OnHoverExiting</c> and <c>OnHoverExited</c> events, and the proxy interactor will remove it from
        /// its list of valid targets.
        /// </summary>
        void EndHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable interactable);

        /// <summary>
        /// Begin selecting the interactable. The interactable will receive
        /// <c>OnSelectEntering</c> and <c>OnSelectEntered</c> events.
        /// </summary>
        void StartSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable);

        /// <summary>
        /// Begin selecting the interactable. The interactable will receive
        /// <c>OnSelectEntering</c> and <c>OnSelectEntered</c> events. Also includes the worldPosition of the pointer.
        /// </summary>
        void StartSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable, Vector3 worldPosition);

        /// <summary>
        /// Call to periodically update an in-progress selection. Typically
        /// used for drags; worldPosition specifies the world position of the pointer's drag.
        /// </summary>
        void UpdateSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable, Vector3 worldPosition);

        /// <summary>
        /// End selecting the interactable. The interactable will receive
        /// <c>OnSelectExiting</c> and <c>OnSelectExited</c> events. SuppressEvents will prevent StatefulInteractables
        /// from receiving click or toggle events.
        /// </summary>
        void EndSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable, bool suppressEvents = false);
    }
}
