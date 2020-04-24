using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Script to use an independent AlignmentManager to align a specific subtree, independent of the rest of the scene.
    /// </summary>
    /// <remarks>
    /// The subtree aligned by this will remain world-locked by the independent global world-locking
    /// by the WorldLockingManager.
    /// </remarks>
    public class AlignSubtree : MonoBehaviour
    {
        #region Internal members

        /// <summary>
        /// Owned independent AlignmentManager.
        /// </summary>
        private AlignmentManager alignmentManager = null;

        /// <summary>
        /// The transform to align. Defaults to this.gameObject.
        /// </summary>
        public Transform subTree = null;

        #endregion Internal members

        /// <summary>
        /// Create the alignmentManager if needed.
        /// </summary>
        /// <remarks>
        /// The AlignmentManager, though mostly independent, does have a dependency on the WorldLockingManager.
        /// The WorldLockingManager can't be created until Start/OnEnable (whichever comes first). So even
        /// though the AlignmentManager isn't a Unity derived type, it is still limited on how early it can
        /// be created.
        /// </remarks>
        private void CheckInternalWiring()
        {
            if (alignmentManager == null)
            {
                alignmentManager = new AlignmentManager(WorldLockingManager.GetInstance());
            }
            if (subTree == null)
            {
                subTree = transform;
            }
        }

        /// <summary>
        /// Check that all internal wiring is complete.
        /// </summary>
        void Start()
        {
            CheckInternalWiring();
        }

        /// <summary>
        /// Prompt the AlignmentManager to compute a new alignment pose, then apply it to the target subtree.
        /// </summary>
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

        /// <summary>
        /// Check that all internal wiring is complete. Assign our independent alignmentManager
        /// to all space pins beneath us.
        /// </summary>
        private void OnEnable()
        {
            CheckInternalWiring();
            var spacePins = GetComponentsInChildren<SpacePin>();
            foreach (var pin in spacePins)
            {
                pin.AlignmentManager = alignmentManager;
            }
        }
    }
}
