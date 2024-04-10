// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Namespace to separate the placeholder triangulation and interpolation mechanism.
/// </summary>
namespace Microsoft.MixedReality.WorldLocking.Core.Triangulator
{
    /// <summary>
    /// Container for three indices and a weight for each index, everything needed to interpolate
    /// between the data associated with each index, except for the data itself.
    /// </summary>
    /// <remarks>
    /// This could be generalized to have N-indices and weights instead, for example to represent
    /// interpolation along an edge, or exact hit on a single index, or even weighted averages
    /// of N-polygons. 
    /// </remarks>
    public class Interpolant
    {
        /// <summary>
        /// Three indices. 
        /// </summary>
        public readonly int[] idx = new int[3];
        /// <summary>
        /// Three weights.
        /// </summary>
        public readonly float[] weights = new float[3];

        /// <summary>
        /// True if this represents a true interpolation (rather than an extrapolation).
        /// </summary>
        public bool IsInterior
        {
            get
            {
                return weights[0] >= 0 && weights[1] >= 0 && weights[2] >= 0;
            }
        }
    }

    /// <summary>
    /// Very simple interface for triangulator, to avoid building complex dependencies.
    /// </summary>
    public interface ITriangulator
    {
        /// <summary>
        /// Set the bounds of vertices to be triangulated. All vertices entered in Add() should be inside the quad formed by these bounds.
        /// </summary>
        /// <param name="minPos">The minimum coordinates of the bounds.</param>
        /// <param name="maxPos">The maximum coordinates of the bounds.</param>
        /// <remarks>
        /// The vertical coordinate Y is ignored.
        /// Note that queries outsde the bounds are okay, only all vertices must be contained.
        /// </remarks>
        void SetBounds(Vector3 minPos, Vector3 maxPos);

        /// <summary>
        /// Add vertices.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <returns>True on success.</returns>
        bool Add(Vector3[] vertices);

        /// <summary>
        /// Find the interpolant for the given query position.
        /// </summary>
        /// <param name="pos">The query positon.</param>
        /// <returns>An interpolant if found, else null.</returns>
        /// <remarks>
        /// Note that one or more weights may be zero, but otherwise all indices returned will be valid.
        /// But while the data behind a vertex with weight zero may be referenced (as the index is valid),
        /// it is otherwise meaningless.
        /// The return value of null only happens if no vertices have been added to be interpolated.
        /// </remarks>
        Interpolant Find(Vector3 pos);

        /// <summary>
        /// Clear out all vertices added so far.
        /// </summary>
        void Clear();
    };

    /// <summary>
    /// Basic implementation of ITriangulator. Not optimized.
    /// </summary>
    /// <remarks>
    /// This has been written for simplicity for triangulating a small number of vertices.
    /// It lacks optimizations such as a full Delaunay triangulation on setup or
    /// hierarchical search (e.g. quadtree) on search.
    /// </remarks>
    public class SimpleTriangulator : ITriangulator
    {
        #region Internal data types.
        /// <summary>
        /// Representation of an indexed triangle.
        /// </summary>
        private struct Triangle
        {
            public int idx0;
            public int idx1;
            public int idx2;
        }

        /// <summary>
        /// Representation of an indexed edge.
        /// </summary>
        private struct Edge
        {
            public int idx0;
            public int idx1;
        }

        /// <summary>
        /// Interpolant plus the index of the triangle being interpolated.
        /// </summary>
        private class IndexedBary
        {
            public int triangle;
            public Interpolant bary;
        }

        #endregion Internal data types.

        #region Internal data containers
        /// <summary>
        /// List of all vertices.
        /// </summary>
        private List<Vector3> vertices = new List<Vector3>();
        /// <summary>
        /// List of all triangles generated.
        /// </summary>
        private List<Triangle> triangles = new List<Triangle>();
        #endregion Internal data containers

        #region Public APIs
        /// <summary>
        /// Reset to original state, discarding all.
        /// </summary>
        /// <remarks>
        /// Note this discards the bounds as well, so they must be set again after each clear.
        /// </remarks>
        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            exteriorEdges.Clear();
        }

        /// <inheritdocs />
        public void SetBounds(Vector3 minPos, Vector3 maxPos)
        {
            Debug.Assert(vertices.Count == 0, "Must set bounds before adding vertices. To update bounds, Clear, SetBounds, Add(vertices) again.");
            Clear();
            Vector3[] bounds = new Vector3[4];
            bounds[0] = new Vector3(minPos.x, 0.0f, maxPos.z);
            bounds[1] = new Vector3(minPos.x, 0.0f, minPos.z);
            bounds[2] = new Vector3(maxPos.x, 0.0f, minPos.z);
            bounds[3] = new Vector3(maxPos.x, 0.0f, maxPos.z);
            SeedQuad(bounds);
        }

