// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class SyncLists
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
            int iVis = resources.Count - 1;
            int iAnc = currentItems.Count - 1;

            while (iVis >= 0 && iAnc >= 0)
            {
                /// If the existing visuals is greater than the current anchor,
                /// then there is no corresponding current anchor. So delete the visual
                int comparison = compareIds(currentItems[iAnc], resources[iVis]);
                if (comparison > 0)
                {
                    /// delete existingVisuals[iVis].
                    destroyer(resources[iVis]);
                    resources.RemoveAt(iVis);
                    --iVis;
                    /// Remain on iAnc
                }
                /// If the existing visuals is less, then we are missing a visual for the larger current anchors.
                /// Add it now.
                else if (comparison < 0)
                {
                    var item = creator(currentItems[iAnc]);
                    resources.Insert(iVis + 1, item);
                    /// Now ca[ianc] <==> ev[ivis+1]. So move on to ca[ianc-1] / ev[ivis];
                    --iAnc;
                }
                else
                {
                    updater(currentItems[iAnc], resources[iVis]);
                    --iAnc;
                    --iVis;
                }
            }

            // If iVis && iAnc are both less than zero, then we are done.
            // If iVis < 0 but iAnc >= 0, then we need more visuals created, from iAnc on down.
            // If iVis >= 0 but iAnc < 0, then from iVis down needs to be deleted.
            Debug.Assert(iVis < 0 || iAnc < 0);
            while (iAnc >= 0)
            {
                resources.Insert(0, creator(currentItems[iAnc]));
                --iAnc;
            }
            while (iVis >= 0)
            {
                destroyer(resources[iVis]);
                resources.RemoveAt(iVis);
                --iVis;
            }
            Debug.Assert(resources.Count == currentItems.Count);
        }

    }

}