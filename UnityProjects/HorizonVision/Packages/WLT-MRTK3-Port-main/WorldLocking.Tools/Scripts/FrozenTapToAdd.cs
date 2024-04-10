// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
#define WLT_ENABLE_LEGACY_WSA
#endif

using UnityEngine;
#if WLT_ENABLE_LEGACY_WSA
using UnityEngine.XR.WSA.Input;
#endif // WLT_ENABLE_LEGACY_WSA

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Simple class to adapt Unity's input results from spongy space into frozen space.
    /// This is unnecessary when using MRTK's input system, which already provides this
    /// and other enhancements and abstactions.
    /// </summary>
    public class FrozenTapToAdd : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The prefab to place in the world at gaze position on air taps.")]
        private GameObject prefabToPlace = null;
        /// <summary>
        /// The prefab to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabToPlace => prefabToPlace;

        /// <summary>
        /// Enable and disable processing of tap events.
        /// </summary>
        public bool Active { get; set; }

#if WLT_ENABLE_LEGACY_WSA
        private GestureRecognizer gestureRecognizer;
#endif // WLT_ENABLE_LEGACY_WSA

        private WorldLockingManager manager {  get { return WorldLockingManager.GetInstance(); } }

        // Start is called before the first frame update
        private void Start()
        {
#if WLT_ENABLE_LEGACY_WSA
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);

            gestureRecognizer.Tapped += HandleTapped;

            gestureRecognizer.StartCapturingGestures();
#endif // WLT_ENABLE_LEGACY_WSA
        }

#if WLT_ENABLE_LEGACY_WSA
        private void HandleTapped(TappedEventArgs eventArgs)
        {
            
            if (Active && PrefabToPlace != null)
            {
                // The tap event happens in Spongy space, so any arguments
                // from it are in spongy space and need to be converted to frozen space,
                // because the ray tests are done in frozen space.
                var spongyHeadPose = eventArgs.headPose;
                var frozenHeadPose = manager.FrozenFromSpongy.Multiply(spongyHeadPose);

                var rayStart = frozenHeadPose.position;
                var rayDir = frozenHeadPose.forward;

                int ignoreRaycastLayer = Physics.IgnoreRaycastLayer;
                int hitLayers = ~(ignoreRaycastLayer);
                RaycastHit hitInfo;
                if (Physics.Raycast(rayStart, rayDir, out hitInfo, Mathf.Infinity, hitLayers))
                {
                    int uiLayer = LayerMask.GetMask("UI");
                    if (hitInfo.collider == null || ((1 << hitInfo.collider.gameObject.layer) & uiLayer) == 0)
                    {
                        var hitPos = hitInfo.point;
                        var hitUp = hitInfo.normal;
                        var toRay = rayStart - hitPos;
                        var hitDirProj = toRay - Vector3.Dot(toRay, hitInfo.normal) * hitInfo.normal / hitInfo.normal.sqrMagnitude;
                        var hitRot = Quaternion.LookRotation(hitDirProj, hitUp);

                        var newObj = GameObject.Instantiate(PrefabToPlace, hitPos, hitRot, transform);

                        newObj.AddComponent<AdjusterFixed>();
                    }
                }
            }
        }
#endif // WLT_ENABLE_LEGACY_WSA
    }
}