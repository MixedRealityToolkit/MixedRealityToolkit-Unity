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
        private static string themeDefinitionItemNameField;
        private static string ThemeDefinitionItemNameField => themeDefinitionItemNameField ??= InspectorUIUtility.GetBackingField("ThemeDefinitionItemName");

        private static readonly Dictionary<string, string> cachedLabels = new Dictionary<string, string>();
        private static readonly Dictionary<System.Type, System.Type> cachedValueTypes = new Dictionary<System.Type, System.Type>();
        private static readonly List<string> reusableMatchingItemNames = new List<string>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (label.text.StartsWith("Element"))
            {
                if (!cachedLabels.TryGetValue(label.text, out string cachedLabel))
                {
                    cachedLabel = label.text.Replace("Element", "Binder");
                    cachedLabels[label.text] = cachedLabel;
                }
                label = new GUIContent(label) { text = cachedLabel };
            }

            SerializedProperty themeDataSourceProperty = property.serializedObject.FindProperty("themeDataSource");

            string[] names = null;
            SerializedProperty themeDefinitionItemName = null;
            bool hasDataSource = themeDataSourceProperty != null && themeDataSourceProperty.objectReferenceValue != null;

            // Only pay the cost of parsing available items when the property is actively expanded
            if (property.isExpanded && property.managedReferenceValue != null && hasDataSource)
            {
                themeDefinitionItemName = property.FindPropertyRelative(ThemeDefinitionItemNameField);

                ThemeDataSource dataSource = themeDataSourceProperty.objectReferenceValue as ThemeDataSource;
                if (dataSource != null)
                {
                    names = ParseThemeItems(dataSource.Definition, property.managedReferenceValue);
                }
            }

            label = EditorGUI.BeginProperty(position, label, property);

            // Draw the foldout and all child properties within the allocated rect.
            Rect propertyRect = position;
            if (hasDataSource)
            {
                // Reserve the last line for the Bound Theme Item popup when expanded.
                propertyRect.height -= property.isExpanded ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0;
            }
            EditorGUI.PropertyField(propertyRect, property, label, true);

            // Draw the Bound Theme Item popup using the Rect API, within the allocated position.
            if (hasDataSource && property.isExpanded)
            {
                // Child fields are indented 15px from position.x. Unity's label/control
                // split is at position.x + labelWidth, so the label width is (labelWidth - 15f)
                // and the control starts at that same absolute split point.
                const float indentWidth = 15f;
                float rowY = position.y + propertyRect.height + EditorGUIUtility.standardVerticalSpacing;
                float splitX = position.x + EditorGUIUtility.labelWidth;
                float rightEdge = position.x + position.width;
                Rect labelRect = new Rect(position.x + indentWidth, rowY, EditorGUIUtility.labelWidth - indentWidth, EditorGUIUtility.singleLineHeight);
                Rect controlRect = new Rect(splitX, rowY, rightEdge - splitX, EditorGUIUtility.singleLineHeight);

                if (names != null)
                {
                    int selected = System.Array.IndexOf(names, themeDefinitionItemName.stringValue);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.LabelField(labelRect, "Bound Theme Item");
                        selected = EditorGUI.Popup(controlRect, selected, names);
                        if (check.changed)
                        {
                            themeDefinitionItemName.stringValue = names[selected];
                        }
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.LabelField(labelRect, "Bound Theme Item");
                        EditorGUI.Popup(controlRect, 0, new string[] { "(No matching items)" });
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property);

            // Add a line for the Bound Theme Item popup when the foldout is expanded and a data source is available.
            if (property.isExpanded && property.managedReferenceValue != null)
            {
                SerializedProperty themeDataSourceProperty = property.serializedObject.FindProperty("themeDataSource");
                if (themeDataSourceProperty != null && themeDataSourceProperty.objectReferenceValue != null)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return height;
        }

        private string[] ParseThemeItems(ThemeDefinition themeDefinition, object binder)
        {
            if (themeDefinition == null || themeDefinition.ThemeDefinitionItems == null || binder == null)
            {
                return null;
            }

            System.Type binderType = binder.GetType();
            if (!cachedValueTypes.TryGetValue(binderType, out System.Type valueType))
            {
                System.Type baseBinderType = binderType;
                while (baseBinderType != null && (!baseBinderType.IsGenericType || baseBinderType.GetGenericTypeDefinition() != typeof(BaseThemeBinder<,>)))
                {
                    baseBinderType = baseBinderType.BaseType;
                }

                valueType = baseBinderType?.GenericTypeArguments[0];
                cachedValueTypes[binderType] = valueType;
            }

            if (valueType == null)
            {
                return null;
            }

            reusableMatchingItemNames.Clear();

            foreach (ThemeDefinition.ThemeDefinitionItem item in themeDefinition.ThemeDefinitionItems)
            {
                if (item != null
                    && !string.IsNullOrWhiteSpace(item.Name)
                    && item.DataType?.Type != null
                    && item.DataType.Type.BaseType != null
                    && item.DataType.Type.BaseType.GenericTypeArguments.Length > 0
                    && item.DataType.Type.BaseType.GenericTypeArguments[0].IsAssignableFrom(valueType))
                {
                    reusableMatchingItemNames.Add(item.Name);
                }
            }

            return reusableMatchingItemNames.Count > 0 ? reusableMatchingItemNames.ToArray() : null;
        }
    }
}
