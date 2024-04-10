// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core.Triangulator;
using UnityEngine.Assertions.Must;

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    public class SpacePinMeshVisualizer : MonoBehaviour
    {

        [Range(0.1f, 2.0f)]
        public float weightCubeMaxSize = 1.0f;

        public float verticalOffset = -1.65f;
        public float textVerticalOffset = -1.45f;

        public Material wireFrameMaterial = null;
        public Material meshMaterial = null;
        public Material extrapolatedMeshMaterial = null;
        public Material weightsMaterial = null;

        public GameObject percentageWorldSpaceCanvasPrefab;

        [SerializeField]
        [Tooltip("Subtree whose SpacePins should be visualized. Null for global AlignmentManager.")]
        private AlignSubtree targetSubtree = null;

        /// <summary>
        /// Subtree whose SpacePins should be visualized. Null for global AlignmentManager.
        /// </summary>
        public AlignSubtree TargetSubtree
        {
            get { return targetSubtree; }
            set
            {
                targetSubtree = value;
                FindAlignmentManager();
            }
        }

        private IAlignmentManager alignmentManager = null;
        private SimpleTriangulator triangulator = null;
        private Interpolant currentInterpolant = null;

        // Texts visualising the percentage in actual numbers.
        private SpacePinPercentageVisualizer[] percentageVisualizers = new SpacePinPercentageVisualizer[3];

        // For rendering the cubeweights and the currently active triangle
        private MeshRenderer meshRenderer = null;
        private MeshFilter meshFilter = null;

        private bool triangleIsDirty = false;
        private int currentBoundaryVertexIDx = -1;

        private Mesh wireFrameMesh = null;
        private Mesh currentTriangleMesh = null;
        private Mesh[] triangleWeightMeshes = new Mesh[3];

        private Vector3 firstPinPosition, secondPinPosition, thirdPinPosition;
        private Vector3 firstCubePosition, secondCubePosition, thirdCubePosition;

        private int[] lastGeneratedTriangleIDs = new int[3] { -1, -1, -1 };

        [SerializeField]
        private bool isVisible = true;

        private const string HeadsetPositionMaterialProperty = "_HeadSetWorldPosition";
        private const string WeightVectorOffsetMaterialProperty = "_VectorOffset";
        private const string WeightMaterialProperty = "_Weight";

        #region Public APIs
        public bool GetVisibility()
        {
            return isVisible;
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            RefreshVisibility();
        }

        #endregion

        private void RefreshVisibility()
        {
            if (meshRenderer != null)
                meshRenderer.enabled = isVisible;

            for (int i = 0; i < 3; i++)
            {
                if (percentageVisualizers[i] != null)
                {
                    percentageVisualizers[i].SetVisibility(isVisible);
                }
            }
        }

        /// <summary>
        /// Injecting the reference to the triangulation that was newly built.
        /// </summary>
        /// <param name="triangulator">Reference to the data on the triangle that was built.</param>
        private void Initialize(ITriangulator triangulator)
        {
            this.triangulator = (SimpleTriangulator)triangulator;

            if (meshRenderer == null && meshFilter == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshFilter = gameObject.AddComponent<MeshFilter>();

                Material[] materials = new Material[5];

                materials[0] = new Material(meshMaterial);
                materials[1] = new Material(weightsMaterial);
                materials[2] = new Material(weightsMaterial);
                materials[3] = new Material(weightsMaterial);
                materials[4] = new Material(wireFrameMaterial);

                meshRenderer.materials = materials;

                for (int i = 0; i < 3; i++)
                {
                    GameObject instantiatedGameObject = Instantiate(percentageWorldSpaceCanvasPrefab, transform);
                    SpacePinPercentageVisualizer percentageVisualizer = instantiatedGameObject.GetComponent<SpacePinPercentageVisualizer>();
                    percentageVisualizers[i] = percentageVisualizer;
                }
            }

            transform.position = new Vector3(transform.position.x, GetGlobalHeadPosition().y + verticalOffset, transform.position.z);
            triangleIsDirty = true;
        }

        private static Pose GetGlobalFromLocked()
        {
            var wltMgr = WorldLockingManager.GetInstance();
            Pose globalFromLocked = wltMgr.ApplyAdjustment
                ? wltMgr.FrozenFromLocked
                : wltMgr.SpongyFromLocked;

            return globalFromLocked;
        }

        private struct IndexedTriangle
        {
            public int index0;
            public int index1;
            public int index2;

            public IndexedTriangle(int i0, int i1, int i2)
            {
                index0 = i0;
                index1 = i1;
                index2 = i2;
            }
        }

        /// <summary>
        /// Generates the whole mesh inside the triangulation data, except for the boundary triangles/vertices.
        /// </summary>
        /// <returns></returns>
        private Mesh GenerateTriangulationWireFrameMesh()
        {
            Mesh wholeMesh = new Mesh();

            Vector3[] originalVertices = triangulator.Vertices.ToArray();
            Vector3[] vertices = new Vector3[triangulator.Vertices.Count - 4];

            Array.Copy(originalVertices, 4, vertices, 0, originalVertices.Length - 4);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = 0.0f;
            }

            wholeMesh.vertices = vertices;

            List<IndexedTriangle> trimmedTriangles = new List<IndexedTriangle>();

            for (int i = 0; i < triangulator.Triangles.Length; i += 3)
            {
                if (triangulator.Triangles[i] <= 3 || triangulator.Triangles[i + 1] <= 3 || triangulator.Triangles[i + 2] <= 3)
                    continue;

                //Don't render currently selected triangle
                bool triangleDataSameAsClosestTriangle =
                    triangulator.Triangles[i] - 4 == currentInterpolant.idx[0] &&
                    triangulator.Triangles[i + 1] - 4 == currentInterpolant.idx[1] &&
                    triangulator.Triangles[i + 2] - 4 == currentInterpolant.idx[2];

                if (currentInterpolant != null && (triangleDataSameAsClosestTriangle && !AnyWeightInTriangleZero()))
                    continue;

                trimmedTriangles.Add(new IndexedTriangle(
                        triangulator.Triangles[i] - 4,
                        triangulator.Triangles[i + 1] - 4,
                        triangulator.Triangles[i + 2] - 4
                    ));
            }

            int[] tris = new int[trimmedTriangles.Count * 3];

            int triIndex = 0;
            for (int i = 0; i < trimmedTriangles.Count; i++)
            {
                tris[triIndex] = trimmedTriangles[i].index0;
                tris[triIndex + 1] = trimmedTriangles[i].index1;
                tris[triIndex + 2] = trimmedTriangles[i].index2;
                triIndex += 3;
            }

            wholeMesh.triangles = tris;

            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            wholeMesh.normals = normals;

            return wholeMesh;
        }

        /// <summary>
        /// Generates and combines the whole triangulation mesh, the triangle we are currently in, and the cubes representing the three SpacePins weights as sub meshes.
        /// </summary>
        private void GenerateMeshes()
        {
            currentTriangleMesh = new Mesh();

            if (currentInterpolant == null)
                return;

            CalculatePinPositionsFromCurrentInterpolant();

            Vector3[] vertices = new Vector3[3]
            {
                firstPinPosition,
                secondPinPosition,
                thirdPinPosition
            };
            currentTriangleMesh.vertices = vertices;

            int[] tris = new int[3]
            {
                2, 1, 0
            };

            currentTriangleMesh.triangles = tris;

            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            currentTriangleMesh.normals = normals;

            Vector2[] uv = new Vector2[vertices.Length];

            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(Mathf.InverseLerp(-1000f, 1000f, vertices[i].x), Mathf.InverseLerp(-1000f, 1000f, vertices[i].z));
            }

            currentTriangleMesh.uv = uv;

            currentTriangleMesh.RecalculateBounds();

            if (meshRenderer.materials[0] != null)
                Destroy(meshRenderer.materials[0]);

            Material[] materials = meshRenderer.materials;
            materials[0] = currentBoundaryVertexIDx != -1 ? new Material(extrapolatedMeshMaterial) : new Material(meshMaterial);
            meshRenderer.materials = materials;

            CombineInstance[] combine = new CombineInstance[5];

            combine[0].mesh = currentTriangleMesh;
            combine[0].transform = Matrix4x4.zero;
            meshRenderer.materials[0].SetVector(WeightVectorOffsetMaterialProperty, (firstPinPosition + secondPinPosition + thirdPinPosition) / 3);

            firstCubePosition = firstPinPosition;
            combine[1].mesh = triangleWeightMeshes[0] = CreateCube(firstPinPosition);
            combine[1].transform = Matrix4x4.zero;
            meshRenderer.materials[1].SetVector(WeightVectorOffsetMaterialProperty, firstPinPosition);

            secondCubePosition = secondPinPosition;
            combine[2].mesh = triangleWeightMeshes[1] = CreateCube(secondPinPosition);
            combine[2].transform = Matrix4x4.zero;
            meshRenderer.materials[2].SetVector(WeightVectorOffsetMaterialProperty, secondPinPosition);

            thirdCubePosition = thirdPinPosition;
            combine[3].mesh = triangleWeightMeshes[2] = CreateCube(thirdPinPosition);
            combine[3].transform = Matrix4x4.zero;
            meshRenderer.materials[3].SetVector(WeightVectorOffsetMaterialProperty, thirdPinPosition);

            combine[4].mesh = wireFrameMesh = GenerateTriangulationWireFrameMesh();
            combine[4].transform = Matrix4x4.zero;

            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine, false, false);

            triangleIsDirty = false;
        }

        private Mesh CreateCube(Vector3 offset)
        {
            Mesh cube = new Mesh();

            float s = weightCubeMaxSize;

            Vector3[] vertices = {
                new Vector3 (0, 0, 0),
                new Vector3 (s, 0, 0),
                new Vector3 (s, s, 0),
                new Vector3 (0, s, 0),
                new Vector3 (0, s, s),
                new Vector3 (s, s, s),
                new Vector3 (s, 0, s),
                new Vector3 (0, 0, s),
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += offset - new Vector3(s / 2, s / 2, s / 2);
            }

            cube.vertices = vertices;

            int[] triangles = {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            cube.triangles = triangles;

            cube.RecalculateNormals();
            cube.RecalculateBounds();

            return cube;
        }

        private void UpdateMaterialProperties()
        {
            meshRenderer.materials[1].SetFloat(WeightMaterialProperty, currentInterpolant.weights[0]);
            meshRenderer.materials[2].SetFloat(WeightMaterialProperty, currentInterpolant.weights[1]);
            meshRenderer.materials[3].SetFloat(WeightMaterialProperty, currentInterpolant.weights[2]);

            meshRenderer.materials[0].SetVector(HeadsetPositionMaterialProperty, GetLockedHeadPosition());
        }

        private void UpdatePercentageTexts()
        {
            if (currentInterpolant == null || triangulator == null)
                return;

            percentageVisualizers[0].transform.localPosition = new Vector3(firstPinPosition.x, -(verticalOffset - textVerticalOffset), firstPinPosition.z); ;
            percentageVisualizers[0].UpdatePercentage(currentInterpolant.weights[0] * 100.0f);
            percentageVisualizers[0].SetVisibility(triangulator.Vertices.Count >= 6 && currentBoundaryVertexIDx != 0);

            percentageVisualizers[1].transform.localPosition = new Vector3(secondPinPosition.x, -(verticalOffset - textVerticalOffset), secondPinPosition.z); ;
            percentageVisualizers[1].UpdatePercentage(currentInterpolant.weights[1] * 100.0f);
            percentageVisualizers[1].SetVisibility(triangulator.Vertices.Count >= 6 && currentBoundaryVertexIDx != 1);

            percentageVisualizers[2].transform.localPosition = new Vector3(thirdPinPosition.x, -(verticalOffset - textVerticalOffset), thirdPinPosition.z); ;
            percentageVisualizers[2].UpdatePercentage(currentInterpolant.weights[2] * 100.0f);
            percentageVisualizers[2].SetVisibility(triangulator.Vertices.Count >= 6 && currentBoundaryVertexIDx != 2);
        }

        private void CalculatePinPositionsFromCurrentInterpolant()
        {
            if (currentInterpolant == null)
                return;

            bool hasBoundaryVertex = false;

            for (int i = 0; i < currentInterpolant.idx.Length; i++)
            {
                if (currentInterpolant.weights[i] <= 0.001f && currentInterpolant.idx[i] == 0)
                {
                    hasBoundaryVertex = true;
                    currentBoundaryVertexIDx = i;
                }
            }

            currentBoundaryVertexIDx = hasBoundaryVertex ? currentBoundaryVertexIDx : -1;

            Vector3 lockedHeadPosition = GetLockedHeadPosition();
            lockedHeadPosition.y = 0.0f;

            firstPinPosition = currentBoundaryVertexIDx == 0 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[0] + 4];
            secondPinPosition = currentBoundaryVertexIDx == 1 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[1] + 4];
            thirdPinPosition = currentBoundaryVertexIDx == 2 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[2] + 4];

            //    DEBUG TRIANGLE    //
            //firstPinPosition = new Vector3(5.0f, 0.0f, 0.0f);
            //secondPinPosition = new Vector3(1.0f, 0.0f, 1.0f);
            //thirdPinPosition = new Vector3(-1.0f,0.0f,-1.0f);

            firstPinPosition.y = secondPinPosition.y = thirdPinPosition.y = 0.0f;
        }

        /// <summary>
        /// Updates the three vertex's position that make the currently interpolated triangle
        /// </summary>
        private void UpdateVertexPositions()
        {
            CalculatePinPositionsFromCurrentInterpolant();

            List<Vector3> vertices = new List<Vector3>();
            meshFilter.mesh.GetVertices(vertices);

            bool anyPositionChanged = false;

            if (currentBoundaryVertexIDx != 0 && vertices[0] != firstPinPosition)
            {
                vertices[0] = firstPinPosition;
                anyPositionChanged = true;
            }
            if (currentBoundaryVertexIDx != 1 && vertices[1] != secondPinPosition)
            {
                vertices[1] = secondPinPosition;
                anyPositionChanged = true;
            }
            if (currentBoundaryVertexIDx != 2 && vertices[2] != thirdPinPosition)
            {
                vertices[2] = thirdPinPosition;
                anyPositionChanged = true;
            }

            Vector3[] pinPositions = new Vector3[3] { firstPinPosition, secondPinPosition, thirdPinPosition };
            if (currentBoundaryVertexIDx != -1 && vertices[currentBoundaryVertexIDx] != pinPositions[currentBoundaryVertexIDx])
            {
                vertices[currentBoundaryVertexIDx] = pinPositions[currentBoundaryVertexIDx];
                anyPositionChanged = true;
            }

            if (anyPositionChanged)
            {
                meshFilter.mesh.SetVertices(vertices);
                UpdateCubeVertexPositions();
            }

            meshRenderer.materials[0].SetVector(WeightVectorOffsetMaterialProperty, (vertices[0] + vertices[1] + vertices[2]) / 3);
        }


        /// <summary>
        /// Snap the 3 cube meshes to the pin positions by changing local vertex data.
        /// </summary>
        private void UpdateCubeVertexPositions()
        {
            List<Vector3> vertices = new List<Vector3>();
            meshFilter.mesh.GetVertices(vertices);

            Vector3[] oldPositions = new Vector3[3] { firstCubePosition, secondCubePosition, thirdCubePosition };
            Vector3[] newPositions = new Vector3[3] { firstPinPosition, secondPinPosition, thirdPinPosition };

            int index = 0;
            for (int i = 3; i < vertices.Count - wireFrameMesh.vertexCount; i += 8)
            {
                for (int j = i; j < i + 8; j++)
                {
                    vertices[j] -= oldPositions[index];
                    vertices[j] += newPositions[index];
                }

                meshRenderer.materials[index + 1].SetVector(WeightVectorOffsetMaterialProperty, newPositions[index]);

                index++;
            }

            firstCubePosition = firstPinPosition;
            secondCubePosition = secondPinPosition;
            thirdCubePosition = thirdPinPosition;

            meshFilter.mesh.SetVertices(vertices);
        }

        private Vector3 GetLockedHeadPosition()
        {
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Pose lockedHeadPose = wltMgr.LockedFromPlayspace.Multiply(wltMgr.PlayspaceFromSpongy.Multiply(wltMgr.SpongyFromCamera));
            return lockedHeadPose.position;
        }

        private Vector3 GetGlobalHeadPosition()
        {
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Vector3 position = wltMgr.SpongyFromCamera.position;
            if (wltMgr.ApplyAdjustment)
            {
                position = wltMgr.FrozenFromSpongy.Multiply(position);
            }
            return position;
        }

        private void FindAlignmentManager()
        {
            AlignSubtree subTree = targetSubtree;
            if (subTree != null)
            {
                FindAlignmentManagerFromSubtree(subTree);
            }
            else
            {
                SetAlignmentManager(WorldLockingManager.GetInstance().AlignmentManager);
            }
        }

        private void FindAlignmentManagerFromSubtree(AlignSubtree subTree)
        {
            Debug.Assert(subTree != null, "Trying to find alignment manager from null subTree.");
            if (subTree.AlignmentManager == null)
            {
                subTree.OnAlignManagerCreated += (sender, manager) =>
                {
                    SetAlignmentManager(manager);
                };
            }
            else
            {
                SetAlignmentManager(subTree.AlignmentManager);
            }
        }

        private void SetAlignmentManager(IAlignmentManager manager)
        {
            if (alignmentManager != null)
            {
                alignmentManager.OnTriangulationBuilt -= OnNewTriangulationWasBuilt;
            }
            alignmentManager = manager;
            if (alignmentManager != null)
            {
                alignmentManager.OnTriangulationBuilt += OnNewTriangulationWasBuilt;
            }
        }

        private void OnNewTriangulationWasBuilt(object sender, ITriangulator triangulation)
        {
            Initialize(triangulation);
        }

        private bool AnyWeightInTriangleZero()
        {
            float tolerance = 0.0001f; 
            return currentInterpolant.weights[0] < tolerance || currentInterpolant.weights[1] < tolerance || currentInterpolant.weights[2] < tolerance;
        }

        private void OnDestroy()
        {
            if (alignmentManager != null)
            {
                alignmentManager.OnTriangulationBuilt -= OnNewTriangulationWasBuilt;
            }
        }

        private void Update()
        {
            if (alignmentManager == null)
            {
                FindAlignmentManager();
            }
            if (triangulator != null && isVisible)
            {
                // Find the three closest SpacePins this frame
                Interpolant interpolantThisFrame = triangulator.Find(GetLockedHeadPosition());

                if (interpolantThisFrame != null)
                {
                    currentInterpolant = interpolantThisFrame;

                    // Only generate new mesh if SpacePins are different from the currently generated ones
                    if (!Enumerable.SequenceEqual(interpolantThisFrame.idx, lastGeneratedTriangleIDs) || triangleIsDirty || (AnyWeightInTriangleZero() && currentBoundaryVertexIDx == -1) || (!AnyWeightInTriangleZero() && currentBoundaryVertexIDx != -1))
                    {
                        GenerateMeshes();
                        lastGeneratedTriangleIDs = interpolantThisFrame.idx;
                    }
                    // if SpacePins are same update the vertices in case the current SpacePins moved somehow,
                    // or there is a boundary vertex that needs to be snapped to the headset position every frame.
                    else
                    {
                        UpdateVertexPositions();
                    }

                    UpdateMaterialProperties();
                    UpdatePercentageTexts();
                }
                TransformFromLockedToGlobal();
            }
        }

        private void TransformFromLockedToGlobal()
        {
            Pose globalFromLocked = GetGlobalFromLocked();
            globalFromLocked.position += new Vector3(0.0f, verticalOffset, 0.0f);
            transform.SetGlobalPose(globalFromLocked);
        }
    }
}
