using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    public class AlignSubtree : MonoBehaviour
    {
        private AlignmentManager alignmentManager = null;

        public Transform subTree = null;

        private void CheckAlignmentManager()
        {
            if (alignmentManager == null)
            {
                alignmentManager = new AlignmentManager(WorldLockingManager.GetInstance());
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            CheckAlignmentManager();
            if (subTree == null)
            {
                subTree = transform;
            }    
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Assert(alignmentManager != null);

            var wltMgr = WorldLockingManager.GetInstance();
            Debug.Assert(alignmentManager != wltMgr.AlignmentManager);

            Pose lockedHeadPose = wltMgr.LockedFromPlayspace.Multiply(wltMgr.PlayspaceFromSpongy.Multiply(wltMgr.SpongyFromCamera));
            alignmentManager.ComputePinnedPose(lockedHeadPose);
            var pinnedFromLocked = alignmentManager.PinnedFromLocked;
            var lockedFromPinned = pinnedFromLocked.Inverse();

            subTree.SetGlobalPose(lockedFromPinned);
        }

        // mafinc - should this be in Start()?
        private void OnEnable()
        {
            CheckAlignmentManager();
            var spacePins = GetComponentsInChildren<SpacePin>();
            foreach (var pin in spacePins)
            {
                pin.AlignmentManager = alignmentManager;
            }
            // mafinc - Should we try a load here?
        }
    }
}
