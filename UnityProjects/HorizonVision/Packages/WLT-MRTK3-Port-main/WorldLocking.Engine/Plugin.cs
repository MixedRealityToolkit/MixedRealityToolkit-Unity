// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using static Microsoft.MixedReality.FrozenWorld.Engine.Engine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Encapsulate FrozenWorldPlugin.dll with a Unity-friendly interface
    /// 
    /// This class contains no significant logic, only translation between the low-level C-style interface of the library
    /// and corresponding high-level C#/Unity data structures and calling paradigms
    /// 
    /// Though the library itself is implemented as singleton, this class implements constructor and Dispose function,
    /// handling the initialize/destroy functions of the engine, allowing stable loading/unloading cycles of the
    /// FrozenWorld component within the UnityEditor as they happen during typical application development.
    /// 
    /// Though this class has no significant internal state and most methods could technically be declared static, they are
    /// intentionally implemented as regular methods to ensure that the constructor has been called before any other
    /// interaction with the library.
    /// </summary>
    public class Plugin : IPlugin
    {
        private FrozenWorld_Vector UtoF(Vector3 v) => new FrozenWorld_Vector { x = v.x, y = v.y, z = v.z };
        private FrozenWorld_Quaternion UtoF(Quaternion q) => new FrozenWorld_Quaternion { x = q.x, y = q.y, z = q.z, w = q.w };
        private FrozenWorld_Transform UtoF(Pose p) => new FrozenWorld_Transform { rotation = UtoF(p.rotation), position = UtoF(p.position) };

        private Vector3 FtoU(FrozenWorld_Vector v) => new Vector3(v.x, v.y, v.z);
        private Quaternion FtoU(FrozenWorld_Quaternion q) => new Quaternion(q.x, q.y, q.z, q.w);
        private Pose FtoU(FrozenWorld_Transform q) => new Pose(FtoU(q.position), FtoU(q.rotation));

        private static unsafe void checkError()
        {
            if (FrozenWorld_GetError())
            {
                int bufsize = 256;
                byte* buffer = stackalloc byte[bufsize];
                int msgsize = FrozenWorld_GetErrorMessage(bufsize, buffer);
                string msg = System.Text.Encoding.UTF8.GetString(buffer, msgsize);
                throw new EngineException(msg);
            }
        }


        public Plugin()
        {
            FrozenWorld_Init();
            checkError();
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

            FrozenWorld_Destroy();
            checkError();
        }

        ~Plugin()
        {
            Dispose(false);
        }

        public static unsafe bool HasEngine()
        {
            bool haveEngine = false;
            try
            {
                int versionBufferSize = 256;
                byte* versionBuffer = stackalloc byte[versionBufferSize];
                int versionSize = FrozenWorld_GetVersion(false, versionBufferSize, versionBuffer);
                haveEngine = true;
            }
            catch (Exception)
            {
            }
            return haveEngine;
        }

        private string cachedCompact;
        public string VersionCompact => cachedCompact ?? (cachedCompact = getVersion(false));

        private string cachedDetail;
        public string VersionDetailed => cachedDetail ?? (cachedDetail = getVersion(true));

        private unsafe string getVersion(bool detail)
        {
            int versionBufferSize = (detail ? 4096 : 256);
            byte* versionBuffer = stackalloc byte[versionBufferSize];
            int versionSize = FrozenWorld_GetVersion(detail, versionBufferSize, versionBuffer);
            return System.Text.Encoding.UTF8.GetString(versionBuffer, versionSize);
        }

        public class MetricsAccessor : IMetricsAccessor
        {
            internal FrozenWorld_Metrics fw_metrics;

            // Merge and refreeze indicators
            public bool RefitMergeIndicated { get { return fw_metrics.refitMergeIndicated; } }
            public bool RefitRefreezeIndicated { get { return fw_metrics.refitRefreezeIndicated; } }

            // Currently trackable fragments
            public int NumTrackableFragments { get { return fw_metrics.numTrackableFragments; } }

            // Alignment supports
            public int NumVisualSupports { get { return fw_metrics.numVisualSupports; } }
            public int NumVisualSupportAnchors { get { return fw_metrics.numVisualSupportAnchors; } }
            public int NumIgnoredSupports { get { return fw_metrics.numIgnoredSupports; } }
            public int NumIgnoredSupportAnchors { get { return fw_metrics.numIgnoredSupportAnchors; } }

            // Visual deviation metrics
            public float MaxLinearDeviation { get { return fw_metrics.maxLinearDeviation; } }
            public float MaxLateralDeviation { get { return fw_metrics.maxLateralDeviation; } }
            public float MaxAngularDeviation { get { return fw_metrics.maxAngularDeviation; } }
            public float MaxLinearDeviationInFrustum { get { return fw_metrics.maxLinearDeviationInFrustum; } }
            public float MaxLateralDeviationInFrustum { get { return fw_metrics.maxLateralDeviationInFrustum; } }
            public float MaxAngularDeviationInFrustum { get { return fw_metrics.maxAngularDeviationInFrustum; } }
        }

        private MetricsAccessor metrics = new MetricsAccessor();
        public IMetricsAccessor Metrics { get { return metrics; } }

        unsafe public void Step_Init(Pose spongyHeadPose)
        {
            FrozenWorld_Step_Init();
            checkError();

            var pos = UtoF(spongyHeadPose.position);
            var fwdir = UtoF(spongyHeadPose.rotation * Vector3.forward);
            var updir = UtoF(spongyHeadPose.rotation * Vector3.up);
            FrozenWorld_SetHead(FrozenWorld_Snapshot.SPONGY, &pos, &fwdir, &updir);
            checkError();
        }

        unsafe public void Step_Finish()
        {
            FrozenWorld_Step_GatherSupports();
            checkError();

            FrozenWorld_Step_AlignSupports();
            checkError();

            fixed(FrozenWorld_Metrics* m = &(metrics.fw_metrics))
            {
                FrozenWorld_GetMetrics(m);
                checkError();
            }
        }

        unsafe public AnchorId[] GetFrozenAnchorIds()
        {
            int numAnchors = FrozenWorld_GetNumAnchors(FrozenWorld_Snapshot.FROZEN);
            checkError();

            var res = new AnchorId[numAnchors];

            if (numAnchors > 0)
            {
                FrozenWorld_Anchor* fwa = stackalloc FrozenWorld_Anchor[numAnchors];

                numAnchors = FrozenWorld_GetAnchors(FrozenWorld_Snapshot.FROZEN, numAnchors, fwa);
                checkError();

                for (int i = 0; i < numAnchors; i++)
                {
                    res[i] = (AnchorId)fwa[i].anchorId;
                }
            }

            return res;
        }

        unsafe public AnchorFragmentPose[] GetFrozenAnchors()
        {
            int numAnchors = FrozenWorld_GetNumAnchors(FrozenWorld_Snapshot.FROZEN);
            checkError();

            var res = new AnchorFragmentPose[numAnchors];

            if (numAnchors > 0)
            {
                FrozenWorld_Anchor* fwa = stackalloc FrozenWorld_Anchor[numAnchors];

                numAnchors = FrozenWorld_GetAnchors(FrozenWorld_Snapshot.FROZEN, numAnchors, fwa);
                checkError();

                for (int i = 0; i < numAnchors; i++)
                {
                    res[i] = new AnchorFragmentPose()
                    {
                        anchorId = (AnchorId)fwa[i].anchorId,
                        fragmentPose = new FragmentPose()
                        {
                            fragmentId = (FragmentId)fwa[i].fragmentId,
                            pose = FtoU(fwa[i].transform)
                        }
                    };
                }
            }

            return res;
        }

        unsafe public FragmentId GetMostSignificantFragmentId()
        {
            FrozenWorld_FragmentId res;
            FrozenWorld_GetMostSignificantFragmentId(FrozenWorld_Snapshot.FROZEN, &res);
            checkError();
            return (FragmentId)res;
        }

        unsafe public void SetFrozenAnchorTransform(AnchorId anchorId, Pose pose)
        {
            var trans = UtoF(pose);
            FrozenWorld_SetAnchorTransform(FrozenWorld_Snapshot.FROZEN, (FrozenWorld_AnchorId)anchorId, &trans);
            checkError();
        }

        public void RemoveFrozenAnchor(AnchorId anchorId)
        {
            FrozenWorld_RemoveAnchor(FrozenWorld_Snapshot.FROZEN, (FrozenWorld_AnchorId)anchorId);
            checkError();
        }

        public void RemoveFrozenEdge(AnchorId anchorId1, AnchorId anchorId2)
        {
            FrozenWorld_RemoveEdge(FrozenWorld_Snapshot.FROZEN, (FrozenWorld_AnchorId)anchorId1, (FrozenWorld_AnchorId)anchorId2);
            checkError();
        }

        public void ClearFrozenAnchors()
        {
            FrozenWorld_ClearAnchors(FrozenWorld_Snapshot.FROZEN);
            checkError();
        }

        unsafe public void ResetAlignment(Pose pose)
        {
            var alignment = UtoF(pose);
            FrozenWorld_SetAlignment(&alignment);
        }

        unsafe public void AddSpongyAnchors(List<AnchorPose> anchors)
        {
            if (anchors.Count == 0)
            {
                return;
            }

            FrozenWorld_Anchor* fwa = stackalloc FrozenWorld_Anchor[anchors.Count];

            for (int i = 0; i < anchors.Count; ++i)
            {
                fwa[i].anchorId = (FrozenWorld_AnchorId)anchors[i].anchorId;
                fwa[i].fragmentId = UNKNOWN_FRAGMENT_ID;
                fwa[i].transform = UtoF(anchors[i].pose);
            }

            FrozenWorld_AddAnchors(FrozenWorld_Snapshot.SPONGY, anchors.Count, fwa);
            checkError();
        }

        unsafe public void AddFrozenAnchor(AnchorId id, Pose frozenPose)
        {
            FrozenWorld_Anchor fwa;

            fwa.anchorId = (FrozenWorld_AnchorId)id;
            fwa.fragmentId = UNKNOWN_FRAGMENT_ID;
            fwa.transform = UtoF(frozenPose);

            FrozenWorld_AddAnchors(FrozenWorld_Snapshot.FROZEN, 1, &fwa);
            checkError();
        }

        unsafe public void MoveFrozenAnchor(AnchorId id, Pose frozenPose)
        {
            var trans = UtoF(frozenPose);

            FrozenWorld_SetAnchorTransform(FrozenWorld_Snapshot.FROZEN, (FrozenWorld_AnchorId)id, &trans);
            checkError();
        }

        public void ClearSpongyAnchors()
        {
            FrozenWorld_ClearAnchors(FrozenWorld_Snapshot.SPONGY);
            checkError();
        }

        public void SetMostSignificantSpongyAnchorId(AnchorId anchorId)
        {
            FrozenWorld_SetMostSignificantAnchorId(FrozenWorld_Snapshot.SPONGY, (FrozenWorld_AnchorId)anchorId);
            checkError();
        }

        unsafe public AnchorId GetMostSignificantFrozenAnchorId()
        {
            FrozenWorld_AnchorId res;
            FrozenWorld_GetMostSignificantAnchorId(FrozenWorld_Snapshot.FROZEN, &res);
            checkError();
            return (AnchorId)res;
        }


        unsafe public AnchorRelevance[] GetSupportRelevances()
        {
            int numSupports = FrozenWorld_GetNumSupports();
            checkError();

            var res = new AnchorRelevance[numSupports];

            if (numSupports > 0)
            {
                FrozenWorld_Support* fws = stackalloc FrozenWorld_Support[numSupports];
                numSupports = FrozenWorld_GetSupports(numSupports, fws);
                checkError();

                for(int i = 0; i < numSupports; i++)
                {
                    Debug.Assert(FtoU(fws[i].attachmentPoint.locationFromAnchor).Equals(Vector3.zero), "delocalized support not yet implemented");
                    var anchorId = (AnchorId)fws[i].attachmentPoint.anchorId;
                    Debug.Assert(!Array.Exists(res, x => x.anchorId == anchorId), "multiple supports per anchor not yet implemented");
                    res[i] = new AnchorRelevance()
                    {
                        anchorId = anchorId,
                        relevance = fws[i].relevance
                    };
                }
            }

            return res;
        }

        public int GetNumFrozenEdges()
        {
            int numEdges = FrozenWorld_GetNumEdges(FrozenWorld_Snapshot.FROZEN);
            checkError();
            return numEdges;
        }

        unsafe public AnchorEdge[] GetFrozenEdges()
        {
            int numEdges = GetNumFrozenEdges();
            checkError();

            var res = new AnchorEdge[numEdges];

            if (numEdges > 0)
            {
                FrozenWorld_Edge* fwe = stackalloc FrozenWorld_Edge[numEdges];

                numEdges = FrozenWorld_GetEdges(FrozenWorld_Snapshot.FROZEN, numEdges, fwe);
                checkError();

                for (int i = 0; i < numEdges; i++)
                {
                    res[i] = new AnchorEdge() { anchorId1 = (AnchorId)fwe[i].anchorId1, anchorId2 = (AnchorId)fwe[i].anchorId2 };
                }
            }

            return res;
        }

        unsafe public void AddSpongyEdges(ICollection<AnchorEdge> edges)
        {
            if (edges.Count == 0)
            {
                return;
            }

            FrozenWorld_Edge* fwe = stackalloc FrozenWorld_Edge[edges.Count];

            int i = 0;
            foreach (var e in edges)
            {
                fwe[i].anchorId1 = (FrozenWorld_AnchorId)e.anchorId1;
                fwe[i].anchorId2 = (FrozenWorld_AnchorId)e.anchorId2;
                i += 1;
            }

            FrozenWorld_AddEdges(FrozenWorld_Snapshot.SPONGY, edges.Count, fwe);
            checkError();
        }

        public void ClearSpongyEdges()
        {
            FrozenWorld_ClearEdges(FrozenWorld_Snapshot.SPONGY);
            checkError();
        }

        public void ClearFrozenEdges()
        {
            FrozenWorld_ClearEdges(FrozenWorld_Snapshot.FROZEN);
            checkError();
        }

        unsafe public void CreateAttachmentPointFromHead(Vector3 frozenPosition, out AnchorId anchorId, out Vector3 locationFromAnchor)
        {
            FrozenWorld_AttachmentPoint att;
            FrozenWorld_Vector v = UtoF(frozenPosition);
            FrozenWorld_Tracking_CreateFromHead(&v, &att);
            checkError();
            anchorId = (AnchorId)att.anchorId;
            locationFromAnchor = FtoU(att.locationFromAnchor);
        }

        unsafe public void CreateAttachmentPointFromSpawner(AnchorId contextAnchorId, Vector3 contextLocationFromAnchor, Vector3 frozenPosition,
            out AnchorId anchorId, out Vector3 locationFromAnchor)
        {
            FrozenWorld_AttachmentPoint context = new FrozenWorld_AttachmentPoint
            {
                anchorId = (FrozenWorld_AnchorId)contextAnchorId,
                locationFromAnchor = UtoF(contextLocationFromAnchor)
            };
            FrozenWorld_AttachmentPoint att;
            FrozenWorld_Vector v = UtoF(frozenPosition);
            FrozenWorld_Tracking_CreateFromSpawner(&context, &v, &att);
            checkError();
            anchorId = (AnchorId)att.anchorId;
            locationFromAnchor = FtoU(att.locationFromAnchor);
        }

        unsafe public bool ComputeAttachmentPointAdjustment(AnchorId oldAnchorId, Vector3 oldLocationFromAnchor,
            out AnchorId newAnchorId, out Vector3 newLocationFromAnchor, out Pose adjustment)
        {
            FrozenWorld_AttachmentPoint attachmentPoint = new FrozenWorld_AttachmentPoint
            {
                anchorId = (FrozenWorld_AnchorId)oldAnchorId,
                locationFromAnchor = UtoF(oldLocationFromAnchor)
            };
            FrozenWorld_Transform fwAdjustment;
            bool adjusted = FrozenWorld_RefitRefreeze_CalcAdjustment(&attachmentPoint, &fwAdjustment);
            checkError();
            newAnchorId = (AnchorId)attachmentPoint.anchorId;
            newLocationFromAnchor = FtoU(attachmentPoint.locationFromAnchor);
            adjustment = FtoU(fwAdjustment);
            return adjusted;
        }

        unsafe public Vector3 MoveAttachmentPoint(Vector3 newFrozenLocation, AnchorId anchorId, Vector3 locationFromAnchor)
        {
            FrozenWorld_Vector targetLocation = UtoF(newFrozenLocation);
            FrozenWorld_AttachmentPoint attachmentPoint = new FrozenWorld_AttachmentPoint
            {
                anchorId = (FrozenWorld_AnchorId)anchorId,
                locationFromAnchor = UtoF(locationFromAnchor)
            };
            FrozenWorld_Tracking_Move(&targetLocation, &attachmentPoint);
            checkError();
            return FtoU(attachmentPoint.locationFromAnchor);
        }

        unsafe public Pose GetSpongyHead()
        {
            FrozenWorld_Vector pos;
            FrozenWorld_Vector fwdir;
            FrozenWorld_Vector updir;
            FrozenWorld_GetHead(FrozenWorld_Snapshot.SPONGY, &pos, &fwdir, &updir);
            checkError();
            return new Pose(FtoU(pos), Quaternion.LookRotation(FtoU(fwdir), FtoU(updir)));
        }

        unsafe public Pose GetAlignment()
        {
            FrozenWorld_Transform spongyFromFrozenTrans;
            FrozenWorld_GetAlignment(&spongyFromFrozenTrans);
            checkError();
            return FtoU(spongyFromFrozenTrans);
        }

        unsafe public bool Refreeze(out FragmentId mergedId, out FragmentId[] absorbedFragments)
        {
            if (!FrozenWorld_RefitRefreeze_Init())
            {
                checkError();
                mergedId = GetMostSignificantFragmentId();
                absorbedFragments = new FragmentId[0];
                return false;
            }
            checkError();

            FrozenWorld_RefitRefreeze_Prepare();
            checkError();

            int bufSize = FrozenWorld_RefitRefreeze_GetNumAdjustedFragments();
            checkError();

            FrozenWorld_FragmentId* buf = stackalloc FrozenWorld_FragmentId[bufSize];

            int numAffected = FrozenWorld_RefitRefreeze_GetAdjustedFragmentIds(bufSize, buf);
            checkError();

            absorbedFragments = new FragmentId[numAffected];
            for (int i = 0; i < numAffected; ++i)
            {
                absorbedFragments[i] = (FragmentId)buf[i];
            }

            FrozenWorld_FragmentId mergedFragmentId;
            FrozenWorld_RefitRefreeze_GetMergedFragmentId(&mergedFragmentId);
            checkError();
            mergedId = (FragmentId)mergedFragmentId;

            return true;
        }

        unsafe public void RefreezeFinish()
        { 
            FrozenWorld_RefitRefreeze_Apply();
            checkError();
        }

        unsafe public bool Merge(out FragmentId targetFragment, out FragmentPose[] mergedFragments)
        {
            targetFragment = FragmentId.Invalid;

            if (!FrozenWorld_RefitMerge_Init())
            {
                checkError();
                targetFragment = GetMostSignificantFragmentId();
                mergedFragments = new FragmentPose[0];
                return false;
            }
            checkError();

            FrozenWorld_RefitMerge_Prepare();
            checkError();

            int bufSize = FrozenWorld_RefitMerge_GetNumAdjustedFragments();
            checkError();

            FrozenWorld_RefitMerge_AdjustedFragment* buf = stackalloc FrozenWorld_RefitMerge_AdjustedFragment[bufSize];
            int numAdjustedFragments = FrozenWorld_RefitMerge_GetAdjustedFragments(bufSize, buf);
            checkError();
            mergedFragments = new FragmentPose[numAdjustedFragments];

            for (int i = 0; i < numAdjustedFragments; i++)
            {
                var fragmentAdjust = new FragmentPose() { fragmentId = (FragmentId)buf[i].fragmentId, pose = FtoU(buf[i].adjustment) };
                mergedFragments[i] = fragmentAdjust;
            }

            FrozenWorld_FragmentId mergedFragmentId;
            FrozenWorld_RefitMerge_GetMergedFragmentId(&mergedFragmentId);
            checkError();
            targetFragment = (FragmentId)mergedFragmentId;

            FrozenWorld_RefitMerge_Apply();
            checkError();

            return true;
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
        public class Serializer: IPluginSerializer
        {
            private FrozenWorld_Serialize_Stream stream;
            private float lastWriteStreamTime;

            public long BytesSerialized { get; private set; }

            public long BytesPending
            {
                get { return stream.numBytesBuffered; }
            }

            public float Time
            {
                get { return stream.time; }
                set { stream.time = value; }
            }

            public bool IncludePersistent
            {
                get { return stream.includePersistent; }
                set { stream.includePersistent = value; }
            }

            public bool IncludeTransient
            {
                get { return stream.includeTransient; }
                set { stream.includeTransient = value; }
            }

            /// <summary>
            /// Open serialization stream for writing FrozenWorld state
            /// </summary>
            /// <param name="plugin">Unused dummy argument to ensure proper initialization order</param>
            /// <param name="startTime">Relative time stamp initialized into the stream</param>
            public Serializer(IPlugin plugin, float startTime = 0.0f)
            {
                stream.time = startTime;
                stream.includePersistent = true;
                stream.includeTransient = true;

                unsafe
                {
                    fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                    {
                        FrozenWorld_Serialize_Open(fixedStreamPtr);
                        checkError();
                    }
                }
            }

            public void Restart()
            {
                // avoid diagnostic error due to already-gathered record data
                stream.numBytesBuffered = 0;

                // properly offset first record after reopen from last written record
                float reopenStreamTime = stream.time;
                stream.time = lastWriteStreamTime;

                unsafe
                {
                    fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                    {
                        FrozenWorld_Serialize_Close(fixedStreamPtr);
                        checkError();
                        FrozenWorld_Serialize_Open(fixedStreamPtr);
                        checkError();
                    }
                }

                // restore next record time (in case it was set before reopen)
                stream.time = reopenStreamTime;

                BytesSerialized = 0;
            }

            public void GatherRecord()
            {
                unsafe
                {
                    fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                    {
                        FrozenWorld_Serialize_Gather(fixedStreamPtr);
                        checkError();
                    }
                }
            }

            public async Task WriteRecordToAsync(Stream destinationStream, bool flush = true)
            {
                lastWriteStreamTime = stream.time;

                byte[] buffer = new byte[0x1000];

                while (stream.numBytesBuffered > 0)
                {
                    int numBytesRead;

                    unsafe
                    {
                        fixed (byte* fixedBufferPtr = buffer)
                        {
                            fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                            {
                                numBytesRead = FrozenWorld_Serialize_Read(fixedStreamPtr, buffer.Length, fixedBufferPtr);
                                checkError();
                            }
                        }
                    }

                    await destinationStream.WriteAsync(buffer, 0, numBytesRead);
                    BytesSerialized += numBytesRead;

                    if (flush)
                    {
                        await destinationStream.FlushAsync();
                    }
                }
            }

            public List<byte[]> ReadRecordData()
            {
                var dataBlocks = new List<byte[]>();

                lastWriteStreamTime = stream.time;

                while (stream.numBytesBuffered > 0)
                {
                    int dataBlockSize = stream.numBytesBuffered;
                    byte[] dataBlock = new byte[dataBlockSize];

                    unsafe
                    {
                        fixed (byte* fixedDataBlockPtr = dataBlock)
                        {
                            fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                            {
                                int numBytesRead = FrozenWorld_Serialize_Read(fixedStreamPtr, dataBlockSize, fixedDataBlockPtr);
                                checkError();
                                Debug.Assert(numBytesRead == dataBlockSize);
                            }
                        }
                    }

                    dataBlocks.Add(dataBlock);
                    BytesSerialized += dataBlockSize;
                }

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
                if (stream.handle != 0)
                {
                    unsafe
                    {
                        fixed (FrozenWorld_Serialize_Stream* fixedStreamPtr = &stream)
                        {
                            FrozenWorld_Serialize_Close(fixedStreamPtr);
                            
                            // Suppress error reporting if called from finalizer
                            if (disposing)
                            {
                                checkError();
                            }
                        }
                    }
                }
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
        public class Deserializer: IPluginDeserializer
        {
            private FrozenWorld_Deserialize_Stream stream;

            public float Time
            {
                get { return stream.time; }
                set { stream.time = value; }
            }

            public bool IncludePersistent
            {
                get { return stream.includePersistent; }
                set { stream.includePersistent = value; }
            }

            public bool IncludeTransient
            {
                get { return stream.includeTransient; }
                set { stream.includeTransient = value; }
            }

            /// <summary>
            /// Open deserialization stream for reading FrozenWorld state
            /// </summary>
            /// <param name="plugin">Unused dummy argument to ensure proper initialization order</param>
            public Deserializer(IPlugin plugin)
            {
                stream.time = 0.0f;
                stream.includePersistent = true;
                stream.includeTransient = true;

                unsafe
                {
                    fixed (FrozenWorld_Deserialize_Stream* fixedStreamPtr = &stream)
                    {
                        FrozenWorld_Deserialize_Open(fixedStreamPtr);
                        checkError();
                    }
                }
            }

            public async Task ReadRecordFromAsync(Stream sourceStream)
            {
                byte[] buffer = new byte[0x1000];

                while (stream.numBytesRequired > 0)
                {
                    int numBytesRead = await sourceStream.ReadAsync(buffer, 0, Math.Min(stream.numBytesRequired, buffer.Length));

                    unsafe
                    {
                        fixed (byte* fixedBufferPtr = buffer)
                        {
                            fixed (FrozenWorld_Deserialize_Stream* fixedStreamPtr = &stream)
                            {
                                FrozenWorld_Deserialize_Write(fixedStreamPtr, numBytesRead, fixedBufferPtr);
                                checkError();
                            }
                        }
                    }
                }
            }

            public void ApplyRecord()
            {
                unsafe
                {
                    fixed (FrozenWorld_Deserialize_Stream* fixedStreamPtr = &stream)
                    {
                        FrozenWorld_Deserialize_Apply(fixedStreamPtr);
                        checkError();
                    }
                }
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
                if (stream.handle != 0)
                {
                    unsafe
                    {
                        fixed (FrozenWorld_Deserialize_Stream* fixedStreamPtr = &stream)
                        {
                            FrozenWorld_Deserialize_Close(fixedStreamPtr);

                            // Suppress error reporting if called from finalizer
                            if (disposing)
                            {
                                checkError();
                            }
                        }
                    }
                }
            }
        }
    }
}
