// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using System;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// A static utility used to avoid deprecated Find Object functions in favor of replacements introduced in Unity >= 2021.3.18. 
    /// </summary>
    public static class FindObjectUtility
    {

        /// <summary>
        /// If Unity >= 2021.3.18, calls FindFirstObjectByType. Otherwise calls FindObjectOfType. Both return the first object matching the specified type.
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        public static T FindFirstObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
        return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        /// <summary>
        /// If Unity >= 2021.3.18, calls FindAnyObjectByType, a more efficient function which returns an arbitrary object matching specified type.
        /// Otherwise calls FindObjectOfType, which returns the first object matching the specified type. 
        /// </summary>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        public static T FindAnyObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
        return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        /// <summary>
        /// If Unity >= 2021.3.18, calls FindObjectsByType, which can be sorted or unsorted. Otherwise calls FindObjectsOfType, which is always sorted according to InstanceID. 
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID (available in >=2021.3.18 only). True by default.</param>
        public static T[] FindObjectsOfType<T>(bool includeInactive = false, bool sort = true) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        /// <summary>
        /// If Unity >= 2021.3.18, calls FindObjectsByType, which can be sorted or unsorted. Otherwise calls FindObjectsOfType, which is always sorted according to InstanceID. 
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID (available in >=2021.3.18 only). True by default.</param>
        /// <param name="type">The type to search for.</param>
        public static UnityEngine.Object[] FindObjectsOfType(Type type, bool includeInactive = false, bool sort = true)
        {
#if UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType(type, includeInactive);
#endif
        }
    }
}
