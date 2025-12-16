// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all interactors with the concept of handedness implement.
    /// </summary>
    [Obsolete("Use handedness from IXRInteractor instead.")]
    public interface IHandedInteractor : IXRInteractor
    {
        /// <summary>
        /// Returns the Handedness of this interactor.
        /// </summary>
        [Obsolete("Use handedness from IXRInteractor instead.")]
        public Handedness Handedness { get; }
    }
}
