// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    public class ProximityEnteredEventArgs : BaseProximityEventArgs
    {
        /// <summary>
        /// Constructor for ProximityEnteredEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity entered event.</param>
        public ProximityEnteredEventArgs(NearInteractionModeDetector nearInteractionModeDetector) : base(nearInteractionModeDetector)
        {
            //Empty on purpose
        }
    }
}
