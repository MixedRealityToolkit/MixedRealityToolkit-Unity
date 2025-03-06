// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
using Microsoft.MixedReality.OpenXR;
#endif

namespace MixedReality.Toolkit.Input
{
    [AddComponentMenu("MRTK/Input/Visualizers/Platform Hand Mesh Visualizer")]
    public class PlatformHandMeshVisualizer : HandMeshVisualizer
    {
        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private MeshRenderer handRenderer;

        /// <inheritdoc/>
        protected override Renderer HandRenderer => handRenderer;

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        private HandMeshTracker handMeshTracker;
#endif

        private Mesh neutralPoseMesh;
        private bool initializedUVs = false;

        private XRMeshSubsystem meshSubsystem = null;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            handRenderer.enabled = false;

            if (neutralPoseMesh == null)
            {
                neutralPoseMesh = new Mesh();
            }

#if UNITY_OPENXR_PRESENT
            if (UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled("XR_ANDROID_hand_mesh"))
            {
                List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
                SubsystemManager.GetSubsystems(meshSubsystems);
                foreach (XRMeshSubsystem subsystem in meshSubsystems)
                {
                    if (subsystem.subsystemDescriptor.id == "AndroidXRHandMeshProvider")
                    {
                        meshSubsystem = subsystem;
                        break;
                    }
                }
            }
            else if (UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled("XR_MSFT_hand_tracking_mesh"))
            {
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
                handMeshTracker = HandNode == XRNode.LeftHand ? HandMeshTracker.Left : HandMeshTracker.Right;
#endif
            }
            else
#endif
            {
                enabled = false;
            }
        }

        protected void Update()
        {
            if (meshSubsystem != null)
            {
                List<MeshInfo> meshInfos = new List<MeshInfo>();
                if (meshSubsystem.TryGetMeshInfos(meshInfos))
                {
                    int handMeshIndex = HandNode == XRNode.LeftHand ? 0 : 1;

                    if (meshInfos[handMeshIndex].ChangeState == MeshChangeState.Added
                        || meshInfos[handMeshIndex].ChangeState == MeshChangeState.Updated)
                    {
                        meshSubsystem.GenerateMeshAsync(meshInfos[handMeshIndex].MeshId, meshFilter.mesh,
                            null, MeshVertexAttributes.Normals, result => { });

                        if (!handRenderer.enabled)
                        {
                            handRenderer.enabled = true;
                        }
                    }
                }
                else if (handRenderer.enabled)
                {
                    handRenderer.enabled = false;
                }
            }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
            else if (!ShouldRenderHand() ||
                !handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, meshFilter.mesh) ||
                !handMeshTracker.TryLocateHandMesh(FrameTime.OnUpdate, out Pose pose))
            {
                // Hide the hand and abort if we shouldn't be
                // showing the hand, for whatever reason.
                // (Missing joint data, no subsystem, additive
                // display, etc!)
                handRenderer.enabled = false;
                return;
            }

            handRenderer.enabled = true;

            if (!initializedUVs && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, neutralPoseMesh, HandPoseType.ReferenceOpenPalm))
            {
                meshFilter.mesh.uv = InitializeUVs(neutralPoseMesh.vertices);
                initializedUVs = true;
            }

            transform.SetPositionAndRotation(pose.position, pose.rotation);
#endif

            UpdateHandMaterial();
        }

        protected override bool ShouldRenderHand()
        {
            // If we're missing anything, don't render the hand.
            return
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
                handMeshTracker != null &&
#endif
                meshFilter != null && handRenderer != null && base.ShouldRenderHand();
        }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
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
