// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityExitedEventArgs : BaseProximityEventArgs
    {
        /// <summary>
        /// Constructor for ProximityExitedEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity exited event.</param>
        public ProximityExitedEventArgs(NearInteractionModeDetector nearInteractionModeDetector) : base(nearInteractionModeDetector)
        {
            //Empty on purpose
        }
    }
}
