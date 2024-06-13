// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Simple script that makes the attached GameObject follow the specified joint.
    /// Internal for now; TODO possibly made public in the future.
    /// </summary>
    /// <remarks>
    /// Packaging-wise, this should be considered part of the Hands feature. This also does
    /// not depend on XRI.
    /// </remarks>
    [AddComponentMenu("MRTK/Input/Follow Joint")]
    internal class FollowJoint : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The pose source representing the hand joint this interactor tracks")]
        private HandJointPoseSource jointPoseSource;

        /// <summary>
        /// The pose source representing the hand joint this interactor tracks
        /// </summary>
        protected HandJointPoseSource JointPoseSource { get => jointPoseSource; set => jointPoseSource = value; }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        private void Update()
        {
            if (JointPoseSource != null && JointPoseSource.TryGetPose(out Pose pose))
            {
                transform.SetPositionAndRotation(pose.position, pose.rotation);
            }
            else
            {
                // If we have no valid tracked joint, reset to local zero.
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}
