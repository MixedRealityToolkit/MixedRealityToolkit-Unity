// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Dummy class to identify components as implementing Frozen World's adjustment handler(s).
    /// </summary>
    /// <remarks>
    /// Derivation from this class is not necessary to implement the necessary handling
    /// of state and transform messages from the system, as that handling is implemented by delegates, not
    /// inheritance. However, having a component derived from this base class attached to an object notifies the system
    /// that the object's handling of Frozen World system adjustments is covered, and prevents
    /// the system from automatically adding its own handlers to that object (if so configured).
    /// </remarks>
    public class AdjusterBase : MonoBehaviour
    {
    }
}