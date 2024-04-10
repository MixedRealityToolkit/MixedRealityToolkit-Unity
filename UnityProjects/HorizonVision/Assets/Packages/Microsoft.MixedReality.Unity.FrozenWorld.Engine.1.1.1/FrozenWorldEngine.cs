// TKBMS v1.0 -----------------------------------------------------
//
// PLATFORM       : ALL
// PRODUCT        : FROZEN_WORLD
// VISIBILITY     : INTERNAL
//
// ------------------------------------------------------TKBMS v1.0

// This file is an exact C# transcription of the original FrozenWorldEngine.h header file to be distributed along the FrozenWorldPlugin.dll binary

using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.FrozenWorld.Engine
{
    public unsafe static class Engine
    {
#if UNITY_IOS
        // Statically linked FrozenWorldPlugin framework on iOS
        private const string PluginPath = "__Internal";
#else
        // Dynamically linked FrozenWorldPlugin shared library on all other platforms
        private const string PluginPath = "FrozenWorldPlugin";
#endif

        public enum FrozenWorld_AnchorId : ulong { }
        public enum FrozenWorld_FragmentId : ulong { }

        // Special values for FrozenWorld_AnchorId
        public const FrozenWorld_AnchorId INVALID_ANCHOR_ID = 0;
        public const FrozenWorld_AnchorId UNKNOWN_ANCHOR_ID = (FrozenWorld_AnchorId)0xFFFFFFFFFFFFFFFF;

        // Special values for FrozenWorld_FragmentId
        public const FrozenWorld_FragmentId INVALID_FRAGMENT_ID = 0;
        public const FrozenWorld_FragmentId UNKNOWN_FRAGMENT_ID = (FrozenWorld_FragmentId)0xFFFFFFFFFFFFFFFF;

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Vector
        {
            public float x;
            public float y;
            public float z;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Quaternion
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Transform
        {
            public FrozenWorld_Vector position;
            public FrozenWorld_Quaternion rotation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_AttachmentPoint
        {
            public FrozenWorld_AnchorId anchorId;
            public FrozenWorld_Vector locationFromAnchor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_AlignConfig
        {
            // Max edge deviation (0.0..1.0, default 0.05) to cut off significantly deviating anchors from alignment
            public float edgeDeviationThreshold;

            // Relevance gradient away from head
            public float relevanceSaturationRadius;  // 1.0 at this distance from head
            public float relevanceDropoffRadius;     // 0.0 at this distance (must be greater than saturation radius)

            // Tightness gradient away from head
            public float tightnessSaturationRadius;  // 1.0 at this distance from head
            public float tightnessDropoffRadius;     // 0.0 at this distance (must be greater than saturation radius)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Support
        {
            public FrozenWorld_AttachmentPoint attachmentPoint;

            public float relevance;   // 1.0 (max) .. 0.0 (min, excluded)
            public float tightness;   // 1.0 (max) .. 0.0 (min, only lateral alignment)
        }

        public enum FrozenWorld_Snapshot
        {
            SPONGY = 0,
            FROZEN = 1,
            CUSTOM = 1000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Anchor
        {
            public FrozenWorld_AnchorId anchorId;
            public FrozenWorld_FragmentId fragmentId;
            public FrozenWorld_Transform transform;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Edge
        {
            public FrozenWorld_AnchorId anchorId1;
            public FrozenWorld_AnchorId anchorId2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Metrics
        {
            // Merge and refreeze indicators
            public bool refitMergeIndicated;
            public bool refitRefreezeIndicated;          // configurable

            // Currently trackable fragments
            public int numTrackableFragments;

            // Alignment supports
            public int numVisualSupports;
            public int numVisualSupportAnchors;
            public int numIgnoredSupports;
            public int numIgnoredSupportAnchors;

            // Visual deviation metrics
            public float maxLinearDeviation;
            public float maxLateralDeviation;
            public float maxAngularDeviation;            // configurable
            public float maxLinearDeviationInFrustum;    // configurable
            public float maxLateralDeviationInFrustum;   // configurable
            public float maxAngularDeviationInFrustum;   // configurable
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_MetricsConfig
        {
            // Angular deviation capped to this distance
            public float angularDeviationNearDistance;

            // View frustum
            public float frustumHorzAngle;
            public float frustumVertAngle;

            // Thresholds for refreeze indicator
            public float refreezeLinearDeviationThreshold;
            public float refreezeLateralDeviationThreshold;
            public float refreezeAngularDeviationThreshold;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_RefitMerge_AdjustedFragment
        {
            public FrozenWorld_FragmentId fragmentId;
            public int numAdjustedAnchors;
            public FrozenWorld_Transform adjustment;   // post-merged from pre-merged
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Serialize_Stream
        {
            // Internal handle to this serialization stream
            public int handle;

            // Number of bytes that at least remain to be serialized for a complete record
            public int numBytesBuffered;

            // Real time in seconds serialized into this stream so far
            // (can be modified to control relative timestamps serialized into the stream)
            public float time;

            // Selection of data to include in the stream
            // (can be modified to control what is serialized into the stream)
            public bool includePersistent;     // frozen anchors and edges
            public bool includeTransient;      // alignment config, all other snapshot data, supports
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrozenWorld_Deserialize_Stream
        {
            // Internal handle to this deserialization stream
            public int handle;

            // Number of bytes that at least remain to be deserialized for a complete record
            public int numBytesRequired;

            // Real time in seconds deserialized from this stream so far
            // (can be modified to change its base value for subsequent deserialized records)
            public float time;

            // Selection of data applied from the stream
            // (can be modified to control what is deserialized from the stream)
            public bool includePersistent;     // frozen anchors and edges
            public bool includeTransient;      // alignment config, all other snapshot data, supports
        }

        // Version
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetVersion([MarshalAs(UnmanagedType.U1)] bool detail, int versionBufferSize, byte* versionOut);

        // Errors and diagnostics
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_GetError();
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetErrorMessage(int messageBufferSize, byte* messageOut);

        // Startup and tear down
        [DllImport(PluginPath)] public static extern void FrozenWorld_Init();
        [DllImport(PluginPath)] public static extern void FrozenWorld_Destroy();

        // Alignment
        [DllImport(PluginPath)] public static extern void FrozenWorld_Step_Init();
        [DllImport(PluginPath)] public static extern int FrozenWorld_Step_GatherSupports();
        [DllImport(PluginPath)] public static extern void FrozenWorld_Step_AlignSupports();

        // Alignment configuration
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetAlignConfig(FrozenWorld_AlignConfig* configOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetAlignConfig(FrozenWorld_AlignConfig* config);

        // Supports access
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetNumSupports();
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetSupports(int supportsBufferSize, FrozenWorld_Support* supportsOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetSupports(int numSupports, FrozenWorld_Support* supports);

        // Snapshot access: Head and alignment
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetHead(FrozenWorld_Snapshot snapshot, FrozenWorld_Vector* headPositionOut, FrozenWorld_Vector* headDirectionForwardOut, FrozenWorld_Vector* headDirectionUpOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetHead(FrozenWorld_Snapshot snapshot, FrozenWorld_Vector* headPosition, FrozenWorld_Vector* headDirectionForward, FrozenWorld_Vector* headDirectionUp);
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetAlignment(FrozenWorld_Transform* spongyFromFrozenTransformOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetAlignment(FrozenWorld_Transform* spongyFromFrozenTransform);

        // Snapshot access: Most significant anchor
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetMostSignificantAnchorId(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId* anchorIdOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetMostSignificantAnchorId(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId anchorId);
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetMostSignificantFragmentId(FrozenWorld_Snapshot snapshot, FrozenWorld_FragmentId* fragmentIdOut);

        // Snapshot access: Anchors
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetNumAnchors(FrozenWorld_Snapshot snapshot);
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetAnchors(FrozenWorld_Snapshot snapshot, int anchorsBufferSize, FrozenWorld_Anchor* anchorsOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_AddAnchors(FrozenWorld_Snapshot snapshot, int numAnchors, FrozenWorld_Anchor* anchors);
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_SetAnchorTransform(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId anchorId, FrozenWorld_Transform* transform);
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_SetAnchorFragment(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId anchorId, FrozenWorld_FragmentId fragmentId);
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_RemoveAnchor(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId anchorId);
        [DllImport(PluginPath)] public static extern void FrozenWorld_ClearAnchors(FrozenWorld_Snapshot snapshot);

        // Snapshot access: Edges
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetNumEdges(FrozenWorld_Snapshot snapshot);
        [DllImport(PluginPath)] public static extern int FrozenWorld_GetEdges(FrozenWorld_Snapshot snapshot, int edgesBufferSize, FrozenWorld_Edge* edgesOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_AddEdges(FrozenWorld_Snapshot snapshot, int numEdges, FrozenWorld_Edge* edges);
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_RemoveEdge(FrozenWorld_Snapshot snapshot, FrozenWorld_AnchorId anchorId1, FrozenWorld_AnchorId anchorId2);
        [DllImport(PluginPath)] public static extern void FrozenWorld_ClearEdges(FrozenWorld_Snapshot snapshot);

        // Snapshot access: Utilities
        [DllImport(PluginPath)] public static extern int FrozenWorld_MergeAnchorsAndEdges(FrozenWorld_Snapshot sourceSnapshot, FrozenWorld_Snapshot targetSnapshot);
        [DllImport(PluginPath)] public static extern int FrozenWorld_GuessMissingEdges(FrozenWorld_Snapshot snapshot, int guessedEdgesBufferSize, FrozenWorld_Edge* guessedEdgesOut);

        // Metrics
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetMetrics(FrozenWorld_Metrics* metricsOut);

        // Metrics configuration
        [DllImport(PluginPath)] public static extern void FrozenWorld_GetMetricsConfig(FrozenWorld_MetricsConfig* configOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_SetMetricsConfig(FrozenWorld_MetricsConfig* config);

        // Scene object tracking
        [DllImport(PluginPath)] public static extern void FrozenWorld_Tracking_CreateFromHead(FrozenWorld_Vector* frozenLocation, FrozenWorld_AttachmentPoint* attachmentPointOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Tracking_CreateFromSpawner(FrozenWorld_AttachmentPoint* spawnerAttachmentPoint, FrozenWorld_Vector* frozenLocation, FrozenWorld_AttachmentPoint* attachmentPointOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Tracking_Move(FrozenWorld_Vector* targetFrozenLocation, FrozenWorld_AttachmentPoint* attachmentPointInOut);

        // Fragment merge
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_RefitMerge_Init();
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitMerge_Prepare();
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitMerge_Apply();

        // Fragment merge: Adjustments query
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitMerge_GetNumAdjustedFragments();
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitMerge_GetAdjustedFragments(int adjustedFragmentsBufferSize, FrozenWorld_RefitMerge_AdjustedFragment* adjustedFragmentsOut);
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitMerge_GetAdjustedAnchorIds(FrozenWorld_FragmentId fragmentId, int adjustedAnchorIdsBufferSize, FrozenWorld_AnchorId* adjustedAnchorIdsOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitMerge_GetMergedFragmentId(FrozenWorld_FragmentId* mergedFragmentIdOut);

        // Refreeze
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_RefitRefreeze_Init();
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitRefreeze_Prepare();
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitRefreeze_Apply();

        // Refreeze: Adjustments query
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitRefreeze_GetNumAdjustedFragments();
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitRefreeze_GetNumAdjustedAnchors();
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitRefreeze_GetAdjustedFragmentIds(int adjustedFragmentIdsBufferSize, FrozenWorld_FragmentId* adjustedFragmentIdsOut);
        [DllImport(PluginPath)] public static extern int FrozenWorld_RefitRefreeze_GetAdjustedAnchorIds(int adjustedAnchorIdsBufferSize, FrozenWorld_AnchorId* adjustedAnchorIdsOut);
        [DllImport(PluginPath)] [return: MarshalAs(UnmanagedType.U1)] public static extern bool FrozenWorld_RefitRefreeze_CalcAdjustment(FrozenWorld_AttachmentPoint* attachmentPointInOut, FrozenWorld_Transform* objectAdjustmentOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_RefitRefreeze_GetMergedFragmentId(FrozenWorld_FragmentId* mergedFragmentIdOut);

        // Persistence: Serialization
        [DllImport(PluginPath)] public static extern void FrozenWorld_Serialize_Open(FrozenWorld_Serialize_Stream* streamInOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Serialize_Gather(FrozenWorld_Serialize_Stream* streamInOut);
        [DllImport(PluginPath)] public static extern int FrozenWorld_Serialize_Read(FrozenWorld_Serialize_Stream* streamInOut, int bytesBufferSize, byte* bytesOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Serialize_Close(FrozenWorld_Serialize_Stream* streamInOut);

        // Persistence: Deserialization
        [DllImport(PluginPath)] public static extern void FrozenWorld_Deserialize_Open(FrozenWorld_Deserialize_Stream* streamInOut);
        [DllImport(PluginPath)] public static extern int FrozenWorld_Deserialize_Write(FrozenWorld_Deserialize_Stream* streamInOut, int numBytes, byte* bytes);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Deserialize_Apply(FrozenWorld_Deserialize_Stream* streamInOut);
        [DllImport(PluginPath)] public static extern void FrozenWorld_Deserialize_Close(FrozenWorld_Deserialize_Stream* streamInOut);
    }
}

