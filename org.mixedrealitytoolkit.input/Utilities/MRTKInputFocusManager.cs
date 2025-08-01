// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Manages input based on XrSession focus.
    /// </summary>
    public sealed class MRTKInputFocusManager : MonoBehaviour
    {
        [SerializeField, Tooltip("A set of input actions to enable/disable according to the app's focus state.")]
        private InputActionReference[] inputActionReferences;

        /// <summary>
        /// Provides an event based on the current XrSession becoming focused or not.
        /// </summary>
        public static UnityEvent<bool> OnXrSessionFocus { get; } = new UnityEvent<bool>();

        /// <summary>
        /// We want to ensure we're focused for input, as some runtimes continue reporting "tracked" while pose updates are paused.
        /// This is allowed, per-spec, as a "should": "Runtimes should make input actions inactive while the application is unfocused,
        /// and applications should react to an inactive input action by skipping rendering of that action's input avatar
        /// (depictions of hands or other tracked objects controlled by the user)."
        /// </summary>
        private void OnFocusChange(bool focus)
        {
            OnXrSessionFocus?.Invoke(focus);

            foreach (InputActionReference reference in inputActionReferences)
            {
                if (focus)
                {
                    reference.action.Enable();
                }
                else
                {
                    reference.action.Disable();
                }
            }
        }

#if USING_SNAPDRAGON_SPACES_SDK
        private Qualcomm.Snapdragon.Spaces.SpacesOpenXRFeature spacesOpenXRFeature = null;
        private int lastSessionState = -1;

        private void Update()
        {
            if (spacesOpenXRFeature == null)
            {
                spacesOpenXRFeature = UnityEngine.XR.OpenXR.OpenXRSettings.Instance.GetFeature<Qualcomm.Snapdragon.Spaces.SpacesOpenXRFeature>();
            }

            // XrSessionState maps better to this behavior than OnApplicationFocus but isn't
            // easily available in Unity. For now, only the Snapdragon Spaces plugin provides it.
            if (spacesOpenXRFeature != null && lastSessionState != spacesOpenXRFeature.SessionState)
            {
                // If we've lost focus...
                // XR_SESSION_STATE_FOCUSED = 5
                if (lastSessionState == 5)
                {
                    OnFocusChange(false);
                }
                // ...or if we've gained focus
                // XR_SESSION_STATE_FOCUSED = 5
                else if (spacesOpenXRFeature.SessionState == 5)
                {
                    OnFocusChange(true);
                }

                lastSessionState = spacesOpenXRFeature.SessionState;
            }
        }
#elif !UNITY_EDITOR
        /// <summary>
        /// Sent to all GameObjects when the player gets or loses focus.
        /// </summary>
        /// <param name="focus"><see langword="true"/> if the GameObjects have focus, else <see langword="false"/>.</param>
        /// <remarks>
        /// Ideally, we'd use XrSessionState here, as it maps better to this behavior than OnApplicationFocus, but
        /// it isn't easily available in Unity. For now, only the Snapdragon Spaces plugin provides it, and we use
        /// OnApplicationFocus for the rest.
        /// </remarks>
        private void OnApplicationFocus(bool focus) => OnFocusChange(focus);
#endif
    }
}
