// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity events triggered by a NearInteractionModeDetector.
    /// </summary>
    public abstract class BaseProximityEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor for BaseProximityEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity event.</param>
        public BaseProximityEventArgs(NearInteractionModeDetector nearInteractionModeDetector)
        {
            NearInteractionModeDetector = nearInteractionModeDetector;
        }

        /// <summary>
        /// The NearInteractionModeDetector associated with the proximity interaction event.
        /// </summary>
        public NearInteractionModeDetector NearInteractionModeDetector { get; private set; }
    }
}
