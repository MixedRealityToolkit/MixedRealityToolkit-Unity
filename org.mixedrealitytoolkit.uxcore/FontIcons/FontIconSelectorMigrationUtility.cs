// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.UX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MixedReality.Toolkit.Editor
{
    /// <summary>
    /// Utility to bulk-migrate <see cref="FontIconSelector"/> components to the new descriptive naming format.
    /// </summary>
    public static class FontIconSelectorMigrationUtility
    {
        [MenuItem("Mixed Reality/MRTK3/Utilities/Migrate Font Icon Selectors", false, 100)]
        public static void MigrateProject()
        {
            // Prompt the user to save any unsaved work before we start swapping scenes
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            int migratedCount = 0;

            // 1. Migrate all prefabs in the project
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null) { continue; }

                FontIconSelector[] selectors = prefab.GetComponentsInChildren<FontIconSelector>(true);
                foreach (FontIconSelector selector in selectors)
                {
                    if (ProcessSelector(selector))
                    {
                        migratedCount++;
                    }
                }
            }

            // 2. Migrate all scenes in Build Settings
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();

            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled || string.IsNullOrEmpty(buildScene.path)) { continue; }

                Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                bool sceneModified = false;

                FontIconSelector[] sceneSelectors = Object.FindObjectsByType<FontIconSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (FontIconSelector selector in sceneSelectors)
                {
                    // Skip prefabs that might accidentally be caught in FindObjectsOfType
                    if (PrefabUtility.IsPartOfPrefabAsset(selector)) { continue; }

                    if (ProcessSelector(selector))
                    {
                        sceneModified = true;
                        migratedCount++;
                    }
                }

                if (sceneModified)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            // Restore the user's initially opened scenes
            if (setup != null && setup.Length > 0)
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            // Save all the SetDirty changes we applied to the prefabs
            AssetDatabase.SaveAssets();

            Debug.Log($"[{nameof(FontIconSelectorMigrationUtility)}] Migration complete! Processed and upgraded {migratedCount} FontIconSelectors.");
        }

        /// <summary>
        /// Checks a selector's serialization state, runs migration, and dirty flags it if changes occurred.
        /// </summary>
        private static bool ProcessSelector(FontIconSelector selector)
        {
            SerializedObject so = new SerializedObject(selector);
            SerializedProperty migratedProp = so.FindProperty("migratedSuccessfully");

            if (migratedProp != null && !migratedProp.boolValue)
            {
                Undo.RecordObject(selector, "Migrate Font Icon Selector");

                if (selector.TryMigrate())
                {
                    // Ensure that changes to prefab instances in scenes are explicitly recorded as overrides.
                    if (PrefabUtility.IsPartOfPrefabInstance(selector))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(selector);
                    }

                    EditorUtility.SetDirty(selector);
                    return true;
                }
            }

            return false;
        }
    }
}
