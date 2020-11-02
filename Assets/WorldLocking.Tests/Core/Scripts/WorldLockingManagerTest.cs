// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.IO;

using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Microsoft.MixedReality.WorldLocking.Tests.Core
{
    public class WorldLockingManagerTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void WorldLockingManagerTestSimplePasses()
        {
            // Use the Assert class to test conditions
            var wlMgr = WorldLockingManager.GetInstance();
            Assert.IsNotNull(wlMgr); /// wlMgr is not a Unity object, so this should work.
            UnityEngine.Assertions.Assert.IsNotNull(wlMgr); /// This should work whether or not object overrides == null.
        }

        private TestLoadHelpers loadHelper = new TestLoadHelpers();

        [SetUp]
        public void WorldLockingManagerTestSetup()
        {
            Assert.IsTrue(loadHelper.Setup());

        }

        [TearDown]
        public void WorldLockingManagerTestTearDown()
        {
            loadHelper.TearDown();
        }


        [UnityTest]
        public IEnumerator WorldLockingManagerTestContextSwitch()
        {
            GameObject rig = loadHelper.LoadBasicSceneRig();

            Assert.IsTrue(WorldLockingManager.GetInstance().AutoLoad);
            var context = loadHelper.LoadComponentOnGameObject<WorldLockingContext>("Prefabs/CoreTestContext_AllDisabled.prefab");
            Assert.IsFalse(WorldLockingManager.GetInstance().AutoLoad);
            GameObject.Destroy(context.gameObject);
            GameObject.Destroy(rig);

            yield return null;
        }

        [UnityTest]
        public IEnumerator WorldLockingManagerTestSettingsFromScript()
        {
            GameObject rig = loadHelper.LoadBasicSceneRig();

            var settings = WorldLockingManager.GetInstance().Settings;
            bool wasAutoLoad = settings.AutoLoad;
            settings.AutoLoad = !settings.AutoLoad;
            WorldLockingManager.GetInstance().Settings = settings;
            Assert.AreNotEqual(wasAutoLoad, WorldLockingManager.GetInstance().AutoLoad);

            GameObject.Destroy(rig);

            yield return null;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator WorldLockingManagerTestLinkagePassThrough()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            GameObject rig = loadHelper.LoadBasicSceneRig();

            yield return null;

            Transform cameraParentTest = new GameObject("CameraParentTest").transform;
            Transform adjustmentTest = new GameObject("AdjustmentTest").transform;
            Transform cameraParent = Camera.main.transform.parent;
            UnityEngine.Assertions.Assert.IsNotNull(cameraParent);
            GameObject adjustment = GameObject.Find("Adjustment");
            UnityEngine.Assertions.Assert.IsNotNull(adjustment);

            WorldLockingManager.GetInstance().CameraParent = cameraParentTest;
            WorldLockingManager.GetInstance().AdjustmentFrame = adjustmentTest;

            Assert.AreEqual(cameraParentTest, WorldLockingManager.GetInstance().CameraParent);
            Assert.AreEqual(adjustmentTest, WorldLockingManager.GetInstance().AdjustmentFrame);

            Assert.IsTrue(WorldLockingManager.GetInstance().AutoMerge);
            var noMergeNoLinkage = loadHelper.LoadComponentOnGameObject<WorldLockingContext>("Prefabs/CoreTestContext_NoMergeNoLinkage.prefab");
            yield return null;

            Assert.IsFalse(WorldLockingManager.GetInstance().AutoMerge);
            Assert.AreEqual(cameraParentTest, WorldLockingManager.GetInstance().CameraParent);
            Assert.AreEqual(adjustmentTest, WorldLockingManager.GetInstance().AdjustmentFrame);

            Assert.IsTrue(WorldLockingManager.GetInstance().AutoLoad);
            var allDisabled = loadHelper.LoadComponentOnGameObject<WorldLockingContext>("Prefabs/CoreTestContext_AllDisabled.prefab");
            yield return null;

            Assert.IsFalse(WorldLockingManager.GetInstance().AutoLoad);
            Assert.AreEqual(cameraParent, WorldLockingManager.GetInstance().CameraParent);
            Assert.AreEqual(adjustment.transform, WorldLockingManager.GetInstance().AdjustmentFrame);

            yield return null;
        }

        private static AnchorId MakeAnchorId(int idx)
        {
            return (AnchorId)((int)AnchorId.FirstValid + idx);
        }

        /// <summary>
        /// Construct and check a trivial graph and some trivial anchor movements.
        /// </summary>
        /// <returns>null</returns>
        [UnityTest]
        public IEnumerator WorldLockingManagerTestPlugin()
        {
            WorldLockingManager wlMgr = WorldLockingManager.GetInstance();
            IPlugin plugin = wlMgr.Plugin;
            UnityEngine.Assertions.Assert.IsNotNull(plugin); /// This should work whether or not object overrides == null.

            Pose[] poses =
            {
                new Pose(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity),
                new Pose(new Vector3(3.0f, 0.0f, 0.0f), Quaternion.identity),
                new Pose(new Vector3(3.0f, 0.0f, 3.0f), Quaternion.identity),
                new Pose(new Vector3(0.0f, 0.0f, 3.0f), Quaternion.identity)
            };
            List<AnchorPose> anchorPoses = new List<AnchorPose>();
            for (int i = 0; i < poses.Length; ++i)
            {
                anchorPoses.Add(new AnchorPose() { anchorId = MakeAnchorId(i), pose = poses[i] });
            }
            /// Scoping spongyHead inside where it's used was confusing the debugger. And adjustment.
            Pose spongyHead = Pose.identity;
            Pose adjustment = Pose.identity;
            List<AnchorEdge> anchorEdges = new List<AnchorEdge>();
            for (int i = 0; i < anchorPoses.Count; ++i)
            {
                for (int j = i + 1; j < anchorPoses.Count; ++j)
                {
                    anchorEdges.Add(new AnchorEdge() { anchorId1 = anchorPoses[i].anchorId, anchorId2 = anchorPoses[j].anchorId });
                }
            }

            Pose movement = Pose.identity;
            List<AnchorPose> displacedPoses = new List<AnchorPose>(anchorPoses);

            CheckAlignment(displacedPoses, anchorEdges, movement);

            /// Take a random walk.
            Vector3 randomStep = new Vector3(1.0e-3f, 1.0e-3f, 1.0e-3f);
            int numRandomSteps = 100;
            for (int i = 0; i < numRandomSteps; ++i)
            {
                Pose step = new Pose(RandomVector(-randomStep, randomStep), Quaternion.identity);
                movement = step.Multiply(movement);
                PreMultiplyPoses(displacedPoses, anchorPoses, movement);
                CheckAlignment(displacedPoses, anchorEdges, movement);
            }
            /// Now walk back to start.
            Pose furthest = movement;
            for (int i = 0; i < numRandomSteps; ++i)
            {
                movement.position = Vector3.Lerp(furthest.position, Vector3.zero, (float)i / (float)(numRandomSteps - 1));
                PreMultiplyPoses(displacedPoses, anchorPoses, movement);
                CheckAlignment(displacedPoses, anchorEdges, movement);
            }

            /// Try incremental rotation.
            int numRotSteps = 10;
            Pose rotStep = new Pose(Vector3.zero, Quaternion.AngleAxis(1.0f, new Vector3(0.0f, 1.0f, 0.0f)));
            for (int i = 0; i < numRotSteps; ++i)
            {
                movement = rotStep.Multiply(movement);
                PreMultiplyPoses(displacedPoses, anchorPoses, movement);
                CheckAlignment(displacedPoses, anchorEdges, movement);
            }

            yield return null;
        }

        System.Random rand = new System.Random(666);

        private Vector3 RandomVector(Vector3 lo, Vector3 hi)
        {
            return new Vector3(
                lo.x + (float)rand.NextDouble() * (hi.x - lo.x),
                lo.y + (float)rand.NextDouble() * (hi.y - lo.y),
                lo.z + (float)rand.NextDouble() * (hi.z - lo.z));
        }

        private void CheckAlignment(List<AnchorPose> anchorPoses, List<AnchorEdge> anchorEdges, Pose movement)
        {
            Pose spongyHead;
            IPlugin plugin = WorldLockingManager.GetInstance().Plugin;
            for (int k = 0; k < anchorPoses.Count; ++k)
            {
                spongyHead = anchorPoses[k].pose;
                plugin.ClearSpongyAnchors();
                plugin.Step_Init(spongyHead);
                plugin.AddSpongyAnchors(anchorPoses);
                plugin.SetMostSignificantSpongyAnchorId(anchorPoses[k].anchorId);
                plugin.AddSpongyEdges(anchorEdges);
                plugin.Step_Finish();

                var adjustment = plugin.GetAlignment();
                Assert.IsTrue(adjustment == movement);

            }
        }

        private void PreMultiplyPoses(List<AnchorPose> dstPoses, List<AnchorPose> srcPoses, Pose transform)
        {
            Assert.AreEqual(dstPoses.Count, srcPoses.Count);
            for (int i = 0; i < srcPoses.Count; ++i)
            {
                AnchorPose anchorPose = dstPoses[i];
                anchorPose.pose = transform.Multiply(srcPoses[i].pose);
                dstPoses[i] = anchorPose;
            }
        }

    }
}
