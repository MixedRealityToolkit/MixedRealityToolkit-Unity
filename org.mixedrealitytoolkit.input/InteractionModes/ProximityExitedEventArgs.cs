// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverExitedEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityHoverExitedEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity exited event.</param>
        public ProximityHoverExitedEventArgs(NearInteractionModeDetector nearInteractionModeDetector) : base(nearInteractionModeDetector)
        {
            //Empty on purpose
        }
    }
}
