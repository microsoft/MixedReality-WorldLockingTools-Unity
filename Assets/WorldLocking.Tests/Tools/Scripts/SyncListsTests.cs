// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;

namespace Microsoft.MixedReality.WorldLocking.Tests.Tools
{
    public class SyncListsTests
    {
        //#####################################################################################################

        private class AnchorVisTest
        {
            public AnchorId id;

        }

        private class AnchorIdVisTestPair : SyncLists.IdPair<AnchorId, AnchorVisTest>
        {
            
            public AnchorIdVisTestPair(AnchorId id)
            {
                this.id = id;
                this.target = new AnchorVisTest()
                {
                    id = id
                };
            }

            public AnchorIdVisTestPair(int id) : this((AnchorId)id) { }

            public static SyncLists.IdPair<AnchorId, AnchorVisTest> Create(SyncLists.IdPair<AnchorId, AnchorDummy> source)
            {
                return new AnchorIdVisTestPair(source.id);
            }

            public static void Destroy(SyncLists.IdPair<AnchorId, AnchorVisTest> target)
            {

            }
        }

        private struct AnchorDummy
        {
            public AnchorId id;

            public static AnchorDummy Create(AnchorId id)
            {
                return new AnchorDummy() { id = id };
            }

            public static AnchorDummy Create(int id)
            {
                return Create((AnchorId)id);
            }
        }

        [Test]
        public void AnchorListSyncTest()
        {
            UnityEngine.Debug.Log("Enter Sync Test");

            List<SyncLists.IdPair<AnchorId, AnchorVisTest>> existing = new List<SyncLists.IdPair<AnchorId, AnchorVisTest>>();
            for (int i = 2; i < 6; ++i)
            {
                existing.Add(new AnchorIdVisTestPair(i));
            }
            existing.Sort(SyncLists.IdPair<AnchorId, AnchorVisTest>.CompareById);

            List<SyncLists.IdPair<AnchorId, AnchorDummy>> current = new List<SyncLists.IdPair<AnchorId, AnchorDummy>>();
            for (int i = 1; i < 7; ++i)
            {
                current.Add(new SyncLists.IdPair<AnchorId, AnchorDummy>() { id = (AnchorId)i, target = AnchorDummy.Create(i) });
            }
            current.Sort(SyncLists.IdPair<AnchorId, AnchorDummy>.CompareById);

            SyncLists.Sync<AnchorId, AnchorVisTest, SyncLists.IdPair<AnchorId, AnchorDummy>>(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            SyncLists.Sync<AnchorId, AnchorVisTest, SyncLists.IdPair<AnchorId, AnchorDummy>>(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(0);
            current.RemoveAt(current.Count - 1);

            SyncLists.Sync(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.Clear();
            SyncLists.Sync(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

        }

        private class AnchorEdgeVisTest
        {
            public AnchorEdge id;

        }

        private class AnchorEdgeVisTestPair : SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>
        {

            public AnchorEdgeVisTestPair(AnchorEdge id)
            {
                this.id = id;
                this.target = new AnchorEdgeVisTest()
                {
                    id = id
                };
            }

            public AnchorEdgeVisTestPair(int id1, int id2) : this(new AnchorEdge() { anchorId1 = (AnchorId)id1, anchorId2 = (AnchorId)id2 }) { }

            public static SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest> Create(SyncLists.IdItem<AnchorEdge> source)
            {
                return new AnchorEdgeVisTestPair(source.id);
            }

            public static void Destroy(SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest> target)
            {

            }
        }


#if true
        public static int CompareEdges(AnchorEdge lhs, AnchorEdge rhs)
        {
            if (lhs.anchorId1 < rhs.anchorId1)
            {
                return -1;
            }
            if (lhs.anchorId1 > rhs.anchorId1)
            {
                return 1;
            }
            if (lhs.anchorId2 < rhs.anchorId2)
            {
                return -1;
            }
            if (lhs.anchorId2 > rhs.anchorId2)
            {
                return 1;
            }
            return 0;
        }
        public static int CompareEdges(SyncLists.IdItem<AnchorEdge> lhs, SyncLists.IdItem<AnchorEdge> rhs)
        {
            return CompareEdges(lhs.id, rhs.id);
        }

        private class EdgeComparer : Comparer<AnchorEdge>
        {
            public override int Compare(AnchorEdge x, AnchorEdge y)
            {
                return CompareEdges(x, y);
            }
        };

        private void RegularizeEdge(SyncLists.IdItem<AnchorEdge> edge)
        {
            if (edge.id.anchorId2 < edge.id.anchorId1)
            {
                var id = edge.id.anchorId2;
                edge.id.anchorId2 = edge.id.anchorId1;
                edge.id.anchorId1 = id;
            }
        }

        [Test]
        public void EdgeListSyncTest()
        {
            UnityEngine.Debug.Log("Enter Sync Edge");

            List<SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>> existing = new List<SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>>();
            for (int i = 2; i < 6; ++i)
            {
                existing.Add(new AnchorEdgeVisTestPair(i, i+1));
            }
            existing.ForEach(e => RegularizeEdge(e));
            existing.Sort(CompareEdges);

            List<SyncLists.IdItem<AnchorEdge>> current = new List<SyncLists.IdItem<AnchorEdge>>();
            for (int i = 1; i < 7; ++i)
            {
                current.Add(new SyncLists.IdPair<AnchorEdge, AnchorDummy>()
                {
                    id = new AnchorEdge()
                    {
                        anchorId1 = (AnchorId)i,
                        anchorId2 = (AnchorId)(i + 1)
                    },
                    target = AnchorDummy.Create(i)
                });
            }
            current.ForEach(e => RegularizeEdge(e));
            current.Sort(CompareEdges);

            SyncLists.Sync<AnchorEdge, AnchorEdgeVisTest, SyncLists.IdItem<AnchorEdge>>(
                existing,
                current,
                new EdgeComparer(), 
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);
        }
#endif

        private void CheckSynced<IdType, ResourceType>(List<SyncLists.IdPair<IdType, ResourceType>> existing, List<SyncLists.IdItem<IdType>> current)
        {
            Assert.AreEqual(existing.Count, current.Count);
            for (int i = 0; i < existing.Count; ++i)
            {
                Assert.AreEqual(existing[i].id, current[i].id);
            }
        }

        private void CheckSynced<IdType, ResourceType, ItemType>(List<SyncLists.IdPair<IdType, ResourceType>> existing, List<SyncLists.IdPair<IdType, ItemType>> current)
        {
            Assert.AreEqual(existing.Count, current.Count);
            for (int i = 0; i < existing.Count; ++i)
            {
                Assert.AreEqual(existing[i].id, current[i].id);
            }
        }
    }
}
