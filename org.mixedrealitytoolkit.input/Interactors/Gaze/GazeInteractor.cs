// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// An XRRayInteractor that enables eye gaze for focus and interaction.
    /// </summary>
    [AddComponentMenu("MRTK/Input/Gaze Interactor")]
    public class GazeInteractor : XRRayInteractor, IGazeInteractor
    {
    }
}
