// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using PokePath = MixedReality.Toolkit.IPokeInteractor.PokePath;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// An interactor that is used for poking/pressing near interactions.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Poke Interactor")]
    public class PokeInteractor :
        XRBaseInputInteractor,
        IPokeInteractor,
        IHandedInteractor,
        IModeManagedInteractor,
        ITrackedInteractor
    {
        #region PokeInteractor

        [SerializeField, Tooltip("Holds a reference to the <see cref=\"TrackedPoseDriver\"/> associated to this interactor if it exists.")]
        private TrackedPoseDriver trackedPoseDriver = null;

        /// <summary>
        /// Holds a reference to the <see cref="TrackedPoseDriver"/> associated to this interactor if it exists.
        /// </summary>
        protected internal TrackedPoseDriver TrackedPoseDriver => trackedPoseDriver;

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

        [SerializeReference]
        [InterfaceSelector(true)]
        [Tooltip("The pose source representing the poke pose")]
        private IPoseSource pokePoseSource;

        /// <summary>
        /// The pose source representing the poke pose
        /// </summary>
        protected IPoseSource PokePoseSource { get => pokePoseSource; set => pokePoseSource = value; }

        /// <summary>
        /// Called during ProcessInteractor to obtain the poking pose. <see cref="XRBaseInteractor.attachTransform"/> is set to this pose.
        /// Override to customize how poses are calculated.
        /// </summary>
        protected virtual bool TryGetPokePose(out Pose pose)
        {
            pose = Pose.identity;
            return PokePoseSource != null && PokePoseSource.TryGetPose(out pose);
        }

        /// <summary>
        /// Called during ProcessInteractor to obtain the poking radius. All raycasts and other physics detections
        /// are done according to this radius. Override to customize how the radius is calculated.
        /// </summary>
        protected virtual bool TryGetPokeRadius(out float radius)
        {
            HandJointPose jointPose = default;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
            if (forceDeprecatedInput &&
                xrController is ArticulatedHandController handController &&
                (XRSubsystemHelpers.HandsAggregator?.TryGetNearInteractionPoint(handController.HandNode, out jointPose) ?? false))
            {
                radius = jointPose.Radius;
                return true;
            }
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
            else
            {
                if (XRSubsystemHelpers.HandsAggregator?.TryGetNearInteractionPoint(handedness.ToXRNode(), out jointPose) ?? false)
                {
                    radius = jointPose.Radius;
                    return true;
                }
            }

            radius = default;
            return false;
        }

        #endregion PokeInteractor

        #region IHandedInteractor

        /// <inheritdoc />
        Handedness IHandedInteractor.Handedness
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (forceDeprecatedInput)
                {
                    return (xrController is ArticulatedHandController handController) ? handController.HandNode.ToHandedness() : Handedness.None;
                }
#pragma warning restore CS0618 // Type or member is obsolete
                else
                {
                    return handedness.ToHandedness();
                }
            }
        }

        #endregion IHandedInteractor

        #region IPokeInteractor

        /// <summary>
        /// The default poke radius returned by <see cref="IPokeInteractor.PokeRadius"/>
        /// if no joint data is obtained.
        /// </summary>
        private const float DefaultPokeRadius = 0.005f;

        /// <inheritdoc />
        public virtual float PokeRadius => pokeRadius > 0 ? pokeRadius : DefaultPokeRadius;
        private float pokeRadius = 0.0f;

        /// <inheritdoc />
        public virtual PokePath PokeTrajectory => pokeTrajectory;
        private PokePath pokeTrajectory;

        #endregion IPokeInteractor

        #region MonoBehaviour

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Try to get the <see cref="TrackedPoseDriver"> component from the parent if it hasn't been set yet
            if (trackedPoseDriver == null) 
            {
                trackedPoseDriver = GetComponentInParent<TrackedPoseDriver>();
            }

            // If mode managed root is not defined, default to the tracked pose driver's game object
            if (modeManagedRoot == null && trackedPoseDriver != null)
            {
                modeManagedRoot = trackedPoseDriver.gameObject;
            }
        }

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            pokeTrajectory.Start = attachTransform.position;
            pokeTrajectory.End = attachTransform.position;
        }

        /// <summary>
        /// A Unity event function that is called to draw Unity editor gizmos that are also interactable and always drawn.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(pokeTrajectory.Start, PokeRadius);
            Gizmos.DrawLine(pokeTrajectory.Start, pokeTrajectory.End);
            Gizmos.DrawSphere(pokeTrajectory.End, PokeRadius);
        }

        #endregion MonoBehaviour

        #region XRBaseInteractor

        /// <inheritdoc />
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            targets.AddRange(this.targets);
        }

        // Was our poking point tracked the last time we checked?
        // This will drive isHoverActive.
        private bool pokePointTracked;

        /// <inheritdoc/>
        public override bool isHoverActive
        {
            // Only be available for hovering if the joint or controller is tracked.
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (forceDeprecatedInput)
                {
                    return base.isHoverActive && (xrController.currentControllerState.inputTrackingState.HasPositionAndRotation() || pokePointTracked);
                }
#pragma warning restore CS0618 // Type or member is obsolete
                else
                {
                    // If the interactor does not have a <see cref="TrackedPoseDriver"> component then we cannot determine if it is hover active
                    if (TrackedPoseDriver == null) 
                    {
                        return false;
                    }

                    return base.isHoverActive && (TrackedPoseDriver.GetInputTrackingState().HasPositionAndRotation() || pokePointTracked);
                }
            }
        }

        /// <inheritdoc/>
        public override bool isSelectActive => true;

        // Scratchpad for GetValidTargets. Spherecast hits and overlaps are recorded here.
        private HashSet<IXRInteractable> targets = new HashSet<IXRInteractable>();

        // Scratchpad for spherecast intersections.
        private RaycastHit[] results = new RaycastHit[8];

        // Scratchpad for collider overlaps.
        private Collider[] overlaps = new Collider[8];

        private static readonly ProfilerMarker ProcessInteractorPerfMarker =
            new ProfilerMarker("[MRTK] PokeInteractor.ProcessInteractor");

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            using (ProcessInteractorPerfMarker.Auto())
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                {
                    // The start of our new trajectory is the end of the last frame's trajectory.
                    pokeTrajectory.Start = pokeTrajectory.End;

                    // pokePointTracked is used to help set isHoverActive.
                    pokePointTracked = TryGetPokePose(out Pose pose) && TryGetPokeRadius(out pokeRadius);
                    if (pokePointTracked)
                    {
                        // If we can get a joint pose, set our transform accordingly.
                        transform.SetPositionAndRotation(pose.position, pose.rotation);
                    }
                    else
                    {
                        // If we don't have a poke pose, reset to whatever our parent XRController's pose is.
                        transform.localPosition = Vector3.zero;
                        transform.localRotation = Quaternion.identity;
                    }

                    // Ensure that the attachTransform tightly follows the interactor's transform
                    attachTransform.SetPositionAndRotation(transform.position, transform.rotation);

                    // The endpoint of our trajectory is the current attachTransform, regardless
                    // if this interactor set the attachTransform or whether we are just on a motion controller.
                    pokeTrajectory.End = attachTransform.position;

                    targets.Clear();

                    // If the trajectory is essentially stationary, we'll do a sphere overlap instead.
                    // SphereCasts return nothing if the start/end are the same.
                    if ((pokeTrajectory.End - pokeTrajectory.Start).sqrMagnitude <= 0.0001f)
                    {
                        int numOverlaps = UnityEngine.Physics.OverlapSphereNonAlloc(pokeTrajectory.End, PokeRadius, overlaps, UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

                        for (int i = 0; i < numOverlaps; i++)
                        {
                            // Add intersections to target list.
                            if (interactionManager.TryGetInteractableForCollider(overlaps[i], out IXRInteractable interactable))
                            {
                                targets.Add(interactable);
                            }
                        }
                    }
                    else
                    {
                        // Otherwise, we perform a spherecast (essentially, a thick raycast)
                        // from the start to the end of the recorded trajectory, with a radius/thickness
                        // corresponding to the joint radius.
                        int numHits = UnityEngine.Physics.SphereCastNonAlloc(
                            pokeTrajectory.Start,
                            PokeRadius,
                            (pokeTrajectory.End - pokeTrajectory.Start).normalized,
                            results,
                            (pokeTrajectory.End - pokeTrajectory.Start).magnitude,
                            UnityEngine.Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore);

                        for (int i = 0; i < numHits; i++)
                        {
                            // Add intersections to target list.
                            if (interactionManager.TryGetInteractableForCollider(results[i].collider, out IXRInteractable interactable))
                            {
                                targets.Add(interactable);
                            }
                        }
                    }
                }
            }
        }

        #endregion XRBaseInteractor

        #region ITrackedInteractor
        /// <inheritdoc />
        public GameObject TrackedParent => trackedPoseDriver == null ? null : trackedPoseDriver.gameObject;
        #endregion ITrackedInteractor

        #region IModeManagedInteractor
        /// <inheritdoc/>
        [Obsolete("This function is obsolete and will be removed in the next major release. Use ModeManagedRoot instead.")]
        public GameObject GetModeManagedController()
        {
            // Legacy controller-based interactors should return null, so the legacy controller-based logic in the
            // interaction mode manager is used instead.
#pragma warning disable CS0618 // Type or member is obsolete 
            if (forceDeprecatedInput)
            {
                return null;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return ModeManagedRoot;
        }
        #endregion IModeManagedInteractor
    }
}
