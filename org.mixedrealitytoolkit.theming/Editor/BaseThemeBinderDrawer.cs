// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomPropertyDrawer(typeof(BaseThemeBinder<,>), true)]
    public class BaseThemeBinderDrawer : PropertyDrawer
    {
        // Cached SerializedObject for the ThemeDataSource asset.
        // PropertyDrawer is instantiated per-type, not per-property, so multiple
        // binders may reference different data source assets — we track the last seen
        // asset and rebuild the cache only when it changes.
        private SerializedObject cachedDataSourceSerializedObject = null;
        private int cachedDataSourceInstanceID = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = label.text.Replace("Element", "Binder");

            SerializedProperty themeDataSourceProperty = FindThemeDataSourceProperty(property);

            List<string> names = null;
            SerializedProperty themeDefinitionItemName = null;

            if (property.managedReferenceValue != null && themeDataSourceProperty != null && themeDataSourceProperty.objectReferenceValue != null)
            {
                themeDefinitionItemName = property.FindPropertyRelative(InspectorUIUtility.GetBackingField("ThemeDefinitionItemName"));

                // Rebuild the cached SerializedObject only when the referenced asset changes.
                int instanceID = themeDataSourceProperty.objectReferenceValue.GetInstanceID();
                if (instanceID != cachedDataSourceInstanceID)
                {
                    cachedDataSourceSerializedObject?.Dispose();
                    cachedDataSourceSerializedObject = new SerializedObject(themeDataSourceProperty.objectReferenceValue);
                    cachedDataSourceInstanceID = instanceID;
                }
                cachedDataSourceSerializedObject.Update();

                SerializedProperty themeDefinitionProperty = cachedDataSourceSerializedObject.FindProperty("themeDefinition");
                names = ParseThemeItems(themeDefinitionProperty.boxedValue as ThemeDefinition, (dynamic)property.managedReferenceValue);
            }

            label = EditorGUI.BeginProperty(position, label, property);

            // Draw the foldout and all child properties within the allocated rect.
            Rect propertyRect = position;
            if (names != null)
            {
                // Reserve the last line for the Bound Theme Item popup when expanded.
                propertyRect.height -= property.isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0;
            }
            EditorGUI.PropertyField(propertyRect, property, label, true);

            // Draw the Bound Theme Item popup using the Rect API, within the allocated position.
            if (names != null && property.isExpanded)
            {
                int selected = names.IndexOf(themeDefinitionItemName.stringValue);

                // Child fields are indented 15px from position.x. Unity's label/control
                // split is at position.x + labelWidth, so the label width is (labelWidth - 15f)
                // and the control starts at that same absolute split point.
                const float indentWidth = 15f;
                float rowY = position.y + propertyRect.height + EditorGUIUtility.standardVerticalSpacing;
                float splitX = position.x + EditorGUIUtility.labelWidth;
                float rightEdge = position.x + position.width;
                Rect labelRect = new Rect(position.x + indentWidth, rowY, EditorGUIUtility.labelWidth - indentWidth, EditorGUIUtility.singleLineHeight);
                Rect controlRect = new Rect(splitX, rowY, rightEdge - splitX, EditorGUIUtility.singleLineHeight);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.LabelField(labelRect, "Bound Theme Item");
                    selected = EditorGUI.Popup(controlRect, selected, System.Array.ConvertAll(names.ToArray(), n => new GUIContent(n)));
                    if (check.changed)
                    {
                        themeDefinitionItemName.stringValue = names[selected];
                    }
                }
            }
            else if (themeDefinitionItemName != null && names == null)
            {
                themeDefinitionItemName.stringValue = null;
            }

            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property);

            // Add a line for the Bound Theme Item popup when the foldout is expanded and a data source is available.
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                SerializedProperty themeDataSourceProperty = FindThemeDataSourceProperty(property);
                if (themeDataSourceProperty != null && themeDataSourceProperty.objectReferenceValue != null)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        private static SerializedProperty FindThemeDataSourceProperty(SerializedProperty property)
        {
            string path = property.propertyPath;
            string parentPath = path.Substring(0, path.LastIndexOf(path.Contains(".Array.data[") ? ".Array.data[" : "."));
            SerializedProperty parentProperty = property.serializedObject.FindProperty(parentPath);
            return parentProperty?.serializedObject.FindProperty("themeDataSource");
        }

        private List<string> ParseThemeItems<T, K>(ThemeDefinition themeDefinition, BaseThemeBinder<T, K> _)
        {
            if (themeDefinition == null || themeDefinition.ThemeDefinitionItems == null)
            {
                return null;
            }

            List<string> matchingItemNames = new();

            foreach (ThemeDefinition.ThemeDefinitionItem item in themeDefinition.ThemeDefinitionItems)
            {
                if (!string.IsNullOrWhiteSpace(item.Name)
                    && item.DataType?.Type != null
                    && item.DataType.Type.BaseType != null
                    && item.DataType.Type.BaseType.GenericTypeArguments.Length > 0
                    && item.DataType.Type.BaseType.GenericTypeArguments[0].IsAssignableFrom(typeof(T)))
                {
                    matchingItemNames.Add(item.Name);
                }
            }

            return matchingItemNames;
        }
    }
}
