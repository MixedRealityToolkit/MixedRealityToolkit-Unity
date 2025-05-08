// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.OpenXR;

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

        [SerializeField, Range(0, 1)]
        private float fadeDistance = 0.025f;

        /// <inheritdoc/>
        protected override Renderer HandRenderer => handRenderer;

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        private HandMeshTracker handMeshTracker;
        private Mesh neutralPoseMesh;
        private bool initializedUVs = false;
#endif

        // Share these among all instances to only query once per frame
        private static int lastUpdatedFrame = -1;
        private static XRHandSubsystem handSubsystem = null;
        private static XRHandMeshDataQueryResult result;
        private static XRHandMeshDataQueryParams queryParams = new()
        {
            allocator = Unity.Collections.Allocator.Temp,
        };

        // The property block used to modify the wrist position property on the material
        private MaterialPropertyBlock propertyBlock = null;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            handRenderer.enabled = false;
            propertyBlock ??= new MaterialPropertyBlock();

            if (handSubsystem != null)
            {
                return;
            }

            List<XRHandSubsystem> subsystems = XRSubsystemHelpers.GetAllSubsystems<XRHandSubsystem>();
            foreach (XRHandSubsystem subsystem in subsystems)
            {
                if (subsystem.GetProvider() is OpenXRHandProvider provider && provider.handMeshDataSupplier != null)
                {
                    Debug.Log($"Using {provider.handMeshDataSupplier.GetType()} for hand visualization.");
                    handSubsystem = subsystem;
                    return;
                }
            }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
            if (UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled("XR_MSFT_hand_tracking_mesh"))
            {
                Debug.Log($"Using XR_MSFT_hand_tracking_mesh for {HandNode} visualization.");
                handMeshTracker = HandNode == XRNode.LeftHand ? HandMeshTracker.Left : HandMeshTracker.Right;

                if (neutralPoseMesh == null)
                {
                    neutralPoseMesh = new Mesh();
                }

                return;
            }
#endif

            enabled = false;
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

            if (handSubsystem != null
                && handSubsystem.running
                && (lastUpdatedFrame == Time.frameCount || handSubsystem.TryGetMeshData(out result, ref queryParams)))
            {
                lastUpdatedFrame = Time.frameCount;
                XRHandMeshData handMeshData = HandNode == XRNode.LeftHand ? result.leftHand : result.rightHand;

                meshFilter.mesh.Clear();
                meshFilter.mesh.SetVertices(handMeshData.positions);
                meshFilter.mesh.SetUVs(0, handMeshData.uvs);
                meshFilter.mesh.SetIndices(handMeshData.indices, MeshTopology.Triangles, 0);

                handRenderer.enabled = true;

                if (!handMeshData.TryGetRootPose(out Pose rootPose))
                {
                    rootPose = Pose.identity;
                }

                transform.SetWorldPose(PlayspaceUtilities.TransformPose(rootPose));
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

        protected override void UpdateHandMaterial()
        {
            base.UpdateHandMaterial();

            if ((XRSubsystemHelpers.HandsAggregator?.TryGetJoint(TrackedHandJoint.Wrist, HandNode, out HandJointPose wristPose) ?? false)
                && XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.LittleMetacarpal, HandNode, out HandJointPose littleMetaPose)
                && XRSubsystemHelpers.HandsAggregator.TryGetJoint(TrackedHandJoint.ThumbMetacarpal, HandNode, out HandJointPose thumbMetaPose))
            {
                HandRenderer.GetPropertyBlock(propertyBlock);
                float radius = Vector3.Distance(littleMetaPose.Position, thumbMetaPose.Position);
                propertyBlock.SetVector("_FadeSphereCenter", wristPose.Position - (radius * 0.5f * wristPose.Forward));
                propertyBlock.SetFloat("_FadeSphereRadius", radius);
                propertyBlock.SetFloat("_FadeDistance", fadeDistance);
                HandRenderer.SetPropertyBlock(propertyBlock);
            }
        }

#if MROPENXR_PRESENT && (UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID)
        private static Vector2[] InitializeUVs(Vector3[] neutralPoseVertices)
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
