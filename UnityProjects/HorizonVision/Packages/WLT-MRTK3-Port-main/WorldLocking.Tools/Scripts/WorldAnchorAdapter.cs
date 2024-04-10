// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Adapter for aligning an object with a WorldAnchor.
    /// </summary>
    public class WorldAnchorAdapter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The GameObject holding the WorldAnchor component.")]
        private GameObject worldAnchorObject;
        /// <summary>
        /// The GameObject holding the WorldAnchor component.
        /// </summary>
        public GameObject WorldAnchorObject
        {
            get { return worldAnchorObject; }
            set { worldAnchorObject = value; CheckActive(); }
        }

        [SerializeField]
        [Tooltip("The GameObject to be aligned to the WorldAnchor.")]
        private Transform targetObject;
        /// <summary>
        /// The GameObject to be aligned to the WorldAnchor.
        /// </summary>
        public Transform TargetObject
        {
            get { return targetObject; }
            set { targetObject = value; CheckActive(); }
        }

        private bool active = false;

        private void CheckActive()
        {
            active = (targetObject != null)
                && (worldAnchorObject != null);
        }

        // Start is called before the first frame update
        void Start()
        {
            CheckActive();
        }


        // Update is called once per frame
        // Alternatively, we could make this a coroutine, and call it at a user specified interval.
        private void Update()
        {
            if (active)
            {
                Pose spongyFromAnchor = WorldAnchorObject.transform.GetGlobalPose();
                Pose frozenFromSpongy = WorldLockingManager.GetInstance().FrozenFromSpongy;
                Pose frozenFromAnchor = frozenFromSpongy.Multiply(spongyFromAnchor);
                TargetObject.SetGlobalPose(frozenFromAnchor);
            }
        }
    }
}