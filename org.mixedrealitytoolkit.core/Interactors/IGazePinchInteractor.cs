// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit
{
    /// <summary>
    /// An interface that all gaze-pinch-like interactors must implement.
    /// </summary>
    public interface IGazePinchInteractor :
#pragma warning disable CS0618 // Type or member is obsolete
        IVariableSelectInteractor
#pragma warning restore CS0618 // Type or member is obsolete
    { }
}
