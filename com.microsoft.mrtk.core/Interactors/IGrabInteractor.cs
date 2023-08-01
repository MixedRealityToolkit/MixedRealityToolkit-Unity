// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all grab-like interactors implement.
    /// Interactors that implement this interface are expected to use
    /// the <see cref="IXRInteractor"/> attachTransform to specify
    /// the point at which the grab occurs.
    /// </summary>
    public interface IGrabInteractor : IXRInteractor
    {

    }
}