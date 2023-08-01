// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.Events;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// A Unity event raised by <see cref="MixedReality.Toolkit.UX.ToggleCollection">ToggleCollection</see>
    /// when any of the toggle buttons are selected. The event data is the index of the toggle button within the
    /// <see cref="ToggleCollection"/>.
    /// </summary>
    [Serializable]
    public class ToggleSelectedEvent : UnityEvent<int> { }
}