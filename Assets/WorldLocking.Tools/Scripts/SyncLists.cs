// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
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

        public delegate IdPair<IdType, T> CreatePair<IdType, S, T>(IdPair<IdType, S> source);
        public delegate void DestroyPair<IdType, T>(IdPair<IdType, T> item);

        public static void Sync<IdType, VisualType, TargetType>(
            List<IdPair<IdType, VisualType>> existingVisuals,
            List<IdPair<IdType, TargetType>> currentAnchors,
            Comparer<IdType> compareIds,
            CreatePair<IdType, TargetType, VisualType> creator,
            DestroyPair<IdType, VisualType> destroyer)
        {
            int iVis = existingVisuals.Count - 1;
            int iAnc = currentAnchors.Count - 1;

            while (iVis >= 0 && iAnc >= 0)
            {
                /// If the existing visuals is greater than the current anchor,
                /// then there is no corresponding current anchor. So delete the visual
                int comparison = compareIds.Compare(existingVisuals[iVis].id, currentAnchors[iAnc].id);
                if (comparison > 0)
                {
                    /// delete existingVisuals[iVis].
                    destroyer(existingVisuals[iVis]);
                    existingVisuals.RemoveAt(iVis);
                    --iVis;
                    /// Remain on iAnc
                }
                /// If the existing visuals is less, then we are missing a visual for the larger current anchors.
                /// Add it now.
                else if (comparison < 0)
                {
                    var item = creator(currentAnchors[iAnc]);
                    existingVisuals.Insert(iVis + 1, item);
                    /// Now ca[ianc] <==> ev[ivis+1]. So move on to ca[ianc-1] / ev[ivis];
                    --iAnc;
                }
                else
                {
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
                existingVisuals.Insert(0, creator(currentAnchors[iAnc]));
                --iAnc;
            }
            while (iVis >= 0)
            {
                destroyer(existingVisuals[iVis]);
                existingVisuals.RemoveAt(iVis);
                --iVis;
            }
            Debug.Assert(existingVisuals.Count == currentAnchors.Count);
        }

    }

}