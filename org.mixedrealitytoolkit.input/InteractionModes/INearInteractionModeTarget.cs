// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This interface is used to update the front plate and rounded rect of a pressable
    /// button if they are flagged as dynamic (based on proximity), it is needed to prevent
    /// a circular reference between MRTK Input and MRTK UX Core Scripts packages.
    /// </summary>
    public interface INearInteractionModeTarget
    {
        /// <summary>
        /// Sets the enable state of the Front Plate and Rounded Rect if they were tagged as dynamic based on proximity.
        /// </summary>
        /// <param name="enable">True to enable the components and False otherwise.</param>
        public void UpdateFrontPlateAndRoundedRectIfDynamic(bool enable);

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
