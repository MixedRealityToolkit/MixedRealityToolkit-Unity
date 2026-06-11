// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all interactors which offer
    /// variable selection must implement.
    /// </summary>
    [Obsolete(nameof(IVariableSelectInteractor) + " has been deprecated in version 4.0.0. Use " + nameof(IXRInteractionStrengthInteractor) + " instead.")]
    public interface IVariableSelectInteractor : IXRSelectInteractor, IXRHoverInteractor
    {
        /// <summary>
        /// Returns a value [0,1] representing the variable
        /// amount of "selection" that this interactor is performing.
        /// </summary>
        /// <remarks>
        /// For gaze-pinch interactors, this is the pinch progress.
        /// For motion controllers, this is the analog trigger press amount.
        /// </remarks>
        [Obsolete("This property has been deprecated in version 4.0.0. Use " + nameof(IXRInteractionStrengthInteractor.GetInteractionStrength) + " or " + nameof(IXRInteractionStrengthInteractor.largestInteractionStrength) + " instead.")]
        float SelectProgress { get; }
    }
}
