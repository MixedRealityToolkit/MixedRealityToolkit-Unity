// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using MixedReality.Toolkit.Subsystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MixedReality.Toolkit.Input
{
    /// <summary>
    /// Hand visualizer that uses a rigged mesh/armature to render high-quality hand meshes.
    /// Not recommended for AR platforms like HoloLens, both for performance and design reasons.
    /// </summary>
    /// <remarks>
    /// For augmented reality platforms such as HoloLens, we recommend not using any hand visualizations,
    /// as the conflict between the user's real hand and the slightly delayed holographic visualization
    /// can be more distracting than it's worth. However, for opaque platforms, this is a great solution.
    /// </remarks>
    [AddComponentMenu("MRTK/Input/Visualizers/Rigged Hand Mesh Visualizer")]
    public class RiggedHandMeshVisualizer : HandMeshVisualizer
    {
        [SerializeField]
        [Tooltip("The transform of the wrist joint.")]
        private Transform wrist;

        [SerializeField]
        [Tooltip("Renderer of the hand mesh")]
        private SkinnedMeshRenderer handRenderer = null;

        [SerializeField]
        [Tooltip("The primary visualizer. Rigged hand will not render if the primary is rendering.")]
        private HandMeshVisualizer primaryMeshVisualizer = null;

        /// <inheritdoc/>
        protected override Renderer HandRenderer => handRenderer;

        // Automatically calculated over time, based on the accumulated error
        // between the user's actual joint locations and the armature's bones/joints.
        private float handScale = 1.0f;

        // Caching local references
        private HandsAggregatorSubsystem handsSubsystem;

        // The actual, physical, rigged joints that drive the skinned mesh.
        // Otherwise referred to as "armature". Must be in OpenXR order.
        private readonly Transform[] riggedVisualJointsArray = new Transform[(int)TrackedHandJoint.TotalJoints];

        // The substring used to determine the "leaf joint"
        // at the end of a finger, which is discarded.
        private const string endJointName = "end";

        /// <summary>
        /// A Unity event function that is called when an enabled script instance is being loaded.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            if (handRenderer == null)
            {
                handRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (handRenderer == null)
                {
                    Debug.LogWarning("RiggedHandMeshVisualizer couldn't find your rigged mesh renderer! " +
                                     "You should set it manually.");
                }
            }

            if (wrist == null)
            {
                // "Armature" is the default name that Blender assigns
                // to the root of an armature/rig. Also happens to be the wrist joint!
                wrist = transform.Find("Armature");

                if (wrist == null)
                {
                    Debug.LogWarning("RiggedHandMeshVisualizer couldn't find the wrist joint on your hand mesh. " +
                                     "You should set it manually!");

                    // Abort initialization as we don't even have a wrist joint to go off of.
                    return;
                }
            }

            // Start the depth-first-traversal at the wrist index.
            int index = (int)TrackedHandJoint.Wrist;

            // This performs a depth-first-traversal of the armature. Ensure
            // the provided armature's bones/joints are in OpenXR order.
            foreach (Transform child in wrist.GetComponentsInChildren<Transform>())
            {
                // The "leaf joints" are excluded.
                if (child.name.Contains(endJointName)) { continue; }

                riggedVisualJointsArray[index++] = child;
            }
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();

            handsSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>();

            if (handsSubsystem == null)
            {
                StartCoroutine(EnableWhenSubsystemAvailable());
            }
        }

        /// <summary>
        /// Coroutine to wait until subsystem becomes available.
        /// </summary>
        private IEnumerator EnableWhenSubsystemAvailable()
        {
            yield return new WaitUntil(() => XRSubsystemHelpers.GetFirstRunningSubsystem<HandsAggregatorSubsystem>() != null);
            OnEnable();
        }

        /// <summary>
        /// A Unity event function that is called every frame, if this object is enabled.
        /// </summary>
        protected void Update()
        {
            // Query all joints in the hand.
            if (!ShouldRenderHand() ||
                !handsSubsystem.TryGetEntireHand(HandNode, out IReadOnlyList<HandJointPose> joints))
            {
                // Hide the hand and abort if we shouldn't be
                // showing the hand, for whatever reason.
                // (Missing joint data, no subsystem, additive
                // display, etc!)
                handRenderer.enabled = false;
                return;
            }

            handRenderer.enabled = true;

            // We'll accumulate joint error as we iterate over each joint
            // and compare it to the user's actual joint data.
            float error = 0.0f;

            for (int i = 0; i < joints.Count; i++)
            {
                HandJointPose jointPose = joints[i];

                // The actual, physical, rigged joint on the armature.
                // This actually corresponds to the "base" of the bone;
                // as an example, riggedVisualJointsArray[IndexMetacarpal] actually
                // corresponds to a transform that is located at the wrist joint,
                // but points towards the metacarpal joint location.
                // This discrepancy is because OpenXR uses joint locations/rotations,
                // whereas armatures/Unity/Blender use *bones*.
                Transform jointTransform = riggedVisualJointsArray[i];

                if (jointTransform != null)
                {
                    switch ((TrackedHandJoint)i)
                    {
                        case TrackedHandJoint.Palm:
                            // Don't track the palm. The hand mesh shouldn't have a "palm bone".
                            break;
                        case TrackedHandJoint.Wrist:
                            // Set the wrist directly from the joint data.
                            jointTransform.position = jointPose.Position;
                            jointTransform.rotation = jointPose.Rotation;
                            break;
                        case TrackedHandJoint.ThumbTip:
                        case TrackedHandJoint.IndexTip:
                        case TrackedHandJoint.MiddleTip:
                        case TrackedHandJoint.RingTip:
                        case TrackedHandJoint.LittleTip:
                            // The tip bone uses the joint rotation directly.
                            jointTransform.rotation = joints[i - 1].Rotation;
                            // Compute and accumulate the error between the hand mesh and the user's joint data.
                            error += JointError(jointTransform.position, joints[i - 1].Position, jointTransform.forward);
                            break;
                        case TrackedHandJoint.ThumbMetacarpal:
                        case TrackedHandJoint.IndexMetacarpal:
                        case TrackedHandJoint.MiddleMetacarpal:
                        case TrackedHandJoint.RingMetacarpal:
                        case TrackedHandJoint.LittleMetacarpal:
                            // Special case metacarpals, because Wrist is not always i-1.
                            // This is the same "simple IK" as the default case, but with special index logic.
                            jointTransform.rotation = Quaternion.LookRotation(jointPose.Position - joints[(int)TrackedHandJoint.Wrist].Position, jointPose.Up);
                            break;
                        default:
                            // For all other bones, do a simple "IK" from the rigged joint to the joint data's position.
                            jointTransform.rotation = Quaternion.LookRotation(jointPose.Position - jointTransform.position, joints[i - 1].Up);
                            break;
                    }
                }
            }

            // Compute and apply the adjusted scale of the hand.
            // Over time, we'll grow or shrink the rigged hand
            // to more accurately fit the actual size of the
            // user's hand.

            // How quickly the hand will grow or shrink
            // to fit the user's hand size.
            const float errorGainFactor = 0.1f;

            // Reasonable minimum and maximum for how much
            // the hand mesh is allowed to stretch to fit the user.
            const float minScale = 0.8f;
            const float maxScale = 1.1f;

            // Apply.
            handScale += -error * errorGainFactor;
            handScale = Mathf.Clamp(handScale, minScale, maxScale);
            transform.localScale = new Vector3(HandNode == XRNode.LeftHand ? -handScale : handScale, handScale, handScale);

            // Update the hand material based on selectedness value
            UpdateHandMaterial();
        }

        // Computes the error between the rig's joint position and
        // the user's joint position along the finger vector.
        private float JointError(Vector3 armatureJointPosition, Vector3 userJointPosition, Vector3 fingerVector)
        {
            // The computed error between the rigged mesh's joints and the user's joints
            // is essentially the distance between the mesh and user joints, projected
            // along the forward axis of the finger itself; i.e., the "length error" of the finger.
            return Vector3.Dot((armatureJointPosition - userJointPosition), fingerVector);
        }

        protected override bool ShouldRenderHand()
        {
            // If we're missing anything, don't render the hand.
            // Also don't render if the preferred visualizer is rendering.
            return handsSubsystem != null
                && wrist != null
                && handRenderer != null
                && (primaryMeshVisualizer == null || !primaryMeshVisualizer.IsRendering)
                && base.ShouldRenderHand();
        }
    }
}
