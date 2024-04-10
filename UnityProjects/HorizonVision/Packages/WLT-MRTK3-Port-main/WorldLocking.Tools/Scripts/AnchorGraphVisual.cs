// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Core.ResourceMirrorHelper;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Optional visualizer of anchors and edges
    /// </summary>
    public class AnchorGraphVisual : MonoBehaviour
    {
        #region Public properties

        /// <summary>
        /// Prefab for the frame (axes) visualization.
        /// </summary>
        public FrameVisual Prefab_FrameViz;
        /// <summary>
        /// Prefab for spongy anchors.
        /// </summary>
        public SpongyAnchorVisual Prefab_SpongyAnchorViz;
        /// <summary>
        /// Prefab for frozen anchors.
        /// </summary>
        public FrozenAnchorVisual Prefab_FrozenAnchorViz;

        /// <summary>
        /// Placeholder material to ensure the required shader is packaged. The shader used
        /// for connecting lines is in ConnectingLine.cs:
        /// "Legacy Shaders/Particles/Alpha Blended Premultiply"
        /// </summary>
        [Tooltip("Placeholder material to ensure the required shader is packaged.")]
        [SerializeField]
        private Material connectingLine = null;

        /// <summary>
        /// Vertical distance to offset from actual anchors, to avoid visuals at eye level.
        /// </summary>
        /// <remarks>
        /// Set this to zero to display visuals at actual anchor locations.
        /// </remarks>
        public float VerticalDisplacement { get { return verticalDisplacement; } set { verticalDisplacement = value; } }

        /// <summary>
        /// Vertical distance to offset from actual anchors, to avoid visuals at eye level.
        /// </summary>
        /// <remarks>
        /// Set this to zero to display visuals at actual anchor locations.
        /// </remarks>
        [Tooltip("Vertical distance to offset from actual anchors, to avoid visuals at eye level.")]
        [SerializeField]
        private float verticalDisplacement = -1.0f;
        #endregion Public properties

        #region Internal properties

        /// <summary>
        /// The WorldLockingManager.
        /// </summary>
        private WorldLockingManager manager { get { return WorldLockingManager.GetInstance(); } }

        /// <summary>
        /// Root to hang spongy visualizations off of.
        /// </summary>
        private FrameVisual spongyWorldViz;
        /// <summary>
        /// Root to hang frozen visualizations off of.
        /// </summary>
        private GameObject worldLockingVizRoot;

        #endregion Internal properties

        #region Resource lists

        /// <summary>
        /// Dictionary of fragment visualizations.
        /// </summary>
        private Dictionary<FragmentId, FrameVisual> frozenFragmentVizs = new Dictionary<FragmentId, FrameVisual>();

        /// <summary>
        /// Sorted list of spongy anchor visualizations.
        /// </summary>
        private List<IdPair<AnchorId, SpongyAnchorVisual>> spongyResources = new List<IdPair<AnchorId, SpongyAnchorVisual>>();

        /// <summary>
        /// Sorted list of frozen anchor visualizations.
        /// </summary>
        private List<IdPair<AnchorId, FrozenAnchorVisual>> frozenResources = new List<IdPair<AnchorId, FrozenAnchorVisual>>();

        /// <summary>
        /// Sorted list of anchor graph edge visualizations.
        /// </summary>
        private List<IdPair<AnchorEdge, ConnectingLine>> edgeResources = new List<IdPair<AnchorEdge, ConnectingLine>>();

        /// <summary>
        /// Sorted list of lines connecting spongy anchors with corresponding frozen anchors.
        /// </summary>
        private List<IdPair<AnchorId, ConnectingLine>> displacementResources = new List<IdPair<AnchorId, ConnectingLine>>();

        #endregion Resource lists

        #region Actions

        private void Reset()
        {
            if (spongyWorldViz != null)
            {
                Destroy(spongyWorldViz.gameObject);
            }
            spongyWorldViz = null;

            if (worldLockingVizRoot != null)
            {
                Destroy(worldLockingVizRoot);
            }
            worldLockingVizRoot = null;

            foreach (var p in frozenFragmentVizs.Values)
            {
                if (p != null)
                {
                    Destroy(p.gameObject);
                }
            }
            frozenFragmentVizs.Clear();

            spongyResources.Clear();
            frozenResources.Clear();
            edgeResources.Clear();
            displacementResources.Clear();
        }

        private void RefitHandler(FragmentId mergedId, FragmentId[] combined)
        {
            Reset();
        }

        private void OnEnable()
        {
            manager.FragmentManager.RegisterForRefitNotifications(RefitHandler);
        }

        private void OnDisable()
        {
            Reset();
            manager.FragmentManager.UnregisterForRefitNotifications(RefitHandler);
        }

        #endregion Actions

        #region Update

        private void Update()
        {
            if (!manager.Enabled)
            {
                Reset();
            }
            else
            {
                UpdateSpongy();
                UpdateFrozen();
            }
        }

        /// <summary>
        /// Update all spongy visualizations.
        /// </summary>
        private void UpdateSpongy()
        {
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            AnchorManager anchorManager = manager.AnchorManager as AnchorManager;
            Debug.Assert(anchorManager != null, "This should not be called without a valid AnchorManager");

            CheckSpongyRoot(manager.AdjustmentFrame);

            var spongyCurrent = anchorManager.SpongyAnchors;
            spongyCurrent.Sort((x, y) => x.anchorId.CompareTo(y.anchorId));

            SpongyVisualCreator spongyCreator = new SpongyVisualCreator(Prefab_SpongyAnchorViz, spongyWorldViz, verticalDisplacement);
            ResourceMirror.Sync(
                spongyCurrent,
                spongyResources,
                (item, res) => item.anchorId.CompareTo(res.id),
                spongyCreator.CreateSpongyVisual,
                (x, y) => { },
                spongyCreator.DestroySpongyVisual);

            SetSupportRelevances();

        }

        private void SetSupportRelevances()
        {
            // Visualize the support relevances.
            var supportRelevances = manager.Plugin.GetSupportRelevances();
            Array.Sort(supportRelevances, (x, y) => x.anchorId.CompareTo(y.anchorId));

            int iSupport = 0;
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                // Skip any supports with a lower id, these have no corresponding spongy resource.
                while (iSupport < supportRelevances.Length && supportRelevances[iSupport].anchorId < spongyResources[iSpongy].id)
                {
                    ++iSupport;
                }

                if (iSupport < supportRelevances.Length && supportRelevances[iSupport].anchorId == spongyResources[iSpongy].id)
                {
                    spongyResources[iSpongy].target.SetSupportRelevance(supportRelevances[iSupport++].relevance);
                }
                else
                {
                    spongyResources[iSpongy].target.SetNoSupport();
                }
            }
        }

        /// <summary>
        /// Update all frozen visualizations.
        /// </summary>
        private void UpdateFrozen()
        {
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            var plugin = manager.Plugin;

            AnchorFragmentPose[] frozenItems = plugin.GetFrozenAnchors();
            Array.Sort(frozenItems, (x, y) => x.anchorId.CompareTo(y.anchorId));

            UpdateFragmentVisuals();

            /// The "frozen" coordinates here are ignoring the rest of the transform up the camera tree.
            Pose globalFromLocked = manager.ApplyAdjustment ? manager.FrozenFromLocked : manager.SpongyFromLocked;
            /// Apply the vertical displacement through the globalFromLocked transform.
            globalFromLocked.position = new Vector3(globalFromLocked.position.x, globalFromLocked.position.y + verticalDisplacement, globalFromLocked.position.z);

            var frozenCreator = new FrozenAnchorVisualCreator(Prefab_FrozenAnchorViz, frozenFragmentVizs, globalFromLocked);
            ResourceMirror.Sync(
                frozenItems,
                frozenResources,
                (item, res) => item.anchorId.CompareTo(res.id),
                frozenCreator.CreateFrozenVisual,
                frozenCreator.UpdateFrozenVisual,
                frozenCreator.DestroyFrozenVisual);

            // Connect frozen anchors with corresponding spongy anchors with a line.
            DisplacementCreator displacementCreator = new DisplacementCreator();
            SyncDisplacements(displacementCreator,
                frozenResources,
                spongyResources,
                displacementResources);

            var edgeItems = plugin.GetFrozenEdges();
            for (int i = 0; i < edgeItems.Length; ++i)
            {
                edgeItems[i] = RegularizeEdge(edgeItems[i]);
            }
            Array.Sort(edgeItems, anchorEdgeComparer);

            var frozenEdgeCreator = new FrozenEdgeVisualCreator(this, frozenResources);
            ResourceMirror.Sync(
                edgeItems,
                edgeResources,
                (x, y) => CompareAnchorEdges(x, y.id),
                frozenEdgeCreator.CreateFrozenEdge,
                (x, y) => { },
                frozenEdgeCreator.DestroyFrozenEdge);
        }

        /// <summary>
        /// Find all matching pairs of frozen and spongy visuals, and if appropriate,
        /// create edges connecting them. Destroy any other stale lines.
        /// </summary>
        /// <param name="displacementCreator">Object to manage creating, destroying, and deciding existence of displacement lines.</param>
        /// <param name="frozenResources">The frozen anchors.</param>
        /// <param name="spongyResources">The spongy anchors.</param>
        /// <param name="displacementResources">The connecting lines.</param>
        private void SyncDisplacements(
            DisplacementCreator displacementCreator,
            IReadOnlyList<IdPair<AnchorId, FrozenAnchorVisual>> frozenResources,
            IReadOnlyList<IdPair<AnchorId, SpongyAnchorVisual>> spongyResources,
            List<IdPair<AnchorId, ConnectingLine>> displacementResources)
        {
            int iFrozen = 0;
            int iDisplace = 0;
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                while (iFrozen < frozenResources.Count && frozenResources[iFrozen].id < spongyResources[iSpongy].id)
                {
                    iFrozen++;
                }
                /// If we've reached the end of the frozen resources, we're finished creating.
                if (iFrozen >= frozenResources.Count)
                {
                    break;
                }
                if (displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]))
                {
                    AnchorId id = frozenResources[iFrozen].id;
                    Debug.Assert(id == spongyResources[iSpongy].id);
                    while (iDisplace < displacementResources.Count && displacementResources[iDisplace].id < id)
                    {
                        displacementCreator.DestroyDisplacement(displacementResources[iDisplace]);
                        displacementResources.RemoveAt(iDisplace);
                    }
                    Debug.Assert(iDisplace <= displacementResources.Count);
                    Debug.Assert(iDisplace == displacementResources.Count || displacementResources[iDisplace].id >= id);
                    if (iDisplace == displacementResources.Count || displacementResources[iDisplace].id > id)
                    {
                        displacementResources.Insert(iDisplace,
                            displacementCreator.CreateDisplacement(
                                id,
                                frozenResources[iFrozen].target,
                                spongyResources[iSpongy].target));
                    }
                    ++iDisplace;
                }
            }
            // Finished creating. Now destroy any displacements further in the list, as they no longer have matching
            // frozen/spongy pairs.
            Debug.Assert(iDisplace <= displacementResources.Count);
            int displacementCount = iDisplace;
            while (iDisplace < displacementResources.Count)
            {
                displacementCreator.DestroyDisplacement(displacementResources[iDisplace++]);
            }
            displacementResources.RemoveRange(displacementCount, displacementResources.Count - displacementCount);
            Debug.Assert(displacementResources.Count == displacementCount);
        }

        /// <summary>
        /// Update the visuals for all existing fragments.
        /// </summary>
        private void UpdateFragmentVisuals()
        {
            GameObject worldLockingRoot = EnsureWorldLockingVizRoot();
            var fragmentManager = manager.FragmentManager;
            var fragmentIds = fragmentManager.FragmentIds;
            foreach (var fragmentId in fragmentIds)
            {
                FrameVisual frozenFragmentViz;
                if (!frozenFragmentVizs.TryGetValue(fragmentId, out frozenFragmentViz))
                {
                    frozenFragmentViz = Instantiate(Prefab_FrameViz, worldLockingRoot.transform);
                    frozenFragmentViz.name = fragmentId.FormatStr();
                    frozenFragmentVizs[fragmentId] = frozenFragmentViz;
                }
                Color fragmentColor = Color.gray;
                if (fragmentId == fragmentManager.CurrentFragmentId)
                {
                    fragmentColor = Color.blue;
                }
                if (fragmentManager.GetFragmentState(fragmentId) != AttachmentPointStateType.Normal)
                {
                    fragmentColor = Color.red;
                }
                frozenFragmentViz.color = fragmentColor;
            }
        }

        /// <summary>
        /// Ensure there's a game object to hang fragment visualizations off of.
        /// </summary>
        /// <returns></returns>
        private GameObject EnsureWorldLockingVizRoot()
        {
            if (!worldLockingVizRoot)
            {
                worldLockingVizRoot = new GameObject("WorldLockingViz");
            }
            return worldLockingVizRoot;
        }

        #endregion Update

        #region Visualizations management helpers

        #region Sort helpers
        /// <summary>
        /// Ensure that the first anchor id in the edge is the smaller of the two.
        /// </summary>
        /// <param name="edge">The edge to regularize.</param>
        /// <returns>The same edge, but possibly swapped to have the smaller anchor id first.</returns>
        private static AnchorEdge RegularizeEdge(AnchorEdge edge)
        {
            if (edge.anchorId2 < edge.anchorId1)
            {
                var id = edge.anchorId2;
                edge.anchorId2 = edge.anchorId1;
                edge.anchorId1 = id;
            }
            return edge;
        }

        /// <summary>
        /// Comparison function for two edges.
        /// </summary>
        /// <remarks>
        /// Alphabetic sort on first endpoint first, then second endpoint.
        /// </remarks>
        /// <param name="lhs">Left hand edge.</param>
        /// <param name="rhs">Right hand edge.</param>
        /// <returns>Comparison int of edges.</returns>
        private static int CompareAnchorEdges(AnchorEdge lhs, AnchorEdge rhs)
        {
            Debug.Assert(lhs.anchorId1 < lhs.anchorId2);
            Debug.Assert(rhs.anchorId1 < rhs.anchorId2);
            if (lhs.anchorId1 < rhs.anchorId1)
            {
                return -1;
            }
            if (lhs.anchorId1 > rhs.anchorId1)
            {
                return 1;
            }
            if (lhs.anchorId2 < rhs.anchorId2)
            {
                return -1;
            }
            if (lhs.anchorId2 > rhs.anchorId2)
            {
                return 1;
            }
            return 0;
        }
        /// <summary>
        /// Edge comparer instance to use for sorting edges.
        /// </summary>
        private Comparer<AnchorEdge> anchorEdgeComparer = Comparer<AnchorEdge>.Create((x, y) => CompareAnchorEdges(x, y));

        /// <summary>
        /// Do binary search to find a key in a sorted list.
        /// </summary>
        /// <typeparam name="S">Type of the key.</typeparam>
        /// <typeparam name="T">Type of the data associated with the key.</typeparam>
        /// <param name="key">The key to search for.</param>
        /// <param name="list">The list to search.</param>
        /// <param name="comparer">The comparison function to use.</param>
        /// <returns>The data pair with the given key for an id.</returns>
        /// <remarks>It is an error to search for a key which isn't in the list.</remarks>
        private static int FindInSortedList<S, T>(S key, List<IdPair<S, T>> list, IComparer<IdPair<S, T>> comparer)
        {
            IdPair<S, T> item = new IdPair<S, T>() { id = key };
            int idx = list.BinarySearch(item, comparer);
            return idx;
        }

        #endregion Sort helpers

        #region Creators

        /// <summary>
        /// Class to manage creation and destruction of SpongyAnchorVisuals.
        /// </summary>
        private class SpongyVisualCreator
        {
            private readonly SpongyAnchorVisual Prefab_SpongyAnchorVisual;
            private readonly FrameVisual spongyWorldVisual;
            private readonly float verticalDisplacement;

            /// <summary>
            /// Constructor takes the prefab to construct the visual out of, and a FrameVisual
            /// to creat the visual attached to.
            /// </summary>
            /// <param name="prefab">Prefab to create the visual out of.</param>
            /// <param name="spongyWorldVisual">Parent of created visuals.</param>
            public SpongyVisualCreator(SpongyAnchorVisual prefab, FrameVisual spongyWorldVisual, float verticalDisplacement)
            {
                Prefab_SpongyAnchorVisual = prefab;
                this.spongyWorldVisual = spongyWorldVisual;
                this.verticalDisplacement = verticalDisplacement;
            }

            /// <summary>
            /// Create a Spongy Anchor Visual matching the spongy anchor source.
            /// </summary>
            /// <param name="source">The source Spongy Anchor.</param>
            /// <param name="resource">The created SpongyAnchorVisual with matching id.</param>
            /// <returns></returns>
            public bool CreateSpongyVisual(AnchorManager.SpongyAnchorWithId source, out IdPair<AnchorId, SpongyAnchorVisual> resource)
            {
                var spongyAnchorVisual = Prefab_SpongyAnchorVisual.Instantiate(
                    spongyWorldVisual,
                    source.spongyAnchor,
                    verticalDisplacement);

                resource = new IdPair<AnchorId, SpongyAnchorVisual>()
                {
                    id = source.anchorId,
                    target = spongyAnchorVisual
                };
                return true;
            }

            /// <summary>
            /// Destroy the visual which is no longer needed.
            /// </summary>
            /// <param name="target">The visual to destroy.</param>
            public void DestroySpongyVisual(IdPair<AnchorId, SpongyAnchorVisual> target)
            {
                Destroy(target.target.gameObject);
            }
        }

        /// <summary>
        /// Class to manage the creation and destruction of FrozenAnchorVisuals.
        /// </summary>
        private class FrozenAnchorVisualCreator
        {
            private readonly FrozenAnchorVisual Prefab_FrozenAnchorViz;
            private readonly Dictionary<FragmentId, FrameVisual> frozenFragmentVisuals;
            private readonly Pose frozenFromLocked;

            /// <summary>
            /// Constructor taking all dependencies.
            /// </summary>
            /// <param name="prefab">The prefab to create visuals out of.</param>
            /// <param name="fragmentVisuals">Fragments to attach the visuals to.</param>
            /// <param name="frozenFromLocked">Transform for setting the created visual's pose.</param>
            public FrozenAnchorVisualCreator(
                FrozenAnchorVisual prefab,
                Dictionary<FragmentId, FrameVisual> fragmentVisuals,
                Pose frozenFromLocked)
            {
                Prefab_FrozenAnchorViz = prefab;
                frozenFragmentVisuals = fragmentVisuals;
                this.frozenFromLocked = frozenFromLocked;
            }

            /// <summary>
            /// Create a frozen anchor visual in the indicated fragment.
            /// </summary>
            /// <param name="source">Source data to create from.</param>
            /// <param name="resource">The created resource.</param>
            /// <returns></returns>
            public bool CreateFrozenVisual(AnchorFragmentPose source, out IdPair<AnchorId, FrozenAnchorVisual> resource)
            {
                // Already ensured this fragment exists.
                FragmentId fragmentId = source.fragmentPose.fragmentId;

                AnchorId anchorId = source.anchorId;

                FrameVisual frozenFragmentViz;
                if (!frozenFragmentVisuals.TryGetValue(fragmentId, out frozenFragmentViz))
                {
                    resource = new IdPair<AnchorId, FrozenAnchorVisual>() { id = AnchorId.Invalid, target = null };
                    return false;
                }

                // If there isn't a visualization for this anchor, add one.
                FrozenAnchorVisual frozenAnchorVisual;
                frozenAnchorVisual = Prefab_FrozenAnchorViz.Instantiate(anchorId.FormatStr(), frozenFragmentViz);

                // Put the frozen anchor vis at the world locked transform of the anchor
                SetPose(source, frozenAnchorVisual);

                resource = new IdPair<AnchorId, FrozenAnchorVisual>()
                {
                    id = source.anchorId,
                    target = frozenAnchorVisual
                };
                return true;
            }

            /// <summary>
            /// Set or update the pose of the resource.
            /// </summary>
            /// <param name="source">Source FrozenAnchor associated with the resource.</param>
            /// <param name="target">The resource to set the pose of.</param>
            private void SetPose(AnchorFragmentPose source, FrozenAnchorVisual target)
            {
                Pose localPose = source.fragmentPose.pose;
                localPose = frozenFromLocked.Multiply(localPose);
                // The following line introduces an artificial displacement between corresponding frozen and
                // spongy anchors, to make sure the connecting line visualization is working.
                //localPose.position.y += 0.25f;
                target.transform.SetLocalPose(localPose);
            }

            /// <summary>
            /// Update the pose of the existing resource.
            /// </summary>
            /// <param name="source">The resource's source data.</param>
            /// <param name="target">The resource to update.</param>
            public void UpdateFrozenVisual(AnchorFragmentPose source, IdPair<AnchorId, FrozenAnchorVisual> target)
            {
                SetPose(source, target.target);
            }

            /// <summary>
            /// Destroy a no longer needed FrozenAnchorVisual.
            /// </summary>
            /// <param name="target"></param>
            public void DestroyFrozenVisual(IdPair<AnchorId, FrozenAnchorVisual> target)
            {
                Destroy(target.target.gameObject);
            }

        }

        /// <summary>
        /// Class to manage creating and destroying edge visualization instances.
        /// </summary>
        private class FrozenEdgeVisualCreator
        {
            private readonly AnchorGraphVisual owner;
            private readonly List<IdPair<AnchorId, FrozenAnchorVisual>> frozenResources;
            private readonly Comparer<IdPair<AnchorId, FrozenAnchorVisual>> frozenAnchorVisualComparer = Comparer<IdPair<AnchorId, FrozenAnchorVisual>>.Create((x, y) => x.id.CompareTo(y.id));

            /// <summary>
            /// Constructor takes the owning AnchorGraphVisual and a list of 
            /// FrozenAnchorVisuals as resources.
            /// </summary>
            /// <remarks>The visuals must be sorted for fast lookup of the endpoints.</remarks>
            /// <param name="owner">The owning component.</param>
            /// <param name="frozenResources">The *sorted* list of FrozenAnchorVisuals.</param>
            public FrozenEdgeVisualCreator(AnchorGraphVisual owner, List<IdPair<AnchorId, FrozenAnchorVisual>> frozenResources)
            {
                this.owner = owner;
                this.frozenResources = frozenResources;
            }


            /// <summary>
            /// Look up the frozen anchor endpoints and connect them with a line.
            /// </summary>
            /// <param name="edge">Pair of anchor ids.</param>
            /// <param name="resource">The resource to create.</param>
            /// <returns></returns>
            public bool CreateFrozenEdge(AnchorEdge edge, out IdPair<AnchorEdge, ConnectingLine> resource)
            {
                var anchorId1 = edge.anchorId1;
                var anchorId2 = edge.anchorId2;

                var frozenAnchor1Idx = FindInSortedList(anchorId1, frozenResources, frozenAnchorVisualComparer);
                var frozenAnchor2Idx = FindInSortedList(anchorId2, frozenResources, frozenAnchorVisualComparer);
                if (frozenAnchor1Idx < 0 || frozenAnchor2Idx < 0)
                {
                    resource = new IdPair<AnchorEdge, ConnectingLine>() { id = edge, target = null };
                    return false;
                }
                var frozenAnchor1 = frozenResources[frozenAnchor1Idx];
                var frozenAnchor2 = frozenResources[frozenAnchor2Idx];

                Transform parent1 = frozenAnchor1.target.transform.parent;
                Transform parent2 = frozenAnchor2.target.transform.parent;
                bool sameFragment = parent1 == parent2;
                Color color = Color.blue;
                float width = 0.002f;
                Transform parent = parent1;

                if (!sameFragment)
                {
                    color = Color.yellow;
                    width = 0.004f;
                }

                var edgeVisual = ConnectingLine.Create(parent,
                    frozenAnchor1.target.transform, frozenAnchor2.target.transform,
                    width, color);

                resource = new IdPair<AnchorEdge, ConnectingLine>()
                {
                    id = edge,
                    target = edgeVisual
                };
                return true;
            }

            /// <summary>
            /// Release resources for stale edges.
            /// </summary>
            /// <param name="target">The resource to release.</param>
            public void DestroyFrozenEdge(IdPair<AnchorEdge, ConnectingLine> target)
            {
                Destroy(target.target.gameObject);
            }
        }

        /// <summary>
        /// Class to manage creation and destruction of displacement lines (lines connecting spongy anchors with
        /// corresponding frozen anchors).
        /// </summary>
        /// <remarks>
        /// This is not used with the ResourceMirror, but is in specific code similar to the resource mirror.
        /// </remarks>
        private class DisplacementCreator
        {
            public DisplacementCreator()
            {
            }

            /// <summary>
            /// Create a visible line connecting the frozen anchor to the spongy anchor.
            /// </summary>
            /// <param name="id">The anchor id of the frozen and spongy anchors.</param>
            /// <param name="frozen">The frozen anchor.</param>
            /// <param name="spongy">The spongy anchor.</param>
            /// <returns></returns>
            public IdPair<AnchorId, ConnectingLine> CreateDisplacement(AnchorId id, FrozenAnchorVisual frozen, SpongyAnchorVisual spongy)
            {
                var newLine = ConnectingLine.Create(spongy.transform.parent,
                                    frozen.transform,
                                    spongy.transform,
                                    0.01f, Color.red);

                return new IdPair<AnchorId, ConnectingLine>()
                {
                    id = id,
                    target = newLine
                };
            }

            /// <summary>
            /// Destroy a no longer needed connecting line.
            /// </summary>
            /// <param name="target">The resource to destroy.</param>
            public void DestroyDisplacement(IdPair<AnchorId, ConnectingLine> target)
            {
                Destroy(target.target.gameObject);
            }

            /// <summary>
            /// Check whether an edge should be drawn connecting the frozen to the spongy anchor.
            /// </summary>
            /// <param name="frozen">The frozen anchor.</param>
            /// <param name="spongy">The spongy anchor.</param>
            /// <returns>True if a line should be drawn connecting them.</returns>
            /// <remarks>
            /// The current implementation checks that the frozen and spongy anchors
            /// have the same id (i.e. refer to related anchors), and also that they
            /// are at least a minimum distance apart. This might benefit from some hysteresis
            /// check, e.g. edges are created when at least X apart, and destroyed when less than Y apart,
            /// with X GT Y, to prevent edges being created and destroyed frequently when the distance
            /// is near the threshold.
            /// </remarks>
            public bool ShouldConnect(
                IdPair<AnchorId, FrozenAnchorVisual> frozen,
                IdPair<AnchorId, SpongyAnchorVisual> spongy)
            {
                if (frozen.id != spongy.id)
                {
                    return false;
                }
                float MinDistanceSquared = 0.01f; // one centimeter.
                float distanceSq = (frozen.target.transform.position - spongy.target.transform.position).sqrMagnitude;
                if (distanceSq < MinDistanceSquared)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion Creators

        /// <summary>
        /// Ensure there is a "spongy root" to attach spongy anchor visuals to.
        /// </summary>
        /// <param name="parentTransform">The transform to attach the spongy root to.</param>
        private void CheckSpongyRoot(Transform parentTransform)
        {
            // The spongyWorldViz object is hung off the SpongyFrame.
            if (!spongyWorldViz)
            {
                spongyWorldViz = Instantiate(Prefab_FrameViz, parentTransform);
                spongyWorldViz.name = "Spongy";
                spongyWorldViz.color = Color.green;
                if (connectingLine != null)
                {
                    /// Connecting line material is only included to force import of the shader.
                    /// No-op access it here to suppress warning that it is unused.
                    spongyWorldViz.color = Color.green;
                }
            }
        }
        #endregion Visualizations management helpers

    }
}
