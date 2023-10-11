// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// A manipulation specialization for an <see cref="ObjectManipulator"/> when being selected
    /// by a <see cref="XRSocketInteractor"/>.
    /// </summary>
    public class SocketManipulationSpecialization : BaseManipulationSpecialization
    {
        [SerializeField]
        [Tooltip("The amount of time to transition into the socket upon selection.  A value of 0 means immediate transition.")]
        [Range(0.0f, 10.0f)]
        private float socketTransitionTime = .125f;

        /// <summary>
        /// The amount of time to transition into the socket upon selection.
        /// A value of 0 means immediate transition.
        /// </summary>
        public float SocketTransitionTime
        {
            get => socketTransitionTime;
            set
            {
                socketTransitionTime = Mathf.Max(0.0f, value);
            }
        }

        [SerializeField]
        [Tooltip("The transition animation curve.")]
        private AnimationCurve socketTransitionCurve = AnimationCurve.EaseInOut(0,0,1,1);

        /// <summary>
        /// The transition animation curve.
        /// </summary>
        public AnimationCurve SocketTransitionCurve
        {
            get => socketTransitionCurve;
            set => socketTransitionCurve = value;
        }

        private bool wasGravity = false;
        private bool wasKinematic = false;
        private float transitionTimer = 0;
        private Vector3 startPosition = Vector3.zero;
        private Quaternion startRotation = Quaternion.identity;

        /// <inheritdoc />
        public override bool CanProcessSelection(List<IXRSelectInteractor> interactors,
                                                 IXRSelectInteractable interactable)
        {
            return isActiveAndEnabled &&
                   interactors.Count == 1 &&
                   interactors[0] is XRSocketInteractor;
        }

        /// <inheritdoc />
        public override void OnSelectManipulationStarted(List<IXRSelectInteractor> interactors,
                                                         IXRSelectInteractable interactable,
                                                         Transform objectTransform,
                                                         Rigidbody rigidBody)
        {
            base.OnSelectManipulationStarted(interactors, interactable, objectTransform, rigidBody);

            if (rigidBody != null)
            {
                wasGravity = rigidBody.useGravity;
                wasKinematic = rigidBody.isKinematic;

                rigidBody.useGravity = false;
                // Use kinematic movement to avoid any possible colliders near the socket.
                rigidBody.isKinematic = true;
            }

            // Set the original position/rotation and reset transition timer.
            startPosition = objectTransform.position;
            startRotation = objectTransform.rotation;
            transitionTimer = 0;
        }

        /// <inheritdoc />
        public override void OnSelectManipulationEnded(List<IXRSelectInteractor> interactors,
                                                       IXRSelectInteractable interactable,
                                                       Transform objectTransform,
                                                       Rigidbody rigidBody)
        {
            base.OnSelectManipulationEnded(interactors, interactable, objectTransform, rigidBody);

            if (rigidBody != null)
            {
                rigidBody.useGravity = wasGravity;
                rigidBody.isKinematic = wasKinematic;
            }
        }

        /// <inheritdoc />
        public override void Process(XRInteractionUpdateOrder.UpdatePhase updatePhase,
                                     List<IXRSelectInteractor> interactors,
                                     IXRSelectInteractable interactable,
                                     Transform objectTransform,
                                     Rigidbody rigidBody)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                var attachTransform = interactors[0].GetAttachTransform(interactable);
                if (socketTransitionTime > 0.0f && transitionTimer < socketTransitionTime)
                {
                    var normalizedTime = Mathf.InverseLerp(0, socketTransitionTime, transitionTimer);
                    var t = socketTransitionCurve.Evaluate(normalizedTime);
                    var position = Vector3.Lerp(startPosition, attachTransform.position, t);
                    var rotation = Quaternion.Slerp(startRotation, attachTransform.rotation, t);
                    objectTransform.SetPositionAndRotation(position, rotation);
                }
                else
                {
                    objectTransform.SetPositionAndRotation(attachTransform.position,
                                                           attachTransform.rotation);
                }

                transitionTimer += Time.deltaTime;
            }
        }
    }
}
