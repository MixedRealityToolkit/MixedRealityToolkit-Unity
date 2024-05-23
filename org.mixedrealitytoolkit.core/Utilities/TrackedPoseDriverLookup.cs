// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A basic convenience registry allowing easy reference to <see cref="TrackedPoseDriver"/> components.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MRTK/Core/Tracked Pose Driver Lookup")]
    public class TrackedPoseDriverLookup : MonoBehaviour
    {
        // Gaze
        [SerializeField]
        [Tooltip("The rig's gaze prefab Tracked Pose Driver.")]
        private TrackedPoseDriver gazeTrackedPoseDriver = null;

        /// <summary>
        /// The rig's gaze prefab Tracked Pose Driver.
        /// </summary>
        public TrackedPoseDriver GazeTrackedPoseDriver
        {
            get => gazeTrackedPoseDriver;
            set => gazeTrackedPoseDriver = value;
        }

        // Left Hand
        [SerializeField]
        [Tooltip("The rig's left hand prefab Tracked Pose Driver.")]
        private TrackedPoseDriver leftHandTrackedPoseDriver = null;

        /// <summary>
        /// The rig's left hand prefab Tracked Pose Driver.
        /// </summary>
        public TrackedPoseDriver LeftHandTrackedPoseDriver
        {
            get => leftHandTrackedPoseDriver;
            set => leftHandTrackedPoseDriver = value;
        }

        // Right Hand
        [SerializeField]
        [Tooltip("The rig's right hand prefab Tracked Pose Driver.")]
        private TrackedPoseDriver rightHandTrackedPoseDriver = null;

        /// <summary>
        /// The rig's right hand prefab Tracked Pose Driver.
        /// </summary>
        public TrackedPoseDriver RightHandTrackedPoseDriver
        {
            get => rightHandTrackedPoseDriver;
            set => rightHandTrackedPoseDriver = value;
        }

        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (FindObjectUtility.FindObjectsByType<TrackedPoseDriverLookup>(false, false).Length > 1)
            {
                Debug.LogWarning("Found more than one instance of the ControllerLookup class in the hierarchy. There should only be one");
            }
        }
    }
}
