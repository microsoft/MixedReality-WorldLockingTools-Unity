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

            public static void Update(SyncLists.IdPair<AnchorId, AnchorDummy> source, SyncLists.IdPair<AnchorId, AnchorVisTest> target)
            {
                Assert.AreEqual(source.id, target.id);
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

            SyncLists.Sync<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                (item, res) => item.id.CompareTo(res.id),
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Update,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            SyncLists.CompareToResource<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>> comparisonById = (item, res) => item.id.CompareTo(res.id);
            SyncLists.Sync<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                comparisonById,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Update,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(0);
            current.RemoveAt(current.Count - 1);

            SyncLists.Sync(
                current,
                existing,
                comparisonById, // reused from above
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Update,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.Clear();
            SyncLists.Sync(
                current,
                existing,
                comparisonById,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Update,
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

            public static SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest> Create(AnchorEdge source)
            {
                return new AnchorEdgeVisTestPair(source);
            }

            public static void Update(AnchorEdge source, SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest> target)
            {
                Assert.AreEqual(source, target.id);
            }

            public static void Destroy(SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest> target)
            {

            }
        }



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
        private class EdgeComparer : Comparer<AnchorEdge>
        {
            public override int Compare(AnchorEdge x, AnchorEdge y)
            {
                return CompareEdges(x, y);
            }
        };

        private AnchorEdge RegularizeEdge(AnchorEdge edge)
        {
            if (edge.anchorId2 < edge.anchorId1)
            {
                var id = edge.anchorId2;
                edge.anchorId2 = edge.anchorId1;
                edge.anchorId1 = id;
            }
            return edge;
        }

        [Test]
        public void EdgeListSyncTest()
        {
            UnityEngine.Debug.Log("Enter Sync Edge");

            List<SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>> existing = new List<SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>>();

            List<AnchorEdge> current = new List<AnchorEdge>();
            for (int i = 1; i < 7; ++i)
            {
                current.Add(
                    RegularizeEdge(new AnchorEdge()
                    {
                        anchorId1 = (AnchorId)i,
                        anchorId2 = (AnchorId)(i + 1)
                    }
                ));
            }
            current.Sort(CompareEdges);

            SyncLists.Sync<AnchorEdge, SyncLists.IdPair<AnchorEdge, AnchorEdgeVisTest>>(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id), 
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            SyncLists.Sync(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.Add(
                RegularizeEdge(new AnchorEdge()
                {
                    anchorId1 = (AnchorId)100,
                    anchorId2 = (AnchorId)(100 + 1)
                }
            ));
            current.RemoveAt(current.Count / 2);

            SyncLists.Sync(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.Clear();

            SyncLists.Sync(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

        }

        private void CheckSynced<IdType, ResourceType>(List<SyncLists.IdPair<IdType, ResourceType>> existing, List<IdType> current)
        {
            Assert.AreEqual(existing.Count, current.Count);
            for (int i = 0; i < existing.Count; ++i)
            {
                Assert.AreEqual(existing[i].id, current[i]);
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