        /// <summary>
        /// Add vertices to further triangulate.
        /// </summary>
        /// <param name="vertices">The new vertices to add.</param>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// Bounds should already be set. Also, the bounds should be big enough to contain all
        /// vertices being added.
        /// </remarks>
        public bool Add(Vector3[] vertices)
        {
            Debug.Assert(this.vertices.Count >= 4, "Must set bounds before adding vertices.");
            for (int i = 0; i < vertices.Length; ++i)
            {
                AddVertexSubdividing(vertices[i]);
            }
            FlipLongEdges();
            FindExteriorEdges();
            return true;
        }

        /// <inheritdocs />
        public Interpolant Find(Vector3 pos)
        {
            Interpolant bary = FindTriangleOrEdgeOrVertex(pos);
            AdjustForBoundingIndices(bary);
            return bary;
        }

        /// <summary>
        /// SpacePinMeshVisualizer uses this dto as reference.
        /// </summary>
        public List<Vector3> Vertices => vertices;

        /// <summary>
        /// SpacePinMeshVisualizer uses this dto as reference.
        /// </summary>
        public int[] Triangles
        {
            get
            {
                int[] tris = new int[triangles.Count * 3];
                for (int i = 0; i < triangles.Count; i++)
                {
                    tris[i * 3] = triangles[i].idx0; 
                    tris[i * 3 + 1] = triangles[i].idx1; 
                    tris[i * 3 + 2] = triangles[i].idx2;
                }
                return tris;
            }
        }

        #endregion Public APIs

        #region Internal query helpers
        /// <summary>
        /// Adjust the indices accounting for the 4 dummy verts introduced in SetBounds()/SeedQuad().
        /// </summary>
        /// <param name="bary">The interpolant whose indices need adjusting.</param>
        /// <remarks>
        /// Note that with this current implementation, once the indices are corrected, it's no longer possible to tell whether they are boundary vertices.
        /// </remarks>
        private void AdjustForBoundingIndices(Interpolant bary)
        {
            if (bary != null)
            {
                for (int i = 0; i < 3; ++i)
                {
                    if (!IsBoundary(bary.idx[i]))
                    {
                        bary.idx[i] = bary.idx[i] - 4;
                    }
                    else
                    {
                        Debug.Assert(bary.weights[i] == 0.0f);
                    }
                }
            }
        }
        #endregion Internal query helpers

        #region Internal triangulation
        /// <summary>
        /// Add the list of vertices as a quad.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <returns>True on success.</returns>
        private bool SeedQuad(Vector3[] vertices)
        {
            Clear();
            for (int i = 0; i < vertices.Length; ++i)
            {
                this.vertices.Add(vertices[i]);
            }
            for (int idxBase = 0; idxBase < vertices.Length; idxBase += 3)
            {
                if (idxBase + 2 < vertices.Length)
                {
                    triangles.Add(MakeTriangle(idxBase + 1, idxBase + 2, idxBase + 0));
                }
                if (idxBase + 3 < vertices.Length)
                {
                    triangles.Add(MakeTriangle(idxBase + 0, idxBase + 2, idxBase + 3));
                }
            }
            return triangles.Count > 0;
        }

        /// <summary>
        /// Add a triangle with proper winding order.
        /// </summary>
        /// <param name="idx0">Index of first vertex.</param>
        /// <param name="idx1">Index of second vertex.</param>
        /// <param name="idx2">Index of third vertex.</param>
        /// <returns>The filled out triangle struct.</returns>
        private Triangle MakeTriangle(int idx0, int idx1, int idx2)
        {
            float cross = -Vector3.Cross(vertices[idx2] - vertices[idx1], vertices[idx0] - vertices[idx1]).y;
            Debug.Assert(cross != 0, "Degenerate triangle");
            if (cross < 0)
            {
                return new Triangle() { idx0 = idx0, idx1 = idx2, idx2 = idx1 };
            }
            return new Triangle() { idx0 = idx0, idx1 = idx1, idx2 = idx2 };
        }

        /// <summary>
        /// Verify that a triangle has proper winding order of vertices and is not degenerate.
        /// </summary>
        /// <param name="triIdx">The triangle to test.</param>
        /// <returns>True if the triangle is okay.</returns>
        private bool WindingCorrect(int triIdx)
        {
            Triangle tri = triangles[triIdx];
            float cross = -Vector3.Cross(vertices[tri.idx2] - vertices[tri.idx1], vertices[tri.idx0] - vertices[tri.idx1]).y;
            return cross > 0;
        }

