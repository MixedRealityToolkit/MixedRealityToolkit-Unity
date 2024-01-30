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
    /// The interface is needed to prevent a circular reference between MRTK Input and MRTK UX Core Scripts packages.
    /// </remarks>
    public interface INearInteractionModeTarget
    {
        /// <summary>
        /// Sets the enable state of the Components that are in the ProximityEnabledComponents array (set in Editor).
        /// </summary>
        /// <param name="enable">True to enable the components and False otherwise.</param>
        public void SetEnabledDynamicComponents(bool enable);

        /// <summary>
        /// Registers the duple Collider + XRBaseInteractor as triggering proximity.
        /// </summary>
        /// <param name="collider">Collider triggering proximity.</param>
        /// <param name="xrBaseInteractor">Interactor triggering proximity.</param>
        public void RegisterActiveColliderWithInteractor(Collider collider, XRBaseInteractor xrBaseInteractor);

        /// <summary>
        /// Unregisters the duple Collider + XRBaseInteractor as triggering proximity.
        /// </summary>
        /// <param name="collider">Collider that in combination with the interactor was triggering proximity.</param>
        /// <param name="xrBaseInteractor">Interactor that in combination with the collider was triggering proximity.</param>
        public void UnregisterActiveColliderWithInteractor(Collider collider, XRBaseInteractor xrBaseInteractor);

        /// <summary>
        /// The number of Collider + XRBaseInteractor duples that have been registered as triggering proximity.
        /// </summary>
        public int ActiveColliderWithInteractorCount { get; }
    }
}
