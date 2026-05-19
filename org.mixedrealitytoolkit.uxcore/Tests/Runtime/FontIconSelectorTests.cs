// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the <see cref="FontIconSelector"/> component.
    /// </summary>
    public class FontIconSelectorTests
    {
        [UnityTest]
        public IEnumerator TestMigrationToDescriptiveNames()
        {
            // 1. Create a mock FontIconSetDefinition
            FontIconSetDefinition iconSetDef = ScriptableObject.CreateInstance<FontIconSetDefinition>();

            // 2. Create a mock FontIconSet and assign the definition to it via Reflection
            FontIconSet iconSet = ScriptableObject.CreateInstance<FontIconSet>();
            typeof(FontIconSet).GetField("fontIconSetDefinition", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(iconSet, iconSetDef);

            // 3. Add an icon with a descriptive name and a specific unicode value
            uint checkmarkUnicode = 0xE10B;
            iconSet.AddIcon("Checkmark", checkmarkUnicode);

            // 4. Create a GameObject with a text component and a FontIconSelector
            GameObject go = new GameObject("IconSelectorTest");
            TMP_Text textComponent = go.AddComponent<TextMeshPro>();

            // Set the text component to the old un-migrated unicode character
            textComponent.text = FontIconSet.ConvertUnicodeToHexString(checkmarkUnicode);

            FontIconSelector selector = go.AddComponent<FontIconSelector>();

            // Assign our mock FontIconSet
            typeof(FontIconSelector).GetField("fontIcons", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(selector, iconSet);

            // At this point, the selector doesn't know its descriptive name
            Assert.AreNotEqual("Checkmark", selector.CurrentIconName);

            // 5. Trigger migration
            bool migrationSucceeded = selector.TryMigrate();

            // 6. Verify the migration accurately found and assigned the descriptive name
            Assert.IsTrue(migrationSucceeded, "TryMigrate should have returned true.");
            Assert.AreEqual("Checkmark", selector.CurrentIconName, "FontIconSelector did not migrate to the descriptive icon name.");

            // Cleanup
            Object.Destroy(go);
            Object.Destroy(iconSet);
            Object.Destroy(iconSetDef);

            yield return null;
        }
    }
}
