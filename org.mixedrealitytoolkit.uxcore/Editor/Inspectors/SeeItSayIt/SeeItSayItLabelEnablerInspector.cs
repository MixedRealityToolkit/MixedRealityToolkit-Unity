// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using UnityEditor;

namespace MixedReality.Toolkit.Editor
{
    [CustomEditor(typeof(SeeItSayItLabelEnabler))]
    public class SeeItSayItLabelEnablerInspector : UnityEditor.Editor
    {
        private SerializedProperty localizedPattern = null;
        private SerializedProperty pattern = null;

        private void OnEnable()
        {
            localizedPattern = serializedObject.FindProperty(nameof(localizedPattern));
            pattern = serializedObject.FindProperty(nameof(pattern));
        }

        /// <summary>
        /// Called by the Unity editor to render custom inspector UI for this component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "localizedPattern", "pattern");

            if (localizedPattern != null)
            {
                EditorGUILayout.PropertyField(localizedPattern);
            }

            if (localizedPattern == null ||
                (string.IsNullOrEmpty(localizedPattern.FindPropertyRelative("m_TableEntryReference").FindPropertyRelative("m_Key").stringValue)
                && localizedPattern.FindPropertyRelative("m_TableEntryReference").FindPropertyRelative("m_KeyId").longValue == 0)
                || string.IsNullOrEmpty(localizedPattern.FindPropertyRelative("m_TableReference").FindPropertyRelative("m_TableCollectionName").stringValue))
            {
                if (localizedPattern != null)
                {
                    EditorGUILayout.HelpBox("Pattern is only used when the Localized Pattern above is not set.", MessageType.Info);
                }
                EditorGUILayout.PropertyField(pattern);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
