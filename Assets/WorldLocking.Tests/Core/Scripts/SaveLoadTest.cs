// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tests.Core
{
    public class SaveLoadTest
    {
        private TestLoadHelpers loadHelper = new TestLoadHelpers();

        [SetUp]
        public void SaveLoadTestSetup()
        {
            Assert.IsTrue(loadHelper.Setup());

        }

        [TearDown]
        public void SaveLoadTestTearDown()
        {
            var alignMgr = WorldLockingManager.GetInstance().AlignmentManager;
            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            loadHelper.TearDown();
        }
        private struct PinData
        {
            public string name;
            public Pose virtualPose;
            public Pose lockedPose;
            public AnchorId anchorId;
        }

        private PinData[] pinData =
            {
                new PinData()
                {
                    name = "pin0",
                    virtualPose = new Pose(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity),
                    lockedPose =  new Pose(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity),
                    anchorId = AnchorId.Invalid
                },
                new PinData()
                {
                    name = "pin1",
                    virtualPose = new Pose(new Vector3(1.0f, 0.0f, 0.0f), Quaternion.identity),
                    lockedPose =  new Pose(new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity),
                    anchorId = AnchorId.Invalid
                },
                new PinData()
                {
                    name = "pin2",
                    virtualPose = new Pose(new Vector3(1.0f, 0.0f, 1.0f), Quaternion.identity),
                    lockedPose =  new Pose(new Vector3(2.0f, 0.0f, 1.0f), Quaternion.AngleAxis(Mathf.Deg2Rad * 10.0f, new Vector3(0.0f, 1.0f, 0.0f))),
                    anchorId = AnchorId.Invalid
                }
            };

        [UnityTest]
        public IEnumerator SaveLoadTestSaveThenLoad()
        {
            GameObject rig = loadHelper.LoadBasicSceneRig();
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            var settings = wltMgr.Settings;
            settings.AutoLoad = false;
            settings.AutoSave = false;
            wltMgr.Settings = settings;

            IAlignmentManager alignMgr = wltMgr.AlignmentManager;

            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            // Verify alignment is identity
            VerifyAlignmentIdentity(alignMgr, pinData);

            // Add pins
            for (int i = 0; i < pinData.Length; ++i)
            {
                pinData[i].anchorId = alignMgr.AddAlignmentAnchor(pinData[i].name, pinData[i].virtualPose, pinData[i].lockedPose);
            }
            alignMgr.SendAlignmentAnchors();

            yield return null;

            // Verify alignment at pins 
            VerifyAlignment(alignMgr, pinData);

            // Save
            wltMgr.Save();

            yield return null;

            // Verify alignment at pins, saving should be non-destructive.
            VerifyAlignment(alignMgr, pinData);

            // Clear
            alignMgr.ClearAlignmentAnchors();
            alignMgr.SendAlignmentAnchors();

            yield return null;


            // Verify alignment is identity
            VerifyAlignmentIdentity(alignMgr, pinData);

            // Load
            wltMgr.Load();

            yield return null;

            for (int i = 0; i < pinData.Length; ++i)
            {
                pinData[i].anchorId = alignMgr.RestoreAlignmentAnchor(pinData[i].name, pinData[i].virtualPose);
            }
            alignMgr.SendAlignmentAnchors();

            yield return null;

            // Verify alignment at pins, load should have restored them.
            VerifyAlignment(alignMgr, pinData);

            GameObject.Destroy(rig);

            yield return null;
        }

        private void VerifyAlignmentIdentity(IAlignmentManager alignMgr, PinData[] pinData)
        {
            for (int i = 0; i < pinData.Length; ++i)
            {
                alignMgr.ComputePinnedPose(new Pose(pinData[i].lockedPose.position, Quaternion.identity));
                Pose pinnedFromLocked = alignMgr.PinnedFromLocked;
                bool isIdentityPosition = pinnedFromLocked.position == Vector3.zero;
                Assert.IsTrue(isIdentityPosition);
                bool isIdentityRotation = pinnedFromLocked.rotation == Quaternion.identity;
                Assert.IsTrue(isIdentityRotation);
            }
        }

        private void VerifyAlignment(IAlignmentManager alignMgr, PinData[] pinData)
        {
            for (int i = 0; i < pinData.Length; ++i)
            {
                CheckAlignment(alignMgr, pinData[i].virtualPose, pinData[i].lockedPose);
            }
        }

        private void CheckAlignment(IAlignmentManager alignMgr, Pose virtualPose, Pose lockedPose)
        {
            WorldLockingManager mgr = WorldLockingManager.GetInstance();
            alignMgr.ComputePinnedPose(new Pose(lockedPose.position, Quaternion.identity));
            Pose pinnedFromLocked = alignMgr.PinnedFromLocked;
            Pose frozenFromLocked = mgr.FrozenFromPinned.Multiply(pinnedFromLocked);
            Pose lockedFromFrozen = frozenFromLocked.Inverse();
            Pose computedLocked = lockedFromFrozen.Multiply(virtualPose);
            bool areEqualPositions = computedLocked.position == lockedPose.position;
            Assert.IsTrue(areEqualPositions);
            bool areEqualRotatons = computedLocked.rotation == lockedPose.rotation;
            Assert.IsTrue(areEqualRotatons);
        }

        [UnityTest]
        public IEnumerator SaveLoadTestSpacePinOrientable()
        {
            GameObject rig = loadHelper.LoadBasicSceneRig();

            GameObject[] gos = new GameObject[pinData.Length];
            SpacePinOrientable[] spos = new SpacePinOrientable[pinData.Length];

            Quaternion rotThirty = Quaternion.AngleAxis(30.0f, new Vector3(0.0f, 1.0f, 0.0f));

            GameObject orienterGO = new GameObject("Orienter");
            IOrienter orienter = orienterGO.AddComponent<Orienter>();
            for (int i = 0; i < pinData.Length; ++i)
            {
                gos[i] = new GameObject("GOs_" + i.ToString());
                spos[i] = gos[i].AddComponent<SpacePinOrientable>();
                spos[i].Orienter = orienter;
            }
            /// Wait for their Start's to be called.
            yield return null;

            for (int i = 0; i < spos.Length; ++i)
            { 
                spos[i].transform.SetGlobalPose(pinData[i].virtualPose);
                spos[i].ResetModelingPose();
                Vector3 rotPosition = rotThirty * pinData[i].virtualPose.position;
                spos[i].SetFrozenPosition(rotPosition);
            }
            yield return null;

            IAlignmentManager alignMgr = WorldLockingManager.GetInstance().AlignmentManager;
            /// This is an arbitrary position, not actually one of the pinned positions currently.
            alignMgr.ComputePinnedPose(new Pose(pinData[0].lockedPose.position, Quaternion.identity));
            Quaternion rot = Quaternion.Inverse(alignMgr.PinnedFromLocked.rotation);
            bool isThirty = rot == rotThirty;
            Assert.IsTrue(isThirty);

            GameObject.Destroy(rig);

            yield return null;
        }
    }
}