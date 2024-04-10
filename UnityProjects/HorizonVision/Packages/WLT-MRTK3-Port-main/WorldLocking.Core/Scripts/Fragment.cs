// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{

    /// <summary>
    /// Fragment class is a container for attachment points in the same WorldLocking Fragment.
    /// It manages their update and adjustment, including merging in the attachment points from
    /// another fragment.
    /// </summary>
    public class Fragment
    {
        private readonly FragmentId fragmentId;

        private readonly List<AttachmentPoint> attachmentList = new List<AttachmentPoint>();

        private AdjustStateDelegate updateStateAllAttachments;

        public FragmentId FragmentId { get { return fragmentId; } }

        public AttachmentPointStateType State { get; private set; }

        public Fragment(FragmentId fragmentId)
        {
            this.fragmentId = fragmentId;
        }

        /// <summary>
        /// Add an existing attachment point to this fragment.
        /// </summary>
        /// <remarks>
        /// The attachment point might currently belong to another fragment, if
        /// it is being moved from the other to this.
        /// Since this is only used internally, it operates directly on an AttachmentPoint
        /// rather than an interface to avoid an unnecessary downcast.
        /// </remarks>
        /// <param name="attachPoint"></param>
        public void AddAttachmentPoint(AttachmentPoint attachPoint)
        {
            if (attachPoint != null)
            {
                if (attachPoint.StateHandler != null)
                {
                    updateStateAllAttachments += attachPoint.StateHandler;
                }
                attachPoint.HandleStateChange(State);
                attachmentList.Add(attachPoint);
            }
        }

        /// <summary>
        /// Notify system attachment point is no longer needed. See <see cref="IAttachmentPointManager.ReleaseAttachmentPoint"/>
        /// </summary>
        /// <param name="attachmentPoint"></param>
        public void ReleaseAttachmentPoint(IAttachmentPoint attachmentPoint)
        {
            AttachmentPoint attachPoint = attachmentPoint as AttachmentPoint;
            if (attachPoint != null)
            {
                if (attachPoint.StateHandler != null)
                {
                    updateStateAllAttachments -= attachPoint.StateHandler;
                }
                attachPoint.HandleStateChange(AttachmentPointStateType.Released);
                attachmentList.Remove(attachPoint);
            }
            else
            {
                Debug.LogError("On release, IAttachmentPoint isn't AttachmentPoint");
            }
        }

        /// <summary>
        /// Release all resources for this fragment.
        /// </summary>
        public void ReleaseAll()
        {
            updateStateAllAttachments = null;
            attachmentList.Clear();
        }

        /// <summary>
        /// Absorb the contents of another fragment, emptying it.
        /// </summary>
        /// <param name="other">The fragment to lose all its contents to this.</param>
        public void AbsorbOtherFragment(Fragment other)
        {
            Debug.Assert(other != this, $"Trying to merge to and from the same fragment {FragmentId}");
            int otherCount = other.attachmentList.Count;
            for (int i = 0; i < otherCount; ++i)
            {
                AttachmentPoint att = other.attachmentList[i];
                att.Set(FragmentId, att.CachedPosition, att.AnchorId, att.LocationFromAnchor);
                if (att.StateHandler != null)
                {
                    updateStateAllAttachments += att.StateHandler;
                }
                att.HandleStateChange(State);
                attachmentList.Add(att);
            }
            other.ReleaseAll();
        }

        /// <summary>
        /// Absorb the contents of another fragment, emptying it, and applying an adjustment transform.
        /// </summary>
        /// <param name="other">The fragment to lose all its contents to this.</param>
        /// <param name="adjustment">Pose adjustment to apply to contents of other on transition.</param>
        public void AbsorbOtherFragment(Fragment other, Pose adjustment)
        {
            Debug.Assert(other != this, $"Trying to merge to and from the same fragment {FragmentId}");
            int otherCount = other.attachmentList.Count;
            for (int i = 0; i < otherCount; ++i)
            {
                AttachmentPoint att = other.attachmentList[i];
                att.Set(FragmentId, att.CachedPosition, att.AnchorId, att.LocationFromAnchor);
                att.HandlePoseAdjustment(adjustment);
                if (att.StateHandler != null)
                {
                    updateStateAllAttachments += att.StateHandler;
                }
                att.HandleStateChange(State);
                attachmentList.Add(att);
            }
            other.ReleaseAll();
        }

        /// <summary>
        /// Set the state of the contents of this fragment.
        /// </summary>
        /// <param name="attachmentState">New state</param>
        public void UpdateState(AttachmentPointStateType attachmentState)
        {
            if (State != attachmentState)
            {
                State = attachmentState;
                updateStateAllAttachments?.Invoke(attachmentState);
            }
        }

        /// <summary>
        /// Run through all attachment points, get their adjustments from the plugin and apply them.
        /// </summary>
        /// <remarks>
        /// This must be called between plugin.Refreeze() and plugin.RefreezeFinish().
        /// </remarks>
        public void AdjustAll(IPlugin plugin)
        {
            int count = attachmentList.Count;
            for (int i = 0; i < count; ++i)
            {
                AttachmentPoint attach = attachmentList[i];

                AnchorId newAnchorId;
                Vector3 newLocationFromAnchor;
                Pose adjustment;
                if (plugin.ComputeAttachmentPointAdjustment(attach.AnchorId, attach.LocationFromAnchor,
                    out newAnchorId, out newLocationFromAnchor, out adjustment))
                {
                    attach.Set(FragmentId, attach.CachedPosition, newAnchorId, newLocationFromAnchor);

                    attach.HandlePoseAdjustment(adjustment);
                }
                else
                {
                    Debug.LogWarning($"No adjustment during refreeze for {attach.AnchorId.FormatStr()}");
                }

            }
        }
    }
}

