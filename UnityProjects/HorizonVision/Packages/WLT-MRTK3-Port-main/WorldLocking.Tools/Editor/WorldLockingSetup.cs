// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.XR.CoreUtils;

#if WLT_ARFOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif // WLT_ARFOUNDATION_PRESENT

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Collection of menu driven Editor only functions to automate WLT configuration.
    /// </summary>
    public class WorldLockingSetup 
    {
        private const int setupPriority = 1100;
        /// <summary>
        /// Find the object to which all WLT related objects will be attached,
        /// creating it if it doesn't already exist.
        /// </summary>
        /// <returns>The GameObject to attach WLT related objects</returns>
        /// <remarks>
        /// Search goes like this:
        /// 1) If there is a WorldLockingContext, the parent is the WLT root.
        /// 2) If there is an object called WorldLocking, that will be WLT root.
        /// 3) Not found, so create a parent-less object named "WorldLocking".
        /// </remarks>
        private static Transform CheckWorldLockingRoot()
        {
            Transform root = null;
            var wltContext = GameObject.FindObjectOfType<WorldLockingContext>();
            if (wltContext != null)
            {
                root = wltContext.transform.parent;
            }
            if (root == null)
            {
                var wltGO = GameObject.Find("WorldLocking");
                if (wltGO != null)
                {
                    root = wltGO.transform;
                }
            }
            if (root == null)
            {
                root = new GameObject("WorldLocking").transform;
            }
            return root;
        }

        /// <summary>
        /// Search for a prefab with given name (no extension), which has "pathFilter" in its path.
        /// </summary>
        /// <param name="pathFilter">The path substring to filter on.</param>
        /// <param name="name">The name of the object (no extension).</param>
        /// <returns>The prefab, or null if not found.</returns>
        private static GameObject InstantiatePrefab(string pathFilter, string name)
        {
            string[] assetGuids = AssetDatabase.FindAssets(name);
            foreach (var guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(pathFilter))
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                    GameObject found = GameObject.Instantiate(obj) as GameObject;
                    found.name = obj.name;
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Find or create the WorldLockingContext for this scene.
        /// </summary>
        /// <param name="worldLockingRoot">Parent object to attach the context to.</param>
        /// <returns>The found or created context.</returns>
        /// <remarks>
        /// If the context exists, but isn't a child of worldLockingRoot, its parent will still be set to worldLockingRoot.
        /// </remarks>
        private static WorldLockingContext CheckWorldLockingManager(Transform worldLockingRoot)
        {
            // Look for a WorldLockingContext component in the scene. 
            var wltContext = GameObject.FindObjectOfType<WorldLockingContext>();

            // If not found, instantiate the WorldLockingManager prefab, and attach to WorldLocking root
            if (wltContext == null)
            {
                GameObject wltObject = InstantiatePrefab("WorldLocking.Core/Prefabs", "WorldLockingManager");
                Debug.Assert(wltObject != null, "Missing WorldLockingManager from WorldLocking.Core/Prefabs");
                wltContext = wltObject.GetComponent<WorldLockingContext>();
                Debug.Assert(wltContext != null, "WorldLockingManager prefab corrupt?");
            }
            // Now we definitely have a WorldLockingContext. Make sure it is attached to WorldLocking root object.
            wltContext.transform.parent = worldLockingRoot;
            return wltContext;
        }

        /// <summary>
        /// Check that the camera has proper hierarchy setup, and record it into the worldLockingContext.
        /// </summary>
        /// <param name="worldLockingContext">The context to setup with linkage information.</param>
        private static void CheckCamera(WorldLockingContext worldLockingContext)
        {
            // Find main camera. If not found, issue warning but we are done.
            if (Camera.main == null)
            {
                Debug.LogWarning($"Scene has no main camera, camera linkage will not be configured.");
                return;
            }
            if (!worldLockingContext.SharedSettings.linkageSettings.ApplyAdjustment)
            {
                Debug.LogWarning($"System application of world locking adjustments is disabled. Ignoring camera linkage.");
                return;
            }
            Transform mainCamera = Camera.main.transform;

            // If the camera doesn't have a parent
            //      Add MRTKPlayspace object, and attach camera to it.
            // If MRTKPlayspace object doesn't have a parent
            //      Add WLTAdjustment object, and attach MRTKPlayspace to it.
            // Set WorldLockingContext CameraParent to MRTKPlayspace object.
            // Set WorldLockingContext Adjustment to WLTAdjustment object.
            if (mainCamera.parent == null)
            {
                mainCamera.parent = new GameObject("MixedRealityPlayspace").transform;
            }
            Transform mrtkPlayspace = mainCamera.parent;
            if (mrtkPlayspace.parent == null)
            {
                mrtkPlayspace.parent = new GameObject("WLT_Adjustment").transform;
            }
            Transform wltAdjustment = mrtkPlayspace.parent;

            var sharedSettings = worldLockingContext.SharedSettings;
            sharedSettings.linkageSettings.CameraParent = mrtkPlayspace;
            sharedSettings.linkageSettings.AdjustmentFrame = wltAdjustment;
        }

        private static bool ConfiguredForARF(WorldLockingContext context)
        {
#if WLT_ARFOUNDATION_PRESENT
            ARSession session = GameObject.FindObjectOfType<ARSession>();
            XROrigin sessionOrigin = GameObject.FindObjectOfType<XROrigin>();

            if (session != null && sessionOrigin == null)
            {
                Debug.LogError($"Found ARSession on {session.name}, but no XROrigin. Check ARFoundation configuration.");
            }
            if (session == null && sessionOrigin != null)
            {
                Debug.LogError($"Found XROrigin on {sessionOrigin.name}, but no ARSession. Check ARFoundation configuration.");
            }
            if (session != null && sessionOrigin != null)
            {
                var sharedSettings = context.SharedSettings;
                sharedSettings.anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.ARFoundation;
                sharedSettings.anchorSettings.ARSessionSource = session.gameObject;
                sharedSettings.anchorSettings.XROriginSource = sessionOrigin.gameObject;
                return true;
            }
#endif // WLT_ARFOUNDATION_PRESENT
            return false;
        }

        private static void CheckAnchorManagement(WorldLockingContext context)
        {
            var sharedSettings = context.SharedSettings;
            sharedSettings.anchorSettings.UseDefaults = false;
            sharedSettings.anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.Null;

            if (!ConfiguredForARF(context))
            {
#if WLT_ARSUBSYSTEMS_PRESENT
                sharedSettings.anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.XRSDK;
#elif UNITY_WSA && !UNITY_2020_1_OR_NEWER
                sharedSettings.anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.WSA;
#elif WLT_ARCORE_SDK_INCLUDED
                sharedSettings.anchorSettings.anchorSubsystem = AnchorSettings.AnchorSubsystem.ARCore;
#endif // WLT_ARCORE_SDK_INCLUDED
            }
            if (sharedSettings.anchorSettings.anchorSubsystem == AnchorSettings.AnchorSubsystem.Null)
            {
                Debug.LogError($"Unable to deduce proper anchor management system.\nTry again after installing and setting up XR provider.");
            }
        }

        /// <summary>
        /// Setup the current scene with default basic world locking.
        /// </summary>
        [MenuItem("Mixed Reality/World Locking Tools/Configure scene", priority = setupPriority)]
        private static void AddWorldLockingToScene()
        {
            // Look for WorldLocking root object in scene.
            // If not found, add one.
            Transform worldLockingRoot = CheckWorldLockingRoot();

            WorldLockingContext worldLockingContext = CheckWorldLockingManager(worldLockingRoot);

            CheckCamera(worldLockingContext);

            CheckAnchorManagement(worldLockingContext);

            Selection.activeObject = worldLockingContext.gameObject;

            EditorUtility.SetDirty(worldLockingContext);
            EditorSceneManager.MarkAllScenesDirty();
        }

        /// <summary>
        /// Add an anchor graph visualization to the scene if it doesn't already have one.
        /// </summary>
        /// <param name="wltRoot">The root object to attach the visualization to.</param>
        private static void AddAnchorVisualizer(Transform wltRoot)
        {
            AnchorGraphVisual anchorVisual = GameObject.FindObjectOfType<AnchorGraphVisual>();
            if (anchorVisual == null)
            {
                GameObject anchorVisualObject = InstantiatePrefab("WorldLocking.Tools/Prefabs", "AnchorGraphVisual");
                anchorVisual = anchorVisualObject.GetComponent<AnchorGraphVisual>();
            }
            Debug.Assert(anchorVisual != null, "Missing AnchorGraphVisual prefab?");
            anchorVisual.transform.parent = wltRoot;
        }

        /// <summary>
        /// Add a global space pin visualizer to the scene, if it doesn't alreay have one.
        /// </summary>
        /// <param name="wltRoot">Parent object to attach visualizer to.</param>
        /// <param name="visualizers">List of currently existing space pin visualizers.</param>
        /// <remarks>
        /// At the end of this, the scene should have exactly one global space pin visualizer.
        /// Any extras will be deleted.
        /// </remarks>
        private static void AddGlobalSpacePinVisualizer(Transform wltRoot, SpacePinMeshVisualizer[] visualizers)
        {
            List<SpacePinMeshVisualizer> globalVisualizers = new List<SpacePinMeshVisualizer>();
            foreach (var vis in visualizers)
            {
                if (vis.TargetSubtree == null)
                {
                    globalVisualizers.Add(vis);
                }
            }
            if (globalVisualizers.Count > 1)
            {
                // We have too many, there should be exactly one when we're done, zero or one right now.
                Debug.LogError($"Found too many global space pin visualizers in the scene, deleting all but one.");
                for (int i = 1; i < globalVisualizers.Count; ++i)
                {
                    Debug.LogWarning($"Deleting global space pin visualizer {globalVisualizers[i].name}");
                    GameObject.DestroyImmediate(globalVisualizers[i].gameObject);
                }
            }
            else if (globalVisualizers.Count == 0)
            {
                GameObject newVis = InstantiatePrefab("WorldLocking.Tools/Prefabs", "SpacePinVisualizer");
                newVis.name = $"{newVis.name} (Global)";
                newVis.transform.parent = wltRoot;
            }

        }

        /// <summary>
        /// Add a space pin visualizer for each AlignSubtree in the scene.
        /// </summary>
        /// <param name="wltRoot">Parent object to attach any created visualizers to.</param>
        /// <param name="visualizers">Currently existing visualizers.</param>
        private static void AddSubtreeSpacePinVisualizers(Transform wltRoot, SpacePinMeshVisualizer[] visualizers)
        {
            AlignSubtree[] subtrees = GameObject.FindObjectsOfType<AlignSubtree>();

            foreach (var subtree in subtrees)
            {
                bool found = false;
                foreach(var vis in visualizers)
                {
                    if (vis != null && vis.TargetSubtree == subtree)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    GameObject newVis = InstantiatePrefab("WorldLocking.Tools/Prefabs", "SpacePinVisualizer");
                    newVis.name = $"{newVis.name} ({subtree.name})";
                    newVis.transform.parent = wltRoot;
                    var visualizer = newVis.GetComponent<SpacePinMeshVisualizer>();
                    visualizer.TargetSubtree = subtree;
                }
                     
            }
        }

        /// <summary>
        /// Add global and subtree space pin visualizers to the scene.
        /// </summary>
        /// <param name="wltRoot">Parent object to attach any created visualizers to.</param>
        private static void AddSpacePinVisualizers(Transform wltRoot)
        {
            SpacePinMeshVisualizer[] visualizers = GameObject.FindObjectsOfType<SpacePinMeshVisualizer>();

            AddGlobalSpacePinVisualizer(wltRoot, visualizers);

            AddSubtreeSpacePinVisualizers(wltRoot, visualizers);
        }

        /// <summary>
        /// Add visualization helpers for WLT features to the scene.
        /// </summary>
        [MenuItem("Mixed Reality/World Locking Tools/Add development visualizers", priority = setupPriority)]
        private static void AddWorldLockingVisualizers()
        {
            Transform worldLockingRoot = CheckWorldLockingRoot();

            AddAnchorVisualizer(worldLockingRoot);

            AddSpacePinVisualizers(worldLockingRoot);

            Selection.activeObject = worldLockingRoot.gameObject;
        }

        /// <summary>
        /// Remove any WLT visualizers from the scene.
        /// </summary>
        /// <remarks>
        /// This will remove all WLT visualizers from the scene, whether they were added by AddWorldLockingVisualizers() or by hand.
        /// </remarks>
        [MenuItem("Mixed Reality/World Locking Tools/Remove development visualizers", priority = setupPriority)]
        private static void RemoveWorldLockingVisualisers()
        {
            AnchorGraphVisual[] anchorVisuals = GameObject.FindObjectsOfType<AnchorGraphVisual>();
            foreach( var vis in anchorVisuals)
            {
                GameObject.DestroyImmediate(vis.gameObject);
            }

            SpacePinMeshVisualizer[] visualizers = GameObject.FindObjectsOfType<SpacePinMeshVisualizer>();
            foreach(var vis in visualizers)
            {
                GameObject.DestroyImmediate(vis.gameObject);
            }
        }

        private static readonly string WLTIssuesUrl = "https://github.com/microsoft/MixedReality-WorldLockingTools-Unity/issues";
        private static readonly string WLTDocsUrl = "https://docs.microsoft.com/mixed-reality/world-locking-tools/";
        private static readonly string WLTSamplesUrl = "https://microsoft.github.io/MixedReality-WorldLockingTools-Samples/README.html";

        [MenuItem("Mixed Reality/World Locking Tools/Help/Show Documentation", false, priority = setupPriority)]
        private static void ShowDocumentation()
        {
            Application.OpenURL(WLTDocsUrl);
        }
        [MenuItem("Mixed Reality/World Locking Tools/Help/Samples Documentation", false, priority = setupPriority)]
        private static void ShowSamplesDocumentation()
        {
            Application.OpenURL(WLTSamplesUrl);
        }
        [MenuItem("Mixed Reality/World Locking Tools/Help/File an issue", false, priority = setupPriority)]
        private static void FileIssue()
        {
            Application.OpenURL(WLTIssuesUrl);
        }

    }
}
