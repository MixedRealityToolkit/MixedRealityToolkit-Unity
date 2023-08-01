// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Examples
{
    /// <summary>
    /// Simple class that automatically hides a target on startup. This is, for example, useful for nested canvas object.
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/DisableOnStart")]
    public class DisableOnStart : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
