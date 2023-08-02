// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine.Events;

namespace MixedReality.Toolkit.UX.Experimental
{
    /// <summary>
    /// Wrapper around UnityEvent&lt;NonNativeKey&gt; for serialization
    /// </summary>
    [Serializable]
    public class NonNativeKeyboardPressEvent : UnityEvent<NonNativeKey> { }

    /// <summary>
    /// Wrapper around UnityEvent&lt;bool&gt; for serialization
    /// </summary>
    [Serializable]
    public class NonNativeKeyboardShiftEvent : UnityEvent<bool> { }

    /// <summary>
    /// Wrapper around UnityEvent&lt;string&gt; for serialization
    /// </summary>
    [Serializable]
    public class NonNativeKeyboardTextEvent : UnityEvent<string> { }

    /// <summary>
    /// Wrapper around UnityEvent&lt;int&gt; for serialization
    /// </summary>
    [Serializable]
    public class NonNativeKeyboardIntEvent : UnityEvent<int> { }
}
