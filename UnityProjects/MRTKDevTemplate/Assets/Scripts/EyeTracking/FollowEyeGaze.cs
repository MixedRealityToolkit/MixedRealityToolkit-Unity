// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using global::Unity.XR.CoreUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MixedReality.Toolkit.Examples
{
    /// <summary>
    /// Sample for allowing the game object that this script is attached to follow the user's eye gaze
    /// at a given distance of <see cref="defaultDistanceInMeters"/>. 
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/FollowEyeGaze")]
    public class FollowEyeGaze : MonoBehaviour
    {
        [Tooltip("Display the game object along the eye gaze ray at a default distance (in meters).")]
        [SerializeField]
        private float defaultDistanceInMeters = 2f;

        [Tooltip("The default color of the GameObject.")]
        [SerializeField]
        private Color idleStateColor;

        [Tooltip("The highlight color of the GameObject when hovered over another StatefulInteractable.")]
        [SerializeField]
        private Color hightlightStateColor;

        private Material material;

        [SerializeField]
        [Tooltip("The TrackedPoseDriver that represents the gaze pose.")]
        private TrackedPoseDriver gazePoseDriver;

        [SerializeField]
        [Tooltip("The IGazeInteractor that represents the gaze interaction.")]
        private XRBaseInputInteractor gazeInteractor;

        private List<IXRInteractable> targets;

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
            targets = new List<IXRInteractable>();
        }

        private void Update()
        {
            if (gazeInteractor == null)
            {
                return;
            }

            targets.Clear();
            gazeInteractor.GetValidTargets(targets);
            material.color = targets.Count > 0 ? hightlightStateColor : idleStateColor;

            if (TryGetGazeTransform(out Transform gazeTransform))
            {
                // Note: A better workflow would be to create and attach a prefab to the MRTK Gaze Controller object.
                // Doing this will parent the cursor to the gaze controller transform and be updated automatically.
                var pose = gazeTransform.GetWorldPose();
                transform.position = pose.position + gazeTransform.forward * defaultDistanceInMeters;
            }
        }

        /// <summary>
        /// Attempt to obtain the gaze transform.
        /// </summary>
        private bool TryGetGazeTransform(out Transform transform)
        {
            if (gazePoseDriver != null)
            {
                transform = gazePoseDriver.transform;
                return true;
            }
            else
            {
                transform = null;
                return false;
            }
        }
    }
}
