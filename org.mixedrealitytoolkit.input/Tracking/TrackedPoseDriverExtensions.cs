// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Extensions for Unity's <see cref="TrackedPoseDriver"/>
    /// </summary>
    public static class TrackedPoseDriverExtensions
    {
        /// <summary>
        /// Gets the tracking state of the <see cref="TrackedPoseDriver"/>. If the tracking state is not available, returns false.
        /// </summary>
        public static bool TryGetTrackingState(this TrackedPoseDriver driver, out InputTrackingState state)
        {
            state = InputTrackingState.None;
            var trackingStateAction = driver.trackingStateInput.action;
            if (trackingStateAction == null || trackingStateAction.bindings.Count == 0)
            {
                return false;
            }

            if (trackingStateAction.enabled)
            {
                return false;
            }

            if (!trackingStateAction.HasAnyControls())
            {
                return false;
            }

            state = (InputTrackingState)trackingStateAction.ReadValue<int>();
            return true;
        }

        /// <summary>
        /// Gets the tracking state of the <see cref="TrackedPoseDriver"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="TrackedPoseDriver"/> has no tracking state action or the action has no bindings, it will return `<see cref="InputTrackingState.Position"/> |
        /// <see cref="InputTrackingState.Rotation"/>`. If the action is enabled, it will return `<see cref="InputTrackingState.None"/>`. If the action has controls, it will return the value of the action.
        /// </remarks>
        public static InputTrackingState GetInputTrackingState(this TrackedPoseDriver driver)
        {
            // If the driver is a HandPoseDriver, return the cached value, istead of hitting the overhead of querying the action.
            if (driver is HandPoseDriver handPoseDriver)
            {
                return handPoseDriver.CachedTrackingState;
            }

            return GetInputTrackingStateNoCache(driver);
        }


        /// <summary>
        /// Gets the tracking state of the <see cref="TrackedPoseDriver"/>, avoid reading value for internal caches.
        /// </summary>
        /// <remarks>
        /// If the <see cref="TrackedPoseDriver"/> has no tracking state action or the action has no bindings, it will return `<see cref="InputTrackingState.Position"/> |
        /// <see cref="InputTrackingState.Rotation"/>`. If the action is enabled, it will return `<see cref="InputTrackingState.None"/>`. If the action has controls, it will return the value of the action.
        /// </remarks>
        internal static InputTrackingState GetInputTrackingStateNoCache(this TrackedPoseDriver driver)
        {
            // Note, that the logic in this class is meant to reproduce the same logic as the base. The base
            // `TrackedPoseDriver` also sets the tracking state in a similar manner. Please see 
            // `TrackedPoseDriver::ReadTrackingState`. Replicating this logic in a subclass is not ideal, but it is
            // necessary since the base class does not expose the tracking static update logic.

            var trackingStateAction = driver.trackingStateInput.action;
            if (trackingStateAction == null || trackingStateAction.bindings.Count == 0)
            {
                // Treat an Input Action Reference with no reference the same as
                // an enabled Input Action with no authored bindings, and allow driving the Transform pose.
                return InputTrackingState.Position | InputTrackingState.Rotation;
            }

            if (!trackingStateAction.enabled)
            {
                // Treat a disabled action as the default None value for the ReadValue call
                return InputTrackingState.None;
            }

            if (trackingStateAction.HasAnyControls())
            {
                return (InputTrackingState)trackingStateAction.ReadValue<int>();
            }
            else
            {
                return InputTrackingState.None;
            }
        }
    }
}
