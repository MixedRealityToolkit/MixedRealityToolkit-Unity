// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.UX
{
    [CreateAssetMenu(fileName = "FontIconSetDefinition", menuName = "MRTK/UX/Font Icon Set Definition")]
    public class FontIconSetDefinition : ScriptableObject
    {
        [SerializeField]
        private string[] iconNames;

        /// <summary>
        /// The list of icon names defined by this asset.
        /// </summary>
        public IReadOnlyList<string> IconNames => iconNames;

#if UNITY_EDITOR
        /// <summary>
        /// Sorts the icon names and removes duplicates for cleaner serialization.
        /// </summary>
        [ContextMenu("Sort and Deduplicate")]
        public void SortAndDeduplicate()
        {
            if (iconNames == null || iconNames.Length == 0)
            {
                return;
            }

            UnityEditor.Undo.RecordObject(this, "Sort and Deduplicate Icon Names");

            var uniqueSorted = new List<string>(new HashSet<string>(iconNames));
            uniqueSorted.Sort();

            iconNames = uniqueSorted.ToArray();

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
