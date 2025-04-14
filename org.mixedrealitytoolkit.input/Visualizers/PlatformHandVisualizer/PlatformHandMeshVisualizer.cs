// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

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
        private Mesh neutralPoseMesh;
        private bool initializedUVs = false;
#endif

        private XRMeshSubsystem meshSubsystem = null;
        private readonly List<MeshInfo> meshInfos = new List<MeshInfo>();

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            handRenderer.enabled = false;

#if UNITY_OPENXR_PRESENT
            if (UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled("XR_ANDROID_hand_mesh"))
            {
                List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
                SubsystemManager.GetSubsystems(meshSubsystems);
                foreach (XRMeshSubsystem subsystem in meshSubsystems)
                {
                    if (subsystem.subsystemDescriptor.id == "AndroidXRHandMeshProvider")
                    {
                        Debug.Log($"Using XR_ANDROID_hand_mesh for {HandNode} visualization.");
                        meshSubsystem = subsystem;
                        break;
                    }
                }
            }
            else if (UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled("XR_MSFT_hand_tracking_mesh"))
            {
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
                Debug.Log($"Using XR_MSFT_hand_tracking_mesh for {HandNode} visualization.");
                handMeshTracker = HandNode == XRNode.LeftHand ? HandMeshTracker.Left : HandMeshTracker.Right;

                if (neutralPoseMesh == null)
                {
                    neutralPoseMesh = new Mesh();
                }
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
            if (!ShouldRenderHand())
            {
                // Hide the hand and abort if we shouldn't be
                // showing the hand, for whatever reason.
                // (Missing joint data, no subsystem, additive
                // display, etc!)
                handRenderer.enabled = false;
                return;
            }

            if (meshSubsystem != null
                && meshSubsystem.running
                && meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                int handMeshIndex = HandNode == XRNode.LeftHand ? 0 : 1;

                MeshInfo meshInfo = meshInfos[handMeshIndex];
                if (meshInfo.ChangeState == MeshChangeState.Added
                    || meshInfo.ChangeState == MeshChangeState.Updated)
                {
                    meshSubsystem.GenerateMeshAsync(meshInfo.MeshId, meshFilter.mesh,
                        null, MeshVertexAttributes.Normals, result => { });

                    handRenderer.enabled = true;

                    // This hand mesh is provided pre-translated from the world origin,
                    // so we want to ensure the mesh is "centered" at the world origin
                    PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                    transform.SetPositionAndRotation(position, rotation);
                }
            }
#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
            else if (handMeshTracker != null
                && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, meshFilter.mesh)
                && handMeshTracker.TryLocateHandMesh(FrameTime.OnUpdate, out Pose pose))
            {
                // On some runtimes, the mesh is moved in its local space instead of world space,
                // while its world space location is unchanged. In this case, we want to ensure the
                // bounds follow the hand around by manually recalculating them.
                meshFilter.mesh.RecalculateBounds();

                handRenderer.enabled = true;

                if (!initializedUVs && handMeshTracker.TryGetHandMesh(FrameTime.OnUpdate, neutralPoseMesh, HandPoseType.ReferenceOpenPalm))
                {
                    meshFilter.mesh.uv = InitializeUVs(neutralPoseMesh.vertices);
                    initializedUVs = true;
                }

                transform.SetWorldPose(PlayspaceUtilities.TransformPose(pose));
            }
#endif
            else
            {
                // Hide the hand and abort if we shouldn't be
                // showing the hand, for whatever reason.
                // (Missing joint data, no subsystem, additive
                // display, etc!)
                handRenderer.enabled = false;
                return;
            }

            UpdateHandMaterial();
        }

        protected override bool ShouldRenderHand()
        {
            // If we're missing anything, don't render the hand.
            return meshFilter != null && handRenderer != null && base.ShouldRenderHand();
        }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        private static Color32[] InitializeColors(Vector3[] neutralPoseVertices)
        {
            if (neutralPoseVertices?.Length == 0)
            {
                Debug.LogError("Loaded 0 vertices for neutralPoseVertices");
                return System.Array.Empty<Color32>();
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

            Color32[] colors = new Color32[neutralPoseVertices.Length];

            for (int i = 0; i < neutralPoseVertices.Length; i++)
            {
                Debug.Log($"{neutralPoseVertices[i].y} | {neutralPoseVertices[i].y * scale} | {Mathf.Clamp01(((neutralPoseVertices[i].y * scale) - 0.1f) * 10f)}");
                //colors[i] = new Color32(164, 25, 28, 255);
                colors[i] = Color32.Lerp(Color.black, new Color32(164, 25, 28, 255), Mathf.Clamp01(((neutralPoseVertices[i].y * scale) - 0.1f) * 10f));
            }

            return colors;
        }

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
