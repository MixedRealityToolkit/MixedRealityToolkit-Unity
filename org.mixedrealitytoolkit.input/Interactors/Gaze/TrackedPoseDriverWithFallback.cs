// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A specilized version of Unity's <seealso cref="TrackedPoseDriver"/> that will fallback to other Input System actions for
    /// position, rotation, and tracking input actions when <seealso cref="TrackedPoseDriver"/>'s default input actions cannot
    /// provide data.
    /// </summary>
    /// <remarks>
    /// This is useful when the <seealso cref="Interactor"/> has multiple active devices backing it, and some devices are not being
    /// tracked. For example, HoloLens 2 eye gaze might be active but not calibrated, in which case eye gaze tracking
    /// state will have no position and no rotation data. In this case, the <seealso cref="Interactor"/> may want to fallback to head pose.
    /// </remarks>
    [DisallowMultipleComponent]
    [AddComponentMenu("MRTK/Input/Tracked Pose Driver (with Fallbacks)")]
    public class TrackedPoseDriverWithFallback : TrackedPoseDriver
    {
        #region Fallback actions values

        [SerializeField, Tooltip("The fallback Input System action to use for Position Tracking for this GameObject when the default position input action has no data. Must be a Vector3Control Control.")]
        private InputActionProperty fallbackPositionAction;

        /// <summary>
        /// The fallback Input System action to use for Position Tracking for this GameObject when the default position
        /// input action has no data. Must be a Vector3Control Control.
        /// </summary>
        public InputActionProperty FallbackPositionAction => fallbackPositionAction;

        [SerializeField, Tooltip("The fallback Input System action to use for Rotation Tracking for this GameObject when the default rotation input action has no data. Must be a Vector3Control Control.")]
        private InputActionProperty fallbackRotationAction;

        /// <summary>
        /// The fallback Input System action to use for Rotation Tracking for this GameObject when the default rotation
        /// input action has no data. Must be a Vector3Control Control.
        /// </summary>
        public InputActionProperty FallbackRotationAction => fallbackRotationAction;

        [SerializeField, Tooltip("The fallback Input System action to get the Tracking State for this GameObject when the default track status action has no data. If not specified, this will fallback to the device's tracking state that drives the position or rotation action. Must be a IntegerControl Control.")]
        private InputActionProperty fallbackTrackingStateAction;

        /// <summary>
        /// The fallback Input System action to get the Tracking State for this GameObject when the default track status
        /// action has no data. If not specified, this will fallback to the device's tracking state that drives the position
        /// or rotation action. Must be a IntegerControl Control.
        /// </summary>
        public InputActionProperty FallbackTrackingStateAction => fallbackTrackingStateAction;

        #endregion Fallback action values

        #region TrackedPoseDriver Overrides 
        /// <inheritdoc />
        protected override void PerformUpdate()
        {
            base.PerformUpdate();

            if (trackingStateInput.action == null)
            {
                Debug.LogWarning("TrackedPoseDriverWithFallback.trackingStateInput.action is null, no fallback will be used.");
                return;
            }

            var positionAction = fallbackPositionAction.action;
            var hasPositionAction = positionAction != null;
            var hasPositionFallbackAction = fallbackPositionAction != null;

            var rotationAction = fallbackRotationAction.action;
            var hasRotationAction = rotationAction != null;
            var hasRotationFallbackAction = fallbackRotationAction != null;

            InputTrackingState inputTrackingState = (InputTrackingState)trackingStateInput.action.ReadValue<int>();

            if (!inputTrackingState.HasFlag(InputTrackingState.Position) && !inputTrackingState.HasFlag(InputTrackingState.Rotation) && FallbackTrackingStateAction.action != null)
            {
                inputTrackingState = (InputTrackingState)FallbackTrackingStateAction.action.ReadValue<int>();
            }

            // If no position data then swap the position action with the fallback position action if it exists
            if (!inputTrackingState.HasFlag(InputTrackingState.Position) && hasPositionAction && hasPositionFallbackAction)
            {
                (fallbackPositionAction, positionInput) = (positionInput, fallbackPositionAction);
            }

            // If no rotation data then swap the rotation action with the fallback rotation action if it exists
            if (!inputTrackingState.HasFlag(InputTrackingState.Rotation) && hasRotationAction && hasRotationFallbackAction)
            {
                (fallbackRotationAction, rotationInput) = (rotationInput, fallbackRotationAction);
            }
        }
        #endregion ActionBasedController Overrides 
    }
}
