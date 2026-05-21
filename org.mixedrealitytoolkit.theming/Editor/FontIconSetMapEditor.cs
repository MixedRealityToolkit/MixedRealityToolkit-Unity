// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomEditor(typeof(FontIconSetMap))]
    public class FontIconSetMapEditor : UnityEditor.Editor
    {
        private SerializedProperty setDefinitionProperty;
        private SerializedProperty fontIconSetsProperty;

        private int numColumns = 4;
        private bool editToggled = false;
        private List<bool> foldoutStates = new();

        // Deferred State Variables
        private FontIconSet pendingIconSet = null;
        private string pendingIconToRenameOld = null;
        private string pendingIconToRenameNew = null;

        public void OnEnable()
        {
            setDefinitionProperty = serializedObject.FindProperty("setDefinition");
            fontIconSetsProperty = serializedObject.FindProperty("fontIconSets");
            // Ensure we have enough values to track all assigned icon sets
            for (int i = foldoutStates.Count; i < fontIconSetsProperty.arraySize; i++)
            {
                foldoutStates.Add(false);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Event.current.type == EventType.Layout)
            {
                if (pendingIconSet != null && pendingIconToRenameOld != null && pendingIconToRenameNew != null)
                {
                    pendingIconSet.UpdateIconName(pendingIconToRenameOld, pendingIconToRenameNew);
                    EditorUtility.SetDirty(pendingIconSet);
                    pendingIconSet = null;
                    pendingIconToRenameOld = null;
                    pendingIconToRenameNew = null;
                }
            }

            EditorGUILayout.PropertyField(setDefinitionProperty);
            EditorGUILayout.PropertyField(fontIconSetsProperty);
            editToggled = EditorGUILayout.Toggle("Edit Names", editToggled);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            FontIconSetDefinition setDefinition = setDefinitionProperty.objectReferenceValue as FontIconSetDefinition;
            const int TileSize = 90;

            Dictionary<string, List<FontIconSet>> iconMatches = new Dictionary<string, List<FontIconSet>>();

            for (int i = 0; i < fontIconSetsProperty.arraySize; i++)
            {
                FontIconSet iconSet = fontIconSetsProperty.GetArrayElementAtIndex(i).objectReferenceValue as FontIconSet;
                if (iconSet == null)
                {
                    continue;
                }

                List<KeyValuePair<string, uint>> glyphs = new List<KeyValuePair<string, uint>>(iconSet.GlyphIconsByName);

                int column = 0;

                if (editToggled)
                {
                    glyphs.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

                    if (foldoutStates.Count <= i)
                    {
                        foldoutStates.Add(false);
                    }

                    if (foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], iconSet.name, true))
                    {
                        string[] availableNamesArray = null;
                        if (setDefinition != null && setDefinition.IconNames != null)
                        {
                            availableNamesArray = FontIconSetInspector.GetAvailableIconNames(iconSet, setDefinition);
                            FontIconSetInspector.DrawInvalidIconNameHelpBox(iconSet, setDefinition);
                        }

                        EditorGUILayout.BeginHorizontal();
                        foreach (KeyValuePair<string, uint> kv in glyphs)
                        {
                            if (column >= numColumns)
                            {
                                column = 0;
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginHorizontal();
                            }

                            EditorGUILayout.BeginVertical(GUILayout.Width(TileSize));

                            Rect textureRect = GUILayoutUtility.GetRect(TileSize, TileSize, GUI.skin.box);
                            EditorGUI.DrawRect(textureRect, new Color(0f, 0f, 0f, 0.1f));
                            FontIconSetInspector.EditorDrawTMPGlyph(textureRect, kv.Value, iconSet.IconFontAsset);

                            if (availableNamesArray != null && FontIconSetInspector.DrawIconNamePopup(kv.Key, availableNamesArray, setDefinition.IconNames, TileSize, out string newName))
                            {
                                pendingIconSet = iconSet;
                                pendingIconToRenameOld = kv.Key;
                                pendingIconToRenameNew = newName;
                            }

                            EditorGUILayout.EndVertical();

                            column++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, uint> kv in glyphs)
                    {
                        if (iconMatches.TryGetValue(kv.Key, out List<FontIconSet> icons))
                        {
                            icons.Add(iconSet);
                        }
                        else
                        {
                            iconMatches.Add(kv.Key, new List<FontIconSet> { iconSet });
                        }
                    }
                }
            }

            if (!editToggled)
            {
                // Sort the matched keys so the icons display in a predictable alphabetical order.
                List<string> sortedKeys = new List<string>(iconMatches.Keys);
                sortedKeys.Sort();

                foreach (string key in sortedKeys)
                {
                    List<FontIconSet> icons = iconMatches[key];

                    if (setDefinition != null && setDefinition.IconNames != null)
                    {
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.LabelField(key, EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();

                        for (int i = 0; i < fontIconSetsProperty.arraySize; i++)
                        {
                            FontIconSet iconSet = fontIconSetsProperty.GetArrayElementAtIndex(i).objectReferenceValue as FontIconSet;
                            Rect iconRect = GUILayoutUtility.GetRect(TileSize, TileSize, GUI.skin.box);
                            EditorGUI.DrawRect(iconRect, new Color(0f, 0f, 0f, 0.1f));

                            if (icons.Contains(iconSet))
                            {
                                FontIconSetInspector.EditorDrawTMPGlyph(iconRect, iconSet.GlyphIconsByName[key], iconSet.IconFontAsset);
                            }
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.EndVertical();
                    }
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                float editorWindowWidth = GUILayoutUtility.GetLastRect().width;
                numColumns = (int)Mathf.Floor(editorWindowWidth / (TileSize + GUI.skin.button.margin.right));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
