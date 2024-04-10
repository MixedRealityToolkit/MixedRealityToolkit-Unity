// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Manager for multiple fragments (isolated islands of spatial relevance).
    /// </summary>
    internal class FragmentManager : IFragmentManager, IAttachmentPointManager
    {
        private Dictionary<FragmentId, Fragment> fragments = new Dictionary<FragmentId, Fragment>();

        private struct PendingAttachmentPoint
        {
            public AttachmentPoint target;
            public IAttachmentPoint context;
        };
        private List<PendingAttachmentPoint> pendingAttachments = new List<PendingAttachmentPoint>();

        private readonly IPlugin plugin;

        /// <summary>
        /// Current number of fragments.
        /// </summary>
        public int NumFragments => fragments.Count;

        /// <summary>
        /// Get id of currently active fragment
        /// </summary>
        public FragmentId CurrentFragmentId { get; set; }

        /// <summary>
        /// Multicast delegate called after any refit operation. Includes both Refreeze and Merge operations.
        /// </summary>
        private RefitNotificationDelegate refitNotifications;

        /// <summary>
        /// Register a delegate for refit notifications.
        /// </summary>
        /// <param name="del">The delegate to call.</param>
        public void RegisterForRefitNotifications(RefitNotificationDelegate del)
        {
            refitNotifications += del;
        }

        /// <summary>
        /// Unregister a previously registered delegate for refit notifications.
        /// </summary>
        /// <param name="del">The delegate to unregister.</param>
        public void UnregisterForRefitNotifications(RefitNotificationDelegate del)
        {
            refitNotifications -= del;
        }

        /// <summary>
        /// Return a copy of the current list of fragment ids.
        /// </summary>
        public FragmentId[] FragmentIds
        {
            get
            {
                FragmentId[] arr = new FragmentId[fragments.Count];
                fragments.Keys.CopyTo(arr, 0);
                return arr;
            }
        }

        public FragmentManager(IPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <inheritdoc />
        public void Update(bool autoRefreeze, bool autoMerge)
        {
            CurrentFragmentId = plugin.GetMostSignificantFragmentId();
            Debug.Assert(CurrentFragmentId.IsKnown(), $"F={Time.frameCount} - Update shouldn't be called with no active fragment.");
            if (!CurrentFragmentId.IsKnown())
            {
                return;
            }
            EnsureFragment(CurrentFragmentId);

            if (plugin.Metrics.RefitRefreezeIndicated && autoRefreeze)
            {
                Refreeze();
            }
            else if (plugin.Metrics.RefitMergeIndicated && autoMerge)
            {
                // there are multiple mergeable fragments -- do the merge and show the result
                Merge();
            }

            // There are multiple mergeable fragments with valid adjustments, but we show only the current one
            ApplyActiveCurrentFragment();

            ProcessPendingAttachmentPoints();
        }

        /// <summary>
        /// Get the current state of a given fragment.
        /// </summary>
        /// <param name="id">Identifier of the fragment to query.</param>
        /// <returns>The state</returns>
        public AttachmentPointStateType GetFragmentState(FragmentId id)
        {
            Fragment fragment;
            if (fragments.TryGetValue(id, out fragment))
            {
                return fragment.State;
            }
            return AttachmentPointStateType.Invalid;
        }

        /// <summary>
        /// Create and register a new attachment point.
        /// </summary>
        /// <remarks>
        /// The attachment point itself is a fairly opaque handle. Its effects are propagated to the client via the
        /// two handlers associated with it.
        /// The optional context attachment point provides an optional contextual hint to where in the anchor
        /// graph to bind the new attachment point.
        /// See <see cref="IAttachmentPointManager.CreateAttachmentPoint"/>.
        /// </remarks>
        /// <param name="frozenPosition">The position in the frozen space at which to start the attachment point</param>
        /// <param name="context">The optional context into which to create the attachment point (may be null)</param>
        /// <param name="locationHandler">Delegate to handle WorldLocking system adjustments to position</param>
        /// <param name="stateHandler">Delegate to handle WorldLocking connectivity changes</param>
        /// <returns>The new attachment point interface.</returns>
        public IAttachmentPoint CreateAttachmentPoint(Vector3 frozenPosition, IAttachmentPoint context,
            AdjustLocationDelegate locationHandler, AdjustStateDelegate stateHandler)
        {
            FragmentId fragmentId = GetTargetFragmentId(context);
            AttachmentPoint attachPoint = new AttachmentPoint(locationHandler, stateHandler);
            attachPoint.ObjectPosition = frozenPosition;
            if (fragmentId.IsKnown())
            {
                SetupAttachmentPoint(plugin, attachPoint, context);

                Fragment fragment = EnsureFragment(fragmentId);
                Debug.Assert(fragment != null, "Valid fragmentId but no fragment found");
                fragment.AddAttachmentPoint(attachPoint);
            }
            else
            {
                AddPendingAttachmentPoint(attachPoint, context);
            }
            return attachPoint;
        }

        /// <summary>
        /// Release an attachment point for disposal. The attachment point is no longer valid after this call.
        /// </summary>
        /// <remarks>
        /// In the unlikely circumstance that another attachment point has been spawned from this one
        /// but has not yet been processed (is still in the pending queue),
        /// that relationship is broken on release of this one, and when the other attachment point is
        /// finally processed, it will be as if it was created with a null context.
        /// </remarks>
        /// <param name="attachPointIface">The attachment point to release.</param>
        public void ReleaseAttachmentPoint(IAttachmentPoint attachPointIface)
        {
            AttachmentPoint attachPoint = attachPointIface as AttachmentPoint;
            if (attachPoint != null)
            {
                Fragment fragment = EnsureFragment(attachPoint.FragmentId);
                if (fragment != null)
                {
                    // Fragment handles notification.
                    fragment.ReleaseAttachmentPoint(attachPoint);
                }
                else
                {
                    // Notify of the state change to released.
                    attachPoint.HandleStateChange(AttachmentPointStateType.Released);
                    // The list of pending attachments is expected to be small, and release of an attachment
                    // point while there are pending attachments is expected to be rare. So brute force it here.
                    // If the attachment point being released is a target in the pending list, remove it.
                    // If it is the context of another pending target, set that context to null.
                    // Proceed through the list in reverse order, because context fixes will only be found
                    // later in the list than the original, and once the original is found we are done.
                    int pendingCount = pendingAttachments.Count;
                    for (int i = pendingCount - 1; i >= 0; --i)
                    {
                        if (pendingAttachments[i].context == attachPoint)
                        {
                            var p = pendingAttachments[i];
                            p.context = null;
                            pendingAttachments[i] = p;
                        }
                        else if (pendingAttachments[i].target == attachPoint)
                        {
                            pendingAttachments.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Move (as opposed to Teleport) means that the object is meant to have traversed 
        /// flozen space from its old position to the given new position on some continuous path.
        /// </summary>
        /// <remarks>
        /// Not to be used for automatic (i.e. FrozenWorld Engine instigated) moves.
        /// See <see cref="WorldLockingManager.MoveAttachmentPoint"/>
        /// </remarks>
        /// <param name="attachPoint">Attachment point to move</param>
        /// <param name="newFrozenPosition">The new position in frozen space</param>
        public void MoveAttachmentPoint(IAttachmentPoint attachPointIface, Vector3 newFrozenPosition)
        {
            AttachmentPoint attachPoint = attachPointIface as AttachmentPoint;
            if (attachPoint != null)
            {
                attachPoint.ObjectPosition = newFrozenPosition;

                // If it's not in a valid fragment, it is still pending and will get processed when the system is ready.
                if (attachPoint.FragmentId.IsKnown())
                {
                    float minDistToUpdateSq = 0.5f * 0.5f;

                    float moveDistanceSq = (newFrozenPosition - attachPoint.CachedPosition).sqrMagnitude;
                    if (moveDistanceSq > minDistToUpdateSq)
                    {
                        attachPoint.LocationFromAnchor = plugin.MoveAttachmentPoint(newFrozenPosition, attachPoint.AnchorId, attachPoint.LocationFromAnchor);
                        attachPoint.CachedPosition = newFrozenPosition;
                    }
                    // Else we haven't moved enough to bother doing anything.
                }
            }
        }

        /// <summary>
        /// Teleport (as opposed to Move) means that the object is meant to have disappeared at its old position 
        /// and instantaneously reappeared at its new position in frozen space without traversing the space in between.
        /// </summary>
        /// <remarks>
        /// This is equivalent to releasing the existing attachment point and creating a new one,
        /// except in that the attachment point reference remains valid.
        /// See <see cref="WorldLockingManager.TeleportAttachmentPoint"/>.
        /// </remarks>
        /// <param name="attachPointIface">The attachment point to teleport</param>
        /// <param name="newFrozenPosition">The position to teleport to.</param>
        /// <param name="context">The optional context.</param>
        public void TeleportAttachmentPoint(IAttachmentPoint attachPointIface, Vector3 newFrozenPosition, IAttachmentPoint context)
        {
            AttachmentPoint attachPoint = attachPointIface as AttachmentPoint;
            if (attachPoint != null)
            {
                attachPoint.ObjectPosition = newFrozenPosition;

                // Save the fragment it's currently in, in case it changes here.
                FragmentId oldFragmentId = attachPoint.FragmentId;

                // If it's not in a valid fragment, it is still pending and will get processed when the system is ready.
                if (oldFragmentId.IsKnown())
                {
                    FragmentId newFragmentId = GetTargetFragmentId(context);
                    // If there is a valid current fragment, 
                    if (newFragmentId.IsKnown())
                    {
                        // Fill it in with a new one.
                        SetupAttachmentPoint(plugin, attachPoint, context);

                        if (attachPoint.FragmentId != oldFragmentId)
                        {
                            ChangeAttachmentPointFragment(oldFragmentId, attachPoint);
                        }
                    }
                    else
                    {
                        AddPendingAttachmentPoint(attachPoint, context);
                    }
                }
            }
        }

        /// <summary>
        /// Helper to move an attachment point from one fragment to another.
        /// </summary>
        /// <remarks>
        /// Assumes that the attachment point's FragmentId property has already been set to the new fragment.
        /// </remarks>
        /// <param name="oldFragmentId">Source fragment</param>
        /// <param name="attachPoint">The attachment point</param>
        private void ChangeAttachmentPointFragment(FragmentId oldFragmentId, AttachmentPoint attachPoint)
        {
            Debug.Assert(oldFragmentId != attachPoint.FragmentId, "Moving attachment point from and to same fragment");

            Fragment oldFragment = EnsureFragment(oldFragmentId);
            Fragment newFragment = EnsureFragment(attachPoint.FragmentId);
            Debug.Assert(oldFragment != null, "Valid fragmentId's but null source fragment");
            Debug.Assert(newFragment != null, "Valid fragmentId's but null destination fragment");

            // Add to the new fragment
            newFragment.AddAttachmentPoint(attachPoint);

            // Remove from the old fragment
            oldFragment.ReleaseAttachmentPoint(attachPoint);
        }

        /// <summary>
        /// If conditions have changed to allow finalizing creation of any pending attachment points,
        /// do it now.
        /// </summary>
        private void ProcessPendingAttachmentPoints()
        {
            if (CurrentFragmentId.IsKnown() && pendingAttachments.Count > 0)
            {
                // We have a valid destination fragment. Note that since this queue is in order of submission,
                // if an attachment point depends on a second attachment point for context,
                // that second will be either earlier in the list (because there was no valid current fragment when it was
                // created) or it will have a valid fragment. So by the time we get to the one with a dependency (pending.context != null),
                // its dependency will have a valid fragment id.
                int pendingCount = pendingAttachments.Count;
                for (int i = 0; i < pendingCount; ++i)
                {
                    AttachmentPoint target = pendingAttachments[i].target;
                    Vector3 frozenPosition = pendingAttachments[i].target.ObjectPosition;
                    IAttachmentPoint context = pendingAttachments[i].context;

                    SetupAttachmentPoint(plugin, target, context);

                    FragmentId fragmentId = CurrentFragmentId;
                    if (context != null)
                    {
                        fragmentId = context.FragmentId;
                    }
                    Debug.Assert(fragmentId.IsKnown(), $"FragmentId {fragmentId.FormatStr()} invalid from {(context != null ? "context" : "head")} in processing pending");
                    Fragment fragment = EnsureFragment(fragmentId);
                    Debug.Assert(fragment != null, "Valid fragmentId but no fragment found");
                    fragment.AddAttachmentPoint(target);
                }
                // All pending must now be in a good home fragment, clear the to-do list.
                pendingAttachments.Clear();
            }
        }

        /// <summary>
        /// Establish which fragment a new attachment point should join.
        /// </summary>
        /// <param name="context">Optional spawning attachment point. May be null to "spawn from head".</param>
        /// <returns>Id of fragment to join. May be FragmentId.Invalid if not currently tracking.</returns>
        private FragmentId GetTargetFragmentId(IAttachmentPoint context)
        {
            FragmentId fragmentId = CurrentFragmentId;
            if (context != null)
            {
                fragmentId = context.FragmentId;
            }
            return fragmentId;
        }

        /// <summary>
        /// Add a new attachment point to the pending list to be processed when the system is ready.
        /// </summary>
        /// <param name="attachPoint">Attachment point to process later.</param>
        /// <param name="context">Optional spawning attachment point, may be null.</param>
        private void AddPendingAttachmentPoint(AttachmentPoint attachPoint, IAttachmentPoint context)
        {
            // Flag as being in an invalid state
            attachPoint.HandleStateChange(AttachmentPointStateType.Pending);
            pendingAttachments.Add(
                new PendingAttachmentPoint
                {
                    target = attachPoint,
                    context = context
                }
            );
        }

        /// <summary>
        /// Helper function for setting up the internals of an AttachmentPoint
        /// </summary>
        /// <param name="plugin">The global plugin</param>
        /// <param name="target">The attachment point to setup</param>
        /// <param name="context">The optional context <see cref="CreateAttachmentPoint"/></param>
        public static void SetupAttachmentPoint(IPlugin plugin, AttachmentPoint target, IAttachmentPoint context)
        {
            if (context != null)
            {
                AnchorId anchorId;
                Vector3 locationFromAnchor;
                plugin.CreateAttachmentPointFromSpawner(context.AnchorId, context.LocationFromAnchor, target.ObjectPosition,
                    out anchorId, out locationFromAnchor);
                FragmentId fragmentId = context.FragmentId;
                target.Set(fragmentId, target.ObjectPosition, anchorId, locationFromAnchor);
            }
            else
            {
                FragmentId currentFragmentId = plugin.GetMostSignificantFragmentId();
                AnchorId anchorId;
                Vector3 locationFromAnchor;
                plugin.CreateAttachmentPointFromHead(target.ObjectPosition,
                    out anchorId, out locationFromAnchor);
                FragmentId fragmentId = currentFragmentId;
                target.Set(fragmentId, target.ObjectPosition, anchorId, locationFromAnchor);
            }
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (CurrentFragmentId != FragmentId.Invalid)
            {
                CurrentFragmentId = FragmentId.Invalid;
                ApplyActiveCurrentFragment();
            }
        }

        /// <summary>
        /// Clear all internal state and resources.
        /// </summary>
        public void Reset()
        {
            fragments.Clear();
            CurrentFragmentId = FragmentId.Invalid;
            refitNotifications?.Invoke(FragmentId.Invalid, new FragmentId[0]);
        }

        /// <summary>
        /// Check existence of fragment with indicated id, 
        /// and create it if it doesn't already exist.
        /// </summary>
        /// <param name="id">The fragment id</param>
        private Fragment EnsureFragment(FragmentId id)
        {
            if (!id.IsKnown())
            {
                return null;
            }

            if (!fragments.ContainsKey(id))
            {
                fragments[id] = new Fragment(id);
            }
            return fragments[id];
        }

        /// <summary>
        /// Notify all fragments of their current state.
        /// </summary>
        public void ApplyActiveCurrentFragment()
        {
            foreach (var fragment in fragments.Values)
            {
                AttachmentPointStateType state = fragment.FragmentId == CurrentFragmentId
                    ? AttachmentPointStateType.Normal
                    : AttachmentPointStateType.Unconnected;
                fragment.UpdateState(state);
            }
        }

        private FragmentId[] ExtractFragmentIds(FragmentPose[] source)
        {
            FragmentId[] ids = new FragmentId[source.Length];
            for(int i = 0; i < ids.Length; ++i)
            {
                ids[i] = source[i].fragmentId;
            }
            return ids;
        }

        /// <summary>
        /// Call on the plugin to compute the merge, then apply by
        /// setting transforms and adjusting scene graph.
        /// </summary>
        /// <returns>True for successful merge.</returns>
        public bool Merge()
        {
            FragmentId targetFragmentId;
            FragmentPose[] mergeAdjustments;
            if (!plugin.Merge(out targetFragmentId, out mergeAdjustments))
            {
                return false;
            }
            Debug.Assert(targetFragmentId.IsKnown(), "Received invalid merged fragment id from successful merge");

            Fragment targetFragment = EnsureFragment(targetFragmentId);
            Debug.Assert(targetFragment != null, "Valid fragmentId but null target fragment from Merge");

            int numAbsorbed = mergeAdjustments.Length;
            for (int i = 0; i < numAbsorbed; ++i)
            {
                FragmentId sourceId = mergeAdjustments[i].fragmentId;
                Pose adjustment = mergeAdjustments[i].pose;
                Fragment sourceFragment;
                if (fragments.TryGetValue(sourceId, out sourceFragment))
                {
                    targetFragment.AbsorbOtherFragment(sourceFragment, adjustment);
                    fragments.Remove(sourceId);
                }
                else
                {
                    Debug.LogError($"Try to merge in a non-existent fragment {sourceId.FormatStr()}");
                }
            }
            CurrentFragmentId = targetFragmentId;

            ApplyActiveCurrentFragment();

            refitNotifications?.Invoke(targetFragment.FragmentId, ExtractFragmentIds(mergeAdjustments));

            return true;
        }

        /// <summary>
        /// Invoke a refreeze operation on the plugin, and make all necessary adjustments
        /// in bookeeping after.
        /// </summary>
        /// <returns>True for successful refreeze.</returns>
        public bool Refreeze()
        {
            FragmentId targetFragmentId;
            FragmentId[] absorbedIds;
            if (!plugin.Refreeze(out targetFragmentId, out absorbedIds))
            {
                return false;
            }
            Debug.Assert(targetFragmentId.IsKnown(), "Received invalid merged fragment id from successful refreeze");

            Fragment targetFragment = EnsureFragment(targetFragmentId);
            Debug.Assert(targetFragment != null, "Valid fragmentId but no fragment found");

            for (int i = 0; i < absorbedIds.Length; ++i)
            {
                FragmentId sourceId = absorbedIds[i];
                if (sourceId != targetFragmentId)
                {
                    Fragment sourceFragment;
                    if (fragments.TryGetValue(sourceId, out sourceFragment))
                    {
                        targetFragment.AbsorbOtherFragment(sourceFragment);
                        fragments.Remove(sourceId);
                    }
                    else
                    {
                        Debug.LogError($"Try to merge in a non-existent fragment {sourceId.FormatStr()}");
                    }
                }
            }
            CurrentFragmentId = targetFragmentId;

            // now apply individual adjustments to each attachment point.
            targetFragment.AdjustAll(plugin);

            // now that all adjustments have been made, notify the plugin to finish up the operation.
            plugin.RefreezeFinish();

            refitNotifications?.Invoke(targetFragment.FragmentId, absorbedIds);

            return true;
        }
    }
}
