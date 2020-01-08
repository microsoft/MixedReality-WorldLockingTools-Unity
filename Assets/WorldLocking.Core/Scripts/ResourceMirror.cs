// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    namespace ResourceMirrorHelper
    {
        public class IdPair<IdType, T>
        {
            public IdType id;
            public T target;

            public static int CompareById(IdPair<IdType, T> lhs, IdPair<IdType, T> rhs)
            {
                return Comparer<IdType>.Default.Compare(lhs.id, rhs.id);
            }
        };

    };

    public class ResourceMirror
    {
        public delegate ResourceType CreateResource<ItemType, ResourceType>(ItemType item);
        public delegate void UpdateResource<ItemType, ResourceType>(ItemType item, ResourceType resource);
        public delegate void DestroyResource<ResourceType>(ResourceType resource);
        public delegate int CompareToResource<ItemType, ResourceType>(ItemType item, ResourceType resource);

        public static void Sync<ItemType, ResourceType>(
            List<ItemType> currentItems,
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
                    var item = creator(currentItems[iItem]);
                    resources.Insert(iRsrc + 1, item);
                    /// Now ca[iItem] <==> ev[iRsrc+1]. So move on to ca[iItem-1] / ev[iRsrc];
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
                resources.Insert(0, creator(currentItems[iItem]));
                --iItem;
            }
            while (iRsrc >= 0)
            {
                destroyer(resources[iRsrc]);
                resources.RemoveAt(iRsrc);
                --iRsrc;
            }
            Debug.Assert(resources.Count == currentItems.Count);
        }

    }

}