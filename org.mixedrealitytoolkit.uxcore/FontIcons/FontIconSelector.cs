// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MixedReality.Toolkit.UX
{
    /// <summary>
    /// Allows the user to select a specific icon for display via a Unity text component.
    /// </summary>
    [AddComponentMenu("MRTK/UX/Font Icon Selector")]
    public class FontIconSelector : MonoBehaviour
    {
        [Tooltip("The FontIconSet that contains the icons available for use.")]
        [SerializeField]
        private FontIconSet fontIcons;

        /// <summary>
        /// The <see cref="FontIconSet"/> that contains the icons
        /// available for use, and their human-readable names.
        /// </summary>
        public FontIconSet FontIcons => fontIcons;

        [Tooltip("The currently selected icon's name, as defined by the FontIconSet.")]
        [SerializeField]
        private string currentIconName;

        /// <summary>
        /// The currently selected icon's name, as defined by the <see cref="FontIcons"/>.
        /// </summary>
        public string CurrentIconName
        {
            get => currentIconName;

            set
            {
                if (value != currentIconName)
                {
                    SetIcon(value);
                }
            }
        }

        [Tooltip("The Unity text component used to show the icon.")]
        [SerializeField]
        private TMP_Text textMeshProComponent;

        /// <summary>
        /// The Unity text component used to show the icon.
        /// </summary>
        public TMP_Text TextMeshProComponent => textMeshProComponent;

        /// <summary>
        /// A temporary variable used to migrate instances of FontIconSelector to use new FontIconSetDefinition names.
        /// </summary>
        /// <remarks>TODO: Remove this after some time to ensure users have successfully migrated.</remarks>
        [SerializeField, HideInInspector]
        private bool migratedSuccessfully = false;

        /// <summary>
        /// A Unity event function that is called when the script component is added to a GameObject.
        /// </summary>
        private void Reset()
        {
            textMeshProComponent = GetComponent<TMP_Text>();
        }

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            if (textMeshProComponent == null)
            {
                textMeshProComponent = GetComponent<TMP_Text>();
            }
            TryMigrate();
            SetIcon(currentIconName);
        }

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            SetIcon(currentIconName);
        }

        /// <summary>
        /// Looks up the Unicode value for the specified icon name and applies it to the text component.
        /// </summary>
        /// <param name="newIconName">The descriptive name of the icon to set.</param>
        private void SetIcon(string newIconName)
        {
            if (fontIcons != null && textMeshProComponent != null && fontIcons.TryGetGlyphIcon(newIconName, out uint unicodeValue))
            {
                currentIconName = newIconName;
                textMeshProComponent.text = FontIconSet.ConvertUnicodeToHexString(unicodeValue);
            }
        }

        /// <summary>
        /// Attempts to migrate the icon selector to use descriptive names.
        /// </summary>
        /// <returns><see langword="true"/> if a migration was successfully performed or an empty icon state was resolved, otherwise <see langword="false"/>.</returns>
        public bool TryMigrate()
        {
            if (fontIcons == null || fontIcons.FontIconSetDefinition == null)
            {
                return false;
            }

            uint unicodeValue = 0;

            if (textMeshProComponent != null && !string.IsNullOrEmpty(textMeshProComponent.text))
            {
                unicodeValue = FontIconSet.ConvertHexStringToUnicode(textMeshProComponent.text);
            }

            if (unicodeValue == 0)
            {
                // If there's no icon text assigned, there's nothing to migrate.
                if (textMeshProComponent != null)
                {
                    if (!migratedSuccessfully)
                    {
                        migratedSuccessfully = true;
                        return true;
                    }
                }
                return false;
            }

            // Check if the current icon name is already in sync with the text component's unicode value.
            if (fontIcons.TryGetGlyphIcon(currentIconName, out uint expectedUnicode) && expectedUnicode == unicodeValue)
            {
                if (!migratedSuccessfully)
                {
                    migratedSuccessfully = true;
                    return true;
                }
                return false;
            }

            bool foundMatch = false;

            foreach (KeyValuePair<string, uint> kv in fontIcons.GlyphIconsByName)
            {
                if (kv.Value == unicodeValue)
                {
                    if (currentIconName != kv.Key)
                    {
                        Debug.Log($"[{nameof(FontIconSelector)}] Successfully migrated icon: \"{currentIconName}\" to \"{kv.Key}\"", this);

                        // Update the backing field directly to avoid calling the property setter during initialization.
                        currentIconName = kv.Key;
                    }

                    migratedSuccessfully = true;
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                Debug.LogWarning($"[{nameof(FontIconSelector)}] Failed to migrate icon. Unicode value '{unicodeValue}' was not found in FontIconSet '{fontIcons.name}'.", this);
            }

            return foundMatch;
        }
    }
}
