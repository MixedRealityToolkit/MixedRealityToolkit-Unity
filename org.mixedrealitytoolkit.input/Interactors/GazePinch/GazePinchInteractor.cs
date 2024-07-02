// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// An indirectly-targeted interactor that performs interactions driven by variable pinch distance.
    /// The valid targets of this interactor are harvested from the valid targets of the specified
    /// <see cref="GazePinchInteractor.DependentInteractor"/>.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Gaze Pinch Interactor")]
    public class GazePinchInteractor :
        XRBaseInputInteractor,
        IGazePinchInteractor,
        IHandedInteractor,
        IModeManagedInteractor,
        ITrackedInteractor
    {
        #region GazePinchInteractor

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

        [Header("Gaze Pinch interactor settings")]

        [SerializeField]
        [Tooltip("The hand controller used to get the selection progress values")]
        [Obsolete("Deprecated, please use this.TrackedPoseDriver instead.")]
        private ArticulatedHandController handController;

        /// <summary>
        /// Indicates whether the pinch interactor has completed the pinch gesture.
        /// </summary>
        private bool pinchReady = false;

        /// <summary>
        /// Is the hand ready to select? Typically, this
        /// represents whether the hand is in a pinching pose,
        /// within the FOV set by the aggregator config.
        /// </summary>
        protected bool PinchReady { get => pinchReady; }

        /// <summary>
        /// The world-space pose of the hand pinching point.
        /// </summary>
        protected Pose PinchPose => (PinchPoseSource != null && PinchPoseSource.TryGetPose(out Pose pinchPose)) ? pinchPose : new Pose(transform.position, transform.rotation);

        [SerializeReference]
        [InterfaceSelector(true)]
        [Tooltip("The pose source representing the device triggering the interaction.")]
        private IPoseSource devicePoseSource;

        /// <summary>
        /// The pose source representing the device triggering the interaction.
        /// </summary>
        protected IPoseSource DevicePoseSource { get => devicePoseSource; set => devicePoseSource = value; }

        [SerializeReference]
        [InterfaceSelector(true)]
        [Tooltip("The pose source representing the world-space pose of the hand pinching point.")]
        private IPoseSource pinchPoseSource;

        /// <summary>
        /// The pose source representing the world-space pose of the hand pinching point.
        /// </summary>
        protected IPoseSource PinchPoseSource { get => pinchPoseSource; set => pinchPoseSource = value; }

        [SerializeReference]
        [InterfaceSelector(true)]
        [Tooltip("The pose source representing the pose this interactor uses for aiming and positioning. Follows the 'pointer pose'.")]
        private IPoseSource aimPoseSource;

        /// <summary>
        /// The pose source representing the pose this interactor uses for aiming and positioning. Follows the 'pointer pose'.
        /// </summary>
        protected IPoseSource AimPoseSource { get => aimPoseSource; set => aimPoseSource = value; }

        [SerializeField]
        [Tooltip("The interactor we're using to query potential gaze pinch targets.")]
        private XRBaseInputInteractor dependentInteractor;

        /// <summary>
        /// The interactor we're using to query potential gaze pinch targets.
        /// </summary>
        protected internal XRBaseInputInteractor DependentInteractor { get => dependentInteractor; set => dependentInteractor = value; }

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The pinch amount at which the currently hovered target will 'stick' to the gaze.")]
        private float stickyHoverThreshold = 0.5f;

        /// <summary>
        /// The pinch amount at which the currently hovered target will 'stick' to the gaze.
        /// </summary>
        public float StickyHoverThreshold
        {
            get => stickyHoverThreshold;
            set => stickyHoverThreshold = Mathf.Clamp01(value);
        }

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("How un-pinched the hand must be to initiate a new hover or selection on a new target.")]
        private float relaxationThreshold = 0.1f;

        /// <summary>
        /// How un-pinched the hand must be to initiate a new hover or selection on a new target.
        /// </summary>
        public float RelaxationThreshold
        {
            get => relaxationThreshold;
            set => relaxationThreshold = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The distance from the body at the time of selection.
        /// </summary>
        /// <remarks>
        /// This is computed with <see cref="PoseUtilities.GetDistanceToBody"/>,
        /// which approximates the body distance as the distance of the interactor
        /// position to a 2D line parallel to y+, extending up to the head height
        /// and extending y- infinitely.
        /// </remarks>
        private float bodyDistanceOnSelect = 0;

        /// <summary>
        /// The attach transform position on the selected object at the time of selection,
        /// relative to the interactor transform. This is different than the
        /// GetLocalAttachPoseOnSelect, as that caches the pre-selection attach point.
        /// This caches the post-selection attach point, which has already been
        /// calculated/offset from the targeted object.
        /// </summary>
        private Vector3 interactorLocalAttachPoint;

        /// <summary>
        /// Used to check if the parent controller is tracked or not.
        /// Hopefully this becomes part of the base Unity XRI API.
        /// </summary>
        private bool IsTracked
        {
            get
            {
#pragma warning disable CS0618
                if (forceDeprecatedInput)
                {
                    return xrController.currentControllerState.inputTrackingState.HasPositionAndRotation();
                }
#pragma warning restore CS0618
                else
                {
                    if (TrackedPoseDriver == null) //If the interactor does not have a TrackedPoseDriver associated to it then it is not tracked
                    {
                        return false;
                    }

                    // If this interactor has a TrackedPoseDriver then use it to check if this interactor is tracked
                    return TrackedPoseDriver.GetInputTrackingState().HasPositionAndRotation();
                }
            }
        }

        #endregion GazePinchInteractor

        #region IHandedInteractor

        /// <inheritdoc />
        Handedness IHandedInteractor.Handedness
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (forceDeprecatedInput)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    return handController.HandNode.ToHandedness();
#pragma warning restore CS0612 // Type or member is obsolete
                }
