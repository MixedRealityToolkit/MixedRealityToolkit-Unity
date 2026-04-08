// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MixedReality.Toolkit.Examples.Demos.Editor
{
    internal class AndroidXRConfig
    {
        private static AddAndRemoveRequest request;

        [MenuItem("Mixed Reality/MRTK3/Examples/Configure for Android XR...", priority = int.MaxValue)]
        public static void InstallPackages()
        {
            // Already a request in progress, so don't re-run
            if (request != null)
            {
                return;
            }

            Debug.Log("Adding the Unity OpenXR Android XR package...");
            request = Client.AddAndRemove(new[] { "com.unity.xr.androidxr-openxr" });
            EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            if (request.IsCompleted)
            {
                Debug.Log($"Package install request complete ({request.Status}).");
                EditorApplication.update -= Progress;
                request = null;
            }
        }
    }
}
