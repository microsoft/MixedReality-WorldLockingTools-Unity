// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Core.Tests
{
    public class AlignmentManagerTest
    {
        private struct PinData
        {
            public string name;
            public Pose virtualPose;
            public Pose lockedPose;
        }

        // A Test behaves as an ordinary method
        [Test]
        public void AlignmentManagerTestSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        private TestLoadHelpers loadHelper = new TestLoadHelpers();

        [SetUp]
        public void AlignmentManagerTestSetup()
        {
            Assert.IsTrue(loadHelper.Setup());
        }

        [TearDown]
        
        public void AlignmentManagerTestTearDown()
        {
            loadHelper.TearDown();
        }

        private PinData[] pinData =
            {
                new PinData()
                {
                    name = "pin0",
                    virtualPose = new Pose(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity),
                    lockedPose =  new Pose(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity)
                },
                new PinData()
                {
                    name = "pin1",
                    virtualPose = new Pose(new Vector3(1.0f, 0.0f, 0.0f), Quaternion.identity),
                    lockedPose =  new Pose(new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity)
                },
                new PinData()
                {
                    name = "pin2",

                    virtualPose = new Pose(new Vector3(1.0f, 0.0f, 1.0f), Quaternion.identity),
                    lockedPose = new Pose(new Vector3(2.0f, 0.0f, 2.0f), Quaternion.identity)
                },
                new PinData()
                {
                    name = "pin3",

                    virtualPose = new Pose(new Vector3(0.0f, 0.0f, 1.0f), Quaternion.identity),
                    lockedPose = new Pose(new Vector3(0.0f, 0.0f, 2.0f), Quaternion.identity)
                }
            };

        private IEnumerator CheckSinglePin(IAlignmentManager alignMgr, int pinIdx)
        {
            alignMgr.ClearAlignmentAnchors();
            var id0 = alignMgr.AddAlignmentAnchor(pinData[pinIdx].name, pinData[pinIdx].virtualPose, pinData[pinIdx].lockedPose);
            alignMgr.SendAlignmentAnchors();

            yield return null;

            CheckAlignment(alignMgr, pinData[pinIdx].virtualPose.position, pinData[pinIdx].lockedPose.position);

            CheckAlignment(alignMgr, pinData[pinIdx].virtualPose.position + new Vector3(1.0f, 0, 0), pinData[pinIdx].lockedPose.position + new Vector3(1.0f, 0, 0));

            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            yield return null;
        }

        private IEnumerator CheckDualPins(IAlignmentManager alignMgr, int pinIdx0, int pinIdx1)
        {
            alignMgr.AddAlignmentAnchor(pinData[pinIdx0].name, pinData[pinIdx0].virtualPose, pinData[pinIdx0].lockedPose);
            alignMgr.AddAlignmentAnchor(pinData[pinIdx1].name, pinData[pinIdx1].virtualPose, pinData[pinIdx1].lockedPose);
            alignMgr.SendAlignmentAnchors();

            yield return null;

            CheckAlignment(alignMgr, pinData[pinIdx0].virtualPose.position, pinData[pinIdx0].lockedPose.position);

            CheckAlignment(alignMgr, pinData[pinIdx1].virtualPose.position, pinData[pinIdx1].lockedPose.position);

            CheckAlignment(alignMgr,
                (pinData[pinIdx0].virtualPose.position + pinData[pinIdx1].virtualPose.position) * 0.5f,
                (pinData[pinIdx0].lockedPose.position + pinData[pinIdx1].lockedPose.position) * 0.5f);

            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            yield return null;
        }

        private IEnumerator CheckAllPins(IAlignmentManager alignMgr)
        {
            for (int i = 0; i < pinData.Length; ++i)
            {
                alignMgr.AddAlignmentAnchor(pinData[i].name, pinData[i].virtualPose, pinData[i].lockedPose);
            }
            alignMgr.SendAlignmentAnchors();

            for (int i = 0; i < pinData.Length; ++i)
            {
                int nextIdx = (i + 1) % pinData.Length;
                CheckAlignment(alignMgr, pinData[i].virtualPose.position, pinData[i].lockedPose.position);
                CheckAlignment(alignMgr,
                    (pinData[i].virtualPose.position + pinData[nextIdx].virtualPose.position) * 0.5f,
                    (pinData[i].lockedPose.position + pinData[nextIdx].virtualPose.position) * 0.5f);
            }

            for (int i = 0; i < pinData.Length - 3; ++i)
            {
                int j = (i + 1) % pinData.Length;
                int k = (j + 1) % pinData.Length;
                CheckAlignment(alignMgr,
                    (pinData[i].virtualPose.position + pinData[j].virtualPose.position + pinData[k].virtualPose.position) / 3.0f,
                    (pinData[i].lockedPose.position + pinData[j].lockedPose.position + pinData[k].virtualPose.position) / 3.0f);
            }

            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            yield return null;
        }

        [UnityTest]
        public IEnumerator AlignmentManagerTestBasic()
        {
            var rig = loadHelper.LoadBasicSceneRig();

            /// This context ensures FW is enabled, but also gives a MonoBehavior to run coroutines off of.
            WorldLockingContext context = loadHelper.LoadComponentOnGameObject<WorldLockingContext>("Prefabs/CoreTestContext_AllEnabled.prefab");
            var alignMgr = WorldLockingManager.GetInstance().AlignmentManager;

            yield return context.StartCoroutine(CheckSinglePin(alignMgr, 0));
            yield return context.StartCoroutine(CheckSinglePin(alignMgr, 1));

            yield return context.StartCoroutine(CheckDualPins(alignMgr, 0, 1));
            yield return context.StartCoroutine(CheckDualPins(alignMgr, 1, 2));
            yield return context.StartCoroutine(CheckDualPins(alignMgr, 0, 2));

            alignMgr.ClearAlignmentAnchors();
            for (int i = 0; i < 2; ++i)
            {
                alignMgr.AddAlignmentAnchor(pinData[i].name, pinData[i].virtualPose, pinData[i].lockedPose);
            }
            alignMgr.SendAlignmentAnchors();

            yield return null;

            CheckAlignment(alignMgr, pinData[0].virtualPose.position, pinData[0].lockedPose.position);

            CheckAlignment(alignMgr, pinData[1].virtualPose.position, pinData[1].lockedPose.position);

            CheckAlignment(alignMgr, 
                (pinData[0].virtualPose.position + pinData[1].virtualPose.position) * 0.5f, 
                (pinData[0].lockedPose.position + pinData[1].lockedPose.position) * 0.5f);

            GameObject.Destroy(context.gameObject);
            GameObject.Destroy(rig);

            yield return null;
        }

        private void CheckAlignment(IAlignmentManager alignMgr, Vector3 virtualPos, Vector3 lockedPos)
        {
            WorldLockingManager mgr = WorldLockingManager.GetInstance();
            alignMgr.ComputePinnedPose(new Pose(lockedPos, Quaternion.identity));
            Pose pinnedFromLocked = alignMgr.PinnedFromLocked;
            Pose frozenFromLocked = mgr.FrozenFromPinned.Multiply(pinnedFromLocked);
            Pose lockedFromFrozen = frozenFromLocked.Inverse();
            Vector3 computedLocked = lockedFromFrozen.Multiply(virtualPos);
            bool areEqual = computedLocked == lockedPos;
            Assert.IsTrue(areEqual);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator AlignmentManagerTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