#pragma warning restore CS0618 // Type or member is obsolete
                else
                {
                    return handedness.ToHandedness();
                }
            }
        }

        #endregion IHandedInteractor

        #region IVariableSelectInteractor

        /// <inheritdoc />
        public float SelectProgress
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (forceDeprecatedInput)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    return handController.selectInteractionState.value;
#pragma warning restore CS0612 // Type or member is obsolete
                }
#pragma warning restore CS0618 // Type or member is obsolete
                else if (selectInput != null)
                {
                    return selectInput.ReadValue();
                }
                else
                {
                    Debug.LogWarning($"Unable to determine SelectProgress of {name} because there is no Select Input Configuration set for this interactor.");
                }
                return 0.0f;
            }
        }

        #endregion IVariableSelectInteractor

        #region MonoBehaviour

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Try to get the TrackedPoseDriver component from the parent if it hasn't been set yet
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

        /// <inheritdoc/>
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
            {
                // Draw a yellow sphere at the transform's position
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(attachTransform.position, 0.05f);
            }
        }

        #endregion MonoBehaviour

        #region XRBaseInteractor

        /// <inheritdoc/>
        /// <remarks>
        /// This indirect interactor harvests the valid targets from the associated
        /// <see cref="dependentInteractor"/>, allowing for gaze-targeting or other
        /// indirect targeting mechanisms. As a result, the targeting/hovering rules
        /// are inherited from the <see cref="dependentInteractor"/>.
        /// </remarks>
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            // If we are hovering something and also have gone past the sticky hover threshold,
            // we should *only* consider the current hover target, regardless of what the
            // gaze is currently actually looking at. (Sticky hover, ADO#1941)
            if (hasHover && SelectProgress > stickyHoverThreshold)
            {
                targets.Add(interactablesHovered[0]);
            }
            // If we fail that check, we just use whatever the gaze is currently looking at.
            // This will allow us to be able to hover a new target. However, we may still may
            // not actually hover the new target, as CanHover() will perform a check against
            // the relaxation threshold.
            // (Relaxation threshold not necessarily == sticky hover threshold!)
            else
            {
                dependentInteractor.GetValidTargets(targets);
            }
        }

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Use The aim pose sources to calculate the interactor's pose and the attach transform's position
                if (AimPoseSource != null && AimPoseSource.TryGetPose(out Pose aimPose))
                {
                    transform.SetPositionAndRotation(aimPose.position, aimPose.rotation);
                }
                ComputeAttachTransform(hasSelection ? interactablesSelected[0] : null);

                UpdatePinchState();
            }
        }

        /// <summary>
        /// Given the specified interactable, this computes and applies the relevant
        /// position and rotation to the attach transform. 
        /// </summary>
        /// <remarks>
        /// If there is currently an active selection, the attach transform is computed 
        /// as an offset from selected object, where the offset vector is a function of 
        /// the centroid between all currently participating <see cref="GazePinchInteractor"/>
        /// objects. This models ray-like manipulations, but with virtual attach offsets
        /// from object, modeled from the relationship between each participating hand.
        /// When no selection is active, the attach transform is simply set to the
        /// current pinching pose. In all cases, the attach transform's rotation is
        /// set to the controller's grip pose.
        /// </remarks>
        /// <param name="interactable">The interactable to compute the attach transform for.</param>
        private void ComputeAttachTransform(IXRSelectInteractable interactable)
        {
            if (!AimPoseSource.TryGetPose(out Pose aimPose)) { return; }

            // Separate vars for fused position/rotation setting.
            Quaternion rotationToApply = attachTransform.rotation;
            Vector3 positionToApply = attachTransform.position;

            // Compute the ratio from the current hand-body distance to the distance
            // we recorded on selection. Used to linearly scale the attach transform's
            // distance to the body for easier manipulation. Same as the equivalent
            // math done in the ray interactor.
            float distanceRatio = PoseUtilities.GetDistanceToBody(aimPose) / bodyDistanceOnSelect;

            // Get the actual device/grab rotation. The controller transform is the aiming pose;
            // we must get the underlying grab rotation.
            // TODO: Replace with explicit binding to OpenXR grip pose when the standard is available.
            if (DevicePoseSource != null && DevicePoseSource.TryGetPose(out Pose devicePose) &&
                   PinchPoseSource != null && PinchPoseSource.TryGetPose(out Pose pinchPose))
            {
                rotationToApply = devicePose.rotation;
                if (hasSelection && interactable != null)
                {
                    var pinchCentroid = GetPinchCentroid(interactable);

                    // Compute the "virtual hand" position as the vector from this pinch to the average pinch.
                    Vector3 objectOffset = pinchPose.position - pinchCentroid.position;

                    // Compute the final attachTransform's position by transforming the interactor-local original attach point
                    // by the aiming ray's rotation (sans roll), scaling by the body-distance ratio, and then finally applying
                    // the virtual hand offset.

                    // The "noRollRay" is a rotation obtained by removing the roll component from the aim pose's orientation.
                    // This is useful when transforming the interactor-local attach point, without the influence of the hand's roll.
                    Quaternion noRollRay = Quaternion.LookRotation(aimPose.rotation * Vector3.forward);
                    positionToApply = aimPose.position + objectOffset + (noRollRay * interactorLocalAttachPoint) * distanceRatio;
                }
                else
                {
                    // If we're not selecting, just use the pinching position.
                    positionToApply = pinchPose.position;
                }
            }

            attachTransform.SetPositionAndRotation(positionToApply, rotationToApply);
        }

        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            // To select, we must either be already selecting the object, or have no other current selection.
            // In addition, we must be able to hover the object in order to select.
            return base.CanSelect(interactable) && (!hasSelection || IsSelecting(interactable)) && CanHover(interactable as IXRHoverInteractable);
        }

        /// <inheritdoc />
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            // We stay hovering on things we've selected.
            bool stickySelect = (!hasSelection) || IsSelecting(interactable);

            // We are ready to pinch if we are in the PinchReady position,
            // or if we are already selecting something.
            bool ready = PinchReady || isSelectActive;

            // Is this a new interactable that we aren't already hovering?
            bool isNew = !IsHovering(interactable);

            // If so, should we be allowed to initiate a new hover on it?
            // This prevents us from "rolling off" one target and immediately
            // semi-pressing another.
            bool canHoverNew = !isNew || SelectProgress < relaxationThreshold;

            return base.CanHover(interactable) && stickySelect && ready && canHoverNew;
        }

        /// <inheritdoc />
        public override bool isSelectActive
        {
            get
            {
                // When the gaze pinch interactor is already selecting an object,
                // use the default interactor behavior
                if (hasSelection)
                {
                    return base.isSelectActive;
                }
                // Otherwise, this selector should only be able to select an object
                // if it is "Pinch Ready", meaning that its hand controller
                // is in the pinch ready pose.
                // TODO: Bind PinchReady to the OpenXR-compliant ready state once the standard is available.
                else
                {
                    return base.isSelectActive && PinchReady;
                }
            }
        }

        /// <inheritdoc />
        public override bool isHoverActive => base.isHoverActive && IsTracked;

        /// <summary>
        /// When other interactors select/deselect the object that we are currently selecting,
        /// we must reset the recorded interactor-local attach point and the bodyDistanceOnSelect.
        /// </summary>
        private void ResetManipulationLogic(IXRSelectInteractable interactable)
        {
            var pinchCentroid = GetPinchCentroid(interactable);

            if (!AimPoseSource.TryGetPose(out Pose aimPose)) { return;}

            // The "noRollRay" is a rotation obtained by removing the roll component from the aim pose's orientation.
            // This is useful when transforming the interactor-local attach point, without the influence of the hand's roll.
            Quaternion noRollRay = Quaternion.LookRotation(aimPose.rotation * Vector3.forward);

            // Compute the "virtual hand" position as the vector from this pinch to the average pinch.
            Vector3 objectOffset = PinchPose.position - pinchCentroid.position;

            Vector3 snapPoint;

            // If this is a snap point affordance, we stick our attachTransform directly to the handle.
            if (interactable is ISnapInteractable snapInteractable)
            {
                snapPoint = snapInteractable.HandleTransform.position;
            }
            // If it's not a snap point affordance, we use the current gaze hit point.
            else if (dependentInteractor is FuzzyGazeInteractor gazeInteractor)
            {
                snapPoint = gazeInteractor.PreciseHitResult.raycastHit.point;
            }
            // Otherwise, just use the collider's center.
            else
            {
                snapPoint = interactable.colliders[0].bounds.center;
            }

            // Store a cached version of the attachTransform. We'll transform this into a no-roll-ray-relative
            // coordinate space, store it, and then transform it back into world coordinates throughout the
            // duration of the manipulation.
            Vector3 virtualAttachTransform = snapPoint + objectOffset;

            // Transform this virtual attachTransform into the interactor-local coordinate space.
            interactorLocalAttachPoint = Quaternion.Inverse(noRollRay) * (virtualAttachTransform - aimPose.position);

            // Record the distance from the controller to the body of the user, to use as reference for subsequent
            // distance measurements. 
            bodyDistanceOnSelect = PoseUtilities.GetDistanceToBody(aimPose);
        }

        /// <summary>
        /// Computes the geometric centroid between all PinchPoses of participating GazePinchInteractors.
        /// </summary>
        private Pose GetPinchCentroid(IXRSelectInteractable interactable)
        {
            Vector3 sumPos = Vector3.zero;
            Vector3 sumDir = Vector3.zero;
            int count = 0;

            foreach (IXRSelectInteractor interactor in interactable.interactorsSelecting)
            {
                if (interactor is GazePinchInteractor gazePinchInteractor)
                {
                    // TODO: Replace PinchPose with explicit binding to OpenXR pinch/grip pose when the standard is available.
                    // We currently compute our own pinch pose from joint data; controllers/other devices will expose their
                    // own relevant "pinch" poses when the extension is ratified.
                    sumPos += gazePinchInteractor.PinchPose.position;
                    sumDir += gazePinchInteractor.PinchPose.rotation * Vector3.forward;
                    count++;
                }
            }

            return new Pose
            {
                position = sumPos / Mathf.Max(1, count),
                rotation = Quaternion.LookRotation(sumDir / Mathf.Max(1, count))
            };
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            ResetManipulationLogic(args.interactableObject);

            args.interactableObject.selectEntered.AddListener(OnAdditionalSelect);
            args.interactableObject.selectExited.AddListener(OnAdditionalDeselect);

            ComputeAttachTransform(args.interactableObject);
        }

        private void OnAdditionalSelect(SelectEnterEventArgs args)
        {
            if (args.interactorObject is IGazePinchInteractor && !args.interactorObject.Equals(this))
            {
                ResetManipulationLogic(args.interactableObject);
                ComputeAttachTransform(args.interactableObject);
            }
        }

        private void OnAdditionalDeselect(SelectExitEventArgs args)
        {
            if (args.interactorObject is IGazePinchInteractor && !args.interactorObject.Equals(this))
            {
                ResetManipulationLogic(args.interactableObject);
                ComputeAttachTransform(args.interactableObject);
            }
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            args.interactableObject.selectEntered.RemoveListener(OnAdditionalSelect);
            args.interactableObject.selectExited.RemoveListener(OnAdditionalDeselect);

            base.OnSelectExited(args);

            ComputeAttachTransform(args.interactableObject);
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

        #region Private Methods
        /// <summary>
        /// Updates the pinch state of the GazePinchInteractor.
        /// If handedness is not set then it defaults to right hand.
        /// If the pinch data is not available for the set hand then the other hand is tried.
        /// </summary>
        private void UpdatePinchState()
        {
            if (logicalSelectState == null)
            {
                Debug.LogWarning("GazePinchInteractor is missing logicalSelectState, pinch state won't update.");
                return;
            }

            if (XRSubsystemHelpers.HandsAggregator == null)
            {
                Debug.LogWarning("XRSubsystemHelpers.HandsAggregator is null, pinch state won't update.");
                return;
            }

            var xrNode = handedness.ToXRNode();
            bool gotPinchData = XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(xrNode,
                out bool isPinchReady, out bool isPinching, out float pinchAmount);
            if (!gotPinchData) //Try the other hand if the set hand does not have pinch data.
            {
                gotPinchData = XRSubsystemHelpers.HandsAggregator.TryGetPinchProgress(xrNode == XRNode.LeftHand ? XRNode.RightHand : XRNode.LeftHand,
                    out isPinchReady, out isPinching, out pinchAmount);
            }

            if (gotPinchData)
            {
                pinchReady = isPinchReady;
            }
        }
        #endregion Private Methods
    }
}
