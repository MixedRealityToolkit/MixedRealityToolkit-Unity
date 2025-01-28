// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Input.Editor
{
    /// <summary>
    /// A custom class used when rendering a Unity inspector window editor for a <see cref="MixedReality.Toolkit.Input.InteractionModeManager">InteractionModeManager</see>
    /// class.
    /// </summary>
    [CustomEditor(typeof(InteractionModeManager))]
    [CanEditMultipleObjects]
    public class InteractionModeManagerEditor : UnityEditor.Editor
    {
        private const string InitControllers = "Init Controllers";
        private const string InitSubtypes = "Populate Modes Definitions with Subtypes";

        /// <summary>
        /// Implemented so to make a custom inspector inside Unity's inspector window.
        /// </summary>
        public override void OnInspectorGUI()
        {
            Color prevColor = GUI.color;
            InteractionModeManager interactionModeManager = (InteractionModeManager)target;

            // Raise lots of errors if the interaction mode manager is configured incorrectly
            var duplicateInteractorGroupMappings = GetDuplicateInteractorGroupMappings();
            if (duplicateInteractorGroupMappings.Count > 0)
            {
                var duplicatedNameString = interactionModeManager.CompileDuplicatedNames(duplicateInteractorGroupMappings);

                InspectorUIUtility.DrawError($"Duplicate interactor group mapping keys detected in the interaction mode manager on {interactionModeManager.gameObject.name}. " +
                                    $"Please check the following interactor group mappings: {duplicatedNameString}");

                GUI.color = InspectorUIUtility.ErrorColor;
            }

            var duplicatedNames = interactionModeManager.GetDuplicateInteractionModes();
            if (duplicatedNames.Count > 0)
            {
                var duplicatedNameString = interactionModeManager.CompileDuplicatedNames(duplicatedNames);

                InspectorUIUtility.DrawError($"Duplicate interaction mode definitions detected in the interaction mode manager on {interactionModeManager.gameObject.name}. " +
                                    $"Please check the following interaction modes: {duplicatedNameString}");

                GUI.color = InspectorUIUtility.ErrorColor;
            }

            // Handle the rest of the UI
            base.OnInspectorGUI();
            GUI.color = prevColor;

            if (GUILayout.Button(InitControllers))
            {
                Undo.RecordObject(target, InitControllers);
                interactionModeManager.InitializeInteractorGroups();
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            if (GUILayout.Button(InitSubtypes))
            {
                Undo.RecordObject(target, InitSubtypes);
                interactionModeManager.PopulateModesWithSubtypes();
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private HashSet<string> GetDuplicateInteractorGroupMappings()
        {
            HashSet<string> duplicatedNames = new HashSet<string>();

            SerializedProperty interactorGroupMappings = serializedObject.FindProperty("interactorGroupMappings");
            SerializedProperty entries = interactorGroupMappings?.FindPropertyRelative("entries");

            if (entries != null && entries.arraySize > 0)
            {
                HashSet<int> seenInstanceIDs = new HashSet<int>();

                for (int i = 0; i < entries.arraySize; ++i)
                {
                    SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                    SerializedProperty key = entry.FindPropertyRelative("key");

                    int instanceID = key != null && key.objectReferenceValue != null ?
                        key.objectReferenceValue.GetInstanceID() : 0;
                        
                    if (seenInstanceIDs.Contains(instanceID))
                    {
                        string duplicateName = key != null && key.objectReferenceValue != null ?
                            key.objectReferenceValue.name : "None (Game Object)";

                        duplicatedNames.Add(duplicateName);
                    }
                    else
                    {
                        seenInstanceIDs.Add(instanceID);
                    }
                }
            }

            return duplicatedNames;
        }
    }
}
