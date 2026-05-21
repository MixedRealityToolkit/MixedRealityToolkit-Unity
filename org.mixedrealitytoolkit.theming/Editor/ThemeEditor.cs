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

        private string nameField;
        private string dataField;
        private string valueField;

        protected void OnEnable()
        {
            themeDefinitionProp = serializedObject.FindProperty("definition");
            themeItemsProp = serializedObject.FindProperty("themeItems");

            nameField = InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.Name));
            dataField = InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Data));
            valueField = InspectorUIUtility.GetBackingField(nameof(BaseThemeItemData<object>.Value));
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
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
                    }

                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (int i = 0; i < themeItemsProp.arraySize; i++)
                        {
                            SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(i);
                            SerializedProperty dataProp = themeItemProp.FindPropertyRelative(dataField);
                            SerializedProperty valueProp = dataProp?.FindPropertyRelative(valueField);

                            if (valueProp != null)
                            {
                                // Draw just the Value field, labelled with the item name,
                                // skipping the intermediate "Data" foldout entirely.
                                string themeItemName = themeItemProp.FindPropertyRelative(nameField).stringValue;
                                EditorGUILayout.PropertyField(valueProp, new GUIContent(themeItemName), true);
                            }
                            else
                            {
                                // Fallback for any item whose Data doesn't follow the
                                // BaseThemeItemData<T> shape (e.g. a null reference).
                                EditorGUILayout.PropertyField(themeItemProp, true);
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
                    }
                    else
                    {
                        // No existing item found — insert a fresh one with default values.

                        themeItemsProp.InsertArrayElementAtIndex(i);
                        themeItem = themeItemsProp.GetArrayElementAtIndex(i);

                        Type dataType = definitionItem.DataType?.Type;
                        object dataInstance = null;

                        if (dataType != null)
                        {
                            try
                            {
                                dataInstance = Activator.CreateInstance(dataType);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"Failed to instantiate data for ThemeItem '{themeDefinitionItemName}' (Type: {dataType.Name}). Falling back to null. Exception: {e.Message}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Could not resolve DataType for ThemeItem '{themeDefinitionItemName}'. Falling back to null.");
                        }

                        themeItem.managedReferenceValue = new Theme.ThemeItem(themeDefinitionItemName, dataInstance);
                    }
                }
            }
            themeItemsProp.arraySize = definitionItems.Length;
        }
    }
}
