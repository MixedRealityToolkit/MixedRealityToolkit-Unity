// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause



namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all interactors with the concept of handedness implement.
    /// </summary>
    public interface IHandedInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor
    {
        /// <summary>
        /// Returns the Handedness of this interactor.
        /// </summary>
        public Handedness Handedness { get; }
    }
}
