// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    namespace ResourceMirrorHelper
    {
        /// <summary>
        /// Helper pair for keeping track of things by ID.
        /// </summary>
        /// <remarks>
        /// The IdType is typically an AnchorId, but any type using the Comparer.Default.Compare is fine.
        /// Note this is independent of the ResourceMirror, and currently only used to internally
        /// for resources identified by anchorId, and to expedite tests.
        /// </remarks>
        /// <typeparam name="IdType">Type of the identifier.</typeparam>
        /// <typeparam name="T">Type of the data associated with the identifier.</typeparam>
        public struct IdPair<IdType, T>
        {
            /// <summary>
            /// Identifier field.
            /// </summary>
            public IdType id;
            /// <summary>
            /// Data associated with identifier.
            /// </summary>
            public T target;

            /// <summary>
            /// Convenience comparison function comparing by identifier (ignoring associated data).
            /// </summary>
            /// <param name="lhs">The left hand side.</param>
            /// <param name="rhs">The right hand side.</param>
            /// <returns>If lhs GT rhs then -1 else if lhs LT rhs then 1 else 0</returns>
            public static int CompareById(IdPair<IdType, T> lhs, IdPair<IdType, T> rhs)
            {
                return Comparer<IdType>.Default.Compare(lhs.id, rhs.id);
            }
        };

    };

    /// <summary>
    /// Class to synchronize a list of resources with associated source data (items).
    /// </summary>
    public class ResourceMirror
    {
        /// <summary>
        /// Callback for creating a new instance of a resource matching a specific item. This
        /// will be called for each item in Sync's currentItems list which doesn't have a matching
        /// resource in Sync's resources list.
        /// </summary>
        /// <typeparam name="ItemType">Type of the source data.</typeparam>
        /// <typeparam name="ResourceType">Type of the resources to be managed.</typeparam>
        /// <param name="item">The source item to create a new resource for.</param>
        /// <param name="resource">out param for created resource.</param>
        /// <returns>Returns true if a resource was created successfully.</returns>
        /// <remarks>
        /// Note that it is not an error to return false, it only means that for any reason
        /// the resource was not created. However, if the resource is not created, then in the 
        /// next call to Sync, it will be noted that the item doesn't have a matching resource
        /// and the create call will be made again. To prevent fruitless and possibly expensive
        /// create calls, the offending item should be removed from the items list passed into Sync.
        /// As noted below, all additions and removals from the items list must happen outside
        /// the Sync call.
        /// </remarks>
        public delegate bool CreateResource<ItemType, ResourceType>(ItemType item, out ResourceType resource);

        /// <summary>
        /// Callback to update existing resources. This will be called for each item and its
        /// associated resource in the Sync's currentItems and resources lists.
        /// </summary>
        /// <remarks>
        /// Only one of create/update/destroy will be called for a given item/resource pair
        /// during a single Sync.
        /// </remarks>
        /// <typeparam name="ItemType">Type of the source data.</typeparam>
        /// <typeparam name="ResourceType">Type of the managed resources.</typeparam>
        /// <param name="item">The source item.</param>
        /// <param name="resource">The associated resource.</param>
        public delegate void UpdateResource<ItemType, ResourceType>(ItemType item, ResourceType resource);

        /// <summary>
        /// Callback to release resources. This will be called for each resource in Sync's
        /// resource list for which there is no corresponding source data in Sync's currentItems.
        /// </summary>
        /// <typeparam name="ResourceType">Type of the resource to destroy.</typeparam>
        /// <param name="resource">The resource instance to destroy.</param>
        public delegate void DestroyResource<ResourceType>(ResourceType resource);

        /// <summary>
        /// Function to compare a source item with a resource. It should return:
        /// -1 if resource is associated with a smaller item than item.
        /// 1 if resource is associated with a larger item than item.
        /// 0 if resource is associated with item.
        /// </summary>
        /// <remarks>
        /// Note that "smaller" and "larger" above must have the identical meaning to
        /// the comparison the lists input into Sync are sorted by.
        /// </remarks>
        /// <typeparam name="ItemType">Type of source data.</typeparam>
        /// <typeparam name="ResourceType">Type of managed resource.</typeparam>
        /// <param name="item">Instance of source data.</param>
        /// <param name="resource">Instance of resource.</param>
        /// <returns></returns>
        public delegate int CompareToResource<ItemType, ResourceType>(ItemType item, ResourceType resource);

        /// <summary>
        /// Given a *sorted* list of source data items (currentItems),
        /// and a *sorted* list of resources:
        ///    For each source item that doesn't have a matching resource, attempt to create a resource.
        ///    For each resource that doesn't have a matching source item, destroy that resource.
        ///    For each source item with a matching resource, update the resource.
        /// </summary>
        /// <remarks>
        /// After this Sync, the list of resources will have exactly one resource for each item
        /// in currentItems, and currentItems and resources will be the same length. 
        /// The exception is if the creator function returns false for any item(s), then those item(s)
        /// will not have matching resources, and resources will be shorter than currentItems.
        /// In any case, resources will remain sorted.
        /// Sync completes in a single pass over the data, so in O(max(currentItems.Count, resources.Count)) time.
        /// </remarks>
        /// <typeparam name="ItemType">Type of source items.</typeparam>
        /// <typeparam name="ResourceType">Type of resources.</typeparam>
        /// <param name="currentItems">List of current source items.</param>
        /// <param name="resources">List of resources to by synced to currentItems.</param>
        /// <param name="compareIds">Function to compare an item with a resource. See above.</param>
        /// <param name="creator">Callback to create a missing resource. See above.</param>
        /// <param name="updater">Callback to update an existing resource. See above.</param>
        /// <param name="destroyer">Callback to destroy a resource which no longer has a matching source item.</param>
        public static void Sync<ItemType, ResourceType>(
            IReadOnlyList<ItemType> currentItems,
            List<ResourceType> resources,
            CompareToResource<ItemType, ResourceType> compareIds,
            CreateResource<ItemType, ResourceType> creator,
            UpdateResource<ItemType, ResourceType> updater,
            DestroyResource<ResourceType> destroyer)
        {
            int iRsrc = resources.Count - 1;
            int iItem = currentItems.Count - 1;

            while (iRsrc >= 0 && iItem >= 0)
            {
                /// If the existing resource is greater than the current item,
                /// then there is no corresponding current item. So delete the resource.
                int comparison = compareIds(currentItems[iItem], resources[iRsrc]);
                if (comparison < 0)
                {
                    /// items id less than resources, means
                    ///    no item for this resource.
                    /// delete the surplus resource.
                    destroyer(resources[iRsrc]);
                    resources.RemoveAt(iRsrc);
                    --iRsrc;
                    /// Remain on iItem
                }
                /// If the existing resource is less, then we are missing a resource for the larger current item.
                /// Add it now.
                else if (comparison > 0)
                {
                    /// items id greater than resources, means
                    ///    for this item, no matching resource.
                    /// create and add.
                    ResourceType resource;
                    if (creator(currentItems[iItem], out resource))
                    {
                        resources.Insert(iRsrc + 1, resource);
                    }
                    /// If successful, now ci[iItem] <==> re[iRsrc+1]. So move on to ci[iItem-1] / re[iRsrc];
                    /// If failed, we've tried ci[iItem] (which failed) and re[] is unchanged.
                    /// So either way move on to ci[iItem-1] / re[iRsrc]
                    --iItem;
                }
                else
                {
                    /// item and resource match, just update.
                    updater(currentItems[iItem], resources[iRsrc]);
                    --iItem;
                    --iRsrc;
                }
            }

            // If iRsrc && iItem are both less than zero, then we are done.
            // If iRsrc < 0 but iItem >= 0, then we need more resources created, from iItem on down.
            // If iRsrc >= 0 but iItem < 0, then from iRsrc down needs to be deleted.
            Debug.Assert(iRsrc < 0 || iItem < 0);
            while (iItem >= 0)
            {
                ResourceType resource;
                if (creator(currentItems[iItem], out resource))
                {
                    resources.Insert(0, resource);
                }
                --iItem;
            }
            while (iRsrc >= 0)
            {
                destroyer(resources[iRsrc]);
                resources.RemoveAt(iRsrc);
                --iRsrc;
            }
            Debug.Assert(resources.Count <= currentItems.Count);
        }

    }

}