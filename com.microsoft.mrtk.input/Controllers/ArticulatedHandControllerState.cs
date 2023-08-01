// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Represents the current state of the <see cref="ArticulatedHandController"/>.
    /// Contains extra state values extended from the base <see cref="XRControllerState"/>,
    /// including the left and right pinch/select progress.
    /// </summary>
    [Serializable]
    internal class ArticulatedHandControllerState : XRControllerState
    {
        /// <summary>
        /// Is the controller/hand ready to select via pinch?
        /// </summary>
        public bool PinchSelectReady;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticulatedHandControllerState"/> class.
        /// </summary>
        public ArticulatedHandControllerState() : base()
        {
            PinchSelectReady = false;
        }
    }
}
