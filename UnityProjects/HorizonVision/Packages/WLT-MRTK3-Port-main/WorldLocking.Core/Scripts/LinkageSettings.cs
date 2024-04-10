// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Explicitly set required Transform objects.
    /// </summary>
    /// <remarks>
    /// If Use Existing is not set, then null Transform objects will override the currently set Transforms.
    /// When one of the Transform objects is set to null, the system attempts to infer a reasonable choice.
    /// For complicated scenes, this inference may be incorrect. For non-trivial scenes:
    ///    ** If the camera rig is loaded per scene, then a Linkage Setting (via WorldLockingContext) should
    ///    be set per scene explicitly pointing into that scene's camera hierarchy.
    ///    ** If the camera rig is loaded once in a shared scene, the Linkage Setting should be in that scene only,
    ///    and all other Linkage Settings should set "Use Existing" to true.
    ///    ** If the camera rig is created/managed dynamically from script, then that script should also be responsible
    ///    for setting the appropriate linkages, and all LinkageSettings should specify "Use Existing".
    /// </remarks>
    [System.Serializable]
    public struct LinkageSettings
    {
        [SerializeField]
        [Tooltip("Ignore set values keep existing linkage, and use whatever was set last.")]
        private bool useExisting;
        /// <summary>
        /// Ignore set values keep existing linkage, and use whatever was set last.
        /// </summary>
        public bool UseExisting
        {
            get { return useExisting; }
            set
            {
                useExisting = value;
            }
        }

        [SerializeField]
        [Tooltip("Apply world locking adjustment to the AdjustmentFrame.")]
        private bool applyAdjustment;

        /// <summary>
        /// Zero out pitch and roll from the FrozenWorldEngine correction.
        /// </summary>
        [Tooltip("Zero out pitch and roll from the FrozenWorldEngine correction.")]
        public bool NoPitchAndRoll;

        /// <summary>
        /// Apply world locking adjustment to the AdjustmentFrame.
        /// </summary>
        /// <remarks>
        /// If this is false, then it is up to the application to apply the correction.
        /// This allows the correction to be applied selectively to subsets of the scene hierarchy.
        /// </remarks>
        public bool ApplyAdjustment { get { return applyAdjustment; } set { applyAdjustment = value; } }

        /// <summary>
        /// The transform at which to apply the camera adjustment. This can't be the camera node, as its
        /// transform is overwritten every frame with head pose data. But the camera should be an attached
        /// descendant of this node.
        /// </summary>
        [Tooltip("The transform at which to apply the camera adjustment.")]
        public Transform AdjustmentFrame;

        /// <summary>
        /// The camera parent node defines the "spongy frame of reference". All raw head based data,
        /// such as the spatial mapping, gesture events, and XR head pose data, are relative to this
        /// transform.
        /// </summary>
        [Tooltip("The Transform the camera is attached to. This extra node allows camera movement (e.g. Teleport).")]
        public Transform CameraParent;

        /// <summary>
        /// Init all fields to default values.
        /// </summary>
        public void InitToDefaults()
        {
            UseExisting = false;
            NoPitchAndRoll = false;
            ApplyAdjustment = true;
            AdjustmentFrame = null;
            CameraParent = null;
        }
    }

}