// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

#if UNITY_OPENXR_PRESENT
using MixedReality.Toolkit.Editor;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;
#endif

namespace MixedReality.Toolkit.Input.Editor
{
    /// <summary>
    /// A class adding input related rule(s) to the validator.
    /// </summary>
    internal static class InputValidation
    {
        [InitializeOnLoadMethod]
        private static void AddInputValidationRule()
        {
            foreach (var buildTargetGroup in MRTKProjectValidation.BuildTargetGroups)
            {
                MRTKProjectValidation.AddTargetDependentRules(new List<BuildValidationRule>() { GenerateSpeechInteractorRule(buildTargetGroup) }, buildTargetGroup);

#if UNITY_OPENXR_PRESENT
                // Skip the standalone target as the hand subsystem rule for it is already present for all build targets
                if (buildTargetGroup != BuildTargetGroup.Standalone)
                {
                    MRTKProjectValidation.AddTargetDependentRules(new List<BuildValidationRule>() { GenerateUnityHandsRule(buildTargetGroup) }, buildTargetGroup);
                }
#endif
            }
            MRTKProjectValidation.AddTargetIndependentRules(new List<BuildValidationRule>() { GenerateSkinWeightsRule(), GenerateGLTFastRule(),
#if UNITY_OPENXR_PRESENT
                GenerateUnityHandsRule(BuildTargetGroup.Standalone),
#endif
            });

            // Only generate the KTX rule for platforms related to Meta
            MRTKProjectValidation.AddTargetDependentRules(new List<BuildValidationRule>() { GenerateKTXRule(),
#if UNITY_OPENXR_PRESENT
                GenerateAndroidHandsRule(),
#endif
            }, BuildTargetGroup.Android);
            MRTKProjectValidation.AddTargetDependentRules(new List<BuildValidationRule>() { GenerateKTXRule() }, BuildTargetGroup.Standalone);
        }

        private static BuildValidationRule GenerateSpeechInteractorRule(BuildTargetGroup buildTargetGroup)
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = () => (MRTKProjectValidation.GetLoadedSubsystemsForBuildTarget(buildTargetGroup)?.Any(s => typeof(KeywordRecognitionSubsystem).IsAssignableFrom(s.Type))).GetValueOrDefault()
                    && FindObjectUtility.FindFirstObjectByType<SpeechInteractor>(true),
                Category = "MRTK3",
                Message = "The speech interactor needs to be active and enabled in the scene to allow for speech interactions with interactables (e.g. buttons).",
                CheckPredicate = () => FindObjectUtility.FindFirstObjectByType<SpeechInteractor>(true).isActiveAndEnabled,
                FixIt = () => EditorGUIUtility.PingObject(FindObjectUtility.FindFirstObjectByType<SpeechInteractor>(true)),
                FixItMessage = "Make sure the speech interactor component is enabled and in active in the hierarchy",
                FixItAutomatic = false,
                Error = false
            };
        }

        private static BuildValidationRule GenerateSkinWeightsRule()
        {
            return new BuildValidationRule()
            {
                Category = "MRTK3",
                Message = "The skin weights setting under quality settings needs to be set to TwoBones or higher for optimal rigged hand mesh visualizations.",
                CheckPredicate = () => QualitySettings.skinWeights > SkinWeights.OneBone,
                FixIt = () => QualitySettings.skinWeights = SkinWeights.TwoBones,
                FixItMessage = "Set the skin weights to TwoBones",
                FixItAutomatic = true,
                Error = false
            };
        }

        private static BuildValidationRule GenerateGLTFastRule()
        {
            return new BuildValidationRule()
            {
                Category = "MRTK3",
                Message = "For controller models to show up in scenes, you need a glTF importer. We recommend the open source glTFast importer. " +
                "Please ignore this message if you already have another glTF importer in the project.",
                CheckPredicate = () =>
                {
#if GLTFAST_PRESENT
                    return true;
#else
                    return false;
#endif
                },
                FixIt = () => Client.Add("com.unity.cloud.gltfast"),
                FixItMessage = "Add com.unity.cloud.gltfast to this project.",
                FixItAutomatic = true,
                Error = false
            };
        }

        private static BuildValidationRule GenerateKTXRule()
        {
            return new BuildValidationRule()
            {
                Category = "MRTK3",
                Message = "For Meta's controller models to show up in scenes, you need KTX support for your glTF importer. " +
                "If you are using our recommended importer glTFast, you need to also include the open source KTX Unity package in the project. " +
                "Please ignore this message if you already have another glTF importer with KTX support in the project.",
                CheckPredicate = () =>
                {
#if KTX_PRESENT
                    return true;
#else
                    return false;
#endif
                },
                FixIt = () => Client.Add("com.unity.cloud.ktx"),
                FixItMessage = "Add com.unity.cloud.ktx to this project.",
                FixItAutomatic = true,
                Error = false
            };
        }

