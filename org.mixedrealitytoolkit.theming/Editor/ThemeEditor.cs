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

        private static readonly string ThemeDefinitionItemsField = InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItems));
        private static readonly string NameField = InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.Name));
        private static readonly string DataTypeField = InspectorUIUtility.GetBackingField(nameof(ThemeDefinition.ThemeDefinitionItem.DataType));
        private static readonly string DataField = InspectorUIUtility.GetBackingField(nameof(Theme.ThemeItem.Data));
        private static readonly string ValueField = InspectorUIUtility.GetBackingField(nameof(BaseThemeItemData<object>.Value));

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
                    .FindPropertyRelative(ThemeDefinitionItemsField)
                : null;

            if (themeDefinitionArrayProp != null && themeItemsProp != null)
            {
                itemsFoldout = EditorGUILayout.Foldout(itemsFoldout, "Theme Values", true);
                if (itemsFoldout)
                {
                    if (Event.current.type == EventType.Layout)
                    {
                        ReconcileThemeItems(themeDefinitionArrayProp, themeItemsProp);
                    }

                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (int i = 0; i < themeItemsProp.arraySize; i++)
                        {
                            SerializedProperty themeItemProp = themeItemsProp.GetArrayElementAtIndex(i);
                            SerializedProperty dataProp = themeItemProp.FindPropertyRelative(DataField);
                            SerializedProperty valueProp = dataProp?.FindPropertyRelative(ValueField);

                            if (valueProp != null)
                            {
                                // Draw just the Value field, labelled with the item name,
                                // skipping the intermediate "Data" foldout entirely.
                                string themeItemName = themeItemProp.FindPropertyRelative(NameField).stringValue;
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

        private void ReconcileThemeItems(SerializedProperty themeDefinitionArrayProp, SerializedProperty themeItemsProp)
        {
            for (int i = 0; i < themeDefinitionArrayProp.arraySize; i++)
            {
                SerializedProperty themeDefinitionItem = themeDefinitionArrayProp.GetArrayElementAtIndex(i);
                string themeDefinitionItemName = themeDefinitionItem.FindPropertyRelative(NameField).stringValue;

                SerializedProperty themeItem = themeItemsProp.arraySize > i ? themeItemsProp.GetArrayElementAtIndex(i) : null;
                if (themeItem == null
                    || themeItem.FindPropertyRelative(NameField).stringValue != themeDefinitionItemName)
                {
                    // Search for an existing item with the matching name further in the list,
                    // so we can move it into position rather than discarding its saved values.
                    int existingIndex = -1;
                    for (int j = i + 1; j < themeItemsProp.arraySize; j++)
                    {
                        string existingName = themeItemsProp.GetArrayElementAtIndex(j)
                            .FindPropertyRelative(NameField)
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
                        string valueDataType = themeDefinitionItem.FindPropertyRelative(DataTypeField).FindPropertyRelative("reference").stringValue;

                        themeItemsProp.InsertArrayElementAtIndex(i);
                        themeItem = themeItemsProp.GetArrayElementAtIndex(i);

                        Type dataType = Type.GetType(valueDataType);
                        object dataInstance = dataType != null ? Activator.CreateInstance(dataType) : null;
                        themeItem.managedReferenceValue = new Theme.ThemeItem(themeDefinitionItemName, dataInstance);
                    }
                }
            }
            themeItemsProp.arraySize = themeDefinitionArrayProp.arraySize;
        }
    }
}
