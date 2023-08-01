// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Attribute used to make an <see langword="int"/> field render a dropdown generated from the current layers defined in the Unity tag manager.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PhysicsLayerAttribute : PropertyAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsLayerAttribute"/> class.
        /// </summary>
        public PhysicsLayerAttribute() { }
    }
}
