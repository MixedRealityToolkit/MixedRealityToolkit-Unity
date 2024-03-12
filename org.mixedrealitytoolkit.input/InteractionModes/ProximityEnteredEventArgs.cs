// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityHoverEnteredEventArgs : BaseProximityHoverEventArgs
    {
        /// <summary>
        /// Constructor for ProximityHoverEnteredEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity entered event.</param>
        public ProximityHoverEnteredEventArgs(NearInteractionModeDetector nearInteractionModeDetector) : base(nearInteractionModeDetector)
        {
            //Empty on purpose
        }
    }
}
