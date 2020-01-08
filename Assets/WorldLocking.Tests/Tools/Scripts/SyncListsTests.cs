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

            /// Initial state is:
            ///   current == [1..7]
            ///   existing == [2..6]
            /// Expected is to add 1 and 7 to existing.
            SyncLists.Sync<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                (item, res) => item.id.CompareTo(res.id),
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Update,
                x => { Debug.LogError("Not expecting to be deleting here, only adding."); }
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            /// Lists are the same, except one has been removed from current.
            /// Expect a single matching resource removed from existing.
            SyncLists.CompareToResource<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>> comparisonById = (item, res) => item.id.CompareTo(res.id);
            SyncLists.Sync<SyncLists.IdPair<AnchorId, AnchorDummy>, SyncLists.IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                comparisonById,
                x => { Debug.LogError("Not expecting to be creating resources here, only deleting."); return null; },
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

        private struct PositionDummy
        {
            public struct TransformDummy
            {
                public Vector3 position;
            }
            public TransformDummy transform;

            public PositionDummy(Vector3 pos)
            {
                transform.position = pos;
            }

            public static SyncLists.IdPair<AnchorId, PositionDummy> MakePair(int id, float x, float y)
            {
                SyncLists.IdPair<AnchorId, PositionDummy> ret = new SyncLists.IdPair<AnchorId, PositionDummy>();
                ret.id = (AnchorId)id;
                ret.target = new PositionDummy(new Vector3(x, y, 0));
                return ret;
            }
        }

        private struct PositionDummyPair
        {
            public Vector3 frozen;
            public Vector3 spongy;
        }

        private class DisplacementTestCreator
        {
            public DisplacementTestCreator()
            {
            }

            public SyncLists.IdPair<AnchorId, PositionDummyPair> CreateDisplacement(AnchorId id, PositionDummy frozen, PositionDummy spongy)
            {
                PositionDummyPair newLine;
                newLine.frozen = frozen.transform.position;
                newLine.spongy = spongy.transform.position;

                return new SyncLists.IdPair<AnchorId, PositionDummyPair>()
                {
                    id = id,
                    target = newLine
                };
            }

            public void DestroyDisplacement(SyncLists.IdPair<AnchorId, PositionDummyPair> target)
            {
                
            }

            public bool ShouldConnect(
                SyncLists.IdPair<AnchorId, PositionDummy> frozen,
                SyncLists.IdPair<AnchorId, PositionDummy> spongy)
            {
                if (frozen.id != spongy.id)
                {
                    return false;
                }
                float MinDistanceSquared = 0.01f; // one centimeter.
                float distanceSq = (frozen.target.transform.position - spongy.target.transform.position).sqrMagnitude;
                if (distanceSq < MinDistanceSquared)
                {
                    return false;
                }
                return true;
            }
        }

        private void RunDisplacementTestCopy(
            DisplacementTestCreator displacementCreator,
            List<SyncLists.IdPair<AnchorId, PositionDummy>> frozenResources,
            List<SyncLists.IdPair<AnchorId, PositionDummy>> spongyResources,
            List<SyncLists.IdPair<AnchorId, PositionDummyPair>> displacementResources)
        {
            /// Following is exact copy from AnchorGraphVisual for testing. Not ideal, but expedient.
            int iFrozen = 0;
            int iDisplace = 0;
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                while (iFrozen < frozenResources.Count && frozenResources[iFrozen].id < spongyResources[iSpongy].id)
                {
                    iFrozen++;
                }
                /// If we've reached the end of the frozen resources, we're finished creating.
                if (iFrozen >= frozenResources.Count)
                {
                    break;
                }
                if (displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]))
                {
                    AnchorId id = frozenResources[iFrozen].id;
                    Debug.Assert(id == spongyResources[iSpongy].id);
                    while (iDisplace < displacementResources.Count && displacementResources[iDisplace].id < id)
                    {
                        displacementCreator.DestroyDisplacement(displacementResources[iDisplace]);
                        displacementResources.RemoveAt(iDisplace);
                    }
                    Debug.Assert(iDisplace <= displacementResources.Count);
                    Debug.Assert(iDisplace == displacementResources.Count || displacementResources[iDisplace].id >= id);
                    if (iDisplace == displacementResources.Count || displacementResources[iDisplace].id > id)
                    {
                        displacementResources.Insert(iDisplace,
                            displacementCreator.CreateDisplacement(
                                id,
                                frozenResources[iFrozen].target,
                                spongyResources[iSpongy].target));
                    }
                    ++iDisplace;
                }
            }
            // Finished creating. Now destroy any displacements further in the list, as they no longer have matching
            // frozen/spongy pairs.
            Debug.Assert(iDisplace <= displacementResources.Count);
            int displacementCount = iDisplace;
            while (iDisplace < displacementResources.Count)
            {
                displacementCreator.DestroyDisplacement(displacementResources[iDisplace++]);
            }
            displacementResources.RemoveRange(displacementCount, displacementResources.Count - displacementCount);
            Debug.Assert(displacementResources.Count == displacementCount);
            // End exact copy
        }

        [Test]
        public void ThreeListTest()
        {
            List<SyncLists.IdPair<AnchorId, PositionDummy>> frozenResources = new List<SyncLists.IdPair<AnchorId, PositionDummy>>();
            List<SyncLists.IdPair<AnchorId, PositionDummy>> spongyResources = new List<SyncLists.IdPair<AnchorId, PositionDummy>>();
            List<SyncLists.IdPair<AnchorId, PositionDummyPair>> displacementResources 
                = new List<SyncLists.IdPair<AnchorId, PositionDummyPair>>();
            List<int> intersection = new List<int>();

            /// spongy resources anchorids are generally a subset of frozen ids. First test checks that scenario.
            /// frozen = [1..8], spongy is [2,4,6], expect displacements for [2,4,6]
            SyncLists.IdPair<AnchorId, PositionDummy> ape = new SyncLists.IdPair<AnchorId, PositionDummy>();
            for(int i = 1; i < 9; ++i)
            {
                frozenResources.Add(PositionDummy.MakePair(i, i, 0));
            }
            for(int i = 2; i < 9; i += 2)
            {
                spongyResources.Add(PositionDummy.MakePair(i, i, 1));
            }
            for (int i = 2; i < 9; i += 2)
            {
                intersection.Add(i);
            }
            var displacementCreator = new DisplacementTestCreator();

            RunDisplacementTestCopy(displacementCreator, frozenResources, spongyResources, displacementResources);

            CheckDisplacements(displacementCreator, frozenResources, spongyResources, displacementResources);

            spongyResources.Clear();
            spongyResources.Add(PositionDummy.MakePair(4, 4, 1));

            RunDisplacementTestCopy(displacementCreator, frozenResources, spongyResources, displacementResources);

            CheckDisplacements(displacementCreator, frozenResources, spongyResources, displacementResources);

            spongyResources.Clear();
            for(int i = 2; i < 9; i += 2)
            {
                spongyResources.Add(PositionDummy.MakePair(i, i, 1));
            }

            RunDisplacementTestCopy(displacementCreator, frozenResources, spongyResources, displacementResources);

            CheckDisplacements(displacementCreator, frozenResources, spongyResources, displacementResources);

            frozenResources.Clear();
            frozenResources.Add(PositionDummy.MakePair(4, 4, 0));

            RunDisplacementTestCopy(displacementCreator, frozenResources, spongyResources, displacementResources);

            CheckDisplacements(displacementCreator, frozenResources, spongyResources, displacementResources);

        }

        private void CheckSorted<T>(List<T> list, System.Comparison<T> comp)
        {
            for (int i = 1; i < list.Count; ++i)
            {
                Assert.IsTrue(comp(list[i - 1], list[i]) < 0, "List expected to be sorted isn't");
            }
        }

        private static int FindInSortedList<S, T>(S key, List<SyncLists.IdPair<S, T>> list, IComparer<SyncLists.IdPair<S, T>> comparer)
        {
            SyncLists.IdPair<S, T> item = new SyncLists.IdPair<S, T>() { id = key };
            int idx = list.BinarySearch(item, comparer);
            return idx;
        }

        private static int FindInSortedList<S, T>(S key, List<SyncLists.IdPair<S, T>> list, System.Comparison<S> comparison)
        {
            var comparer = Comparer<SyncLists.IdPair<S, T>>.Create((lhs, rhs) => comparison(lhs.id, rhs.id));
            return FindInSortedList(key, list, comparer);
        }

        private void CheckDisplacements(
            DisplacementTestCreator displacementCreator,
            List<SyncLists.IdPair<AnchorId, PositionDummy>> frozenResources,
            List<SyncLists.IdPair<AnchorId, PositionDummy>> spongyResources,
            List<SyncLists.IdPair<AnchorId, PositionDummyPair>> displacementResources
            )
        {

            // Check lists are sorted
            CheckSorted(frozenResources, (x,y) => x.id.CompareTo(y.id));
            CheckSorted(spongyResources, (x, y) => x.id.CompareTo(y.id));
            CheckSorted(displacementResources, (x, y) => x.id.CompareTo(y.id));

            // Check that each displacement is a correct combination of a frozen and a spongy.
            foreach (var disp in displacementResources)
            {
                AnchorId id = disp.id;

                int iFrozen = FindInSortedList(id, frozenResources, (x, y) => x.CompareTo(y));
                Assert.IsTrue(iFrozen >= 0 && iFrozen < frozenResources.Count, $"Not found or index invalid frozen {iFrozen}");
                int iSpongy = FindInSortedList(id, spongyResources, (x, y) => x.CompareTo(y));
                Assert.IsTrue(iSpongy >= 0 && iSpongy < spongyResources.Count, $"Not found or index invalid spongy {iSpongy}");

                Assert.IsTrue(disp.target.frozen == frozenResources[iFrozen].target.transform.position);
                Assert.IsTrue(disp.target.spongy == spongyResources[iSpongy].target.transform.position);

                Assert.IsTrue(displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]));
            }

            // Check that every spongy that has a matching frozen is in displacements.
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                AnchorId id = spongyResources[iSpongy].id;

                int iFrozen = FindInSortedList(id, frozenResources, (x, y) => x.CompareTo(y));
                if (iFrozen >= 0 && displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]))
                {
                    int iDisp = FindInSortedList(id, displacementResources, (x, y) => x.CompareTo(y));
                    Assert.IsTrue(iDisp >= 0, "Frozen/Spongy match not represented in displacements");
                }
            }

            // Check that every frozen that has a matching spongy is in displacements.
            for (int iFrozen = 0; iFrozen< frozenResources.Count; ++iFrozen)
            {
                AnchorId id = frozenResources[iFrozen].id;

                int iSpongy = FindInSortedList(id, spongyResources, (x, y) => x.CompareTo(y));
                if (iSpongy >= 0 && displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]))
                {
                    int iDisp = FindInSortedList(id, displacementResources, (x, y) => x.CompareTo(y));
                    Assert.IsTrue(iDisp >= 0, "Frozen/Spongy match not represented in displacements");
                }
            }
        }

    }
}