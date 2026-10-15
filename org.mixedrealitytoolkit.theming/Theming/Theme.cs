// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Theming
{
    /// <summary>
    /// A ScriptableObject that provides concrete values for every item declared in a <see cref="ThemeDataSource"/>'s definition.
    /// </summary>
    [CreateAssetMenu(fileName = "Theme", menuName = "MRTK/Theming/Theme", order = 0)]
    public class Theme : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        [Tooltip("The schema that this theme conforms to. Every theme item must map to an item defined here.")]
        private ThemeDefinition definition;

        /// <summary>
        /// The schema that this theme conforms to. Every theme item must map to an item defined here.
        /// </summary>
        public ThemeDefinition Definition => definition;

        [SerializeReference]
        [Tooltip("The items defining this theme's data mapped to the definition's items.")]
        private List<ThemeItem> themeItems = new List<ThemeItem>();

        [NonSerialized]
        private Dictionary<string, object> itemCache;

        private void EnsureCache()
        {
            if (itemCache != null)
            {
                return;
            }

            if (themeItems == null)
            {
                itemCache = new Dictionary<string, object>(0);
                return;
            }

            itemCache = new Dictionary<string, object>(themeItems.Count);
            for (int i = 0; i < themeItems.Count; i++)
            {
                ThemeItem item = themeItems[i];
                if (item != null && !string.IsNullOrEmpty(item.Name) && !itemCache.ContainsKey(item.Name))
                {
                    itemCache.Add(item.Name, item.Data);
                }
            }
        }

        /// <summary>
        /// Invalidates the internal theme item lookup cache.
        /// </summary>
        public void InvalidateCache()
        {
            itemCache = null;
        }

        private void OnEnable()
        {
            InvalidateCache();
        }

        private void OnValidate()
        {
            InvalidateCache();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            InvalidateCache();
        }

        /// <summary>
        /// Attempts to retrieve the data for a specific theme item by its name and expected type.
        /// </summary>
        /// <typeparam name="T">The expected type of the theme item data (e.g., <see cref="BaseThemeItemData{T}"/>).</typeparam>
        /// <param name="itemName">The name of the theme item as defined in the theme definition.</param>
        /// <param name="itemValue">When this method returns, contains the item data if found, or the default value otherwise.</param>
        /// <returns><see langword="true"/> if an item with the specified name and type is found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetItemData<T>(string itemName, out T itemValue)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                EnsureCache();
                if (itemCache.TryGetValue(itemName, out object data) && data is T themeItemData)
                {
                    itemValue = themeItemData;
                    return true;
                }
            }

            itemValue = default;
            return false;
        }

        /// <summary>
        /// Represents a single named entry in a theme containing a data payload.
        /// </summary>
        [Serializable]
        internal class ThemeItem
        {
            /// <summary>
            /// The name of the theme item.
            /// </summary>
            [field: SerializeField, HideInInspector]
            public string Name { get; private set; }

            /// <summary>
            /// The underlying data value for this theme item.
            /// </summary>
            [field: SerializeReference]
            public object Data { get; private set; }

            public ThemeItem(string name, object data)
            {
                Name = name;
                Data = data;
            }
        }
    }
}
