// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Utilities for working with poses.
    /// </summary>
    internal static class PoseUtilities
    {
        /// <summary>
        /// Returns an estimated distance from the provided pose to the user's body.
        /// </summary>
        /// <remarks>
        /// The body is treated as a ray, parallel to the y-axis, where the start is head position.
        /// This means that moving your hand down such that is the same distance from the body will
        /// not cause the manipulated object to move further away from your hand. However, when you
        /// move your hand upward, away from your head, the manipulated object will be pushed away.
        ///
        /// Internal for now, may be made public later.
        /// </remarks>
        internal static float GetDistanceToBody(Pose pose)
        {
            if (pose.position.y > Camera.main.transform.position.y)
            {
                return Vector3.Distance(pose.position, Camera.main.transform.position);
            }
            else
            {
                Vector2 headPosXZ = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
                Vector2 pointerPosXZ = new Vector2(pose.position.x, pose.position.z);

                return Vector2.Distance(pointerPosXZ, headPosXZ);
            }
        }

        /// <summary>
        /// Calculate if the given pose forward direction is facing away from the user.
        /// </summary>
        /// <param name="pose">The pose whoe forward direction will be tested</param>
        /// <param name="tolerance">Degrees of rotation away from the user's head's forward vector for the hand to be considered raised/valid.</param>
        /// <returns><see langword="true"/> if palm is facing away from the user, <see langword="false"/> otherwise.</returns>
        internal static bool IsFacingAway(Pose pose, float tolerance = 75.0f)
        {
            if (Camera.main == null)
            {
                return false;
            }

            // The original palm orientation is based on a horizontal palm facing down.
            // So, if you bring your hand up and face it away from you, the palm.up is the forward vector.
            if (Mathf.Abs(Vector3.Angle(pose.forward, Camera.main.transform.forward)) > tolerance)
            {
                return false;
            }

            return true;
        }
    }
}
