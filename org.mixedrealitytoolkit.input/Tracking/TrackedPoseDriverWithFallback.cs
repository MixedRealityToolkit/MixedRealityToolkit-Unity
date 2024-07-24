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

            var hasPositionFallbackAction = fallbackPositionAction != null;
            var hasRotationFallbackAction = fallbackRotationAction != null;

            // If default InputTrackingState does not have position and rotation data,
            // use fallback if it exists
            InputTrackingState inputTrackingState = trackingStateInput.GetInputTrackingState();

            bool defaultPostionAvailable =
                inputTrackingState.HasFlag(InputTrackingState.Position);

            bool defaultRotationAvailable =
                inputTrackingState.HasFlag(InputTrackingState.Rotation);
            bool defaultPostitionAndRotationDataAvailable =
                defaultPostionAvailable &&
                defaultRotationAvailable;

            // Only allow fallbacks to be used if the default tracking state has no data
            InputTrackingState fallbackInputTrackingState = InputTrackingState.None;
            if (FallbackTrackingStateAction.action != null &&
                !defaultPostitionAndRotationDataAvailable)
            {
                fallbackInputTrackingState = FallbackTrackingStateAction.GetInputTrackingState();
            }

            InputTrackingState fallbackDataUsed = InputTrackingState.None;
            Vector3 position = transform.localPosition;
            Quaternion rotation = transform.localRotation;

            // If no position data then use the data from the fallback action if it exists
            if (!defaultPostionAvailable &&
                hasPositionFallbackAction &&
                fallbackInputTrackingState.HasFlag(InputTrackingState.Position))
            {
                fallbackDataUsed |= InputTrackingState.Position;
                position = fallbackPositionAction.action.ReadValue<Vector3>();
            }

            // If no rotation data then use the data from the fallback action if it exists
            if (!defaultRotationAvailable &&
                hasRotationFallbackAction &&
                fallbackInputTrackingState.HasFlag(InputTrackingState.Rotation))
            {
                fallbackDataUsed |= InputTrackingState.Rotation;
                rotation = fallbackRotationAction.action.ReadValue<Quaternion>();
            }

            // If either position, rotation, or both data were obtained from fallback actions,
            // set the local transform from the fallback actions.
            if (fallbackDataUsed != InputTrackingState.None) 
            {
                SetLocalTransformFromFallback(position, rotation, fallbackDataUsed);
            }
        }
        #endregion TrackedPoseDriver Overrides

        #region Private Methods
        private void SetLocalTransformFromFallback(Vector3 newPosition, Quaternion newRotation, InputTrackingState currentFallbackTrackingState)
        {
            var positionValid = ignoreTrackingState || (currentFallbackTrackingState & InputTrackingState.Position) != 0;
            var rotationValid = ignoreTrackingState || (currentFallbackTrackingState & InputTrackingState.Rotation) != 0;

#if HAS_SET_LOCAL_POSITION_AND_ROTATION
            if (trackingType == TrackingType.RotationAndPosition && rotationValid && positionValid)
            {
                transform.SetLocalPositionAndRotation(newPosition, newRotation);
                return;
            }
#endif
            if (rotationValid &&
                (trackingType == TrackingType.RotationAndPosition ||
                 trackingType == TrackingType.RotationOnly))
            {
                transform.localRotation = newRotation;
            }

            if (positionValid &&
                (trackingType == TrackingType.RotationAndPosition ||
                 trackingType == TrackingType.PositionOnly))
            {
                transform.localPosition = newPosition;
            }
        }
        #endregion Private Methods 
    }
}
