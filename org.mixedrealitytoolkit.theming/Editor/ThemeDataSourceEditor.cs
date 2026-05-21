// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEditor;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomEditor(typeof(ThemeDataSource), true)]
    public class ThemeDataSourceEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                EditorGUILayout.PropertyField(iterator, true);

                if (iterator.name == "activeTheme" && iterator.objectReferenceValue is Theme theme)
                {
                    SerializedProperty definitionProp = serializedObject.FindProperty("themeDefinition");
                    UnityEngine.Object activeDefinition = definitionProp?.objectReferenceValue;

                    if (theme.Definition != activeDefinition)
                    {
                        EditorGUILayout.HelpBox($"Assigned theme's definition ({(theme.Definition != null ? theme.Definition.name : "null")}) does not match this data source's " +
                            $"active definition ({(activeDefinition != null ? activeDefinition.name : "null")}).\nThis will lead to undefined behavior at runtime.", MessageType.Error);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
