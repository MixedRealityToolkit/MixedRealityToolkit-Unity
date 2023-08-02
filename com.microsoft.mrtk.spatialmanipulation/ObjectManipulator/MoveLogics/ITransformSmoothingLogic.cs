// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Defines a method of smoothing the components of a transform.
    /// </summary>
    public interface ITransformSmoothingLogic
    {
        /// <summary>
        /// Smooths from source to goal, provided lerptime and a deltaTime.
        /// </summary>
        Vector3 SmoothPosition(Vector3 source, Vector3 goal, float lerpTime, float deltaTime);

        /// <summary>
        /// Smooths from source to goal, provided slerptime and a deltaTime.
        /// </summary>
        Quaternion SmoothRotation(Quaternion source, Quaternion goal, float slerpTime, float deltaTime);

        /// <summary>
        /// Smooths from source to goal, provided lerptime and a deltaTime.
        /// </summary>
        Vector3 SmoothScale(Vector3 source, Vector3 goal, float lerpTime, float deltaTime);
    }
}