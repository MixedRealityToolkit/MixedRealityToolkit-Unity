// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// This allows for a hand pose driver to be used with hand tracking devices that do not have an interaction profile.
    /// </summary>
    /// <remarks>
    /// This should be removed once there are universal hand interaction profile(s) across vendors.
    /// </remarks>
    public class HandPoseDriver : TrackedPoseDriver
    {
        private static readonly Quaternion rightPalmOffset = Quaternion.Inverse(
            new Quaternion(
                Mathf.Sqrt(0.125f),
                Mathf.Sqrt(0.125f),
                -Mathf.Sqrt(1.5f) / 2.0f,
                Mathf.Sqrt(1.5f) / 2.0f));

        private static readonly Quaternion leftPalmOffset = Quaternion.Inverse(
            new Quaternion(
                Mathf.Sqrt(0.125f),
                -Mathf.Sqrt(0.125f),
                Mathf.Sqrt(1.5f) / 2.0f,
                Mathf.Sqrt(1.5f) / 2.0f));

        private bool m_firstUpdate = true;
        private InputAction m_boundTrackingAction = null;
        private InputTrackingState m_trackingState = InputTrackingState.None;

        /// <summary>
        /// Expose the tracking state for the hand pose driver, to allow <see cref="TrackedPoseDriverExtensions"/> to query it.
        /// </summary>
        /// <remarks
        /// Avoid exposing this publicly as this <see cref="HandPoseDriver"/> is a workaround solution to support hand tracking on devices without interaction profiles.
        /// </remarks>
        internal InputTrackingState CachedTrackingState => m_trackingState;

        /// <summary>
        /// Get if the last pose set was from a polyfill device pose. That is, if the last pose originated from the <see cref="XRSubsystemHelpers.HandsAggregator "/>.
        /// </summary>
        internal bool IsPolyfillDevicePose { get; private set; }

        #region Serialized Fields
        [Header("Hand Pose Driver Settings")]

        [SerializeField, Tooltip("The XRNode associated with this Hand Controller. Expected to be XRNode.LeftHand or XRNode.RightHand.")]
        private XRNode handNode;

        /// <summary>
        /// The XRNode associated with this Hand Controller.
        /// </summary>
        /// <remarks>Expected to be XRNode.LeftHand or XRNode.RightHand.</remarks>
        public XRNode HandNode => handNode;
        #endregion Serialized Fields

        #region TrackedPoseDriver Overrides
        /// <inheritdoc />
        protected override void PerformUpdate()
        {
            base.PerformUpdate();

            if (m_firstUpdate ||
                m_boundTrackingAction != trackingStateInput.action)
            {
                OnFirstUpdate();
                m_firstUpdate = false;
            }

            // In case the pose input actions are not provided or not bound to a control, we will try to query the 
            // `HandsAggregator` subsystem for the device's pose. This logic and class should be removed once we 
            // have universal hand interaction profile(s) across vendors.
            //
            // Note, for this workaround we need to consider the fact that the positon and rotation can be bound
            // to a control, but the control may not be active even if the tracking state is valid. So we need to
            // check if there's an active control before using the position and rotation values. If there's no active
            // this means the action was not updated this frame and we should use the polyfill pose.

            bool missingPositionController =
                (trackingType == TrackingType.RotationAndPosition || trackingType == TrackingType.PositionOnly) &&
                (positionInput.action == null || !positionInput.action.HasAnyControls() || positionInput.action.activeControl == null);

            bool missingRotationController =
                (trackingType == TrackingType.RotationAndPosition || trackingType == TrackingType.RotationOnly) &&
                (rotationInput.action == null || !rotationInput.action.HasAnyControls() || rotationInput.action.activeControl == null);

            // We will also check the tracking state here to account for a bound action but untracked interaction profile.
            if ((missingPositionController || missingRotationController || IsTrackingNone()) &&
                TryGetPolyfillDevicePose(out Pose devicePose))
            {
                m_trackingState = InputTrackingState.Position | InputTrackingState.Rotation;
                IsPolyfillDevicePose = true;
                ForceSetLocalTransform(devicePose.position, devicePose.rotation);
            }
            else
            {
                IsPolyfillDevicePose = false;
            }
        }
        #endregion TrackedPoseDriver Overrides

        #region Private Functions
        /// <summary>
        /// Check the tracking state here to account for a bound but untracked interaction profile.
        /// This could show up on runtimes where a controller is disconnected, hand tracking spins up,
        /// but the interaction profile is not cleared. This is allowed, per-spec: "The runtime may
        /// return the last-known interaction profile in the event that no controllers are active."
        /// </summary>
        private bool IsTrackingNone()
        {
            return m_trackingState == InputTrackingState.None;
        }

        /// <summary>
        /// Workaround for missing device pose on devices without interaction profiles
        /// for hands, such as Varjo and Quest. Should be removed once we have universal
        /// hand interaction profile(s) across vendors.
        /// </summary>
        private bool TryGetPolyfillDevicePose(out Pose devicePose)
        {
            bool poseRetrieved = false;
            Handedness handedness = HandNode.ToHandedness();

            // palmPose retrieved in global space.
            if (XRSubsystemHelpers.HandsAggregator != null &&
                XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.Palm, HandNode, out HandJointPose palmPose))
            {
                // XRControllers work in OpenXR scene-origin-space, so we need to transform
                // our global palm pose back into scene-origin-space.
                devicePose = PlayspaceUtilities.InverseTransformPose(palmPose.Pose);

                switch (handedness)
                {
                    case Handedness.Left:
                        devicePose.rotation *= leftPalmOffset;
                        poseRetrieved = true;
                        break;
                    case Handedness.Right:
                        devicePose.rotation *= rightPalmOffset;
                        poseRetrieved = true;
                        break;
                    default:
                        Debug.LogError("No polyfill available for device with handedness " + handedness);
                        devicePose = Pose.identity;
                        poseRetrieved = false;
                        break;
                };
            }
            else
            {
                devicePose = Pose.identity;
            }

            return poseRetrieved;
        }

        /// <summary>
        /// Sets the transform that is being driven by the <see cref="TrackedPoseDriver"/>. This will only set requested values, but regardless of tracking state.
        /// </summary>
        /// <param name="newPosition">The new local position to possibly set.</param>
        /// <param name="newRotation">The new local rotation to possibly set.</param>
        protected virtual void ForceSetLocalTransform(Vector3 newPosition, Quaternion newRotation)
        {
#if HAS_SET_LOCAL_POSITION_AND_ROTATION
            if (trackingType == TrackingType.RotationAndPosition)
            {
                transform.SetLocalPositionAndRotation(newPosition, newRotation);
                return;
            }
#endif

            if (trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.RotationOnly)
            {
                transform.localRotation = newRotation;
            }

            if (trackingType == TrackingType.RotationAndPosition ||
                trackingType == TrackingType.PositionOnly)
            {
                transform.localPosition = newPosition;
            }
        }

        /// <summary>
        /// The base class hasn't made OnEnable virtual, so we need to bind to tracking state updates
        /// in the first update. If base ever makes OnEnable virtual, we can move binding to OnEnabled.
        ///  </summary>
        private void OnFirstUpdate()
        {
            UnbindTrackingState();
            BindTrackingState();
            ForceTrackingStateUpdate();
        }

        /// <summary>
        /// The base class has not made OnDisable virtual, so we need to check for disablement in
        /// tracking state callbacks. If base ever make OnDisable virtual, we can unbind in OnDisable instead.
        /// </summary>
        private bool HandleDisablement()
        {
            // If backing native object has been destroyed (this == null) or component is
            // disabled, we should unbind the tracking state updates.
            if (this == null || !isActiveAndEnabled || !Application.isPlaying)
            {
                UnbindTrackingState();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Force an update of the tracking state from the Input Action Reference.
        /// </summary>
        private void ForceTrackingStateUpdate()
        {
            // Note, that the logic in this class is meant to reproduce the same logic as the base. The base
            // `TrackedPoseDriver` also sets the tracking state in a similar manner. Please see 
            // `TrackedPoseDriver::ReadTrackingState`. Replicating this logic in a subclass is not ideal, but it is
            // necessary since the base class does not expose the tracking state logic.
            m_trackingState = this.GetInputTrackingStateNoCache();
        }

        /// <summary>
        /// Listen for tracking state changes and update the tracking state.
        /// </summary>
        private void BindTrackingState()
        {
            if (m_boundTrackingAction != null)
            {
                return;
            }

            m_boundTrackingAction = trackingStateInput.action;
            if (m_boundTrackingAction == null)
            {
                return;
            }

            m_boundTrackingAction.performed += OnTrackingStateInputPerformed;
            m_boundTrackingAction.canceled += OnTrackingStateInputCanceled;
        }

        /// <summary>
        /// Stop listening for tracking state changes.
        /// </summary>
        private void UnbindTrackingState()
        {
            if (m_boundTrackingAction == null)
            {
                return;
            }

            m_boundTrackingAction.performed -= OnTrackingStateInputPerformed;
            m_boundTrackingAction.canceled -= OnTrackingStateInputCanceled;
            m_boundTrackingAction = null;
        }

        private void OnTrackingStateInputPerformed(InputAction.CallbackContext context)
        {
            if (!HandleDisablement())
            {
                m_trackingState = (InputTrackingState)context.ReadValue<int>();
            }
        }

        private void OnTrackingStateInputCanceled(InputAction.CallbackContext context)
        {
            if (!HandleDisablement())
            {
                m_trackingState = InputTrackingState.None;
            }
        }
        #endregion Private Functions
    }
}
