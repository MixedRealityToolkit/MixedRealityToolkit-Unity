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

        private SerializedObject themeDefinitionSerializedObject = null;
        private UnityEngine.Object cachedThemeDefinitionRef = null;

        private static bool itemsFoldout = false;

        protected void OnEnable()
        {
            themeDefinitionProp = serializedObject.FindProperty("definition");
            themeItemsProp = serializedObject.FindProperty("themeItems");
        }

        protected void OnDisable()
        {
            themeDefinitionSerializedObject?.Dispose();
            themeDefinitionSerializedObject = null;
            cachedThemeDefinitionRef = null;
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(themeDefinitionProp);

            // Rebuild the cached SerializedObject if the referenced definition asset has changed.
            if (themeDefinitionProp.objectReferenceValue != cachedThemeDefinitionRef)
            {
                themeDefinitionSerializedObject?.Dispose();
                cachedThemeDefinitionRef = themeDefinitionProp.objectReferenceValue;
                themeDefinitionSerializedObject = cachedThemeDefinitionRef != null
                    ? new SerializedObject(cachedThemeDefinitionRef)
                    : null;
            }
            themeDefinitionSerializedObject?.Update();

            SerializedProperty themeDefinitionArrayProp = themeDefinitionSerializedObject != null
                ? themeDefinitionSerializedObject
                    .FindProperty("themeDefinition")
                    .FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItems)))
                : null;

            if (themeDefinitionArrayProp != null)
            {
                itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Theme Values", true);
                if (itemsFoldout)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (int i = 0; i < themeDefinitionArrayProp.arraySize; i++)
                        {
                            SerializedProperty themeDefinitionItem = themeDefinitionArrayProp.GetArrayElementAtIndex(i);
                            string themeDefinitionItemName = themeDefinitionItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.Name))).stringValue;

                            SerializedProperty themeItem = themeItemsProp.arraySize > i ? themeItemsProp.GetArrayElementAtIndex(i) : null;
                            if (themeItem == null
                                || themeItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Name))).stringValue != themeDefinitionItemName)
                            {
                                // Search for an existing item with the matching name further in the list,
                                // so we can move it into position rather than discarding its saved values.
                                int existingIndex = -1;
                                for (int j = i + 1; j < themeItemsProp.arraySize; j++)
                                {
                                    string existingName = themeItemsProp.GetArrayElementAtIndex(j)
                                        .FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Name)))
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
                                    string valueDataType = themeDefinitionItem.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.DataType))).FindPropertyRelative("reference").stringValue;

                                    themeItemsProp.InsertArrayElementAtIndex(i);
                                    themeItem = themeItemsProp.GetArrayElementAtIndex(i);
                                    themeItem.managedReferenceValue = new Theme.ThemeItem(themeDefinitionItemName, Activator.CreateInstance(Type.GetType(valueDataType)));
                                }
                            }

                            SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(i);
                            SerializedProperty dataProp = themeItemProp.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Data)));
                            SerializedProperty valueProp = dataProp?.FindPropertyRelative(InspectorUIUtility.GetBackingField(nameof(BaseThemeItemData<object>.Value)));

                            if (valueProp != null)
                            {
                                // Draw just the Value field, labelled with the item name,
                                // skipping the intermediate "Data" foldout entirely.
                                EditorGUILayout.PropertyField(valueProp, new GUIContent(themeDefinitionItemName), true);
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
                themeItemsProp.arraySize = themeDefinitionArrayProp.arraySize;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
