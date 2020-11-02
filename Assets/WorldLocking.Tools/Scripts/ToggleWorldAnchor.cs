// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_WSA
using UnityEngine.XR.WSA;
#endif // UNITY_WSA

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    public class ToggleWorldAnchor : MonoBehaviour
    {
        protected IAttachmentPoint AttachmentPoint { get; private set; }

#if UNITY_WSA
        private WorldAnchor worldAnchor = null;
#endif // UNITY_WSA

        private bool frozenPoseIsSpongy = false;
        private Pose frozenPose = Pose.identity;

        [SerializeField]
        [Tooltip("Always use WorldAnchor to world lock, whether Frozen World is active or not.")]
        private bool alwaysLock = false;
        /// <summary>
        /// Always use WorldAnchor to world lock, whether Frozen World is active or not.
        /// </summary>
        public bool AlwaysLock { get { return alwaysLock; } set { alwaysLock = value; } }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(WorldLockingManager.GetInstance() != null, "Unexpected null WorldLockingManager");
            // dummy use of variables to silence unused variable warning in non-WSA build.
            if (frozenPoseIsSpongy)
            {
                frozenPose = Pose.identity;
            }
        }

        private void OnEnable()
        {
#if UNITY_WSA
            /// Setup world anchor helper.
            CreateWorldAnchorHelper();
#endif // UNITY_WSA
        }

        private void OnDisable()
        {
#if UNITY_WSA
            /// Tear down world anchor helper.
            DestroyWorldAnchorHelper();
#endif // UNITY_WSA
        }

#if UNITY_WSA
        // Update is called once per frame
        void Update()
        {
            if (AlwaysLock || !WorldLockingManager.GetInstance().Enabled)
            {
                Pose spongyPose = worldAnchor.transform.GetGlobalPose();
                frozenPose = WorldLockingManager.GetInstance().FrozenFromSpongy.Multiply(spongyPose);
                transform.SetGlobalPose(frozenPose);
            }
            else
            {
                CheckFrozenPose();
                // Set pose to frozen space pose
                transform.SetGlobalPose(frozenPose);
            }
        }

        private void CheckFrozenPose()
        {
            if (frozenPoseIsSpongy)
            {
                Pose spongyPose = worldAnchor.transform.GetGlobalPose();
                Debug.Assert(WorldLockingManager.GetInstance().Enabled, "Should wait until WorldLockingManager is active to check the frozen pose.");
                frozenPose = WorldLockingManager.GetInstance().FrozenFromSpongy.Multiply(spongyPose);
                frozenPoseIsSpongy = false;
            }
        }

        private void CreateWorldAnchorHelper()
        {
            GameObject goHelper = new GameObject(name + "WorldAnchorHelper");
            goHelper.transform.SetParent(transform);
            goHelper.hideFlags = HideFlags.HideAndDontSave;
            frozenPose = transform.GetGlobalPose();
            Pose spongyPose = WorldLockingManager.GetInstance().SpongyFromFrozen.Multiply(frozenPose);
            if (!WorldLockingManager.GetInstance().Enabled)
            {
                frozenPoseIsSpongy = true;
            }
            goHelper.transform.SetGlobalPose(spongyPose);
            worldAnchor = goHelper.AddComponent<WorldAnchor>();

            AttachmentPoint = WorldLockingManager.GetInstance().AttachmentPointManager.CreateAttachmentPoint(gameObject.transform.position, null,
                HandleAdjustLocation,   // Handle adjustments to position
                null  // Handle connectedness of fragment
                );
            AttachmentPoint.Name = string.Format($"{gameObject.name}=>{AttachmentPoint.Name}");
        }

        /// Adjust frozen space pose upon refit operations.
        private void HandleAdjustLocation(Pose adjustment)
        {
            frozenPose = adjustment.Multiply(frozenPose);
        }

        private void DestroyWorldAnchorHelper()
        {
            if (worldAnchor != null)
            {
                Destroy(worldAnchor.gameObject);
                worldAnchor = null;
            }
            if (AttachmentPoint != null)
            {
                WorldLockingManager.GetInstance().AttachmentPointManager.ReleaseAttachmentPoint(AttachmentPoint);
                AttachmentPoint = null;
            }
        }
#endif // UNITY_WSA

    }
}