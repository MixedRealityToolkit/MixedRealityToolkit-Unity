// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace MixedReality.Toolkit.Editor
{
    /// <summary>
    /// A custom Unity editor for the <see cref="FontIconSet"/> class.
    /// </summary>
    [CustomEditor(typeof(FontIconSet))]
    [CanEditMultipleObjects]
    public class FontIconSetInspector : UnityEditor.Editor
    {
        private const string ShowGlyphIconsFoldoutKey = "MixedRealityToolkit.FontIconSet.ShowIconFoldout";
        private const string AvailableIconsFoldoutKey = "MixedRealityToolkit.FontIconSet.ShowAvailableIcons";
        private const string SelectedIconsFoldoutKey = "MixedRealityToolkit.FontIconSet.ShowSelectedIcons";

        private const string DefaultShaderName = "TextMeshPro/Distance Field SSD"; // Only used for presentation in inspector, not at runtime.
        private const int GlyphDrawSize = 75;
        private const int ButtonDimension = 75;
        private const int MaxButtonsPerColumn = 6;

        private const string NoIconFontMessage = "No icon font asset selected. Icon fonts will be unavailable.";
        private const string DownloadIconFontMessage = "For instructions on how to install the HoloLens icon font asset, click the button below.";
        private const string HoloLensIconFontUrl = "https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/button";
        private const string MDL2IconFontName = "holomdl2";
        private const string TextMeshProMenuItem = "Window/TextMeshPro/Font Asset Creator";

        private SerializedProperty iconFontAssetProp = null;
        private SerializedProperty fontIconSetDefinitionProp = null;

        private SortedList<uint, string> iconEntries = new SortedList<uint, string>();

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            FontIconSet fontIconSet = (FontIconSet)target;
            iconFontAssetProp = serializedObject.FindProperty("iconFontAsset");
            fontIconSetDefinitionProp = serializedObject.FindProperty("fontIconSetDefinition");

            // Make a list out of dictionary to avoid changing order while editing names
            foreach (KeyValuePair<string, uint> kv in fontIconSet.GlyphIconsByName)
            {
                iconEntries.Add(kv.Value, kv.Key);
            }
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            bool showGlyphIconFoldout = SessionState.GetBool(ShowGlyphIconsFoldoutKey, false);
            bool showAvailableIcons = SessionState.GetBool(AvailableIconsFoldoutKey, true);
            bool showSelectedIcons = SessionState.GetBool(SelectedIconsFoldoutKey, true);

            serializedObject.Update();

            showGlyphIconFoldout = EditorGUILayout.Foldout(showGlyphIconFoldout, "Font Icons", true);
            if (showGlyphIconFoldout)
            {
                EditorGUILayout.PropertyField(iconFontAssetProp);
                EditorGUILayout.PropertyField(fontIconSetDefinitionProp);

                if (iconFontAssetProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(NoIconFontMessage, MessageType.Warning);
                    if (!CheckIfHoloLensIconFontExists())
                    {
                        EditorGUILayout.HelpBox(DownloadIconFontMessage, MessageType.Info);
                        if (GUILayout.Button("View Font Asset Icons Documentation"))
                        {
                            EditorApplication.ExecuteMenuItem(TextMeshProMenuItem);
                            Application.OpenURL(HoloLensIconFontUrl);
                        }
                    }
                }
                else
                {
                    FontIconSet fontIconSet = target as FontIconSet;
                    TMP_FontAsset fontAsset = iconFontAssetProp.objectReferenceValue as TMP_FontAsset;

                    showAvailableIcons = EditorGUILayout.Foldout(showAvailableIcons, "Available Icons", true);
                    if (showAvailableIcons)
                    {
                        if (fontAsset.characterTable.Count == 0)
                        {
                            EditorGUILayout.HelpBox("No icons are available in this font. The font may be configured incorrectly.", MessageType.Warning);
                            if (GUILayout.Button("Open Font Editor"))
                            {
                                Selection.activeObject = fontAsset;
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Click an icon to add it to your selected icons.", MessageType.Info);
                            if (GUILayout.Button("Open Font Editor"))
                            {
                                Selection.activeObject = fontAsset;
                            }

                            DrawFontGlyphsGrid(fontAsset, fontIconSet, MaxButtonsPerColumn);
                        }

                        EditorGUILayout.Space();
                    }

                    showSelectedIcons = EditorGUILayout.Foldout(showSelectedIcons, "Selected Icons", true);
                    if (showSelectedIcons)
                    {
                        if (fontIconSet.GlyphIconsByName.Count > 0)
                        {
                            EditorGUILayout.HelpBox("These icons will appear in the button config helper inspector. Click an icon to remove it from this list.", MessageType.Info);

                            if (fontIconSetDefinitionProp.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("It's recommended to use a Font Icon Set Definition to ensure consistent icon names across icon sets.", MessageType.Warning);
                            }

                            int column = 0;
                            string iconToRemove = null;
                            string iconToRename = null;
                            EditorGUILayout.BeginHorizontal();
                            foreach (KeyValuePair<uint, string> iconEntry in iconEntries)
                            {
                                if (column >= MaxButtonsPerColumn)
                                {
                                    column = 0;
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                }

                                EditorGUILayout.BeginVertical(GUILayout.Width(ButtonDimension));

                                if (GUILayout.Button(" ",
                                    GUILayout.Height(ButtonDimension),
                                    GUILayout.MaxWidth(ButtonDimension)))
                                {
                                    iconToRemove = iconEntry.Value;
                                }

                                Rect textureRect = GUILayoutUtility.GetLastRect();
                                textureRect.width = GlyphDrawSize;
                                textureRect.height = GlyphDrawSize;
                                EditorDrawTMPGlyph(textureRect, iconEntry.Key, fontAsset);

                                string currentName = EditorGUILayout.TextField(iconEntry.Value);
                                if (currentName != iconEntry.Value)
                                {
                                    iconToRename = currentName;
                                    iconToRemove = iconEntry.Value;
                                }
                                EditorGUILayout.EndVertical();

                                column++;
                            }
                            EditorGUILayout.EndHorizontal();

                            if (iconToRename != null)
                            {
                                UpdateIconName(fontIconSet, iconToRemove, iconToRename);
                            }
                            else if (iconToRemove != null)
                            {
                                RemoveIcon(fontIconSet, iconToRemove);
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("No icons added yet. Click available icons to add.", MessageType.Info);
                        }
                    }
                }
            }

            SessionState.SetBool(ShowGlyphIconsFoldoutKey, showGlyphIconFoldout);
            SessionState.SetBool(AvailableIconsFoldoutKey, showAvailableIcons);
            SessionState.SetBool(SelectedIconsFoldoutKey, showSelectedIcons);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw a grid of buttons than can be clicked to select a glyph from a set up glyphs.
        /// </summary>
        /// <param name="fontIconSet">The set of font glyphs to draw.</param>
        /// <param name="maxButtonsPerColumn">The number of buttons per column.</param>
        [Obsolete("This method has been removed.")]
        public void DrawFontGlyphsGrid(FontIconSet fontIconSet, int maxButtonsPerColumn)
        {
            DrawFontGlyphsGrid(fontIconSet.IconFontAsset, fontIconSet, maxButtonsPerColumn);
        }

        /// <summary>
        /// Draw a grid of buttons than can be clicked to select a glyph from a set up glyphs.
        /// </summary>
        /// <param name="fontAsset">The font asset containing the glyphs.</param>
        /// <param name="fontIconSet">The set of font glyphs to draw.</param>
        /// <param name="maxButtonsPerColumn">The number of buttons per column.</param>
        private void DrawFontGlyphsGrid(TMP_FontAsset fontAsset, FontIconSet fontIconSet, int maxButtonsPerColumn)
        {
            int column = 0;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < fontAsset.characterTable.Count; i++)
            {
                if (column >= maxButtonsPerColumn)
                {
                    column = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                using (new EditorGUI.DisabledGroupScope(fontIconSet.GlyphIconsByName.ContainsValue(fontAsset.characterTable[i].unicode)))
                {
                    if (GUILayout.Button(" ",
                        GUILayout.Height(ButtonDimension),
                        GUILayout.MaxWidth(ButtonDimension)))
                    {
                        AddIcon(fontIconSet, fontAsset.characterTable[i].unicode);
                        EditorUtility.SetDirty(target);
                    }

                    Rect textureRect = GUILayoutUtility.GetLastRect();
                    textureRect.width = GlyphDrawSize;
                    textureRect.height = GlyphDrawSize;
                    EditorDrawTMPGlyph(textureRect, fontAsset, fontAsset.characterTable[i]);
                }

                column++;
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool AddIcon(FontIconSet fontIconSet, uint unicodeValue)
        {
            string name = $"Icon {unicodeValue}";
            if (fontIconSet.AddIcon(name, unicodeValue))
            {
                iconEntries.Add(unicodeValue, name);
                EditorUtility.SetDirty(fontIconSet);
                return true;
            }
            return false;
        }

        private bool RemoveIcon(FontIconSet fontIconSet, string iconName)
        {
            if (fontIconSet.TryGetGlyphIcon(iconName, out uint unicodeValue) && fontIconSet.RemoveIcon(iconName))
            {
                iconEntries.Remove(unicodeValue);
                EditorUtility.SetDirty(fontIconSet);
                return true;
            }
            return false;
        }

        private void UpdateIconName(FontIconSet fontIconSet, string oldName, string newName)
        {
            if (fontIconSet.UpdateIconName(oldName, newName) && fontIconSet.TryGetGlyphIcon(newName, out uint unicodeValue))
            {
                iconEntries[unicodeValue] = newName;
                EditorUtility.SetDirty(fontIconSet);
            }
        }

        private bool CheckIfHoloLensIconFontExists()
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(Font)}"))
            {
                if (AssetDatabase.GUIDToAssetPath(guid).Contains(MDL2IconFontName))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Draws a Text Mesh Pro glyph in the supplied <see cref="Rect"/>.
        /// </summary>
        public static void EditorDrawTMPGlyph(Rect position, uint unicode, TMP_FontAsset fontAsset, bool selected = false, Material fontRenderMaterial = null)
        {
            if (fontAsset.characterLookupTable.TryGetValue(unicode, out TMP_Character character))
            {
                EditorDrawTMPGlyph(position, fontAsset, character, selected, fontRenderMaterial);
            }
        }

        /// <summary>
        /// Draws a Text Mesh Pro glyph in the supplied <see cref="Rect"/>.
        /// </summary>
        public static void EditorDrawTMPGlyph(Rect glyphRect, TMP_FontAsset fontAsset, TMP_Character character, bool selected = false, Material fontRenderMaterial = null)
        {
            if (Event.current.type == EventType.Repaint)
            {
                try
                {
                    const float IconSizeMultiplier = 0.625f;

                    // Get a reference to the Glyph Table
                    int glyphIndex = (int)character.glyphIndex;
                    int elementIndex = fontAsset.glyphTable.FindIndex(item => item.index == glyphIndex);

                    if (elementIndex >= 0)
                    {
                        Glyph glyph = character.glyph;

                        // Get reference to atlas texture.
                        int atlasIndex = glyph.atlasIndex;
                        Texture2D atlasTexture = fontAsset.atlasTextures.Length > atlasIndex ? fontAsset.atlasTextures[atlasIndex] : null;

                        if (atlasTexture != null)
                        {
                            if (fontRenderMaterial == null)
                            {
                                fontRenderMaterial = new Material(Shader.Find(DefaultShaderName));
                            }

                            Material glyphMaterial = fontRenderMaterial;
                            glyphMaterial.mainTexture = atlasTexture;
                            glyphMaterial.SetColor("_FaceColor", selected ? Color.green : Color.white);

                            int glyphOriginX = glyph.glyphRect.x;
                            int glyphOriginY = glyph.glyphRect.y;
                            int glyphWidth = glyph.glyphRect.width;
                            int glyphHeight = glyph.glyphRect.height;

                            float normalizedHeight = fontAsset.faceInfo.ascentLine - fontAsset.faceInfo.descentLine;
                            float scale = Mathf.Min(glyphRect.width, glyphRect.height) / normalizedHeight * IconSizeMultiplier;

                            // Compute the normalized texture coordinates
                            Rect texCoords = new Rect((float)glyphOriginX / atlasTexture.width, (float)glyphOriginY / atlasTexture.height, (float)glyphWidth / atlasTexture.width, (float)glyphHeight / atlasTexture.height);

                            glyphWidth = (int)Mathf.Min(GlyphDrawSize, glyphWidth * scale);
                            glyphHeight = (int)Mathf.Min(GlyphDrawSize, glyphHeight * scale);

                            glyphRect.x += (glyphRect.width - glyphWidth) / 2;
                            glyphRect.y += (glyphRect.height - glyphHeight) / 2;
                            glyphRect.width = glyphWidth;
                            glyphRect.height = glyphHeight;

                            // Could switch to using the default material of the font asset which would require passing scale to the shader.
                            Graphics.DrawTexture(glyphRect, atlasTexture, texCoords, 0, 0, 0, 0, new Color(1f, 1f, 1f), glyphMaterial);
                        }
                    }
                }
                catch (Exception)
                {
                    EditorGUILayout.LabelField("Couldn't draw character icon. UnicodeValue may not be available in the font asset.");
                }
            }
        }
    }
}
