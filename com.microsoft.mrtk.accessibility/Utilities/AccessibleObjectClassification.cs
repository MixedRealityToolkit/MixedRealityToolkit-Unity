// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit.Accessibility
{
    /// <summary>
    /// Classification for accessible objects that may appear in the scene.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AccessibleObjectClassification.asset",
        menuName = "MRTK/Accessibility/Accessible Object Classification")]
    public class AccessibleObjectClassification : ScriptableObject
    {
        [SerializeField, Experimental]
        [Tooltip("Friendly description of the classification.")]
        private string description;

        /// <summary>
        /// Friendly description of the classification (ex: "Locations in the world").
        /// </summary>
        public string Description { get; set; }
    }
}
