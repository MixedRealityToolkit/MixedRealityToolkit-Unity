// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause



namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all interactors which offer
    /// variable selection must implement.
    /// </summary>
    public interface IVariableSelectInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor, UnityEngine.XR.Interaction.Toolkit.Interactors.IXRHoverInteractor
    {
        /// <summary>
        /// Returns a value [0,1] representing the variable
        /// amount of "selection" that this interactor is performing.
        /// </summary>
        /// <remarks>
        /// For gaze-pinch interactors, this is the pinch progress.
        /// For motion controllers, this is the analog trigger press amount.
        /// </remarks>
        float SelectProgress { get; }
    }
}