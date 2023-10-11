// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// An interface that allows for specialized manipulation of an <see cref="ObjectManipulator"/>
    /// under certain circumstances, such as when being selected by an <see cref="XRSocketInteractor"/>,
    /// that may call for manipulation behavior that is distinct from the default.
    /// </summary>
    /// <remarks>
    /// When active, the manipulation specialization is responsible for complete
    /// manipulation details, including final position, rotation, and scale of the object
    /// along with any smoothing and handling of the <see cref="Rigidbody"/> when present.
    /// A <see cref="BaseManipulationSpecialization"/> is provided for convenience, but implementers
    /// can use this interface directly as well.
    /// </remarks>
    /// <seealso cref="ObjectManipulator"/>
    public interface IManipulationSpecialization
    {
        /// <summary>
        /// Whether this manipulation specialization is valid for and can process the
        /// current state of the object with selecting interactors.
        /// </summary>
        /// <param name="interactors">
        /// List of all <see cref="IXRSelectInteractor"/> objects selecting this object.
        ///</param>
        /// <param name= "interactable">
        /// The <see cref="IXRSelectInteractable"/> that is being manipulated.
        /// <remarks>
        /// The list of attached specialization components will be queried in order during
        /// <see cref="ObjectManipulator.OnSelectEntered(SelectEnterEventArgs)"/> and the first to
        /// respond with <c>true</c> will be used as the active manipulation specialization.
        /// Otherwise, if no specializations respond, the default object manipulation will be used.
        /// </remarks>
        public bool CanProcessSelection(List<IXRSelectInteractor> interactors,
                                        IXRSelectInteractable interactable);

        /// <summary>
        /// Called when select manipulation is starting with the active specialization.
        /// Use this to do any code initialization needed for the object.
        /// </summary>
        /// <param name="interactors">
        /// List of all <see cref="IXRSelectInteractor"/> objects selecting this object.
        ///</param>
        /// <param name= "interactable">
        /// The <see cref="IXRSelectInteractable"/> that is being manipulated.
        /// </param>
        /// <param name="objectTransform">The transform of the object to be manipulated.</param>
        /// <param name="rigidBody">The RigidBody of the object if present.  Can be null.</param>
        public void OnSelectManipulationStarted(List<IXRSelectInteractor> interactors,
                                                IXRSelectInteractable interactable,
                                                Transform objectTransform,
                                                Rigidbody rigidBody);

        /// <summary>
        /// Called on the active manipulation specialization when the number of selections
        /// has changed on the object.  The specialization remains active.
        /// </summary>
        /// <param name="interactors">
        /// List of all <see cref="IXRSelectInteractor"/> objects selecting this object.
        ///</param>
        /// <param name= "interactable">
        /// The <see cref="IXRSelectInteractable"/> that is being manipulated.
        /// </param>
        /// <param name="objectTransform">The transform of the object to be manipulated.</param>
        /// <param name="rigidBody">The RigidBody of the object if present.  Can be null.</param>
        public void OnSelectionChanged(List<IXRSelectInteractor> interactors,
                                       IXRSelectInteractable interactable,
                                       Transform objectTransform,
                                       Rigidbody rigidBody);

        /// <summary>
        /// Called when the select manipulation is ending for the specialization, either
        /// during continued selection and control is going back to default manipulation
        /// behavior or to another specialization, or when releasing the object with
        /// <c>0</c> selecting interactors.
        /// </summary>
        /// <param name="interactors">
        /// List of all <see cref="IXRSelectInteractor"/> objects selecting this object.
        ///</param>
        /// <param name= "interactable">
        /// The <see cref="IXRSelectInteractable"/> that is being manipulated.
        /// </param>
        /// <param name="objectTransform">The transform of the object to be manipulated.</param>
        /// <param name="rigidBody">The RigidBody of the object if present.  Can be null.</param>
        /// <remarks>
        /// Use this to restore any modifications made to the object by this specialization during
        /// setup or to handle the object being released if still active when the selecting
        /// interactors count has reached <c>0</c>.
        /// </remarks>
        public void OnSelectManipulationEnded(List<IXRSelectInteractor> interactors,
                                              IXRSelectInteractable interactable,
                                              Transform objectTransform,
                                              Rigidbody rigidBody);

        /// <summary>
        /// Called on the active specialization for the object in order to perform the manipulation.
        /// </summary>
        /// <param name="updatePhase">
        /// The update phase this is called during.
        /// </param>
        /// <param name="interactors">
        /// List of all <see cref="IXRSelectInteractor"/> objects selecting this object.
        ///</param>
        /// <param name= "interactable">
        /// The <see cref="IXRSelectInteractable"/> that is being manipulated.
        /// </param>
        /// <param name="objectTransform">The transform of the object to be manipulated.</param>
        /// <param name="rigidBody">The RigidBody of the object if present.  Can be null.</param>
        public void Process(XRInteractionUpdateOrder.UpdatePhase updatePhase,
                            List<IXRSelectInteractor> interactors,
                            IXRSelectInteractable interactable,
                            Transform objectTransform,
                            Rigidbody rigidBody);
    }
}
