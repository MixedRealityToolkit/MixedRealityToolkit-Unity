// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomEditor(typeof(Theme), true)]
    public class ThemeEditor : UnityEditor.Editor
    {
        private SerializedProperty themeDefinitionProp = null;
        private SerializedProperty themeItemsProp = null;

        private static bool itemsFoldout = false;

        private static readonly System.Collections.Generic.HashSet<Type> failedTypes = new System.Collections.Generic.HashSet<Type>();

        private string nameField;
        private string dataField;
        private string valueField;

        static ThemeEditor()
        {
            AssemblyReloadEvents.afterAssemblyReload += failedTypes.Clear;
        }

        protected void OnEnable()
        {
            themeDefinitionProp = serializedObject.FindProperty("definition");
            themeItemsProp = serializedObject.FindProperty("themeItems");

            nameField = InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.Name));
            dataField = InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Data));
            valueField = InspectorUIUtility.GetBackingField(nameof(BaseThemeItemData<object>.Value));

            // Clear the cache when the asset is (re)selected
            failedTypes.Clear();
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(themeDefinitionProp);

            ThemeDefinition themeDefinition = themeDefinitionProp.objectReferenceValue as ThemeDefinition;

            if (themeDefinition != null && themeDefinition.ThemeDefinitionItems != null && themeItemsProp != null)
            {
                itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Theme Values", true);
                if (itemsFoldout)
                {
                    if (Event.current.type == EventType.Layout)
                    {
                        ReconcileThemeItems(themeDefinition, themeItemsProp);

                        // Force the SerializedObject to immediately sync any newly instantiated
                        // [SerializeReference] payloads so their child properties (like 'Value')
                        // are fully discoverable during this exact same Layout pass.
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }

                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (int i = 0; i < themeItemsProp.arraySize; i++)
                        {
                            SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(i);
                            SerializedProperty dataProp = themeItemProp.FindPropertyRelative(dataField);
                            SerializedProperty valueProp = dataProp?.FindPropertyRelative(valueField);

                            string themeItemName = themeItemProp.FindPropertyRelative(nameField).stringValue;
                            string displayLabel = string.IsNullOrWhiteSpace(themeItemName) ? "(null)" : themeItemName;

                            if (valueProp != null)
                            {
                                // Draw just the Value field, labeled with the item name,
                                // skipping the intermediate "Data" foldout entirely.
                                EditorGUILayout.PropertyField(valueProp, new GUIContent(displayLabel), true);
                            }
                            else
                            {
                                if (dataProp?.managedReferenceValue == null)
                                {
                                    if (i < themeDefinition.ThemeDefinitionItems.Length)
                                    {
                                        var definitionItem = themeDefinition.ThemeDefinitionItems[i];
                                        if (definitionItem.DataType?.Type == null)
                                        {
                                            EditorGUILayout.HelpBox($"'{displayLabel}' has no valid DataType selected in the ThemeDefinition.", MessageType.Warning);
                                        }
                                        else
                                        {
                                            EditorGUILayout.HelpBox($"Failed to initialize data for '{displayLabel}'. Ensure its DataType ({definitionItem.DataType.Type.Name}) is a concrete class with a default constructor.", MessageType.Warning);
                                        }
                                    }

                                    using (new EditorGUI.DisabledScope(true))
                                    {
                                        EditorGUILayout.LabelField(displayLabel, "null");
                                    }
                                }
                                else
                                {
                                    // Fallback for any item whose Data doesn't follow the
                                    // BaseThemeItemData<T> shape.
                                    EditorGUILayout.PropertyField(dataProp, new GUIContent(displayLabel), true);
                                }
                            }
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ReconcileThemeItems(ThemeDefinition themeDefinition, SerializedProperty themeItemsProp)
        {
            var definitionItems = themeDefinition.ThemeDefinitionItems;
            for (int i = 0; i < definitionItems.Length; i++)
            {
                var definitionItem = definitionItems[i];
                if (definitionItem == null) { continue; }
                string themeDefinitionItemName = definitionItem.Name;

                SerializedProperty themeItem = themeItemsProp.arraySize > i ? themeItemsProp.GetArrayElementAtIndex(i) : null;
                if (themeItem == null
                    || themeItem.FindPropertyRelative(nameField).stringValue != themeDefinitionItemName)
                {
                    // Search for an existing item with the matching name further in the list,
                    // so we can move it into position rather than discarding its saved values.
                    int existingIndex = -1;
                    for (int j = i + 1; j < themeItemsProp.arraySize; j++)
                    {
                        string existingName = themeItemsProp.GetArrayElementAtIndex(j)
                            .FindPropertyRelative(nameField)
                            .stringValue;
                        if (existingName == themeDefinitionItemName)
                        {
                            existingIndex = j;
                            break;
                        }
                    }

                    if (existingIndex >= 0)
                    {
                        // Move the found item up to position i, preserving its saved values.
                        themeItemsProp.MoveArrayElement(existingIndex, i);
                        themeItem = themeItemsProp.GetArrayElementAtIndex(i);
                    }
                    else
                    {
                        // No existing item found — insert a fresh one with default values.
                        themeItemsProp.InsertArrayElementAtIndex(i);
                        themeItem = themeItemsProp.GetArrayElementAtIndex(i);
                        themeItem.managedReferenceValue = new Theme.ThemeItem(themeDefinitionItemName, null);
                    }
                }

                // Unified instantiation and auto-healing for missing or mismatched data types
                Type expectedType = definitionItem.DataType?.Type;
                SerializedProperty dataProp = themeItem.FindPropertyRelative(dataField);
                Type actualType = dataProp?.managedReferenceValue?.GetType();

                if (dataProp != null && expectedType != null && expectedType != actualType && !failedTypes.Contains(expectedType))
                {
                    try
                    {
                        dataProp.managedReferenceValue = Activator.CreateInstance(expectedType);
                    }
                    catch
                    {
                        failedTypes.Add(expectedType);
                    }
                }
            }
            themeItemsProp.arraySize = definitionItems.Length;
        }
    }
}
