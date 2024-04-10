// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Custom editor for the collections of settings managed by the WorldLockingManager.
    /// </summary>
    [CustomEditor(typeof(WorldLockingContext))]
    public class WorldLockingContextEditor : Editor
    {
        bool showWorld = true;
        bool showLinkage = true;
        bool showAnchor = true;
        bool showDiagnostics = false;

        /// <summary>
        /// Put up the GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            string versionInfo = WorldLockingManager.Version;
            EditorGUILayout.LabelField("Version: ", versionInfo);

            var context = target as WorldLockingContext;

            bool useMgrDefaults = false;

            showWorld = EditorGUILayout.Foldout(showWorld, "Automation settings", true);
            if (showWorld)
            {
                string mgrPath = "shared.settings.";

                SerializedProperty mgrUseDefaultsProp = AddProperty(mgrPath, "useDefaults");

                useMgrDefaults = mgrUseDefaultsProp.boolValue;

                using (new EditorGUI.DisabledScope(useMgrDefaults))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(mgrPath, "Enabled");

                        AddProperty(mgrPath, "AutoMerge");

                        AddProperty(mgrPath, "AutoRefreeze");

                        AddProperty(mgrPath, "AutoLoad");

                        AddProperty(mgrPath, "AutoSave");
                    }
                }

            } 

            EditorGUILayout.Space();

            showLinkage = EditorGUILayout.Foldout(showLinkage, "Camera Transform Links", true);
            if (showLinkage)
            {
                string mgrPath = "shared.linkageSettings.";

                SerializedProperty mgrUseExisting = AddProperty(mgrPath, "useExisting");

                bool useExisting = mgrUseExisting.boolValue;

                using (new EditorGUI.DisabledScope(useExisting))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(mgrPath, "applyAdjustment");

                        AddProperty(mgrPath, "NoPitchAndRoll");

                        AddProperty(mgrPath, "AdjustmentFrame");

                        AddProperty(mgrPath, "CameraParent");
                    }
                }

            }

            EditorGUILayout.Space();

            bool useAnchorDefaults = false;
            showAnchor = EditorGUILayout.Foldout(showAnchor, "Anchor Management", true);
            if (showAnchor)
            {
                string mgrPath = "shared.anchorSettings.";

                SerializedProperty anchorUseDefaults = AddProperty(mgrPath, "useDefaults");

                useAnchorDefaults = anchorUseDefaults.boolValue;

                using (new EditorGUI.DisabledScope(useAnchorDefaults))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        SerializedProperty anchorSubsystem = AddProperty(mgrPath, "anchorSubsystem");
                        bool isARF = anchorSubsystem.intValue == (int)(AnchorSettings.AnchorSubsystem.ARFoundation);

                        if (isARF)
                        {
                            AddProperty(mgrPath, "ARSessionSource");

                            AddProperty(mgrPath, "XROriginSource");
                        }

                        AddProperty(mgrPath, "MinNewAnchorDistance");

                        AddProperty(mgrPath, "MaxAnchorEdgeLength");

                        AddProperty(mgrPath, "MaxLocalAnchors");

                        AddProperty(mgrPath, "NullSubsystemInEditor");
                    }
                }

            }

            EditorGUILayout.Space();

            bool useDiagDefaults = false;

            showDiagnostics = EditorGUILayout.Foldout(showDiagnostics, "Diagnostics settings", true);
            if (showDiagnostics)
            {
                string diagPath = "diagnosticsSettings.settings.";

                var diagnostics = context.DiagnosticsSettings;

                SerializedProperty diagUseDefaultsProp = AddProperty(diagPath, "useDefaults");

                useDiagDefaults = diagUseDefaultsProp.boolValue;

                using (new EditorGUI.DisabledScope(useDiagDefaults))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(diagPath, "Enabled");

                        AddProperty(diagPath, "StorageSubdirectory");

                        AddProperty(diagPath, "StorageFileTemplate");

                        AddProperty(diagPath, "MaxKilobytesPerFile");

                        AddProperty(diagPath, "MaxNumberOfFiles");
                    }
                }

            }
            serializedObject.ApplyModifiedProperties();
            // This next section syncs the internal state to the default states, changing the scene object (if appropriate).
            // For some reason, this seems to need to happen outside the modify/ApplyModified block.
            if (useMgrDefaults)
            {
                context.SharedSettings.settings.UseDefaults = true;
            }
            if (useAnchorDefaults)
            {
                context.SharedSettings.anchorSettings.UseDefaults = true;
            }
            if (useDiagDefaults)
            {
                context.DiagnosticsSettings.settings.UseDefaults = true;
            }
        }

        /// <summary>
        /// Find a property, possibly with a path down from the serializedObject.
        /// </summary>
        /// <remarks>
        /// Path seems to work like so:
        ///   struct MySubStruct { int myIntField; }
        ///   struct MyStruct { MySubStruct mySubStruct; }
        ///   class myObj : Monobehavior { MyStruct myStruct; }
        ///   var intProp = serializedObject.FindProperty("myStruct.mySubStruct.myIntField");
        /// </remarks>
        /// <param name="path">Path including trailing '.' (or empty).</param>
        /// <param name="name">Field name.</param>
        /// <returns></returns>
        private SerializedProperty FindProperty(string path, string name)
        {
            return serializedObject.FindProperty(path + name);
        }

        /// <summary>
        /// Find a property and add to the GUI.
        /// </summary>
        /// <param name="path">Path including trailing '.' (or empty).</param>
        /// <param name="name">Field name.</param>
        /// <returns></returns>
        private SerializedProperty AddProperty(string path, string name)
        {
            SerializedProperty prop = FindProperty(path, name);
            EditorGUILayout.PropertyField(prop);
            return prop;
        }
    }
}

#endif // UNITY_EDITOR