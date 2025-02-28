// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

#if UNITY_6000_0_OR_NEWER
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
#endif

internal class AndroidXRConfig
{
#if UNITY_6000_0_OR_NEWER
    [MenuItem("Mixed Reality/MRTK3/Examples/Configure for Android XR...", priority = int.MaxValue)]
    public static void InstallPackages()
    {
        Debug.Log("Adding com.unity.xr.androidxr-openxr and com.google.xr.extensions...");
        Client.AddAndRemove(new[] { "com.unity.xr.androidxr-openxr", "https://github.com/android/android-xr-unity-package.git" });
    }
#endif
}
