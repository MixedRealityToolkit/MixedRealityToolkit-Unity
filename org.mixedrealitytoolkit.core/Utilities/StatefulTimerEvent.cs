// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.Events;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// A Unity event used by <see cref="TimedFlag"/> instances.
    /// </summary>
    /// <remarks>
    /// The timer event fired when a <see cref="TimedFlag"/> is triggered.
    /// Passes a single float argument, representing the timestamp at which
    /// the event (entered, exited) occurred.
    /// </remarks>
    [System.Serializable]
    public class StatefulTimerEvent : UnityEvent<float> { }
}
