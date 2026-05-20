// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.UX;
using System.Collections.Generic;
using System.Linq;
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
            EditorGUILayout.PropertyField(setDefinitionProperty);
            EditorGUILayout.PropertyField(fontIconSetsProperty);
            editToggled = EditorGUILayout.Toggle("Edit Names", editToggled);
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);

            FontIconSetDefinition setDefinition = setDefinitionProperty.objectReferenceValue as FontIconSetDefinition;
            const int TileSize = 90;

            List<string> validNames = new List<string>();
            Dictionary<string, List<FontIconSet>> iconMatches = new Dictionary<string, List<FontIconSet>>();

            for (int i = 0; i < fontIconSetsProperty.arraySize; i++)
            {
                FontIconSet iconSet = fontIconSetsProperty.GetArrayElementAtIndex(i).objectReferenceValue as FontIconSet;
                if (iconSet == null)
                {
                    continue;
                }

                List<KeyValuePair<string, uint>> glyphs = iconSet.GlyphIconsByName.ToList();

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

                            if (setDefinition != null && setDefinition.IconNames != null)
                            {
                                validNames.Clear();
                                validNames.Add(kv.Key);
                                foreach (string name in setDefinition.IconNames)
                                {
                                    if (!iconSet.GlyphIconsByName.Keys.Contains(name))
                                    {
                                        validNames.Add(name);
                                    }
                                }

                                validNames.Sort();
                                string[] validNamesArray = validNames.ToArray();

                                using (var check = new EditorGUI.ChangeCheckScope())
                                {
                                    int selected = validNames.IndexOf(kv.Key);
                                    // If the currently selected name isn't in our icon set map names, highlight the popup
                                    Color oldColor = GUI.backgroundColor;
                                    if (!validNames.Contains(kv.Key))
                                    {
                                        GUI.backgroundColor = Color.yellow;
                                    }
                                    selected = EditorGUILayout.Popup(string.Empty, selected, validNamesArray, GUILayout.MaxWidth(TileSize));
                                    if (check.changed)
                                    {
                                        iconSet.UpdateIconName(kv.Key, validNamesArray[selected]);
                                        EditorUtility.SetDirty(iconSet);
                                    }
                                    GUI.backgroundColor = oldColor;
                                }
                            }

                            EditorGUILayout.EndVertical();

                            column++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    glyphs.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
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
                foreach (KeyValuePair<string, List<FontIconSet>> kv in iconMatches)
                {
                    if (setDefinition != null && setDefinition.IconNames != null)
                    {
                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.LabelField(kv.Key, EditorStyles.boldLabel);

                        EditorGUILayout.BeginHorizontal();

                        for (int i = 0; i < fontIconSetsProperty.arraySize; i++)
                        {
                            FontIconSet iconSet = fontIconSetsProperty.GetArrayElementAtIndex(i).objectReferenceValue as FontIconSet;
                            Rect iconRect = GUILayoutUtility.GetRect(TileSize, TileSize, GUI.skin.box);
                            EditorGUI.DrawRect(iconRect, new Color(0f, 0f, 0f, 0.1f));

                            if (kv.Value.Contains(iconSet))
                            {
                                FontIconSetInspector.EditorDrawTMPGlyph(iconRect, iconSet.GlyphIconsByName[kv.Key], iconSet.IconFontAsset);
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
