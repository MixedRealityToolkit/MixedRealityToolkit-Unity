// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for the experimental package.
// While nice to have, documentation is not required for this experimental package.
#pragma warning disable CS1591

using UnityEditor;

namespace MixedReality.Toolkit.Data.Editor
{
    [CustomEditor(typeof(DataKeyPathMapperGODictionary.ViewToDataKeypathMap))]
    [CanEditMultipleObjects]
    public class DataKeyPathMapperInspector : UnityEditor.Editor
    {
        private SerializedProperty viewKeypathToDataKeypathMapper;

        /// <summary>
        /// A Unity event function that is called when the script component has been enabled.
        /// </summary> 
        private void OnEnable()
        {
            viewKeypathToDataKeypathMapper = serializedObject.FindProperty("viewKeypathToDataKeypathMapper");
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(viewKeypathToDataKeypathMapper, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#pragma warning restore CS1591
