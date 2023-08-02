// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Base abstract class for all profiles.
    /// </summary>
    /// <remarks>
    /// Profiles specify which subsystems should be started at launch,
    /// given a certain build target group (target platform),
    /// and specifies which configuration assets are associated
    /// with each started subsystem.
    /// </remarks>
    [Serializable]
    public abstract class BaseMRTKProfile : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private bool isCustomProfile = false;

        /// <summary>
        /// Is this a custom (i.e. user-made) profile, or 
        /// an immutable profile shipped with MRTK?
        /// </summary>
        /// <remarks>
        /// Not yet used.
        /// </remarks>
        internal bool IsCustomProfile => isCustomProfile;
    }
}
