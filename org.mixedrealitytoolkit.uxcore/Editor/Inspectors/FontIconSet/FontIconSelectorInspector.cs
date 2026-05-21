// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Editor
{
    [CustomEditor(typeof(FontIconSelector))]
    [CanEditMultipleObjects]
    class FontIconSelectorInspector : UnityEditor.Editor
    {
        private const string NoFontIconsMessage = "No FontIconSet profile selected. No icons available.";
        private const string EmptyFontIconSetMessage = "The selected FontIconSet profile has no icons defined. Please edit the FontIconSet.";

        private SerializedProperty fontIconsProp = null;
        private SerializedProperty currentIconNameProp = null;
        private SerializedProperty tmProProp = null;

        private GUIStyle currentButtonStyle = null;

        private float fontTileSize = 32;

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            fontIconsProp = serializedObject.FindProperty("fontIcons");
            currentIconNameProp = serializedObject.FindProperty("currentIconName");
            tmProProp = serializedObject.FindProperty("textMeshProComponent");

            bool migratedAny = false;
            foreach (Object targetObject in targets)
            {
                FontIconSelector selector = targetObject as FontIconSelector;
                if (selector != null)
                {
                    Undo.RecordObject(selector, "Migrate Font Icon Selector");

                    if (selector.TryMigrate())
                    {
                        EditorUtility.SetDirty(selector);
                        if (PrefabUtility.IsPartOfPrefabInstance(selector))
                        {
                            PrefabUtility.RecordPrefabInstancePropertyModifications(selector);
                        }
                        migratedAny = true;
                    }
                }
            }

            if (migratedAny)
            {
                serializedObject.Update();
            }
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            currentButtonStyle ??= new GUIStyle(GUI.skin.button);

            FontIconSelector fontIconSelector = (FontIconSelector)target;
            FontIconSet fontIconSet = fontIconSelector.FontIcons;

            EditorGUILayout.PropertyField(fontIconsProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(currentIconNameProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(tmProProp);
            EditorGUILayout.Space();

            if (fontIconsProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(NoFontIconsMessage, MessageType.Warning);
            }
            else if (!fontIconSet || fontIconSet.GlyphIconsByName.Count == 0)
            {
                EditorGUILayout.HelpBox(EmptyFontIconSetMessage, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("Choose the icon to show:");

                if (fontIconSet && fontIconSet.GlyphIconsByName.Count > 0)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        fontTileSize = EditorGUILayout.Slider("Zoom", fontTileSize, 16, 64, GUILayout.ExpandWidth(false));
                        DrawIconGrid(fontIconSelector, fontTileSize);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private int numColumns = 4;
        private Vector2 scrollAmount;

        public void DrawIconGrid(FontIconSelector fontIconSelector, float tileSize)
        {
            FontIconSet fontIconSet = fontIconSelector.FontIcons;
            TMP_FontAsset fontAsset = fontIconSet.IconFontAsset;
            int column = 0;

            scrollAmount = EditorGUILayout.BeginScrollView(scrollAmount, GUILayout.MaxHeight(128), GUILayout.MinHeight(64));
            EditorGUILayout.BeginHorizontal();

            List<KeyValuePair<string, uint>> sortedIcons = new List<KeyValuePair<string, uint>>(fontIconSet.GlyphIconsByName);
            sortedIcons.Sort((a, b) => a.Key.CompareTo(b.Key));

            foreach (KeyValuePair<string, uint> kvp in sortedIcons)
            {
                string iconName = kvp.Key;
                uint unicodeValue = kvp.Value;

                if (column >= numColumns)
                {
                    column = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                if (GUILayout.Button(" ",
                    GUILayout.Height(tileSize),
                    GUILayout.Width(tileSize)))
                {
                    if (fontIconSelector.TextMeshProComponent != null)
                    {
                        Undo.RecordObjects(new Object[] { fontIconSelector, fontIconSelector.TextMeshProComponent }, "Changed icon");
                    }
                    else
                    {
                        Undo.RecordObject(fontIconSelector, "Changed icon");
                    }

                    fontIconSelector.CurrentIconName = iconName;

                    PrefabUtility.RecordPrefabInstancePropertyModifications(fontIconSelector);
                    if (fontIconSelector.TextMeshProComponent != null)
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(fontIconSelector.TextMeshProComponent);
                    }
                }

                Rect textureRect = GUILayoutUtility.GetLastRect();
                if (textureRect.yMin + 8 < scrollAmount.y || textureRect.yMax - 8 > scrollAmount.y + 128)
                {
                    unicodeValue = 0;
                }
                textureRect.width = tileSize;
                textureRect.height = tileSize;
                FontIconSetInspector.EditorDrawTMPGlyph(textureRect, unicodeValue, fontAsset, iconName == fontIconSelector.CurrentIconName);

                column++;
            }

            EditorGUILayout.EndHorizontal();

            if (Event.current.type == EventType.Repaint)
            {
                float editorWindowWidth = GUILayoutUtility.GetLastRect().width;
                numColumns = (int)Mathf.Floor(editorWindowWidth / (tileSize + currentButtonStyle.margin.right));
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
