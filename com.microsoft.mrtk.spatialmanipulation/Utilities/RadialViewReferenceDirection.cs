// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.SpatialManipulation
{
    /// <summary>
    /// Which direction to orient the radial view object.
    /// </summary>
    public enum RadialViewReferenceDirection
    {
        /// <summary>
        /// Orient towards the target including roll, pitch and yaw
        /// </summary>
        ObjectOriented = 0,

        /// <summary>
        /// Orient toward the target but ignore roll
        /// </summary>
        FacingWorldUp = 1,

        /// <summary>
        /// Orient towards the target but remain vertical or gravity aligned
        /// </summary>
        GravityAligned = 2
    }
}