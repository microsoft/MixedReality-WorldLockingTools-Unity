// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Core.ResourceMirrorHelper;
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

        private class AnchorIdVisTestCreator 
        {

            public static IdPair<AnchorId, AnchorVisTest> Make(AnchorId id)
            {
                IdPair<AnchorId, AnchorVisTest> ret = new IdPair<AnchorId, AnchorVisTest>()
                {
                    id = id,
                    target = new AnchorVisTest()
                    {
                        id = id
                    }
                };
                return ret;
            }

            public static IdPair<AnchorId, AnchorVisTest> Make(int id)
            {
                return Make((AnchorId)id);
            }

            public static bool Create(IdPair<AnchorId, AnchorDummy> source, out IdPair<AnchorId, AnchorVisTest> resource)
            {
                resource = Make(source.id);
                return true;
            }

            public static void Update(IdPair<AnchorId, AnchorDummy> source, IdPair<AnchorId, AnchorVisTest> target)
            {
                Assert.AreEqual(source.id, target.id);
            }

            public static void Destroy(IdPair<AnchorId, AnchorVisTest> target)
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

            List<IdPair<AnchorId, AnchorVisTest>> existing = new List<IdPair<AnchorId, AnchorVisTest>>();
            for (int i = 2; i < 6; ++i)
            {
                existing.Add(AnchorIdVisTestCreator.Make(i));
            }
            existing.Sort(IdPair<AnchorId, AnchorVisTest>.CompareById);

            List<IdPair<AnchorId, AnchorDummy>> current = new List<IdPair<AnchorId, AnchorDummy>>();
            for (int i = 1; i < 7; ++i)
            {
                current.Add(new IdPair<AnchorId, AnchorDummy>() { id = (AnchorId)i, target = AnchorDummy.Create(i) });
            }
            current.Sort(IdPair<AnchorId, AnchorDummy>.CompareById);

            /// Initial state is:
            ///   current == [1..7]
            ///   existing == [2..6]
            /// Expected is to add 1 and 7 to existing.
            ResourceMirror.Sync<IdPair<AnchorId, AnchorDummy>, IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                (item, res) => item.id.CompareTo(res.id),
                AnchorIdVisTestCreator.Create,
                AnchorIdVisTestCreator.Update,
                x => { Debug.LogError("Not expecting to be deleting here, only adding."); }
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            /// Lists are the same, except one has been removed from current.
            /// Expect a single matching resource removed from existing.
            ResourceMirror.CompareToResource<IdPair<AnchorId, AnchorDummy>, IdPair<AnchorId, AnchorVisTest>> comparisonById = (item, res) => item.id.CompareTo(res.id);
            ResourceMirror.Sync<IdPair<AnchorId, AnchorDummy>, IdPair<AnchorId, AnchorVisTest>>(
                current,
                existing,
                comparisonById,
                (IdPair<AnchorId, AnchorDummy> x, out IdPair<AnchorId, AnchorVisTest> y) => { Debug.LogError("Not expecting to be creating resources here, only deleting."); y = new IdPair<AnchorId, AnchorVisTest>(); return false; },
                AnchorIdVisTestCreator.Update,
                AnchorIdVisTestCreator.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(0);
            current.RemoveAt(current.Count - 1);

            ResourceMirror.Sync(
                current,
                existing,
                comparisonById, // reused from above
                AnchorIdVisTestCreator.Create,
                AnchorIdVisTestCreator.Update,
                AnchorIdVisTestCreator.Destroy
                );
            CheckSynced(existing, current);

            current.Clear();
            ResourceMirror.Sync(
                current,
                existing,
                comparisonById,
                AnchorIdVisTestCreator.Create,
                AnchorIdVisTestCreator.Update,
                AnchorIdVisTestCreator.Destroy
                );
            CheckSynced(existing, current);

        }

        private class AnchorEdgeVisTest
        {
            public AnchorEdge id;

        }

        private class AnchorEdgeVisTestPair 
        {

            public static IdPair<AnchorEdge, AnchorEdgeVisTest> Make(AnchorEdge id)
            {
                return new IdPair<AnchorEdge, AnchorEdgeVisTest>()
                {
                    id = id,
                    target = new AnchorEdgeVisTest()
                    {
                        id = id
                    }
                };
            }

            public static IdPair<AnchorEdge, AnchorEdgeVisTest> Make(int id1, int id2)
            {
                return Make(new AnchorEdge() { anchorId1 = (AnchorId)id1, anchorId2 = (AnchorId)id2 });
            }

            public static bool Create(AnchorEdge source, out IdPair<AnchorEdge, AnchorEdgeVisTest> resource)
            {
                resource = Make(source);
                return true;
            }

            public static void Update(AnchorEdge source, IdPair<AnchorEdge, AnchorEdgeVisTest> target)
            {
                Assert.AreEqual(source, target.id);
            }

            public static void Destroy(IdPair<AnchorEdge, AnchorEdgeVisTest> target)
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

            List<IdPair<AnchorEdge, AnchorEdgeVisTest>> existing = new List<IdPair<AnchorEdge, AnchorEdgeVisTest>>();

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

            ResourceMirror.Sync<AnchorEdge, IdPair<AnchorEdge, AnchorEdgeVisTest>>(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            ResourceMirror.Sync(
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

            ResourceMirror.Sync(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.Clear();

            ResourceMirror.Sync(
                current,
                existing,
                (item, res) => CompareEdges(item, res.id),
                AnchorEdgeVisTestPair.Create,
                AnchorEdgeVisTestPair.Update,
                AnchorEdgeVisTestPair.Destroy
                );
            CheckSynced(existing, current);

        }

        private void CheckSynced<IdType, ResourceType>(List<IdPair<IdType, ResourceType>> existing, List<IdType> current)
        {
            Assert.AreEqual(existing.Count, current.Count);
            for (int i = 0; i < existing.Count; ++i)
            {
                Assert.AreEqual(existing[i].id, current[i]);
            }
        }

        private void CheckSynced<IdType, ResourceType, ItemType>(List<IdPair<IdType, ResourceType>> existing, List<IdPair<IdType, ItemType>> current)
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

            public static IdPair<AnchorId, PositionDummy> MakePair(int id, float x, float y)
            {
                IdPair<AnchorId, PositionDummy> ret = new IdPair<AnchorId, PositionDummy>();
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

            public IdPair<AnchorId, PositionDummyPair> CreateDisplacement(AnchorId id, PositionDummy frozen, PositionDummy spongy)
            {
                PositionDummyPair newLine;
                newLine.frozen = frozen.transform.position;
                newLine.spongy = spongy.transform.position;

                return new IdPair<AnchorId, PositionDummyPair>()
                {
                    id = id,
                    target = newLine
                };
            }

            public void DestroyDisplacement(IdPair<AnchorId, PositionDummyPair> target)
            {
                
            }

            public bool ShouldConnect(
                IdPair<AnchorId, PositionDummy> frozen,
                IdPair<AnchorId, PositionDummy> spongy)
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
            List<IdPair<AnchorId, PositionDummy>> frozenResources,
            List<IdPair<AnchorId, PositionDummy>> spongyResources,
            List<IdPair<AnchorId, PositionDummyPair>> displacementResources)
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
            List<IdPair<AnchorId, PositionDummy>> frozenResources = new List<IdPair<AnchorId, PositionDummy>>();
            List<IdPair<AnchorId, PositionDummy>> spongyResources = new List<IdPair<AnchorId, PositionDummy>>();
            List<IdPair<AnchorId, PositionDummyPair>> displacementResources 
                = new List<IdPair<AnchorId, PositionDummyPair>>();
            List<int> intersection = new List<int>();

            /// spongy resources anchorids are generally a subset of frozen ids. First test checks that scenario.
            /// frozen = [1..8], spongy is [2,4,6], expect displacements for [2,4,6]
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

            frozenResources.Clear();
            spongyResources.Clear();
            for(int i = 1; i < 10; ++i)
            {
                frozenResources.Add(PositionDummy.MakePair(i, i, 0));
                spongyResources.Add(PositionDummy.MakePair(i, i, 0));
            }

            RunDisplacementTestCopy(displacementCreator, frozenResources, spongyResources, displacementResources);

            Assert.IsTrue(displacementResources.Count == 0);
            CheckDisplacements(displacementCreator, frozenResources, spongyResources, displacementResources);

        }

        private void CheckSorted<T>(List<T> list, System.Comparison<T> comp)
        {
            for (int i = 1; i < list.Count; ++i)
            {
                Assert.IsTrue(comp(list[i - 1], list[i]) < 0, "List expected to be sorted isn't");
            }
        }

        private static int FindInSortedList<S, T>(S key, List<IdPair<S, T>> list, IComparer<IdPair<S, T>> comparer)
        {
            IdPair<S, T> item = new IdPair<S, T>() { id = key };
            int idx = list.BinarySearch(item, comparer);
            return idx;
        }

        private static int FindInSortedList<S, T>(S key, List<IdPair<S, T>> list, System.Comparison<S> comparison)
        {
            var comparer = Comparer<IdPair<S, T>>.Create((lhs, rhs) => comparison(lhs.id, rhs.id));
            return FindInSortedList(key, list, comparer);
        }

        private void CheckDisplacements(
            DisplacementTestCreator displacementCreator,
            List<IdPair<AnchorId, PositionDummy>> frozenResources,
            List<IdPair<AnchorId, PositionDummy>> spongyResources,
            List<IdPair<AnchorId, PositionDummyPair>> displacementResources
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

        private class AnchorIdOddOnlyCreator
        {

            public static AnchorVisTest Make(AnchorId id)
            {
                AnchorVisTest ret = new AnchorVisTest()
                {
                    id = id
                };
                return ret;
            }

            public static AnchorVisTest Make(int id)
            {
                return Make((AnchorId)id);
            }

            public bool Create(AnchorId source, out AnchorVisTest resource)
            {
                int idAsInt = (int)source;
                if (0 == (idAsInt & 1))
                {
                    resource = new AnchorVisTest()
                    {
                        id = source
                    };
                    return false;
                }
                resource = Make(source);
                return true;
            }

            public void Update(AnchorId source, AnchorVisTest target)
            {
                Assert.AreEqual(source, target.id);
            }

            public void Destroy(AnchorVisTest target)
            {

            }
        }
            
        [Test]
        public void CreateFailTest()
        {
            List<AnchorId> anchorIds = new List<AnchorId>();
            for (int i = (int)AnchorId.FirstValid; i < 10; ++i)
            {
                anchorIds.Add((AnchorId)i);
            }

            List<AnchorVisTest> resources = new List<AnchorVisTest>();

            AnchorIdOddOnlyCreator creator = new AnchorIdOddOnlyCreator();
            ResourceMirror.Sync(anchorIds, resources,
                (x, y) => x.CompareTo(y.id),
                creator.Create,
                creator.Update,
                creator.Destroy);

            Assert.IsTrue(resources.Count == (anchorIds.Count + 1) / 2);
            for (int i = 0; i < resources.Count; ++i)
            {
                Assert.IsTrue(1 == (((int)resources[i].id) & 1));
            }

            ResourceMirror.Sync(resources, anchorIds,
                /* Compare */ (x, y) => x.id.CompareTo(y),
                /* Create  */ (AnchorVisTest x, out AnchorId y) => { y = x.id; return true; },
                /* Update  */ (x, y) => { },
                /* Destroy */ (x) => { }
                );

            Assert.IsTrue(resources.Count == anchorIds.Count);
            for (int i = 0; i < resources.Count; ++i)
            {
                Assert.AreEqual(anchorIds[i], resources[i].id);
            }
        }
        [Test]
        public void WorldLockingManagerTestEdgeMerge()
        {
            List<AnchorEdge> frozenEdges = new List<AnchorEdge>();

            List<AnchorEdge> spongyEdges = new List<AnchorEdge>();

            int preCount = frozenEdges.Count;
            Assert.AreEqual(preCount, 0);

            // Add 3 new edges.
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)1 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)4, anchorId2 = (AnchorId)3 });

            preCount = frozenEdges.Count;
            AddSpongyEdges(spongyEdges, frozenEdges);
            Assert.AreEqual(preCount + 3, frozenEdges.Count);
            spongyEdges.Clear();

            // No changes, redudant add.
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)1 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)4, anchorId2 = (AnchorId)3 });

            preCount = frozenEdges.Count;
            AddSpongyEdges(spongyEdges, frozenEdges);
            Assert.AreEqual(preCount + 0, frozenEdges.Count);
            spongyEdges.Clear();

            // Add 4 more new ones is random order.
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)5, anchorId2 = (AnchorId)1 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)6, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)3, anchorId2 = (AnchorId)8 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)7, anchorId2 = (AnchorId)3 });

            preCount = frozenEdges.Count;
            AddSpongyEdges(spongyEdges, frozenEdges);
            Assert.AreEqual(preCount + 4, frozenEdges.Count);
            spongyEdges.Clear();

            // No change, redundant adds.
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)1 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)4, anchorId2 = (AnchorId)3 });

            preCount = frozenEdges.Count;
            AddSpongyEdges(spongyEdges, frozenEdges);
            Assert.AreEqual(preCount + 0, frozenEdges.Count);
            spongyEdges.Clear();

            // No change, redundant adds in reverse order.
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)7, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)8, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)6, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)5, anchorId2 = (AnchorId)1 });

            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)4, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)3 });
            spongyEdges.Add(new AnchorEdge() { anchorId1 = (AnchorId)2, anchorId2 = (AnchorId)1 });

            preCount = frozenEdges.Count;
            AddSpongyEdges(spongyEdges, frozenEdges);
            Assert.AreEqual(preCount + 0, frozenEdges.Count);
            spongyEdges.Clear();
        }

        public static void AddSpongyEdges(ICollection<AnchorEdge> spongyEdges, List<AnchorEdge> frozenEdges)
        {
            AnchorEdge[] regularEdges = new AnchorEdge[spongyEdges.Count];
            int idx = 0;
            foreach (var edge in spongyEdges)
            {
                regularEdges[idx++] = RegularEdge(edge.anchorId1, edge.anchorId2);
            }
            System.Comparison<AnchorEdge> alphabeticCompare = (x, y) =>
            {
                int cmp1 = x.anchorId1.CompareTo(y.anchorId1);
                if (cmp1 < 0)
                {
                    return -1;
                }
                if (cmp1 > 0)
                {
                    return 1;
                }
                int cmp2 = x.anchorId2.CompareTo(y.anchorId2);
                return cmp2;
            };
            System.Array.Sort(regularEdges, alphabeticCompare);

            int spongyIdx = 0;
            for (int frozenIdx = 0; frozenIdx < frozenEdges.Count; ++frozenIdx)
            {
                if (spongyIdx >= regularEdges.Length)
                {
                    break;
                }
                int frozenToSpongy = alphabeticCompare(frozenEdges[frozenIdx], regularEdges[spongyIdx]);
                if (frozenToSpongy >= 0)
                {
                    if (frozenToSpongy > 0)
                    {
                        // insert edge here
                        frozenEdges.Insert(frozenIdx, regularEdges[spongyIdx]);
                    }
                    // If existing frozen is greater, we just inserted (above) spongy, so advance. 
                    // If they are equal, we want to skip spongy, so advance.
                    // If existing is lesser, we haven't reached insertion point yet, 
                    // so don't advance spongyIdx (stay out of this conditional branch if frozenToSpongy < 0).
                    ++spongyIdx;
                }
            }
            while (spongyIdx < regularEdges.Length)
            {
                frozenEdges.Add(regularEdges[spongyIdx++]);
            }
        }
        private static AnchorEdge RegularEdge(AnchorId idx1, AnchorId idx2)
        {
            return idx1 < idx2
                ? new AnchorEdge() { anchorId1 = idx1, anchorId2 = idx2 }
                : new AnchorEdge() { anchorId1 = idx2, anchorId2 = idx1 };
        }
    }
}