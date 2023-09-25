// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// A static helper class which caches instances of a component of type T.
    /// This is called in place of calling FindFirstObjectByType and FindObjectsByType to improve performance.
    /// </summary>
    public static class ComponentCache<T> where T : Component
    {
        private static T cacheFirstInstance = null;

        /// <summary>
        /// Finds the first instance of an object of the given type. 
        /// </summary>
        /// <returns>
        /// The first instance of an object of the given type. 
        /// </returns>
        public static T FindFirstActiveInstance()
        {
            _ = TryFindFirstActiveInstance(out T result);
            return result;
        }

        /// <summary>
        /// Finds the first instance of an object of the given type. 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if an instance was found, and <see langword="false"/> otherwise. 
        /// </returns>
        public static bool TryFindFirstActiveInstance(out T result)
        {
            if (cacheFirstInstance == null || !cacheFirstInstance.gameObject.activeInHierarchy)
            {
#if UNITY_2021_3_18_OR_NEWER
                cacheFirstInstance = Object.FindFirstObjectByType<T>();
#else
                cacheFirstInstance = Object.FindObjectOfType<T>();
#endif
            }

            result = cacheFirstInstance;
            return result != null;
        }
    }
}
