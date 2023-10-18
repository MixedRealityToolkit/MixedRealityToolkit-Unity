// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using System;

namespace MixedReality.Toolkit
{
    public static class FindObjectUtility
    {
        public static T FindFirstObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
        return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        public static T FindAnyObjectByType<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindAnyObjectByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);
#else
        return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
        }

        public static T[] FindObjectsOfType<T>(bool includeInactive = false, bool sort = true) where T : Component
        {
#if UNITY_2021_3_18_OR_NEWER
        return UnityEngine.Object.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, sort ? FindObjectsSortMode.InstanceID : FindObjectsSortMode.None);
#else
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
#endif
        }

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
