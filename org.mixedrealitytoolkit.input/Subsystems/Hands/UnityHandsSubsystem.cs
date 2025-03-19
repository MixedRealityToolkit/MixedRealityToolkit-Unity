// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// A Unity subsystem that extends <see cref="MixedReality.Toolkit.Subsystems.HandsSubsystem">HandsSubsystem</see> and
    /// obtains hand joint poses from Unity's <see href="https://docs.unity3d.com/Packages/com.unity.xr.hands@1.3">XR Hands package</see>.
    /// </summary>
    [Preserve]
    [MRTKSubsystem(
        Name = "org.mixedrealitytoolkit.unityxrhands",
        DisplayName = "Unity XR Hands",
        Author = "Mixed Reality Toolkit Contributors",
        ProviderType = typeof(HandsProvider<UnityHandContainer>),
        SubsystemTypeOverride = typeof(UnityHandsSubsystem))]
    public class UnityHandsSubsystem : HandsSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Register()
        {
            // Fetch subsystem metadata from the attribute.
            var cinfo = XRSubsystemHelpers.ConstructCinfo<UnityHandsSubsystem, HandsSubsystemCinfo>();

            // Populate remaining cinfo field.
            cinfo.IsPhysicalData = true;

            if (!Register(cinfo))
            {
                Debug.LogError($"Failed to register the {cinfo.Name} subsystem.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityHandsSubsystem"/> class.
        /// </summary>
        public UnityHandsSubsystem()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(HandTrackingPermission))
            {
                PermissionCallbacks callbacks = new();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(HandTrackingPermission, callbacks);
                Debug.Log($"MRTK is requesting {HandTrackingPermission}.");
            }
            else
            {
                Debug.Log($"{HandTrackingPermission} already granted for MRTK.");
            }
        }

        private const string HandTrackingPermission = "android.permission.HAND_TRACKING";

        void OnPermissionDenied(string permission)
        {
            if (permission == HandTrackingPermission)
            {
                Debug.Log($"{HandTrackingPermission} denied or not needed on this runtime" +
#if UNITY_OPENXR_PRESENT
                    $" ({UnityEngine.XR.OpenXR.OpenXRRuntime.name})" +
#endif
                    ". MRTK hand tracking may not work as expected.");
            }
        }

        void OnPermissionGranted(string permission)
        {
            if (permission == HandTrackingPermission)
            {
                Debug.Log($"{HandTrackingPermission} newly granted for MRTK.");
            }
#endif // UNITY_ANDROID
        }

        private class UnityHandContainer : HandDataContainer
        {
            private XRHand hand;
            private XRHandSubsystem xrHandSubsystem;

            public UnityHandContainer(XRNode handNode) : base(handNode)
            {
                hand = GetTrackedHand();
            }

            private static readonly ProfilerMarker TryGetEntireHandPerfMarker =
                new ProfilerMarker("[MRTK] UnityHandContainer.TryGetEntireHand");

            /// <inheritdoc/>
            public override bool TryGetEntireHand(out IReadOnlyList<HandJointPose> result)
            {
                using (TryGetEntireHandPerfMarker.Auto())
                {
                    if (!AlreadyFullQueried)
                    {
                        TryCalculateEntireHand();
                    }

                    result = HandJoints;
                    return FullQueryValid;
                }
            }

            private static readonly ProfilerMarker TryGetJointPerfMarker =
                new ProfilerMarker("[MRTK] UnityHandContainer.TryGetJoint");

            /// <inheritdoc/>
            public override bool TryGetJoint(TrackedHandJoint joint, out HandJointPose pose)
            {
                using (TryGetJointPerfMarker.Auto())
                {
                    bool thisQueryValid = false;
                    int index = HandsUtils.ConvertToIndex(joint);

                    // If we happened to have already queried the entire
                    // hand data this frame, we don't need to re-query for
                    // just the joint. If we haven't, we do still need to
                    // query for the single joint.
                    if (!AlreadyFullQueried)
                    {
                        hand = GetTrackedHand();

                        // If the hand is not tracked, we obviously have no data,
                        // and return immediately.
                        if (!hand.isTracked)
                        {
                            pose = HandJoints[index];
                            return false;
                        }

                        // Joints are relative to the camera floor offset object.
                        Transform origin = PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform;
                        if (origin == null)
                        {
                            pose = HandJoints[index];
                            return false;
                        }

                        thisQueryValid |= TryUpdateJoint(index, hand.GetJoint(TrackedHandJointIndexToXRHandJointID[index]), origin);
                    }
                    else
                    {
                        // If we've already run a full-hand query, this single joint query
                        // is just as valid as the full query.
                        thisQueryValid = FullQueryValid;
                    }

                    pose = HandJoints[index];
                    return thisQueryValid;
                }
            }

            private static readonly ProfilerMarker TryCalculateEntireHandPerfMarker =
                new ProfilerMarker("[MRTK] UnityHandContainer.TryCalculateEntireHand");

            /// <summary>
            /// For a certain hand, query every Bone in the hand, and write all results to the
            /// HandJoints collection.
            /// </summary>
            private void TryCalculateEntireHand()
            {
                using (TryCalculateEntireHandPerfMarker.Auto())
                {
                    hand = GetTrackedHand();

                    if (!hand.isTracked)
                    {
                        // No articulated hand device available this frame.
                        FullQueryValid = false;
                        AlreadyFullQueried = true;
                        return;
                    }

                    // Null checks against Unity objects can be expensive, especially when you do
                    // it 52 times per frame (26 hand joints across 2 hands). Instead, we manage
                    // the playspace transformation internally for hand joints.
                    // Joints are relative to the camera floor offset object.
                    Transform origin = PlayspaceUtilities.XROrigin.CameraFloorOffsetObject.transform;
                    if (origin == null)
                    {
                        return;
                    }

                    FullQueryValid = true;

                    for (int i = 0; i < (int)TrackedHandJoint.TotalJoints; i++)
                    {
                        FullQueryValid &= TryUpdateJoint(i, hand.GetJoint(TrackedHandJointIndexToXRHandJointID[i]), origin);
                    }

                    // Mark this hand as having been fully queried this frame.
                    // If any joint is queried again this frame, we'll reuse the
                    // information to avoid extra work.
                    AlreadyFullQueried = true;
                }
            }

            private static readonly ProfilerMarker TryUpdateJointPerfMarker =
                new ProfilerMarker("[MRTK] UnityHandContainer.TryUpdateJoint");

            /// <summary>
            /// Given a destination jointIndex, apply the pose and radius to the correct struct
            /// in the HandJoints collection.
            /// </summary>
            private bool TryUpdateJoint(int jointIndex, XRHandJoint xrHandJoint, Transform playspaceTransform)
            {
                using (TryUpdateJointPerfMarker.Auto())
                {
                    // Not obtaining a pose is a failure state...
                    if (!xrHandJoint.TryGetPose(out Pose pose))
                    {
                        return false;
                    }

                    // ...but not obtaining a radius has a built-in fallback.
                    if (!xrHandJoint.TryGetRadius(out float radius))
                    {
                        radius = HandsUtils.DefaultHandJointRadius;
                    }

                    HandJoints[jointIndex] = new HandJointPose(
                        playspaceTransform.TransformPoint(pose.position),
                        playspaceTransform.rotation * pose.rotation,
                        radius);

                    return true;
                }
            }

            private static readonly ProfilerMarker GetTrackedHandPerfMarker =
                new ProfilerMarker("[MRTK] UnityHandContainer.GetTrackedHand");

            /// <summary>
            /// Obtains a reference to the actual XRHand object representing the tracked hand
            /// functionality present on HandNode.
            /// </summary>
            private XRHand GetTrackedHand()
            {
                using (GetTrackedHandPerfMarker.Auto())
                {
                    if (xrHandSubsystem == null || !xrHandSubsystem.running)
                    {
                        xrHandSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<XRHandSubsystem>();

                        if (xrHandSubsystem == null)
                        {
                            // No hand subsystem running this frame.
                            return default;
                        }
                    }

                    XRHand hand = HandNode == XRNode.LeftHand ? xrHandSubsystem.leftHand : xrHandSubsystem.rightHand;
                    return hand;
                }
            }

            /// <summary>
            /// Cached mapping of <see cref="MixedReality.Toolkit.TrackedHandJoint"/> index to <see cref="UnityEngine.XR.Hands.XRHandJointID"/>
            /// to ease calculation each frame, since the mapping doesn't change.
            /// </summary>
            private static readonly XRHandJointID[] TrackedHandJointIndexToXRHandJointID = new XRHandJointID[]
            {
                XRHandJointID.Palm,
                XRHandJointID.Wrist,

                XRHandJointID.ThumbMetacarpal,
                XRHandJointID.ThumbProximal,
                XRHandJointID.ThumbDistal,
                XRHandJointID.ThumbTip,

                XRHandJointID.IndexMetacarpal,
                XRHandJointID.IndexProximal,
                XRHandJointID.IndexIntermediate,
                XRHandJointID.IndexDistal,
                XRHandJointID.IndexTip,

                XRHandJointID.MiddleMetacarpal,
                XRHandJointID.MiddleProximal,
                XRHandJointID.MiddleIntermediate,
                XRHandJointID.MiddleDistal,
                XRHandJointID.MiddleTip,

                XRHandJointID.RingMetacarpal,
                XRHandJointID.RingProximal,
                XRHandJointID.RingIntermediate,
                XRHandJointID.RingDistal,
                XRHandJointID.RingTip,

                XRHandJointID.LittleMetacarpal,
                XRHandJointID.LittleProximal,
                XRHandJointID.LittleIntermediate,
                XRHandJointID.LittleDistal,
                XRHandJointID.LittleTip,
            };
        }
    }
}
