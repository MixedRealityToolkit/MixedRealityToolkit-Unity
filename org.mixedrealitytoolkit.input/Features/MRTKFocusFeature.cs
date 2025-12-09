// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Provides focus data based on XrSession state.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = FriendlyName,
        Desc = "Provides focus data based on XrSession state.",
        Company = "Mixed Reality Toolkit Contributors",
        Version = "4.0.0",
        BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.WSA, BuildTargetGroup.Android },
        Category = FeatureCategory.Feature,
        FeatureId = "org.mixedreality.toolkit.input.focus")]
#endif
    public sealed class MRTKFocusFeature : OpenXRFeature
    {
        /// <summary>
        /// The "friendly" name for this feature.
        /// </summary>
        public const string FriendlyName = "MRTK3 Session Focus";

        /// <summary>
        /// Whether the current XrSession has focus or not, stored as a bindable variable that can be subscribed to for value changes.
        /// </summary>
        /// <remarks>Always <see langword="true"/> in the editor.</remarks>
        public static IReadOnlyBindableVariable<bool> XrSessionFocused => xrSessionFocused;
        private static readonly BindableVariable<bool> xrSessionFocused = new(Application.isEditor);

        /// <inheritdoc/>
        protected override void OnSessionStateChange(int oldState, int newState)
        {
            // If we've lost focus...
            // XR_SESSION_STATE_FOCUSED = 5
            if (oldState == 5)
            {
                xrSessionFocused.Value = false;
            }
            // ...or if we've gained focus
            // XR_SESSION_STATE_FOCUSED = 5
            else if (newState == 5)
            {
                xrSessionFocused.Value = true;
            }
        }
    }
}
