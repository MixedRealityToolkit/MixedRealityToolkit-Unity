// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Threading.Tasks;

using static Microsoft.MixedReality.FrozenWorld.Engine.Engine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class PluginNoop : IPlugin
    {
        private static readonly FragmentId currentFragmentId = (FragmentId)1;

        private Pose currentAlignment = Pose.identity;

        private Pose currentSpongyHead = Pose.identity;

        private struct AnchorPoseRelevance
        {
            public AnchorPose anchorPose;
            public float relevance;
        }

        private readonly List<AnchorPoseRelevance> frozenAnchors = new List<AnchorPoseRelevance>();
        private readonly List<AnchorEdge> frozenEdges = new List<AnchorEdge>();
        private AnchorId mostSignificantAnchorId = AnchorId.Unknown;

        private static unsafe void checkError()
        {
        }

        public PluginNoop()
        {
            Debug.LogWarning("Creating PluginNoop, is this intentional?");
            checkError();
            metrics = new MetricsAccessor();
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            disposed = true;

            checkError();
        }

        ~PluginNoop()
        {
            Dispose(false);
        }

        private string cachedCompact;
        public string VersionCompact => cachedCompact ?? (cachedCompact = getVersion(false));

        private string cachedDetail;
        public string VersionDetailed => cachedDetail ?? (cachedDetail = getVersion(true));

        private unsafe string getVersion(bool detail)
        {
            string version = "0.0.0";
            if (detail)
            {
                version = "Pass through mode - " + version;
            }
            return version;
        }

        private class MetricsAccessor : IMetricsAccessor
        {

            // Merge and refreeze indicators
            public bool RefitMergeIndicated { get { return false; } }
            public bool RefitRefreezeIndicated { get { return false; } }

            // Currently trackable fragments
            public int NumTrackableFragments { get { return 0; } }

            // Alignment supports
            public int NumVisualSupports { get { return 0; } }
            public int NumVisualSupportAnchors { get { return 0; } }
            public int NumIgnoredSupports { get { return 0; } }
            public int NumIgnoredSupportAnchors { get { return 0; } }

            // Visual deviation metrics
            public float MaxLinearDeviation { get { return 0; } }
            public float MaxLateralDeviation { get { return 0; } }
            public float MaxAngularDeviation { get { return 0; } }
            public float MaxLinearDeviationInFrustum { get { return 0; } }
            public float MaxLateralDeviationInFrustum { get { return 0; } }
            public float MaxAngularDeviationInFrustum { get { return 0; } }
        }

        private MetricsAccessor metrics = new MetricsAccessor();
        public IMetricsAccessor Metrics { get { return metrics; } }

        unsafe public void Step_Init(Pose spongyHeadPose)
        {
            currentSpongyHead = spongyHeadPose;
            checkError();
        }

        unsafe public void Step_Finish()
        {
            checkError();
        }

        unsafe public AnchorId[] GetFrozenAnchorIds()
        {
            int numAnchors = frozenAnchors.Count;

            var res = new AnchorId[numAnchors];
            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = frozenAnchors[i].anchorPose.anchorId;
            }

            return res;
        }

        unsafe public AnchorFragmentPose[] GetFrozenAnchors()
        {
            int numAnchors = frozenAnchors.Count;

            var res = new AnchorFragmentPose[numAnchors];

            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = new AnchorFragmentPose()
                {
                    anchorId = frozenAnchors[i].anchorPose.anchorId,
                    fragmentPose = new FragmentPose()
                    {
                        fragmentId = currentFragmentId,
                        pose = frozenAnchors[i].anchorPose.pose
                    }
                };
            }

            return res;
        }

        unsafe public FragmentId GetMostSignificantFragmentId()
        {
            return currentFragmentId;
        }

        private class FrozenAnchorById : Comparer<AnchorPoseRelevance>
        {
            public override int Compare(AnchorPoseRelevance x, AnchorPoseRelevance y)
            {
                return x.anchorPose.anchorId.CompareTo(y.anchorPose.anchorId);
            }
        }

        private int FindFrozenAnchor(AnchorId targetAnchorId)
        {
            AnchorPoseRelevance target = new AnchorPoseRelevance()
            {
                anchorPose = new AnchorPose()
                {
                    anchorId = targetAnchorId,
                    pose = Pose.identity
                },
                relevance = 0.0f
            };
            int idx = frozenAnchors.BinarySearch(target, new FrozenAnchorById());

            return idx;
        }

        unsafe public void SetFrozenAnchorTransform(AnchorId anchorId, Pose pose)
        {
            int idx = FindFrozenAnchor(anchorId);
            Debug.Assert(idx >= 0);
            var frozenAnchorPose = frozenAnchors[idx];
            frozenAnchorPose.anchorPose.pose = pose;
            frozenAnchors[idx] = frozenAnchorPose;
            checkError();
        }

        public void RemoveFrozenAnchor(AnchorId anchorId)
        {
            int idx = FindFrozenAnchor(anchorId);
            Debug.Assert(idx >= 0);
            frozenAnchors.RemoveAt(idx);
            checkError();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anchorId1"></param>
        /// <param name="anchorId2"></param>
        /// <remarks>
        /// This doesn't happen often enough to optimize, but could easily be sorted for efficient find.
        /// </remarks>
        public void RemoveFrozenEdge(AnchorId anchorId1, AnchorId anchorId2)
        {
            for (int i = frozenEdges.Count- 1; i >= 0; i--)
            {
                var edge = frozenEdges[i];
                Debug.Assert(edge.anchorId1 < edge.anchorId2, "FrozenEdges should always be regularized");
                if ((edge.anchorId1 == anchorId1) && (edge.anchorId2 == anchorId2))
                {
                    frozenEdges.RemoveAt(i);
                }
            }
            checkError();
        }

        public void ClearFrozenAnchors()
        {
            frozenAnchors.Clear();
            /// If there are no frozen anchors, there can be no frozen edges?
            ClearFrozenEdges();
            checkError();
        }

        unsafe public void ResetAlignment(Pose pose)
        {
            currentAlignment = pose;
        }

        unsafe public void AddSpongyAnchors(List<AnchorPose> spongyAnchors)
        {
            spongyAnchors.Sort((x, y) => { return x.anchorId.CompareTo(y.anchorId); });
            int spongyIdx = 0;
            for (int frozenIdx = 0; frozenIdx < frozenAnchors.Count; ++frozenIdx)
            {
                if (spongyIdx >= spongyAnchors.Count)
                {
                    break;
                }
                if (frozenAnchors[frozenIdx].anchorPose.anchorId == spongyAnchors[spongyIdx].anchorId)
                {
                    var frozenAnchor = frozenAnchors[frozenIdx];
                    frozenAnchor.anchorPose.pose = spongyAnchors[spongyIdx].pose;
                    frozenAnchor.relevance = 1.0f;
                    frozenAnchors[frozenIdx] = frozenAnchor;
                    ++spongyIdx;
                }
                else if (frozenAnchors[frozenIdx].anchorPose.anchorId > spongyAnchors[spongyIdx].anchorId)
                {
                    // Insert.
                    var frozenAnchor = new AnchorPoseRelevance()
                    {
                        anchorPose = spongyAnchors[spongyIdx],
                        relevance = 1.0f
                    };
                    frozenAnchors.Insert(frozenIdx, frozenAnchor);
                    ++spongyIdx;
                }
                /// else frozen[fIdx] < spongy[sIdx]
                ///    ++frozenIdx (but not spongyIdx, because we haven't processed this one yet).
            }
            while(spongyIdx < spongyAnchors.Count)
            {
                // append
                var frozenAnchor = new AnchorPoseRelevance()
                {
                    anchorPose = spongyAnchors[spongyIdx],
                    relevance = 1.0f
                };
                frozenAnchors.Add(frozenAnchor);
                ++spongyIdx;
            }
            checkError();
        }

        unsafe public void AddFrozenAnchor(AnchorId id, Pose frozenPose)
        {
            int idx = FindFrozenAnchor(id);
            if (idx >= 0)
            {
                var frozenAnchorPose = frozenAnchors[idx];
                frozenAnchorPose.anchorPose.pose = frozenPose;
                frozenAnchors[idx] = frozenAnchorPose;
            }
            else
            {
                idx = ~idx;
                var frozenAnchorPose = new AnchorPoseRelevance()
                {
                    anchorPose = new AnchorPose()
                    {
                        anchorId = id,
                        pose = frozenPose
                    },
                    relevance = 0.0f
                };
                frozenAnchors.Insert(idx, frozenAnchorPose);
            }
            checkError();
        }

        unsafe public void MoveFrozenAnchor(AnchorId id, Pose frozenPose)
        {
            SetFrozenAnchorTransform(id, frozenPose);
            checkError();
        }

        public void ClearSpongyAnchors()
        {
            checkError();
        }

        public void SetMostSignificantSpongyAnchorId(AnchorId anchorId)
        {
            mostSignificantAnchorId = anchorId;
            checkError();
        }

        unsafe public AnchorId GetMostSignificantFrozenAnchorId()
        {
            return mostSignificantAnchorId;
        }


        unsafe public AnchorRelevance[] GetSupportRelevances()
        {
            int numSupports = frozenAnchors.Count;

            var res = new AnchorRelevance[numSupports];

            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = new AnchorRelevance()
                {
                    anchorId = frozenAnchors[i].anchorPose.anchorId,
                    relevance = frozenAnchors[i].relevance
                };
            }

            return res;
        }

        public int GetNumFrozenEdges()
        {
            int numEdges = frozenEdges.Count;
            checkError();
            return numEdges;
        }

        unsafe public AnchorEdge[] GetFrozenEdges()
        {
            int numEdges = GetNumFrozenEdges();
            checkError();

            var res = new AnchorEdge[numEdges];
            for (int i = 0; i < res.Length; ++i)
            {
                res[i] = frozenEdges[i];
            }

            return res;
        }

        private AnchorEdge RegularEdge(AnchorId idx1, AnchorId idx2)
        {
            return idx1 < idx2
                ? new AnchorEdge() { anchorId1 = idx1, anchorId2 = idx2 }
                : new AnchorEdge() { anchorId1 = idx2, anchorId2 = idx1 };
        }

        unsafe public void AddSpongyEdges(ICollection<AnchorEdge> spongyEdges)
        {
            AnchorEdge[] regularEdges = new AnchorEdge[spongyEdges.Count];
            int idx = 0;
            foreach (var edge in spongyEdges)
            {
                regularEdges[idx++] = RegularEdge(edge.anchorId1, edge.anchorId2);
            }
            System.Comparison<AnchorEdge> alphabeticCompare = (x, y) =>
            {
                int cmp1 = x.anchorId1.CompareTo(y.anchorId1);
                if (cmp1 < 0)
                {
                    return -1;
                }
                if (cmp1 > 0)
                {
                    return 1;
                }
                int cmp2 = x.anchorId2.CompareTo(y.anchorId2);
                return cmp2;
            };
            System.Array.Sort(regularEdges, alphabeticCompare);

            int spongyIdx = 0;
            for (int frozenIdx = 0; frozenIdx < frozenEdges.Count; ++frozenIdx)
            {
                if (spongyIdx >= regularEdges.Length)
                {
                    break;
                }
                int frozenToSpongy = alphabeticCompare(frozenEdges[frozenIdx], regularEdges[spongyIdx]);
                if (frozenToSpongy >= 0)
                {
                    if (frozenToSpongy > 0)
                    {
                        // insert edge here
                        frozenEdges.Insert(frozenIdx, regularEdges[spongyIdx]);
                    }
                    // If existing frozen is greater, we just inserted (above) spongy, so advance. 
                    // If they are equal, we want to skip spongy, so advance.
                    // If existing is lesser, we haven't reached insertion point yet, 
                    // so don't advance spongyIdx (stay out of this conditional branch if frozenToSpongy < 0).
                    ++spongyIdx;
                }
            }
            while (spongyIdx < regularEdges.Length)
            {
                frozenEdges.Add(regularEdges[spongyIdx++]);
            }
            checkError();
        }

        public void ClearSpongyEdges()
        {
            checkError();
        }

        public void ClearFrozenEdges()
        {
            frozenEdges.Clear();
            checkError();
        }

        private int FindClosestFrozenAnchor(Vector3 pos)
        {
            int idx = -1;
            float minDistSq = float.MaxValue;
            for (int i = 0; i < frozenAnchors.Count; ++i)
            {
                float distSq = (frozenAnchors[i].anchorPose.pose.position - pos).sqrMagnitude;
                if (distSq < minDistSq)
                {
                    idx = i;
                    minDistSq = distSq;
                }
            }
            return idx;
        }

        unsafe public void CreateAttachmentPointFromHead(Vector3 frozenPosition, out AnchorId anchorId, out Vector3 locationFromAnchor)
        {
            checkError();
            int anchorIdx = FindClosestFrozenAnchor(frozenPosition);
            anchorId = frozenAnchors[anchorIdx].anchorPose.anchorId;
            locationFromAnchor = frozenPosition - frozenAnchors[anchorIdx].anchorPose.pose.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextAnchorId"></param>
        /// <param name="contextLocationFromAnchor"></param>
        /// <param name="frozenPosition"></param>
        /// <param name="anchorId"></param>
        /// <param name="locationFromAnchor"></param>
        /// <remarks>
        /// This noop version ignores the context/spawner hint.
        /// </remarks>
        unsafe public void CreateAttachmentPointFromSpawner(AnchorId contextAnchorId, Vector3 contextLocationFromAnchor, Vector3 frozenPosition,
            out AnchorId anchorId, out Vector3 locationFromAnchor)
        {
            CreateAttachmentPointFromHead(frozenPosition, out anchorId, out locationFromAnchor);
        }

        unsafe public bool ComputeAttachmentPointAdjustment(AnchorId oldAnchorId, Vector3 oldLocationFromAnchor,
            out AnchorId newAnchorId, out Vector3 newLocationFromAnchor, out Pose adjustment)
        {
            checkError();
            newAnchorId = oldAnchorId;
            newLocationFromAnchor = oldLocationFromAnchor;
            adjustment = new Pose(Vector3.zero, Quaternion.identity);
            return false;
        }

        unsafe public Vector3 MoveAttachmentPoint(Vector3 newFrozenLocation, AnchorId anchorId, Vector3 locationFromAnchor)
        {
            checkError();
            int idx = FindFrozenAnchor(anchorId);
            Debug.Assert(idx >= 0);

            return newFrozenLocation - frozenAnchors[idx].anchorPose.pose.position;
        }

        unsafe public Pose GetSpongyHead()
        {
            checkError();
            return currentSpongyHead;
        }

        unsafe public Pose GetAlignment()
        {
            checkError();
            return currentAlignment;
        }

        unsafe public bool Refreeze(out FragmentId mergedId, out FragmentId[] absorbedFragments)
        {
            checkError();
            mergedId = currentFragmentId;
            absorbedFragments = new FragmentId[0];
            return false;
        }

        unsafe public void RefreezeFinish()
        {
            checkError();
        }

        unsafe public bool Merge(out FragmentId targetFragment, out FragmentPose[] mergedFragments)
        {
            targetFragment = currentFragmentId;
            mergedFragments = new FragmentPose[0];
            return false;

        }

        /// <summary>
        /// Create a serializer of frozen world state
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public IPluginSerializer CreateSerializer(float startTime = 0.0f) => new Serializer(this, startTime);

        /// <summary>
        /// Class to capture and serialize frozen world state to storage.
        /// </summary>
        public class Serializer : IPluginSerializer
        {
            public long BytesSerialized { get { return 0; } }

            public long BytesPending { get { return 0; } }

            public float Time { get; set; }

            public bool IncludePersistent { get; set; }

            public bool IncludeTransient { get; set; }

            public Serializer(IPlugin plugin, float startTime = 0.0f)
            {
                Time = startTime;
                IncludePersistent = true;
                IncludeTransient = true;
            }

            public void Restart()
            {
            }

            public void GatherRecord()
            {
            }

            public async Task WriteRecordToAsync(Stream destinationStream, bool flush = true)
            {
                await Task.CompletedTask;
            }

            public List<byte[]> ReadRecordData()
            {
                var dataBlocks = new List<byte[]>();

                return dataBlocks;
            }

            ~Serializer()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
            }
        }

        /// <summary>
        /// Create a frozen world state deserializer.
        /// </summary>
        /// <returns>The deserializer</returns>
        public IPluginDeserializer CreateDeserializer() => new Deserializer(this);

        /// <summary>
        /// Class to handle deserialization of frozen world state
        /// </summary>
        public class Deserializer : IPluginDeserializer
        {
            public float Time { get; set; }

            public bool IncludePersistent { get; set; }

            public bool IncludeTransient { get; set; }

            /// <summary>
            /// Open deserialization stream for reading FrozenWorld state
            /// </summary>
            /// <param name="plugin">Unused dummy argument to ensure proper initialization order</param>
            public Deserializer(IPlugin plugin)
            {
                Time = 0.0f;
                IncludePersistent = true;
                IncludeTransient = true;
            }

            public async Task ReadRecordFromAsync(Stream sourceStream)
            {
                await Task.CompletedTask;
            }

            public void ApplyRecord()
            {
            }

            ~Deserializer()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected void Dispose(bool disposing)
            {
            }
        }
    }
}
