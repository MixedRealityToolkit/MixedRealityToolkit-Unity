// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// Generic Dictionary helper class that handles serialization of keys and values into lists before/after serialization time since Dictionary by itself is not Serializable.
    /// Extends C# Dictionary class to support typical API access methods
    /// </summary>
    /// <typeparam name="TKey">Key type for Dictionary</typeparam>
    /// <typeparam name="TValue">Value type for Dictionary</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableDictionaryEntry> entries = new List<SerializableDictionaryEntry>();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if !UNITY_EDITOR
            entries.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                entries.Add(new SerializableDictionaryEntry(pair.Key, pair.Value));
            }
#else
            // While in Editor, the serialized entries list is managed differently and is not necessarily a 1:1 representation of
            // the dictionary.  This allows for temporary duplicate keys, something the dictionary cannot do, while modifications
            // are being made in the Inspector since the default behavior is to duplicate the last entry when adding a new one.

            // Override the first entry that has a matching key from the dictionary, otherwise add to entries.
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                if (TryFindSerializableIndex(pair.Key, out int index))
                {
                    entries[index] = new SerializableDictionaryEntry(pair.Key, pair.Value);
                }
                else
                {
                    entries.Add(new SerializableDictionaryEntry(pair.Key, pair.Value));
                }
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            base.Clear();

            foreach (SerializableDictionaryEntry entry in entries)
            {
                base.TryAdd(entry.Key, entry.Value);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Removes all keys and values from the <see cref="SerializableDictionary{TKey, TValue}"/>.
        /// </summary>
        public new void Clear()
        {
            entries.Clear();
            base.Clear();
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="SerializableDictionary{TKey, TValue}"/>, and copies the element to the value parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The value of the element that was removed, or the default value if key was not found.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public new bool Remove(TKey key, out TValue value)
        {
            if (base.Remove(key, out value))
            {
                if (TryFindSerializableIndex(key, out int index))
                {
                    entries.RemoveAt(index);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the value with the specified key from the <see cref="SerializableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        public new bool Remove(TKey key)
        {
            return Remove(key, out _);
        }

        private bool TryFindSerializableIndex(TKey key, out int index)
        {
            var keyComparer = EqualityComparer<TKey>.Default;

            index = entries.FindIndex((entry) => keyComparer.Equals(entry.Key, key));
            return index != -1;
        }
#endif

        [Serializable]
        private struct SerializableDictionaryEntry
        {
            [SerializeField]
            private TKey key;

            public TKey Key => key;

            [SerializeField]
            private TValue value;

            public TValue Value => value;

            public SerializableDictionaryEntry(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }
    }
}