#if UNITY_OPENXR_PRESENT
        private static BuildValidationRule GenerateUnityHandsRule(BuildTargetGroup buildTargetGroup)
        {
            return new BuildValidationRule()
            {
                IsRuleEnabled = () => MRTKProjectValidation.GetLoadedSubsystemsForBuildTarget(buildTargetGroup)?.Contains(typeof(UnityHandsSubsystem)) ?? false,
                Category = "MRTK3",
                Message = $"When {nameof(UnityHandsSubsystem)} is enabled for the {buildTargetGroup} build target, " +
                $"{nameof(HandTracking)} must also be enabled in the OpenXR settings for {buildTargetGroup}.",
                CheckPredicate = () =>
                {
                    OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
                    if (settings == null)
                    {
                        return false;
                    }

                    HandTracking handFeature = settings.GetFeature<HandTracking>();
                    return handFeature != null && handFeature.enabled;
                },
                FixIt = () =>
                {
                    OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTargetGroup);
                    if (settings == null)
                    {
                        return;
                    }

                    HandTracking handFeature = settings.GetFeature<HandTracking>();
                    if (handFeature != null)
                    {
                        handFeature.enabled = true;
                        EditorUtility.SetDirty(settings);
                    }
                },
                FixItMessage = $"Enable {nameof(HandTracking)} in the OpenXR settings.",
                Error = true
            };
        }

        private static BuildValidationRule GenerateAndroidHandsRule()
        {
            // Disable this warning because this rule's purpose is to help migrate away from the obsolete type
#pragma warning disable CS0618 // Type or member is obsolete
            return new BuildValidationRule()
            {
                Category = "MRTK3",
                Message = "Hand tracking on Android with the Mixed Reality OpenXR Plugin has been deprecated. " +
                $"Please turn off {nameof(OpenXRHandsSubsystem)} in the MRTK profile and use {nameof(UnityHandsSubsystem)} instead.",
                CheckPredicate = () => !MRTKProjectValidation.GetLoadedSubsystemsForBuildTarget(BuildTargetGroup.Android)?.Contains(typeof(OpenXRHandsSubsystem)) ?? true,
                FixIt = () =>
                {
                    MRTKProfile profile = MRTKSettings.ProfileForBuildTarget(BuildTargetGroup.Android);
                    if (profile == null)
                    {
                        return;
                    }

                    profile.LoadedSubsystems.Remove(typeof(OpenXRHandsSubsystem));
                    if (!profile.LoadedSubsystems.Contains(typeof(UnityHandsSubsystem)))
                    {
                        profile.LoadedSubsystems.Add(typeof(UnityHandsSubsystem));
                    }
                    EditorUtility.SetDirty(profile);
                },
                FixItMessage = $"Turn off {nameof(OpenXRHandsSubsystem)} in the MRTK profile and ensure {typeof(UnityHandsSubsystem)} is enabled instead.",
                Error = false
            };
#pragma warning restore CS0618 // Type or member is obsolete
        }
#endif
    }
}
