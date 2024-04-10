// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    /// <summary>
    /// Component to handle frozen world adjustments for dynamic (moving) objects.
    /// </summary>
    /// <remarks>
    /// For stationary objects, use <see cref="AdjusterFixed"/>.
    /// 
    /// This component uses the Unity Update pass to keep the World Locking Tools system
    /// apprised of the target object's position. While that operation is cheap, even
    /// just the cost of an additional Update() is best avoided for stationary objects.
    /// 
    /// If the object moves very infrequently under script control, consider using an <see cref="AdjusterFixed"/>,
    /// and notifying it after moves with <see cref="AdjusterFixed.UpdatePosition"/>.
    /// </remarks>
    public class AdjusterMoving : AdjusterFixed
    {
        /// <summary>
        /// Notify the system each frame of current position.
        /// </summary>
        private void Update()
        {
            UpdatePosition();
        }
    }
}