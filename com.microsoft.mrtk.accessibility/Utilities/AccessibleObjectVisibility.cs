// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Accessibility
{
    /// <summary>
    /// The 'visibility window' that the accessibility subsystem will use to determine the object
    /// descriptions to send to a screen reader.
    /// </summary>
    internal enum AccessibleObjectVisibility
    {
        /// <summary>
        /// Objects that are in front of the user and visible within the field of view.
        /// </summary>
        FieldOfView = 1,

        /// <summary>
        /// Objects that are all around the user, in front, behind, above, below, etc.
        /// </summary>
        Surround = 2
    }
}
