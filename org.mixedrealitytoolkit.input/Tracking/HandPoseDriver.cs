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

            // In case the position input action is not provided, we will try to polyfill it with the device position.
            // Should be removed once we have universal hand interaction profile(s) across vendors.
            bool missingPositionController = (trackingType.HasFlag(TrackingType.PositionOnly) || trackingType.HasFlag(TrackingType.RotationAndPosition)) &&
                (positionInput.action == null || !positionInput.action.HasAnyControls());

            bool missingRotationController = (trackingType.HasFlag(TrackingType.RotationOnly) || trackingType.HasFlag(TrackingType.RotationAndPosition)) &&
                (rotationInput.action == null || !rotationInput.action.HasAnyControls());

            // If we are missing the position or rotation controller, we will try to polyfill the device pose.
            // Should be removed once we have universal hand interaction profile(s) across vendors.
            // We will also check the tracking state here to account for a bound but untracked interaction profile.
            if ((missingPositionController || missingRotationController || IsTrackingNone()) &&
                TryGetPolyfillDevicePose(out Pose devicePose))
            {
                SetLocalTransform(devicePose.position, devicePose.rotation);
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
        private void HandleDisablement()
        {
            if (!isActiveAndEnabled || !Application.isPlaying)
            {
                UnbindTrackingState();
            }
        }

        /// <summary>
        /// Force an update of the tracking state from the Input Action Reference.
        /// </summary>
        private void ForceTrackingStateUpdate()
        {
            var trackingStateAction = trackingStateInput.action;
            if (trackingStateAction != null && !trackingStateAction.enabled)
            {
                // Treat a disabled action as the default None value for the ReadValue call
                m_trackingState = InputTrackingState.None;
                return;
            }

            if (trackingStateAction == null || trackingStateAction.bindings.Count == 0)
            {
                // Treat an Input Action Reference with no reference the same as
                // an enabled Input Action with no authored bindings, and allow driving the Transform pose.
                m_trackingState = InputTrackingState.Position | InputTrackingState.Rotation;
                return;
            }

            if (trackingStateAction.HasAnyControls())
            {
                m_trackingState = (InputTrackingState)trackingStateAction.ReadValue<int>();
            }
            else
            {
                m_trackingState = InputTrackingState.None;
            }
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
            HandleDisablement();
            m_trackingState = (InputTrackingState)context.ReadValue<int>();
        }

        private void OnTrackingStateInputCanceled(InputAction.CallbackContext context)
        {
            HandleDisablement();
            m_trackingState = InputTrackingState.None;
        }
        #endregion Private Functions
    }
}
