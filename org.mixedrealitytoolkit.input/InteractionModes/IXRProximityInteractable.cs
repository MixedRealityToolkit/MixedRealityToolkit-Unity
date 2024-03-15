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
        /// Registers the Detector as triggering proximity.
        /// </summary>
        /// <param name="args">ProximityEnteredEventArgs that has the Detector triggering the proximity event.</param>
        void OnProximityEntered(ProximityEnteredEventArgs args);

        /// <summary>
        /// Unregisters the Detector as triggering proximity.
        /// </summary>
        /// <param name="args">ProximityExitedEventArgs that has the Detector triggering the proximity event.</param>
        void OnProximityExited(ProximityExitedEventArgs args);
    }
}
