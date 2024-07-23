// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A pose source which gets the pose composed of a tracked position and rotation input action.
    /// </summary>
    [Serializable]
    public class InputActionPoseSource : IPoseSource
    {
        [SerializeField]
        [Tooltip("The input action property used when obtaining the tracking information for the current pose.")]
        InputActionProperty trackingStateActionProperty;

        [SerializeField]
        [Tooltip("The input action property used when obtaining the position information for the current pose.")]
        InputActionProperty positionActionProperty;

        [SerializeField]
        [Tooltip("The input action property used when obtaining the rotation information for the current pose.")]
        InputActionProperty rotationActionProperty;

        /// <summary>
        /// Tries to get the pose in world space composed of the provided input action properties when the position and rotation are tracked.
        /// </summary>
        public bool TryGetPose(out Pose pose)
        {
            InputAction trackingStateAction = trackingStateActionProperty.action;
            InputAction positionAction = positionActionProperty.action;
            InputAction rotationAction = rotationActionProperty.action;

            // We need to consider the fact that the positon and rotation can be bound
            // to a control, but the control may not be active even if the tracking state is valid. So we need to
            // check if there's an active control before using the position and rotation values.
            if (trackingStateAction.HasAnyControls() &&
                (positionAction.HasAnyControls() && positionAction.activeControl != null) &&
                (rotationAction.HasAnyControls() && rotationAction.activeControl != null) &&
                ((InputTrackingState)trackingStateAction.ReadValue<int>() & (InputTrackingState.Position | InputTrackingState.Rotation)) != 0)
            {
                // Transform the pose into worldspace, as input actions are returned
                // in floor-offset-relative coordinates.
                pose = PlayspaceUtilities.TransformPose(
                    new Pose(
                        positionAction.ReadValue<Vector3>(),
                        rotationAction.ReadValue<Quaternion>()));
                return true;
            }
            else
            {
                pose = Pose.identity;
                return false;
            }
        }
    }
}
