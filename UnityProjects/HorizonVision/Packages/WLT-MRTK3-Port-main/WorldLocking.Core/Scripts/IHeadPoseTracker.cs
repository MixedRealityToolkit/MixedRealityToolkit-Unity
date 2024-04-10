// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Interface for retrieving the current head pose.
    /// </summary>
    public interface IHeadPoseTracker 
    {
        /// <summary>
        /// Reset is called whenever there is a conceptual restart. Any cached information should be discarded
        /// and re-evaluated.
        /// </summary>
        void Reset();

        /// <summary>
        /// Return the current head pose in Spongy (local) space. 
        /// </summary>
		/// <returns>The current head pose.</returns>
        Pose GetHeadPose();

    }
}
