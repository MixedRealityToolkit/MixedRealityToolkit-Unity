// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// A static utility used to avoid deprecated Find Object functions in favor of replacements introduced in Unity >= 2021.3.18.
    /// </summary>
    [Obsolete("FindObjectUtility has been deprecated in version 4.0.0. Please use the corresponding UnityEngine.Object methods instead.")]
    public static class FindObjectUtility
    {
        /// <summary>
        /// Returns the first object matching the specified type.
        /// </summary>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        public static T FindFirstObjectByType<T>(bool includeInactive = false) where T : Component
        {
            return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
        }

        /// <summary>
        /// Returns an object matching the specified type.
        /// </summary>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        public static T FindAnyObjectByType<T>(bool includeInactive = false) where T : Component
        {
            return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
        }

        /// <summary>
        /// Returns all objects matching the specified type.
        /// </summary>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID. True by default.</param>
        public static T[] FindObjectsByType<T>(bool includeInactive = false, bool sort = true) where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
        }

        /// <summary>
        /// Returns all objects matching the specified type.
        /// </summary>
        /// <param name="includeInactive">If true, inactive objects will be included in the search. False by default.</param>
        /// <param name="sort">If false, results will not sorted by InstanceID. True by default.</param>
        /// <param name="type">The type to search for.</param>
        public static UnityEngine.Object[] FindObjectsByType(Type type, bool includeInactive = false, bool sort = true)
        {
            return UnityEngine.Object.FindObjectsByType(type, includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
        }
    }
}
