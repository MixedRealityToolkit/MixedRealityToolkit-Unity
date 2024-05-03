// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MixedReality.Toolkit
{
    /// <summary>
    /// A basic convenience registry allowing easy reference
    /// to controllers.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("MRTK/Core/Controller Lookup")]
    public class ControllerLookup : MonoBehaviour
    {
        // Gaze
        [SerializeField]
        [Tooltip("The camera rig's gaze controller.")]
        private ActionBasedXRI3Controller gazeController = null;

        /// <summary>
        /// The camera rig's gaze controller.
        /// </summary>
        public ActionBasedXRI3Controller GazeController
        {
            get => gazeController;
            set => gazeController = value;
        }

        // Left Hand
        [SerializeField]
        [Tooltip("The camera rig's left hand controller.")]
        private ActionBasedXRI3Controller leftHandController = null;

        /// <summary>
        /// The camera rig's left hand controller.
        /// </summary>
        public ActionBasedXRI3Controller LeftHandController
        {
            get => leftHandController;
            set => leftHandController = value;
        }

        // Right Hand
        [SerializeField]
        [Tooltip("The camera rig's right hand controller.")]
        private ActionBasedXRI3Controller rightHandController = null;

        /// <summary>
        /// The camera rig's right hand controller.
        /// </summary>
        public ActionBasedXRI3Controller RightHandController
        {
            get => rightHandController;
            set => rightHandController = value;
        }
        
        /// <summary>
        /// A Unity Editor only event function that is called when the script is loaded or a value changes in the Unity Inspector.
        /// </summary>
        private void OnValidate()
        {
            if (FindObjectUtility.FindObjectsByType<ControllerLookup>(false, false).Length > 1)
            {
                Debug.LogWarning("Found more than one instance of the ControllerLookup class in the hierarchy. There should only be one");
            }
        }
    }
}
