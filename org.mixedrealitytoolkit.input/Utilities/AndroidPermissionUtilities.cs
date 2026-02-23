// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;

#if UNITY_ANDROID && UNITY_OPENXR_PRESENT && !UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.Android;
using UnityEngine.XR.OpenXR;
#endif

namespace MixedReality.Toolkit.Input
{
    internal sealed class AndroidPermissionUtilities : MonoBehaviour
    {
        [SerializeField, Tooltip("A key (OpenXR extension name) value (Android permission string) pairing to request at runtime if the extension is supported and enabled.")]
        private Unity.XR.CoreUtils.Collections.SerializableDictionary<string, string> permissions;

#if UNITY_ANDROID && UNITY_OPENXR_PRESENT && !UNITY_EDITOR
        private void Start()
        {
            List<string> neededPermissions = new(permissions.Count);

            for (int i = 0; i < permissions.Count; i++)
            {
                string extensionName = permissions.SerializedItems[i].Key;
                string permissionName = permissions.SerializedItems[i].Value;
                if (!string.IsNullOrWhiteSpace(extensionName) &&
                    !string.IsNullOrWhiteSpace(permissionName) &&
                    OpenXRRuntime.IsExtensionEnabled(extensionName) &&
                    !Permission.HasUserAuthorizedPermission(permissionName))
                {
                    neededPermissions.Add(permissionName);
                }
            }

            if (neededPermissions.Count > 0)
            {
                PermissionCallbacks callbacks = new();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;
                Permission.RequestUserPermissions(neededPermissions.ToArray(), callbacks);
                Debug.Log($"MRTK is requesting the following permissions for {gameObject.name}: {string.Join(", ", neededPermissions)}.");
            }
            else
            {
                Debug.Log($"All permissions for {gameObject.name} already granted for MRTK.");
            }
        }

        void OnPermissionDenied(string permission)
        {
            Debug.Log($"{permission} denied or not needed on this runtime ({OpenXRRuntime.name}). MRTK may not work as expected.");
        }

        void OnPermissionGranted(string permission)
        {
            Debug.Log($"{permission} newly granted for MRTK.");
        }
#endif
    }
}
