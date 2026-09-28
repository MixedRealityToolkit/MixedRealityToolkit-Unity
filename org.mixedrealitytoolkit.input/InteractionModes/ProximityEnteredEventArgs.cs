// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Event data associated with proximity entered events triggered by a <see cref="NearInteractionModeDetector"/>.
    /// </summary>
    public class ProximityEnteredEventArgs : BaseProximityEventArgs
    {
        /// <summary>
        /// Constructor for ProximityEnteredEventArgs.
        /// </summary>
        /// <param name="nearInteractionModeDetector">NearInteractionModeDetector that triggers proximity entered event.</param>
        public ProximityEnteredEventArgs(NearInteractionModeDetector nearInteractionModeDetector) : base(nearInteractionModeDetector)
        {
            // Empty on purpose
        }
    }
}
