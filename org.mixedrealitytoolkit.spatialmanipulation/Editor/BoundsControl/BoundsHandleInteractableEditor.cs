// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using UnityEditor;

namespace MixedReality.Toolkit.SpatialManipulation.Editor
{
    [CustomEditor(typeof(BoundsHandleInteractable), true)]
    [CanEditMultipleObjects]
    public class BoundsHandleInteractableEditor : StatefulInteractableEditor
    {
        private SerializedProperty scaleMaintainType;
        private SerializedProperty targetLossyScale;
        private SerializedProperty minLossyScale;
        private SerializedProperty maxLossyScale;
        private SerializedProperty handleType;

        protected override void OnEnable()
        {
            base.OnEnable();

            scaleMaintainType = serializedObject.FindProperty("scaleMaintainType");
            targetLossyScale = serializedObject.FindProperty("targetLossyScale");
            minLossyScale = serializedObject.FindProperty("minLossyScale");
            maxLossyScale = serializedObject.FindProperty("maxLossyScale");
            handleType = serializedObject.FindProperty("handleType");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.PropertyField(scaleMaintainType);

            if (scaleMaintainType.enumValueIndex == (int)ScaleMaintainType.Advanced)
            {
                EditorGUILayout.PropertyField(targetLossyScale);
                EditorGUILayout.PropertyField(minLossyScale);
                EditorGUILayout.PropertyField(maxLossyScale);
            }

            EditorGUILayout.PropertyField(handleType);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
