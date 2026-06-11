// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEditor;

namespace MixedReality.Toolkit.Theming.Editor
{
    [CustomEditor(typeof(ThemeDefinition))]
    public class ThemeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            ThemeDefinition def = target as ThemeDefinition;
            if (def != null && def.ThemeDefinitionItems != null)
            {
                foreach (var item in def.ThemeDefinitionItems)
                {
                    if (item == null) { continue; }

                    string displayLabel = string.IsNullOrWhiteSpace(item.Name) ? "(null)" : item.Name;
                    System.Type type = item.DataType?.Type;

                    if (type == null)
                    {
                        EditorGUILayout.HelpBox($"Item '{displayLabel}' has no DataType selected, or the type could not be resolved.", MessageType.Warning);
                    }
                    else if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                    {
                        EditorGUILayout.HelpBox($"Item '{displayLabel}' uses an invalid DataType ({type.Name}). It must be a concrete, non-generic class to be instantiated.", MessageType.Warning);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
