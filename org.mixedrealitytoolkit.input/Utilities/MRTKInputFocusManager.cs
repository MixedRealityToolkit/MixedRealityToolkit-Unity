// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
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

        private void OnEnable()
        {
            MRTKFocusFeature.XrSessionFocused.SubscribeAndUpdate(OnXrSessionFocus);
        }

        private void OnDisable()
        {
            MRTKFocusFeature.XrSessionFocused.Unsubscribe(OnXrSessionFocus);
        }

        /// <summary>
        /// Sent when the XrSession gains or loses focus.
        /// </summary>
        /// <param name="focus"><see langword="true"/> if the XrSession has focus, else <see langword="false"/>.</param>
        private void OnXrSessionFocus(bool focus)
        {
            // We want to ensure we're focused for input visualization, as some runtimes continue reporting "tracked" while pose updates are paused.
            // This is allowed, per-spec, as a "should": "Runtimes should make input actions inactive while the application is unfocused,
            // and applications should react to an inactive input action by skipping rendering of that action's input avatar
            // (depictions of hands or other tracked objects controlled by the user)."

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
    }
}
