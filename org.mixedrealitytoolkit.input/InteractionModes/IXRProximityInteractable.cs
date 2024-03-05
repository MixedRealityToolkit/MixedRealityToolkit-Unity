// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This interface is used to update the enable state of the Components that are in the <see cref="ProximityEnabledComponents"/>
    /// array (set in Editor) and to keep track of which <see cref="Collider"/> and <see cref="XRBaseInteractor"/> duples are triggering proximity.
    /// </summary>
    /// <remarks>
    /// This interface is needed to prevent a circular reference between MRTK Input and MRTK UX Core Scripts packages.
    /// </remarks>
    public interface IXRProximityInteractable
    {
        /// <summary>
        /// Registers the interactable as triggering hover-proximity.
        /// </summary>
        /// <param name="args">ProximityHoverEnteredEventArgs that has the Interactable triggering the proximity-hover event.</param>
        void OnProximityEntered(ProximityHoverEnteredEventArgs args);

        /// <summary>
        /// Unregisters the interactable as triggering hover-proximity.
        /// </summary>
        /// <param name="args">ProximityHoverExitedEventArgs that has the Interactable triggering the proximity-hover event.</param>
        void OnProximityExited(ProximityHoverExitedEventArgs args);
    }
}
