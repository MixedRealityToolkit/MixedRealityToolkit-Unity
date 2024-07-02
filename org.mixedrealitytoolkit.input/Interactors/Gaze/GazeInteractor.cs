// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// An XRRayInteractor that enables eye gaze for focus and interaction.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Gaze Interactor")]
    public class GazeInteractor :
        XRRayInteractor,
        IGazeInteractor,
        IModeManagedInteractor
    {
        [SerializeField]
        [Tooltip("The root management GameObject that interactor belongs to.")]
        private GameObject modeManagedRoot = null;

        /// <summary>
        /// Returns the GameObject that this interactor belongs to. This GameObject is governed by the
        /// interaction mode manager and is assigned an interaction mode. This GameObject represents the group that this interactor belongs to.
        /// </summary>
        /// <remarks>
        /// This will default to the GameObject that this attached to a parent <see cref="TrackedPoseDriver"/>.
        /// </remarks>
        public GameObject ModeManagedRoot
        {
            get => modeManagedRoot;
            set => modeManagedRoot = value;
        }

        /// <inheritdoc/>
        [Obsolete("This function is obsolete and will be removed in the next major release. Use ModeManagedRoot instead.")]
        public GameObject GetModeManagedController()
        {
            // Legacy controller-based interactors should return null, so the legacy controller-based logic in the
            // interaction mode manager is used instead.
#pragma warning disable CS0618 // forceDeprecatedInput is obsolete
            if (forceDeprecatedInput)
            {
                return null;
            }
#pragma warning restore CS0618 // forceDeprecatedInput is obsolete

            return ModeManagedRoot;
        }
    }
}
