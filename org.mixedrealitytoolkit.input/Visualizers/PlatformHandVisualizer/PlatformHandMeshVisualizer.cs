// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using UnityEngine.XR.OpenXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
using Microsoft.MixedReality.OpenXR;
#endif

namespace MixedReality.Toolkit.Input
{
    public class PlatformHandMeshVisualizer : HandMeshVisualizer
    {
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private Renderer handRenderer;

        [SerializeField]
        [Tooltip("Name of the shader property used to drive pinch-amount-based visual effects. " +
         "Generally, maps to something like a glow or an outline color!")]
        private string pinchAmountMaterialProperty = "_PinchAmount";

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        private HandMeshTracker handMeshTracker;
#endif

        private Mesh neutralPoseMesh;
        private bool initializedUVs = false;

        // The property block used to modify the pinch amount property on the material
        private MaterialPropertyBlock propertyBlock = null;

        // The XRController that is used to determine the pinch strength (i.e., select value!)
        private XRBaseController controller;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (neutralPoseMesh == null)
            {
                neutralPoseMesh = new Mesh();
            }

            propertyBlock ??= new MaterialPropertyBlock();

            if (OpenXRRuntime.IsExtensionEnabled("XR_MSFT_hand_tracking_mesh"))
            {
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
                handMeshTracker = HandNode == UnityEngine.XR.XRNode.LeftHand ? HandMeshTracker.Left : HandMeshTracker.Right;
#endif
            }
        }

        protected void Update()
        {
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
            if (ShouldRenderHand() && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, meshFilter.mesh))
            {
                if (!initializedUVs && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, neutralPoseMesh, HandPoseType.ReferenceOpenPalm))
                {
                    meshFilter.mesh.uv = InitializeUVs(neutralPoseMesh.vertices);
                    initializedUVs = true;
                }

                if (handMeshTracker.TryLocateHandMesh(FrameTime.OnUpdate, out Pose pose))
                {
                    transform.SetPositionAndRotation(pose.position, pose.rotation);
                }

                if (!handRenderer.enabled)
                {
                    handRenderer.enabled = true;
                }
            }
            else if (handRenderer.enabled)
            {
                handRenderer.enabled = false;
            }
#endif

            UpdateHandMaterial();
        }

        private void UpdateHandMaterial()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<XRBaseController>();
            }

            if (controller == null || handRenderer == null) { return; }

            // Update the hand material
            float pinchAmount = Mathf.Pow(controller.selectInteractionState.value, 2.0f);
            handRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(pinchAmountMaterialProperty, pinchAmount);
            handRenderer.SetPropertyBlock(propertyBlock);
        }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        protected override bool ShouldRenderHand()
        {
            // If we're missing anything, don't render the hand.
            return handMeshTracker != null && meshFilter != null && handRenderer != null && base.ShouldRenderHand();
        }

        private Vector2[] InitializeUVs(Vector3[] neutralPoseVertices)
        {
            if (neutralPoseVertices?.Length == 0)
            {
                Debug.LogError("Loaded 0 vertices for neutralPoseVertices");
                return System.Array.Empty<Vector2>();
            }

            float minY = neutralPoseVertices[0].y;
            float maxY = minY;

            for (int ix = 1; ix < neutralPoseVertices.Length; ix++)
            {
                Vector3 p = neutralPoseVertices[ix];

                if (p.y < minY)
                {
                    minY = p.y;
                }
                else if (p.y > maxY)
                {
                    maxY = p.y;
                }
            }

            float scale = 1.0f / (maxY - minY);

            Vector2[] uvs = new Vector2[neutralPoseVertices.Length];

            for (int ix = 0; ix < neutralPoseVertices.Length; ix++)
            {
                Vector3 p = neutralPoseVertices[ix];

                uvs[ix] = new Vector2(p.x * scale + 0.5f, (p.y - minY) * scale);
            }

            return uvs;
        }
#endif
    }
}
