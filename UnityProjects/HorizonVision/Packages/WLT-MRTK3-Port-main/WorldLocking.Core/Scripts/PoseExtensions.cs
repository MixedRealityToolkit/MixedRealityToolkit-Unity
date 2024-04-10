// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

/// <summary>
/// UnityEngine.Pose is a lightweight class value expressing a mathematical transform (position & rotation)
///
/// (Unlike the Unity Transform, a component that always belongs to a GameObject and also expresses a the
/// parent-child relation within a hierarchy)
/// 
/// This file defines Extension Methods
/// (see https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
/// for missing operations and assignments on that class.
/// 
/// The syntax is suboptimal Extension Methods do not support properties and operators. Ultimately, the best solution would be
/// to define these operations within the Unity code itself where proper syntax could be achieved.
/// </summary>

namespace Microsoft.MixedReality.WorldLocking.Core
{
 
    /// <summary>
    /// Conversion function between a pose and a transform.
    /// </summary>
    public static class TransformExtensions
    {
        public static Pose GetLocalPose(this Transform transform)
        {
            return new Pose(transform.localPosition, transform.localRotation);
        }

        public static Pose GetGlobalPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }

        public static void SetLocalPose(this Transform transform, Pose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
        }

        public static void SetGlobalPose(this Transform transform, Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }
    }

    /// <summary>
    /// Extensions for Poses to enable basic transform math.
    /// </summary>
    public static class PoseExtensions
    {
        /*
         * application of a transform on a position, defined such that:
         * transform.position == FromGlobal(parent.transform) * localPosition
         */
        public static Vector3 Multiply(this Pose pose, Vector3 position)
        {
            return pose.position + pose.rotation * position;
        }

        /*
         * chaining of transforms, defined such that
         * V' = lhs * (rhs * V)
         *    = (lhs.pos,lhs.rot) * (rhs.pos + rhs.rot * V)
         *    = lhs.pos + lhs.rot * (rhs.pos + rhs.rot * V)
         *    = lhs.pos + lhs.rot * rhs.pos + lhs.rot * rhs.rot * V
         *    = (lhs.pos + lhs.rot * rhs.pos , lhs.rot * rhs.rot) * V
         *    = (lhs * rhs) * V
         */

        public static Pose Multiply(this Pose lhs, Pose rhs)
        {
            return new Pose(lhs.position + lhs.rotation * rhs.position, lhs.rotation * rhs.rotation);
        }

        /*
         * inverse of transform, defined such that
         * 1 == inv(t) * t == t * inv(t)
         * 
         *   inv(t) * t
         * = (-inv(t.rot)*t.pos , inv(t.rot)) * (t.pos, t.rot)
         * = (-inv(t.rot)*t.pos + inv(t.rot)  * t.pos , inv(t.rot) * t.rot)
         * = 1
         * 
         *   t * inv(t)
         * = (t.pos, t.rot) * (-inv(t.rot)*t.pos , inv(t.rot))
         * = (t.pos + t.rot * (-inv(t.rot)*t.pos) , t.rot * inv(t.rot))
         * = 1
         */
        public static Pose Inverse(this Pose t)
        {
            var inv_t_rotation = Quaternion.Inverse(t.rotation);
            return new Pose(-(inv_t_rotation * t.position), inv_t_rotation);
        }
    }
}
