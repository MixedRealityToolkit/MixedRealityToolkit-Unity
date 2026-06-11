// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEditor;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomEditor(typeof(ThemeDataSource), true)]
    public class ThemeDataSourceEditor : UnityEditor.Editor
    {
        private UnityEditor.Editor themeDefinitionEditor = null;
        private static bool themeDefinitionFoldout = false;

        protected void OnDisable()
        {
            if (themeDefinitionEditor != null)
            {
                DestroyImmediate(themeDefinitionEditor);
                themeDefinitionEditor = null;
            }
        }

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

                if (iterator.name == "themeDefinition" && iterator.objectReferenceValue != null)
                {
                    UnityEditor.Editor.CreateCachedEditor(iterator.objectReferenceValue, null, ref themeDefinitionEditor);

                    themeDefinitionFoldout = EditorGUILayout.Foldout(themeDefinitionFoldout, "Definition Details", true);
                    if (themeDefinitionFoldout && themeDefinitionEditor != null)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            themeDefinitionEditor.OnInspectorGUI();
                        }
                    }
                }

                if (iterator.name == "activeTheme" && iterator.objectReferenceValue is Theme theme)
                {
                    UnityEngine.Object activeDefinition = serializedObject.FindProperty("themeDefinition")?.objectReferenceValue;
                    if (theme.Definition != null && theme.Definition != activeDefinition)
                    {
                        EditorGUILayout.HelpBox($"Assigned theme's definition ({theme.Definition.name}) does not match this data source's definition ({(activeDefinition != null ? activeDefinition.name : "null")}).\nThis will lead to undefined behavior at runtime.", MessageType.Error);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
