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

                if (iterator.name == "activeTheme" && iterator.boxedValue is Theme theme && theme.Definition != serializedObject.targetObject)
                {
                    EditorGUILayout.HelpBox($"Assigned theme's definition ({theme.Definition.name}) does not match this data source's active definition ({serializedObject.targetObject.name}).\nThis will lead to undefined behavior at runtime.", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
