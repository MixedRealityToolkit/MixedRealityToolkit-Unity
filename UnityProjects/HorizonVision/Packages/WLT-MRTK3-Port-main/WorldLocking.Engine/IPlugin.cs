// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

using static Microsoft.MixedReality.FrozenWorld.Engine.Engine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Numerical identifier for individual anchors within the FrozenWorld.
    /// Assigned by the client when defining new spongy anchors.
    /// Unique within a running session.
    /// Persistent as part of serialized state.
    /// </summary>
    public enum AnchorId : ulong
    {
        Invalid = INVALID_ANCHOR_ID,
        FirstValid = INVALID_ANCHOR_ID + 1,
        Unknown = UNKNOWN_ANCHOR_ID,
    };

    /// <summary>
    /// Numerical identifier for frozen fragments.
    /// Assigned by engine. Persistent as part of serialized state.
    /// </summary>
    public enum FragmentId : ulong
    {
        Invalid = INVALID_ANCHOR_ID,
        Unknown = UNKNOWN_ANCHOR_ID,
    }

    /// <summary>
    /// Simple struct for passing id,pose tuples, to avoid C# version dependency (e.g. ValueTuple)
    /// </summary>
    public struct AnchorPose
    {
        public AnchorId anchorId;
        public Pose pose;
    }

    /// <summary>
    /// Simple struct for passing id,pose tuples, to avoid C# version dependency (e.g. ValueTuple)
    /// </summary>
    public struct FragmentPose
    {
        public FragmentId fragmentId;
        public Pose pose;
    }

    /// <summary>
    /// Simple struct associating a FragmentPose with an AnchorId.
    /// </summary>
    public struct AnchorFragmentPose
    {
        public AnchorId anchorId;
        public FragmentPose fragmentPose;
    }

    /// <summary>
    /// Simple struct representing a non-directional edge between two anchors.
    /// </summary>
    public struct AnchorEdge
    {
        public AnchorId anchorId1;
        public AnchorId anchorId2;
    };

    /// <summary>
    /// Simple struct for relevance by anchor id.
    /// </summary>
    public struct AnchorRelevance
    {
        public AnchorId anchorId;
        public float relevance;
    }


    /// <summary>
    /// Provide string formatting for id types.
    /// </summary>
    public static class ConversionExt
    {
        /// <summary>
        /// Format AnchorId as string (used for visualization and persistence)
        /// </summary>
        /// <param name="id">Anchor Id to be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatStr(this AnchorId id)
        {
            switch (id)
            {
                case AnchorId.Invalid:
                    return "A#INV";
                case AnchorId.Unknown:
                    return "A#UNK";
                default:
                    return String.Format("A{0}", (int)id);
            }
        }

        /// <summary>
        /// Check that an id is valid and refers to a known anchor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsKnown(this AnchorId id)
        {
            return id != AnchorId.Invalid && id != AnchorId.Unknown;
        }

        /// <summary>
        /// Format FragmentId as string (used for visualization)
        /// </summary>
        /// <param name="id">Fragment Id to be formatted</param>
        /// <returns>Formatted string</returns>
        public static string FormatStr(this FragmentId id)
        {
            switch (id)
            {
                case FragmentId.Invalid:
                    return "F#INV";
                case FragmentId.Unknown:
                    return "F#UNK";
                default:
                    return String.Format("F{0}", (int)id);
            }
        }

        /// <summary>
        /// Check that an id is valid and refers to a known fragment.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsKnown(this FragmentId id)
        {
            return id != FragmentId.Invalid && id != FragmentId.Unknown;
        }
    }

    /// <summary>
    /// Thin layer on exceptions for engine generated exceptions.
    /// </summary>
    public class EngineException : Exception
    {
        public EngineException(string message)
        : base(String.Format("Error in call to FrozenWorld.Engine: \"{0}\"", message))
        {
        }
    }

    public interface IMetricsAccessor
    {

        // Merge and refreeze indicators
        bool RefitMergeIndicated { get; }
        bool RefitRefreezeIndicated { get; }

        // Currently trackable fragments
        int NumTrackableFragments { get; }

        // Alignment supports
        int NumVisualSupports { get; }
        int NumVisualSupportAnchors { get; }
        int NumIgnoredSupports { get; }
        int NumIgnoredSupportAnchors { get; }

        // Visual deviation metrics
        float MaxLinearDeviation { get; }
        float MaxLateralDeviation { get; }
        float MaxAngularDeviation { get; }
        float MaxLinearDeviationInFrustum { get; }
        float MaxLateralDeviationInFrustum { get; }
        float MaxAngularDeviationInFrustum { get; }
    }



    public interface IPlugin : IDisposable
    {
        string VersionCompact { get; }

        string VersionDetailed { get; }

        IMetricsAccessor Metrics { get; }

        unsafe void Step_Init(Pose spongyHeadPose);
        unsafe void Step_Finish();

        unsafe AnchorId[] GetFrozenAnchorIds();

        unsafe AnchorFragmentPose[] GetFrozenAnchors();

        unsafe FragmentId GetMostSignificantFragmentId();

        unsafe void SetFrozenAnchorTransform(AnchorId anchorId, Pose pose);

        void RemoveFrozenAnchor(AnchorId anchorId);

        void RemoveFrozenEdge(AnchorId anchorId1, AnchorId anchorId2);

        void ClearFrozenAnchors();

        unsafe void ResetAlignment(Pose pose);

        unsafe void AddSpongyAnchors(List<AnchorPose> anchors);

        unsafe void AddFrozenAnchor(AnchorId id, Pose frozenPose);

        unsafe void MoveFrozenAnchor(AnchorId id, Pose frozenPose);

        void ClearSpongyAnchors();

        void SetMostSignificantSpongyAnchorId(AnchorId anchorId);

        unsafe AnchorId GetMostSignificantFrozenAnchorId();

        unsafe AnchorRelevance[] GetSupportRelevances();

        int GetNumFrozenEdges();

        unsafe AnchorEdge[] GetFrozenEdges();

        unsafe void AddSpongyEdges(ICollection<AnchorEdge> edges);

        void ClearSpongyEdges();

        void ClearFrozenEdges();

        unsafe void CreateAttachmentPointFromHead(Vector3 frozenPosition, out AnchorId anchorId, out Vector3 locationFromAnchor);

        unsafe void CreateAttachmentPointFromSpawner(AnchorId contextAnchorId, Vector3 contextLocationFromAnchor, Vector3 frozenPosition,
            out AnchorId anchorId, out Vector3 locationFromAnchor);

        unsafe bool ComputeAttachmentPointAdjustment(AnchorId oldAnchorId, Vector3 oldLocationFromAnchor,
            out AnchorId newAnchorId, out Vector3 newLocationFromAnchor, out Pose adjustment);

        unsafe Vector3 MoveAttachmentPoint(Vector3 newFrozenLocation, AnchorId anchorId, Vector3 locationFromAnchor);

        unsafe Pose GetSpongyHead();

        unsafe Pose GetAlignment();

        unsafe bool Refreeze(out FragmentId mergedId, out FragmentId[] absorbedFragments);

        unsafe void RefreezeFinish();

        unsafe bool Merge(out FragmentId targetFragment, out FragmentPose[] mergedFragments);

        IPluginSerializer CreateSerializer(float startTime = 0.0f);

        IPluginDeserializer CreateDeserializer();
    }

    public interface IPluginSerializer : IDisposable
    {
        long BytesSerialized { get; }

        long BytesPending { get; }

        float Time { get; set; }

        bool IncludePersistent { get; set; }

        bool IncludeTransient { get; set; }

        void Restart();

        void GatherRecord();

        Task WriteRecordToAsync(Stream destinationStream, bool flush = true);

        List<byte[]> ReadRecordData();
    }

    public interface IPluginDeserializer : IDisposable
    {
        float Time { get; set; }

        bool IncludePersistent { get; set; }

        bool IncludeTransient { get; set; }

        Task ReadRecordFromAsync(Stream sourceStream);

        void ApplyRecord();

    }
}