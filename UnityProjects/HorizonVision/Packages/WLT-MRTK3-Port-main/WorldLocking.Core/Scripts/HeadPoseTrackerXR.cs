// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class HeadPoseTrackerXR : IHeadPoseTracker
    {
        private readonly List<XRNodeState> nodeStates = new List<XRNodeState>();

        private Pose headPose = Pose.identity;

        /// <inheritdoc/>
        public void Reset()
        {
            headPose = Pose.identity;
        }

        /// <inheritdoc/>
        public Pose GetHeadPose()
        {
            // Note:
            // The low-level input obtained via InputTracking.GetLocal???(XRNode.Head) is automatically kept in sync with
            // Camera.main.transform.local??? (unless XRDevice.DisableAutoXRCameraTracking(Camera.main, true) is used to deactivate
            // this mechanism). In theory, both could be used interchangeably, potentially allowing to avoid the dependency
            // on low-level code at this point. It is not clear though, whether both values follow exactly the same timing or which
            // one is more correct to be used at this point. More research might be necessary.
            // 
            // The decision between low-level access via InputTracking and high-level access via Camera.main.transform should
            // be coordinated with the decision between high-level access to anchors and low-level access to
            // Windows.Perception.Spatial.SpatialAnchor -- see comment at top of SpongyAnchor.cs
            nodeStates.Clear();
            InputTracking.GetNodeStates(nodeStates);
            for (int i = 0; i < nodeStates.Count; ++i)
            {
                if (nodeStates[i].nodeType == XRNode.Head)
                {
                    Vector3 position;
                    Quaternion rotation;
                    if (nodeStates[i].tracked && nodeStates[i].TryGetPosition(out position) && nodeStates[i].TryGetRotation(out rotation))
                    {
                        headPose = new Pose(position, rotation);
                    }
                }
            }
            return headPose;
        }

    }
}
