// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Interface for managing fragments. This mostly comprises the bookkeeping of managing <see cref="IAttachmentPoint"/> 
    /// associations, and the intimately related application of refit operations.
    /// </summary>
    public interface IFragmentManager
    {
        /// <summary>
        /// Current number of fragments.
        /// </summary>
        int NumFragments { get; }

        /// <summary>
        /// Get id of currently active fragment
        /// </summary>
        FragmentId CurrentFragmentId { get; }

        /// <summary>
        /// Return a copy of the current list of fragment ids.
        /// </summary>
        FragmentId[] FragmentIds { get; }

        /// <summary>
        /// Get the current state of a given fragment.
        /// </summary>
        /// <param name="id">Identifier of the fragment to query.</param>
        /// <returns>The state</returns>
        AttachmentPointStateType GetFragmentState(FragmentId id);

        /// <summary>
        /// Notify all fragments of their current state.
        /// </summary>
        void ApplyActiveCurrentFragment();

        /// <summary>
        /// Register a delegate for refit notifications.
        /// </summary>
        /// <param name="del">The delegate to call.</param>
        void RegisterForRefitNotifications(RefitNotificationDelegate del);

        /// <summary>
        /// Unregister a previously registered delegate for refit notifications.
        /// </summary>
        /// <param name="del">The delegate to unregister.</param>
        void UnregisterForRefitNotifications(RefitNotificationDelegate del);

        /// <summary>
        /// Perform any pending refit operations and reconcile state accordingly.
        /// </summary>
        /// <param name="autoRefreeze">True to automatically perform a refreeze if indicated by the plugin.</param>
        /// <param name="autoMerge">True to automatically perform a merge if indicated by the plugin.</param>
        void Update(bool autoRefreeze, bool autoMerge);

        /// <summary>
        /// Set all fragments unconnected during a temporary system outage, especially
        /// while tracking is lost.
        /// </summary>
        /// <remarks>
        /// Fragments to resume as they were on next update. Pause may be called multiple 
        /// consecutive frames, as long as the system outage continues, but only Pause or 
        /// Update should be called on a given frame.
        /// </remarks>
        void Pause();

        /// <summary>
        /// Clear all internal state and resources.
        /// </summary>
        void Reset();

        /// <summary>
        /// Call on the plugin to compute the merge, then apply by
        /// setting transforms and adjusting scene graph.
        /// </summary>
        /// <returns>True for successful merge.</returns>
        /// <remarks>
        /// It is unnecessary to manually merge if autoMerge is true with Update()
        /// </remarks>
        bool Merge();

        /// <summary>
        /// Manually invoke a refreeze operation on the plugin, and make all necessary adjustments
        /// in bookkeeping after.
        /// </summary>
        /// <returns>True for successful refreeze.</returns>
        /// <remarks>
        /// It is unnecessary to manually refreeze if autoRefreeze is true with Update()
        /// </remarks>
        bool Refreeze();

    }
}