// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Abstract base class from which all manipulation specialization behaviors derive.
    /// </summary>
    /// <seealso cref="IManipulationSpecialization"/>
    public abstract class BaseManipulationSpecialization : MonoBehaviour, IManipulationSpecialization
    {
        /// <inheritdoc />
        public abstract bool CanProcessSelection(List<IXRSelectInteractor> interactors,
                                                 IXRSelectInteractable interactable);

        /// <inheritdoc />
        public virtual void OnSelectManipulationStarted(List<IXRSelectInteractor> interactors,
                                                        IXRSelectInteractable interactable,
                                                        Transform objectTransform,
                                                        Rigidbody rigidBody)
        { }

        /// <inheritdoc />
        public void OnSelectionChanged(List<IXRSelectInteractor> interactors,
                                       IXRSelectInteractable interactable,
                                       Transform objectTransform,
                                       Rigidbody rigidBody)
        { }

        /// <inheritdoc />
        public virtual void OnSelectManipulationEnded(List<IXRSelectInteractor> interactors,
                                                      IXRSelectInteractable interactable,
                                                      Transform objectTransform,
                                                      Rigidbody rigidBody)
        { }

        /// <inheritdoc />
        public abstract void Process(XRInteractionUpdateOrder.UpdatePhase updatePhase,
                                     List<IXRSelectInteractor> interactors,
                                     IXRSelectInteractable interactable,
                                     Transform objectTransform,
                                     Rigidbody rigidBody);
    }
}
