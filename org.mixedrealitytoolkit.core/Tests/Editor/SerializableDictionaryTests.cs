// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// Unit tests for SerializableDictionary.
    /// These run outside of PlayMode and do not require Unity engine initialization.
    /// </summary>
    public class SerializableDictionaryTests
    {
        [Test]
        public void Serialization_RestoresDictionary_FromInternalEntries()
        {
            var dict = new SerializableDictionary<string, int>();
            dict.Add("Key1", 100);
            dict.Add("Key2", 200);

            ISerializationCallbackReceiver receiver = dict;

            // Simulate Unity preparing to serialize the object (populates the internal 'entries' list)
            receiver.OnBeforeSerialize();

            // Clear the base dictionary to simulate starting fresh after deserialization
            ((Dictionary<string, int>)dict).Clear();
            Assert.AreEqual(0, dict.Count, "Base dictionary should be empty before deserialization.");

            // Simulate Unity finishing deserialization (repopulates the dictionary from 'entries')
            receiver.OnAfterDeserialize();

            Assert.AreEqual(2, dict.Count, "Dictionary should have restored 2 items.");
            Assert.AreEqual(100, dict["Key1"]);
            Assert.AreEqual(200, dict["Key2"]);
        }

        [Test]
        public void EditorOverride_Clear_RemovesAllSerializedEntries()
        {
            var dict = new SerializableDictionary<string, int>();
            dict.Add("Key1", 100);

            ISerializationCallbackReceiver receiver = dict;
            receiver.OnBeforeSerialize(); // Populate internal list

            // Call the custom overridden Clear()
            dict.Clear();

            // Attempt to deserialize (which would normally restore Key1 if the internal list wasn't cleared)
            receiver.OnAfterDeserialize();

            Assert.AreEqual(0, dict.Count, "Dictionary should remain empty because the internal serialized entries were cleared.");
        }

        [Test]
        public void EditorOverride_Remove_RemovesSpecificSerializedEntry()
        {
            var dict = new SerializableDictionary<string, int>();
            dict.Add("A", 1);
            dict.Add("B", 2);
            dict.Add("C", 3);

            ISerializationCallbackReceiver receiver = dict;
            receiver.OnBeforeSerialize();

            // Use the overridden remove method which should also remove from the internal list
            bool removedB = dict.Remove("B");
            bool removedC = dict.Remove("C", out int valC);

            // Clear the dictionary and restore from the serialized list to verify they are gone
            ((Dictionary<string, int>)dict).Clear();
            receiver.OnAfterDeserialize();

            Assert.IsTrue(removedB, "Remove(key) should return true for existing key.");
            Assert.IsTrue(removedC, "Remove(key, out val) should return true for existing key.");
            Assert.AreEqual(3, valC, "Remove(key, out val) should output the correct value.");

            Assert.AreEqual(1, dict.Count, "Only 1 item should remain after deserialization.");
            Assert.IsTrue(dict.ContainsKey("A"), "Key 'A' should have been preserved.");
            Assert.IsFalse(dict.ContainsKey("B"), "Key 'B' should have been permanently removed.");
            Assert.IsFalse(dict.ContainsKey("C"), "Key 'C' should have been permanently removed.");
        }
    }
}