        /// <summary>
        /// Add a new vertex to the field, breaking existing triangles as necessary.
        /// </summary>
        /// <param name="vtx">Position of the new vertex to add.</param>
        private void AddVertexSubdividing(Vector3 vtx)
        {
            vertices.Add(vtx);
            int newVertIdx = vertices.Count - 1;

            // Find closest triangle
            IndexedBary bary = FindTriangle(vtx);
            Debug.Assert(bary.bary.IsInterior, "Should be contained by background seed vertices.");

            // Find closest edge
            Edge edge = ClosestEdge(bary);

            // Find any other triangle with that edge
            int oppositieTriIdx = FindTriangleWithEdge(edge, bary.triangle);

            bool canSplit = CanSplit(edge, oppositieTriIdx, newVertIdx);

            if (canSplit)
            {
                AddVertexSplitEdge(edge, bary.triangle, oppositieTriIdx, newVertIdx);
            }
            else
            {
                AddVertexMidTriangle(bary.triangle, newVertIdx);
            }
        }

        /// <summary>
        /// Test whether a triangle can be split along the given edge to insert the given vertex.
        /// </summary>
        /// <param name="edge">The shared edge to split.</param>
        /// <param name="triIdx">The triangle on the shared edge which doesn't contain the vertex.</param>
        /// <param name="newVertIdx">The vertex</param>
        /// <returns>True if the edge can be split.</returns>
        private bool CanSplit(Edge edge, int triIdx, int newVertIdx)
        {
            if (triIdx < 0)
            {
                return false;
            }
            Triangle tri = triangles[triIdx];
            if (!EdgesEqual(edge, tri.idx0, tri.idx1))
            {
                if (IsOutsideEdge(tri.idx0, tri.idx1, newVertIdx))
                {
                    return false;
                }
            }
            if (!EdgesEqual(edge, tri.idx1, tri.idx2))
            {
                if (IsOutsideEdge(tri.idx1, tri.idx2, newVertIdx))
                {
                    return false;
                }
            }
            if (!EdgesEqual(edge, tri.idx2, tri.idx0))
            {
                if (IsOutsideEdge(tri.idx2, tri.idx0, newVertIdx))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test that a vertex is outside the triangle edge formed by the other vertices.
        /// </summary>
        /// <param name="vtx0">First triangle edge vertex.</param>
        /// <param name="vtx1">Second triangle edge vertex.</param>
        /// <param name="vtxTest">Vertex to test.</param>
        /// <returns>True if the tested vertex is outside the triangle based on that edge alone.</returns>
        private bool IsOutsideEdge(int vtx0, int vtx1, int vtxTest)
        {
            float cross = -Vector3.Cross(vertices[vtx1] - vertices[vtx0], vertices[vtxTest] - vertices[vtx0]).y;
            return cross <= 0;
        }

        /// <summary>
        /// Add a vertex splitting the edge between two triangles, to form 4 new triangles.
        /// </summary>
        /// <param name="edge">The edge to split.</param>
        /// <param name="triIdx0">The triangle on the first side of the edge.</param>
        /// <param name="triIdx1">The triangle on the other side of the edge.</param>
        /// <param name="newVertIdx">The vertex to insert.</param>
        /// <remarks>
        /// Note that restrictions apply. Specifically, the new vertex must be inside
        /// one of the triangles (or on the shared edge), and furthermore it inside
        /// the spaces defined by the two non-shared edges of the triangle that doesn't
        /// contain it. The latter condition is necessary to ensure that the created triangle
        /// doesn't overlap any other existing triangles.
        /// </remarks>
        private void AddVertexSplitEdge(Edge edge, int triIdx0, int triIdx1, int newVertIdx)
        {
            SplitEdge(triIdx0, edge, newVertIdx);
            SplitEdge(triIdx1, edge, newVertIdx);
        }

        /// <summary>
        /// Add a vertex in the middle of a triangle, converting the one triangle into three.
        /// </summary>
        /// <param name="triIdx">The triangle to subdivide.</param>
        /// <param name="newVertIdx">The vertex to insert.</param>
        private void AddVertexMidTriangle(int triIdx, int newVertIdx)
        {
            Triangle tri = triangles[triIdx];

            Triangle newTri = new Triangle()
            {
                idx0 = tri.idx0,
                idx1 = tri.idx1,
                idx2 = newVertIdx
            };
            triangles[triIdx] = newTri;
            newTri = new Triangle()
            {
                idx0 = tri.idx1,
                idx1 = tri.idx2,
                idx2 = newVertIdx
            };
            triangles.Add(newTri);
            newTri = new Triangle()
            {
                idx0 = tri.idx2,
                idx1 = tri.idx0,
                idx2 = newVertIdx
            };
            triangles.Add(newTri);
        }

        /// <summary>
        /// Split an edge, inserting a new vertex. This will break the indicated triangle into two triangles.
        /// </summary>
        /// <param name="triIdx">The triangle to split.</param>
        /// <param name="edge">The triangle edge to split.</param>
        /// <param name="newVertIdx">The index of the new vertex.</param>
        private void SplitEdge(int triIdx, Edge edge, int newVertIdx)
        {
            Triangle tri = triangles[triIdx];
            if (EdgesEqual(edge, tri.idx0, tri.idx1))
            {
                Triangle newTri0 = new Triangle()
                {
                    idx0 = tri.idx0,
                    idx1 = newVertIdx,
                    idx2 = tri.idx2
                };
                Triangle newTri1 = new Triangle()
                {
                    idx0 = newVertIdx,
                    idx1 = tri.idx1,
                    idx2 = tri.idx2
                };
                triangles[triIdx] = newTri0;
                triangles.Add(newTri1);
            }
            else if (EdgesEqual(edge, tri.idx1, tri.idx2))
            {
                Triangle newTri0 = new Triangle()
                {
                    idx0 = tri.idx0,
                    idx1 = tri.idx1,
                    idx2 = newVertIdx
                };
                Triangle newTri1 = new Triangle()
                {
                    idx0 = newVertIdx,
                    idx1 = tri.idx2,
                    idx2 = tri.idx0
                };
                triangles[triIdx] = newTri0;
                triangles.Add(newTri1);
            }
            else
            {
                Debug.Assert(EdgesEqual(edge, tri.idx2, tri.idx0));
                Triangle newTri0 = new Triangle()
                {
                    idx0 = newVertIdx,
                    idx1 = tri.idx1,
                    idx2 = tri.idx2
                };
                Triangle newTri1 = new Triangle()
                {
                    idx0 = newVertIdx,
                    idx1 = tri.idx0,
                    idx2 = tri.idx1
                };
                triangles[triIdx] = newTri0;
                triangles.Add(newTri1);
            }
        }

        /// <summary>
        /// Generate a list of all unique interior edges, sorted by length, longest to shortest.
        /// </summary>
        /// <remarks>
        /// Some bounding edges may be absent, but they aren't used by the caller of this function anyway.
        /// Only edges with the first index smaller than the second are returned. 
        /// *  An interior edge will appear in exactly two triangles.
        /// *  The order of the two edge vertices will be swapped between the two triangles that share it
        ///     (because of winding order).
        /// So this will return all interior edges only once.
        /// It may also return some exterior (boundary) edges. They could be filtered, but aren't.
        /// </remarks>
        /// <returns>New list of unique edges.</returns>
        private List<Edge> ListSharedEdges()
        {
            List<Edge> edges = new List<Edge>();
            for (int i = 0; i < triangles.Count; ++i)
            {
                Triangle tri = triangles[i];
                if (tri.idx0 < tri.idx1)
                {
                    edges.Add(new Edge() { idx0 = tri.idx0, idx1 = tri.idx1 });
                }
                if (tri.idx1 < tri.idx2)
                {
                    edges.Add(new Edge() { idx0 = tri.idx1, idx1 = tri.idx2 });
                }
                if (tri.idx2 < tri.idx0)
                {
                    edges.Add(new Edge() { idx0 = tri.idx2, idx1 = tri.idx0 });
                }
            }
            /// Sort the edges longest to shortest.
            edges.Sort((e0, e1) =>
                Vector3.SqrMagnitude(vertices[e1.idx0] - vertices[e1.idx1])
                    .CompareTo(Vector3.SqrMagnitude(vertices[e0.idx0] - vertices[e0.idx1])));
            return edges;
        }

        /// <summary>
        /// Test if a vertex is inside the triangle formed by 3 other vertices.
        /// </summary>
        /// <param name="t0">First triangle vertex</param>
        /// <param name="t1">Second triangle vertex</param>
        /// <param name="t2">Third triangle vertex</param>
        /// <param name="ttest">Vertex to test.</param>
        /// <returns>True if ttest is on or inside the triangle.</returns>
        /// <remarks>
        /// This will return true if the tested vertex is "near" an edge. This helps
        /// prevent creating long obtuse triangles by turning edges.
        /// </remarks>
        private bool IsInsideTriangle(int t0, int t1, int t2, int ttest)
        {
            Vector3 v0 = vertices[t0];
            Vector3 v1 = vertices[t1];
            Vector3 v2 = vertices[t2];
            float area = -Vector3.Cross(v2 - v1, v0 - v1).y;
            float nearIn = -area * 1.0e-4f;
            Vector3 vt = vertices[ttest];
            /// Note the order swap here to account for this being xz not xy.
            return Vector3.Cross(vt - v0, v1 - v0).y >= nearIn
                && Vector3.Cross(vt - v1, v2 - v1).y >= nearIn
                && Vector3.Cross(vt - v2, v0 - v2).y >= nearIn;
        }

        /// <summary>
        /// Do partial regularization of triangles by going through and flipping edges where 
        /// it will result in a shorter shared edge between two triangles.
        /// </summary>
        private void FlipLongEdges()
        {
            /// Make a list of all unique edges (no duplicates).
            List<Edge> edges = ListSharedEdges();

            /// For each edge in the list
            for (int iEdge = 0; iEdge < edges.Count; ++iEdge)
            {
                Edge edge = edges[iEdge];
                /// Find the two triangles sharing it.
                int tri0 = FindTriangleWithEdge(edge, -1);
                Debug.Assert(tri0 >= 0, "Can't find a triangle with a known edge");
                int tri1 = FindTriangleWithEdge(edge, tri0);

                /// If there are two triangles
                if (tri1 >= 0)
                {
                    /// Shift the indices to form (i,j,k),(k,j,l) where (j,k) is the edge.
                    ShiftTriangles(edge, tri0, tri1);

                    Triangle t0 = triangles[tri0];
                    Triangle t1 = triangles[tri1];
                    if (!IsInsideTriangle(t0.idx0, t0.idx1, t1.idx2, t0.idx2)
                        && !IsInsideTriangle(t0.idx0, t1.idx2, t0.idx2, t0.idx1))
                    {
                        float edgeLengthSq = (vertices[edge.idx0] - vertices[edge.idx1]).sqrMagnitude;
                        /// Find distance between vertices that aren't on the edge,
                        /// which is vert[0] from tri0 and vert[2] from tri1.
                        float crossLengthSq = (vertices[triangles[tri0].idx0] - vertices[triangles[tri1].idx2]).sqrMagnitude;

                        /// If that distance is shorter than edge length
                        if (crossLengthSq < edgeLengthSq)
                        {
                            /// change tri0 to (k,i,l) and tri1 to (l,i,j)
                            t0 = new Triangle()
                            {
                                idx0 = triangles[tri0].idx2,
                                idx1 = triangles[tri0].idx0,
                                idx2 = triangles[tri1].idx2
                            };
                            t1 = new Triangle()
                            {
                                idx0 = triangles[tri1].idx2,
                                idx1 = triangles[tri0].idx0,
                                idx2 = triangles[tri0].idx1
                            };
                            triangles[tri0] = t0;
                            triangles[tri1] = t1;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Test an edge for having a vertex as either endpoint.
        /// </summary>
        /// <param name="edge">The edge to test.</param>
        /// <param name="vertIdx">The vertex to look for.</param>
        /// <returns></returns>
        private static bool EdgeHasVertex(Edge edge, int vertIdx)
        {
            return vertIdx == edge.idx0 || vertIdx == edge.idx1;
        }

        /// <summary>
        /// Shift the indices of both triangles to form (i,j,k),(k,j,l) where (j,k) is the shared edge.
        /// </summary>
        /// <param name="edge">The shared edge.</param>
        /// <param name="tri0">The first triangle.</param>
        /// <param name="tri1">The second triangle.</param>
        private void ShiftTriangles(Edge edge, int tri0, int tri1)
        {
            Triangle t0 = triangles[tri0];
            while (t0.idx0 == edge.idx0 || t0.idx0 == edge.idx1)
            {
                int k = t0.idx0;
                t0.idx0 = t0.idx1;
                t0.idx1 = t0.idx2;
                t0.idx2 = k;
            }
            Debug.Assert(EdgeHasVertex(edge, t0.idx1));
            Debug.Assert(EdgeHasVertex(edge, t0.idx2));
            triangles[tri0] = t0;

            t0 = triangles[tri1];
            while (t0.idx2 == edge.idx0 || t0.idx2 == edge.idx1)
            {
                int k = t0.idx0;
                t0.idx0 = t0.idx1;
                t0.idx1 = t0.idx2;
                t0.idx2 = k;
            }
            Debug.Assert(EdgeHasVertex(edge, t0.idx0));
            Debug.Assert(EdgeHasVertex(edge, t0.idx1));
            triangles[tri1] = t0;
        }

        /// <summary>
        /// Find a triangle (if any) which has the given edge but isn't the triangle specified.
        /// </summary>
        /// <remarks>
        /// Primary use of this is to find the triangle on the other side of the edge from 
        /// the triangle with notTriangle index.
        /// If the notTriangle doesn't actually have the edge (or is not a valid triangle
        /// index), then the first triangle that does have the edge is returned.
        /// </remarks>
        /// <param name="edge">The edge to find.</param>
        /// <param name="notTriangle">The triangle to not return.</param>
        /// <returns>Index of triangle satisfying search criteria, or -1 if no such triangle exists.</returns>
        private int FindTriangleWithEdge(Edge edge, int notTriangle)
        {
            for (int i = 0; i < triangles.Count; ++i)
            {
                if (i != notTriangle)
                {
                    Triangle tri = triangles[i];
                    if (EdgesEqual(edge, tri.idx0, tri.idx1)
                        || EdgesEqual(edge, tri.idx1, tri.idx2)
                        || EdgesEqual(edge, tri.idx2, tri.idx0))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Evaluate as equivalent (vertex order independent) an edge with a the edge 
        /// that might be created from the two vertex indices.
        /// </summary>
        /// <param name="edge">The edge to evaluate.</param>
        /// <param name="idx0">One vertex of the potential edge.</param>
        /// <param name="idx1">Other vertex of the potential edge.</param>
        /// <returns>True if the two edges would be equivalent.</returns>
        private bool EdgesEqual(Edge edge, int idx0, int idx1)
        {
            if (edge.idx0 == idx0 && edge.idx1 == idx1)
            {
                return true;
            }
            if (edge.idx1 == idx0 && edge.idx0 == idx1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find the edge closest to the interpolation point, or equivalently the edge opposite the
        /// vertex with the weakest influence.
        /// </summary>
        /// <param name="bary">The triangular interpolation.</param>
        /// <returns>Edge data for strongest edge.</returns>
        private Edge ClosestEdge(IndexedBary bary)
        {
            Triangle tri = triangles[bary.triangle];
            Edge edge;
            edge.idx0 = tri.idx1;
            edge.idx1 = tri.idx2;
            float minWeight = bary.bary.weights[0];
            if (bary.bary.weights[1] < minWeight)
            {
                edge.idx0 = tri.idx0;
                edge.idx1 = tri.idx2;
                minWeight = bary.bary.weights[1];
            }
            if (bary.bary.weights[2] < minWeight)
            {
                edge.idx0 = tri.idx0;
                edge.idx1 = tri.idx1;
            }
            return edge;
        }

#endregion Internal triangulation

#region Internal query support
        /// <summary>
        /// Brute force search to find a triangle that the query position projects onto.
        /// </summary>
        /// <param name="pos">The query position.</param>
        /// <returns>The triangle projected onto, with interpolation information.</returns>
        /// <remarks>
        /// Note the returned value is never null. Also, if there is any data to query (Add() has been
        /// called with a vertex list of length > 0), then the returned value.bary will also be non-null
        /// and valid.
        /// However, if there is no data to query, then returned_value.bary will be null.
        /// </remarks>
        private IndexedBary FindTriangle(Vector3 pos)
        {
            IndexedBary bary = new IndexedBary();
            bary.bary = new Interpolant();
            for (int i = 0; i < triangles.Count; ++i)
            {
                Triangle tri = triangles[i];
                Vector3 p0 = vertices[tri.idx0];
                p0.y = 0;
                Vector3 p1 = vertices[tri.idx1];
                p1.y = 0;
                Vector3 p2 = vertices[tri.idx2];
                p2.y = 0;
                /// Note area will be negative, because this is xz, not xy, but that will cancel with the negative cross products below.
                float area = Vector3.Cross(p2 - p1, p0 - p1).y;
                if (area >= 0)
                {
                    area = 0;
                }
                Debug.Assert(area < 0, "Degenerate triangle in Find");

                Vector3 ps = new Vector3(pos.x, 0, pos.z);

                bary.bary.weights[0] = Vector3.Cross(p2 - p1, ps - p1).y / area;
                bary.bary.weights[1] = Vector3.Cross(p0 - p2, ps - p2).y / area;
                bary.bary.weights[2] = Vector3.Cross(p1 - p0, ps - p0).y / area;

                if (bary.bary.IsInterior)
                {
                    bary.triangle = i;
                    bary.bary.idx[0] = tri.idx0;
                    bary.bary.idx[1] = tri.idx1;
                    bary.bary.idx[2] = tri.idx2;

                    return bary;
                }

            }
            Debug.Assert(false, "Found no triangle for which this position is interior.");
            bary.bary = null;
            return bary;
        }

        /// <summary>
        /// Query for a triangle containing the projection of this position, or failing that,
        /// find the closest point on the closest edge.
        /// </summary>
        /// <param name="pos">The query position.</param>
        /// <returns>The interpolant, or null if there is no data to query.</returns>
        private Interpolant FindTriangleOrEdgeOrVertex(Vector3 pos)
        {
            if (PointInsideBounds(pos))
            {
                Interpolant bary = FindTriangle(pos).bary;

                if (IsInteriorTriangle(bary))
                {
                    return bary;
                }
            }
            return FindClosestExteriorEdge(pos);
        }

        /// <summary>
        /// Evaluate whether the query position is inside the set bounds.
        /// </summary>
        /// <param name="pos">The query position.</param>
        /// <returns>True if inside the bounds as set by SetBounds.</returns>
        /// <remarks>
        /// This makes a dependency on exactly how the bounds are implemented as the first 4 vertices. See SeedQuad().
        /// </remarks>
        private bool PointInsideBounds(Vector3 pos)
        {
            if (vertices.Count < 4)
            {
                return false;
            }
            return pos.x >= vertices[1].x
                && pos.x <= vertices[3].x
                && pos.z >= vertices[1].z
                && pos.z <= vertices[3].z;
        }

        /// <summary>
        /// Determine if the interpolant is interpolating between real vertices, or is
        /// exterior (i.e. one or more vertices are bounding dummy vertices).
        /// </summary>
        /// <param name="bary"></param>
        /// <returns></returns>
        private bool IsInteriorTriangle(Interpolant bary)
        {
            if (bary == null)
            {
                return false;
            }
            for (int i = 0; i < bary.idx.Length; ++i)
            {
                if (IsBoundary(bary.idx[i]))
                {
                    return false;
                }
            }
            return true;
        }
#endregion Internal query support

#region Build exterior edge list
        private List<Edge> exteriorEdges = new List<Edge>();

        private bool IsBoundary(int vertIdx)
        {
            return vertIdx < 4;
        }

        private int HasExteriorEdge(Triangle tri)
        {
            int outVertIdx = -1;
            int numOutVerts = 0;
            if (IsBoundary(tri.idx0))
            {
                ++numOutVerts;
                outVertIdx = 0;
            }
            if (IsBoundary(tri.idx1))
            {
                ++numOutVerts;
                outVertIdx = 1;
            }
            if (IsBoundary(tri.idx2))
            {
                ++numOutVerts;
                outVertIdx = 2;
            }
            if (numOutVerts == 1)
            {
                return outVertIdx;
            }
            return -1;
        }

        /// <summary>
        /// Fill out an edge struct with the edge opposite the indicated vertex in the triangle.
        /// Edge will have idx0 < idx1.
        /// </summary>
        /// <param name="tri">Source triangle.</param>
        /// <param name="outVertIdx">Vertex opposite desired edge. Vertex index is within triangle [0..3), not index in vertex list.</param>
        /// <returns>Filled out edge with idx0 < idx1.</returns>
        private Edge ExtractEdge(Triangle tri, int outVertIdx)
        {
            Edge edge = new Edge() { idx0 = tri.idx0, idx1 = tri.idx1 };
            switch (outVertIdx)
            {
                case 0:
                    {
                        edge.idx0 = tri.idx1;
                        edge.idx1 = tri.idx2;
                    }
                    break;
                case 1:
                    {
                        edge.idx0 = tri.idx2;
                        edge.idx1 = tri.idx0;
                    }
                    break;
                case 2:
                    // already filled out correctly in initialization of edge.
                    break;
                default:
                    Debug.Assert(false, "Invalid vertex index, must be in [0..3).");
                    break;
            }
            if (edge.idx0 > edge.idx1)
            {
                int t = edge.idx0;
                edge.idx0 = edge.idx1;
                edge.idx1 = t;
            }
            return edge;
        }

        private void FindExteriorEdges()
        {
            exteriorEdges.Clear();
            for (int iTri = 0; iTri < triangles.Count; ++iTri)
            {
                Triangle tri = triangles[iTri];
                int outVertIdx = HasExteriorEdge(tri);
                if (outVertIdx >= 0)
                {
                    exteriorEdges.Add(ExtractEdge(tri, outVertIdx));
                }
            }
            RemoveRedundantEdges(exteriorEdges);
        }

        private void RemoveRedundantEdges(List<Edge> edges)
        {
            // Don't really need to check length, if it's empty or single edge nothing will happen.
            if (edges.Count > 1)
            {
                edges.Sort((e0, e1) =>
                {
                // Sort by first index first.
                if (e0.idx0 < e1.idx0)
                    {
                        return -1;
                    }
                    if (e0.idx0 > e1.idx0)
                    {
                        return 1;
                    }
                // First index equal, sort by second index.
                if (e0.idx1 < e1.idx1)
                    {
                        return -1;
                    }
                    if (e0.idx1 > e1.idx1)
                    {
                        return 1;
                    }
                    return 0;
                });
                // Note i doesn't reach zero, because edges[i] is compared to edges[i-1].
                for (int i = edges.Count - 1; i > 0; --i)
                {
                    if (edges[i - 1].idx0 == edges[i].idx0 && edges[i - 1].idx1 == edges[i].idx1)
                    {
                        edges.RemoveAt(i);
                    }
                }
            }
        }
#endregion Build exterior edge list

#region Find closest point on exterior edge
        /// <summary>
        /// Convenience struct for returning tuple of info about closest point on an edge to a queried position.
        /// </summary>
        private struct PointOnEdge
        {
            /// <summary>
            /// parametric distance from idx0 to idx1 (clamped [0..1]).
            /// </summary>
            public float parm;
            /// <summary>
            /// actual 2D distance from query position to closest point on edge.
            /// </summary>
            public float distanceSqr; 
        };

        /// <summary>
        /// Find the exterior edge with the closest point on the edge to the query position.
        /// </summary>
        /// <param name="pos">The query position.</param>
        /// <returns>A triangle interpolant that will evaluate to the interpolation between the two edge endpoints. Returns null if there are no exterior edges.</returns>
        /// <remarks>
        /// The third triangle vertex always has index 0 and weight 0.
        /// </remarks>
        private Interpolant FindClosestExteriorEdge(Vector3 pos)
        {
            if (exteriorEdges.Count == 0)
            {
                if(vertices.Count == 5)
                {
                    /// There is a single real vertex, so no exterior edges.
                    /// That's okay, the single vertex wins all the weight.
                    Interpolant singleVert = new Interpolant();
                    singleVert.idx[0] = singleVert.idx[1] = singleVert.idx[2] = 4;
                    singleVert.weights[0] = 1.0f;
                    singleVert.weights[1] = singleVert.weights[2] = 0.0f;
                    return singleVert;
                }
                return null;
            }
            int closestEdge = -1;
            float closestDistance = float.MaxValue;
            float closestParm = 0.0f;
            for (int i = 0; i < exteriorEdges.Count; ++i)
            {
                PointOnEdge point = PositionOnEdge(exteriorEdges[i], pos);

                if (point.distanceSqr < closestDistance)
                {
                    closestEdge = i;
                    closestDistance = point.distanceSqr;
                    closestParm = point.parm;
                }
            }
            Debug.Assert(closestEdge >= 0, "If there are any edges, there must be a closest one.");
            Edge edge = exteriorEdges[closestEdge];
            Interpolant bary = new Interpolant();
            bary.idx[0] = edge.idx0;
            bary.idx[1] = edge.idx1;
            bary.idx[2] = 0;
            bary.weights[0] = 1.0f - closestParm;
            bary.weights[1] = closestParm;
            bary.weights[2] = 0;

            return bary;
        }

        private PointOnEdge PositionOnEdge(Edge edge, Vector3 pos)
        {
            /// Project everything onto the horizontal plane.
            pos.y = 0;
            Vector3 p0 = vertices[edge.idx0];
            p0.y = 0;
            Vector3 p1 = vertices[edge.idx1];
            p1.y = 0;

            Vector3 p0to1 = p1 - p0;
            float dist0to1Sqr = p0to1.sqrMagnitude;
            Debug.Assert(dist0to1Sqr > 0);
            float parm = Vector3.Dot((pos - p0), p0to1) / dist0to1Sqr;
            parm = Mathf.Clamp01(parm);
            Vector3 pointOnEdge = p0 + parm * (p1 - p0);
            float distanceSqr = (pointOnEdge - pos).magnitude;

            return new PointOnEdge() { parm = parm, distanceSqr = distanceSqr };
        }
#endregion Find closest point on exterior edge
    }
}