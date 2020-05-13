// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.WorldLocking.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
        #region Inspector fields
        [SerializeField]
        [Tooltip("File name for saving to and loading from. Defaults to gameObject's name. Use forward slash '/' for subfolders.")]
        private string saveFileName = "";

        /// <summary>
        /// File name for saving to and loading from. Defaults to gameObject's name. Use forward slash '/' for subfolders.
        /// </summary>
        public string SaveFileName 
        { 
            get 
            {
                string name = saveFileName;
                if (string.IsNullOrEmpty(name))
                {
                    name = gameObject.name;
                }
                if (Path.GetExtension(name) != "fwb")
                {
                    name = Path.ChangeExtension(name, "fwb");
                }
                return name; 
            } 
            set 
            { 
                saveFileName = value; 
                if (alignmentManager != null)
                {
                    alignmentManager.SaveFileName = saveFileName;
                }
            } 
        }

        [SerializeField]
        [Tooltip("Whether to perform saves automatically and load at startup.")]
        private bool autoSave = false;

        /// <summary>
        /// Whether to perform saves automatically and load at startup.
        /// </summary>
        public bool AutoSave { get { return autoSave; } set { autoSave = value; } }

        /// <summary>
        /// The transform to align. If unset, will align this.transform.
        /// </summary>
        public Transform subTree = null;

        #endregion Inspector fields

        #region Internal members

        /// <summary>
        /// Owned independent AlignmentManager.
        /// </summary>
        private AlignmentManager alignmentManager = null;

        #endregion Internal members

        #region Public APIs

        public bool Save()
        {
            if (alignmentManager != null)
            {
                return alignmentManager.Save();
            }
            return false;
        }

        public bool Load()
        {
            if (alignmentManager != null)
            {
                return alignmentManager.Load();
            }
            return false;
        }
        #endregion Public APIs

        #region Internal AlignmentManager management
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
                alignmentManager.SaveFileName = SaveFileName;
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
            if (AutoSave && !WorldLockingManager.GetInstance().AutoSave)
            {
                Debug.LogError("AutoSaving alignment requires WorldLockingManager.AutoSave to work as expected.");
            }
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
        /// Load state from previous session if available and so configured.
        /// </summary>
        private void OnEnable()
        {
            CheckInternalWiring();
            var spacePins = GetComponentsInChildren<SpacePin>();
            foreach (var pin in spacePins)
            {
                pin.AlignmentManager = alignmentManager;
            }
            if (AutoSave)
            {
                Load();
            }
        }

        /// <summary>
        /// Force a save on the way out.
        /// </summary>
        private void OnDisable()
        {
            if (AutoSave)
            {
                Save();
            }
        }

        #endregion Internal AlignmentManager management
    }
}
