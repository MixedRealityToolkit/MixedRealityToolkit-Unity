// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
#endif

namespace MixedReality.Toolkit.Core.Tests
{
    /// <summary>
    /// Ensures XR initialization on start is disabled during PlayMode tests so that input simulation
    /// and synthetic hands function properly without being suppressed by active OpenXR or XR displays.
    /// State is maintained in memory without dirtying project settings on disk, and restored to its original value upon test completion or exit.
    /// </summary>
    internal class PlayModeTestXRDisabler : IPrebuildSetup, IPostBuildCleanup
    {
        private const string WasXRDisabledByTestKey = "MRTK_PlayModeTest_XRDisabledByTest";
        private static bool isSubscribed;

        /// <inheritdoc />
        void IPrebuildSetup.Setup()
        {
#if UNITY_EDITOR
            SubscribePlayModeStateChanged();
            DisableXRIfEnabled();
#endif
        }

        /// <inheritdoc />
        void IPostBuildCleanup.Cleanup()
        {
#if UNITY_EDITOR
            UnsubscribePlayModeStateChanged();
            RestoreXR();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
#if UNITY_EDITOR
            if (SessionState.GetBool(WasXRDisabledByTestKey, false))
            {
                ApplyDisableXR();
            }
#endif
        }

#if UNITY_EDITOR
        private static void SubscribePlayModeStateChanged()
        {
            if (!isSubscribed)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                isSubscribed = true;
            }
        }

        private static void UnsubscribePlayModeStateChanged()
        {
            if (isSubscribed)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                isSubscribed = false;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                UnsubscribePlayModeStateChanged();
                RestoreXR();
            }
        }

        private static XRGeneralSettings GetTargetSettings()
        {
            try
            {
                BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                return XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(targetGroup);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayModeTestXRDisabler] Failed to get XRGeneralSettings: {e.Message}");
                return null;
            }
        }

        private static void DisableXRIfEnabled()
        {
            XRGeneralSettings settings = GetTargetSettings();
            bool isEnabledOnTarget = settings != null && settings.InitManagerOnStart;
            bool isEnabledOnInstance = XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.InitManagerOnStart;

            if (isEnabledOnTarget || isEnabledOnInstance)
            {
                SessionState.SetBool(WasXRDisabledByTestKey, true);
                ApplyDisableXR();
            }
        }

        private static void ApplyDisableXR()
        {
            if (XRGeneralSettings.Instance != null)
            {
                XRGeneralSettings.Instance.InitManagerOnStart = false;
            }

            XRGeneralSettings settings = GetTargetSettings();
            if (settings != null)
            {
                settings.InitManagerOnStart = false;
            }
        }

        private static void RestoreXR()
        {
            if (!SessionState.GetBool(WasXRDisabledByTestKey, false))
            {
                return;
            }

            SessionState.EraseBool(WasXRDisabledByTestKey);

            XRGeneralSettings settings = GetTargetSettings();
            if (settings != null)
            {
                settings.InitManagerOnStart = true;
            }

            if (XRGeneralSettings.Instance != null)
            {
                XRGeneralSettings.Instance.InitManagerOnStart = true;
            }
        }
#endif
    }
}
#pragma warning restore CS1591
