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
        public void ListSyncTest()
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

            SyncLists.Sync<AnchorId, AnchorVisTest, AnchorDummy>(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(current.Count / 2);

            SyncLists.Sync<AnchorId, AnchorVisTest, AnchorDummy>(
                existing,
                current,
                Comparer<AnchorId>.Default,
                AnchorIdVisTestPair.Create,
                AnchorIdVisTestPair.Destroy
                );
            CheckSynced(existing, current);

            current.RemoveAt(0);
            current.RemoveAt(current.Count - 1);

            SyncLists.Sync<AnchorId, AnchorVisTest, AnchorDummy>(
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

        private void CheckSynced<IdType, VisType, AnchorType>(List<SyncLists.IdPair<IdType, VisType>> existing, List<SyncLists.IdPair<IdType, AnchorType>> current)
        {
            Assert.AreEqual(existing.Count, current.Count);
            for (int i = 0; i < existing.Count; ++i)
            {
                Assert.AreEqual(existing[i].id, current[i].id);
            }
        }
    }
}
