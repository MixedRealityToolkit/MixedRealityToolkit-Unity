// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all interactors with the concept of handedness implement.
    /// </summary>
    [Obsolete(nameof(IHandedInteractor) + " has been deprecated in version 4.0.0. Use " + nameof(IXRInteractor) + " instead.")]
    public interface IHandedInteractor : IXRInteractor
    {
        /// <summary>
        /// Returns the Handedness of this interactor.
        /// </summary>
        [Obsolete("This property has been deprecated in version 4.0.0. Use " + nameof(handedness) + " instead.")]
        public Handedness Handedness { get; }
    }
}
