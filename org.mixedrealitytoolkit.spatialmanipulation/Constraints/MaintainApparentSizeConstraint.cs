// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Component for setting the min/max scale values for ObjectManipulator
    /// or BoundsControl.
    /// </summary>
    /// <remarks>
    /// MRTK's constraint system might be redesigned in the near future. When
    /// this occurs, the old constraint components will be deprecated.
    /// </remarks>
    [AddComponentMenu("MRTK/Spatial Manipulation/Maintain Apparent Size Constraint")]
    public class MaintainApparentSizeConstraint : TransformConstraint
    {
        #region Properties

        private float initialDist;

        /// <inheritdoc />
        public override TransformFlags ConstraintType => TransformFlags.Scale;

        #endregion Properties

        #region Public Methods

        /// <inheritdoc />
        public override void OnManipulationStarted(MixedRealityTransform worldPose)
        {
            base.OnManipulationStarted(worldPose);

            Camera cam = Camera.main;
            initialDist = cam != null ? (worldPose.Position - cam.transform.position).magnitude : 0f;
        }

        /// <summary>
        /// Constrains scale such that the object's apparent size at manipulation
        /// start does not change when the object is moved towards and away from 
        /// the head.
        /// </summary>
        public override void ApplyConstraint(ref MixedRealityTransform transform)
        {
            Camera cam = Camera.main;
            if (cam == null || initialDist <= MathUtilities.ZeroTolerance)
            {
                return;
            }

            float dist = (transform.Position - cam.transform.position).magnitude;
            transform.Scale = (dist / initialDist) * WorldPoseOnManipulationStart.Scale;
        }

        #endregion Public Methods
    }
}
