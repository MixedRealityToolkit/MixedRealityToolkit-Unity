// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using UnityEngine;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A pose source composed computed from an ordered list of pose sources. Returns the result of the first pose source
    /// which successfully returns a pose.
    /// </summary>
    [Serializable]
    public class FallbackCompositePoseSource : IPoseSource
    {
        [SerializeReference]
        [InterfaceSelector]
        [Tooltip("An ordered list of pose sources to query.")]
        private IPoseSource[] poseSourceList;

        /// <summary>
        /// An ordered list of pose sources to query.
        /// </summary>
        protected IPoseSource[] PoseSources { get => poseSourceList; set => poseSourceList = value; }

        /// <summary>
        /// Tries to get a pose from each pose source in order, returning the result of the first pose source
        /// which returns a success.
        /// </summary>
        public bool TryGetPose(out Pose pose)
        {
            for (int i = 0; i < poseSourceList.Length; i++)
            {
                IPoseSource currentPoseSource = poseSourceList[i];
                if (currentPoseSource != null && currentPoseSource.TryGetPose(out pose))
                {
                    return true;
                }
            }

            pose = Pose.identity;
            return false;
        }
    }
}
