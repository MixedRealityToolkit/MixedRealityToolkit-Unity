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
        /// Returns the first object matching the specified type.
        /// </summary>
        /// <remarks>
        /// If Unity >= 6000.4, calls FindAnyObjectByType. If Unity >= 2021.3.18, calls FindFirstObjectByType. Otherwise calls FindObjectOfType.
        /// </remarks>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        public static T FindFirstObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_6000_4_OR_NEWER
            return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#elif UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        /// <summary>
        /// Returns an object matching the specified type. 
        /// </summary>
        /// <remarks>
        /// If Unity >= 2021.3.18, calls FindAnyObjectByType. Otherwise calls FindObjectOfType.
        /// </remarks>
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
        /// Returns all objects matching the specified type.
        /// </summary>
        /// <remarks>
        /// If Unity >= 6000.4, calls FindObjectsByType without sorting. If Unity >= 2021.3.18, calls FindObjectsByType. Otherwise calls FindObjectsOfType.
        /// </remarks>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID. True by default. Ignored on Unity 6.4 and newer.</param>
        public static T[] FindObjectsByType<T>(bool includeInactive = false, bool sort = true) where T : Component
        {
#if UNITY_6000_4_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#elif UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

        /// <summary>
        /// Returns all objects matching the specified type.
        /// </summary>
        /// <remarks>
        /// If Unity >= 6000.4, calls FindObjectsByType without sorting. If Unity >= 2021.3.18, calls FindObjectsByType. Otherwise calls FindObjectsOfType.
        /// </remarks>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID. True by default. Ignored on Unity 6.4 and newer.</param>
        /// <param name="type">The type to search for.</param>
        public static UnityEngine.Object[] FindObjectsByType(Type type, bool includeInactive = false, bool sort = true)
        {
#if UNITY_6000_4_OR_NEWER
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#elif UNITY_2021_3_18_OR_NEWER
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType(type, includeInactive);
#endif
        }
    }
}
