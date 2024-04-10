// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class HeadPoseTrackerCamera : IHeadPoseTracker
    {
        private Camera camera = Camera.main;

        public virtual void Reset()
        {
            camera = Camera.main;
        }

        public void BindToCamera(Camera c)
        {
            camera = c;
        }

        public virtual Pose GetHeadPose()
        {
            Pose headPose = Pose.identity;
            if (camera != null)
            {
                Pose spongyFromCamera = camera.transform.GetLocalPose();
                headPose = spongyFromCamera;
            }
            return headPose;
        }
    }
}