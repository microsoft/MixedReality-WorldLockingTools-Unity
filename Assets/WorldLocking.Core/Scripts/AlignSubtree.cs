// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Script to use an independent AlignmentManager to align a specific subtree, independent of the rest of the scene.
    /// </summary>
    /// <remarks>
    /// The subtree aligned by this will remain world-locked by the independent global world-locking
    /// by the WorldLockingManager.
    /// This script illustrates how to create and manage an independent AlignmentManager, and
    /// apply its alignment to a specific subtree within the scene (the Sub Tree).
    /// The global AlignmentManager, owned and managed by the WorldLockingManager, applies its
    /// alignment to the global Unity coordinate space (frozen space). The desire here is to
    /// use the same Space Pin feature to pin parts of a virtual model (subtree) to the physical world,
    /// without affecting global space. To do this requires several steps:
    /// 1. Create a new locally owned AlignmentManager (distinct from the one owned by the WorldLockingManager). See <see cref="CheckInternalWiring"/>.
    /// 2. Point the desired SpacePins to use the locally owned AlignmentManager (they default to use the global one). See <see cref="OnEnable"/>.
    /// 3. Use the local AlignmentManager to compute a correction pose, and apply it to the subtree. See <see cref="Update"/>.
    /// On point 2., there are a number of reasonable ways to harvest which SpacePins should use this local AlignmentManager, the
    /// method used here, invoking GetComponentsInChildren, is just one such way.
    /// </remarks>
    public class AlignSubtree : MonoBehaviour
    {
        #region Inspector fields

        [SerializeField]
        [Tooltip("Collect all SpacePins from this subtree to manage.")]
        private bool collectFromTree = true;

        /// <summary>
        /// Collect all SpacePins from this subtree to manage.
        /// </summary>
        public bool CollectFromTree { get { return collectFromTree; } set { collectFromTree = value; } }

        [SerializeField]
        [Tooltip("Explicit list of Space Pins to manage.")]
        private List<SpacePin> ownedPins = new List<SpacePin>();

        [SerializeField]
        [Tooltip("File name for saving to and loading from. Defaults to gameObject's name. Use forward slash '/' for subfolders.")]
        private string saveFileName = "";

        /// <summary>
        /// File name for saving to and loading from. Defaults to gameObject's name. Use forward slash '/' for subfolders.
        /// </summary>
        /// <remarks>
        /// Any non-existent file and/or containing folders will be created if possible.
        /// </remarks>
        public string SaveFileName
        {
            get
            {
                string name = saveFileName;
                if (string.IsNullOrEmpty(name))
                {
                    name = gameObject.name;
                }
                saveFileName = FixExtension(name, "fwb");
                return saveFileName;
            }
            set
            {
                saveFileName = FixExtension(value, "fwb");
                if (alignmentManager != null)
                {
                    alignmentManager.SaveFileName = saveFileName;
                }
            }
        }

        private static string FixExtension(string name, string ext)
        {
            if (Path.GetExtension(name) != ext)
            {
                name = Path.ChangeExtension(name, ext);
            }
            return name;
        }

        /// <summary>
        /// The transform to align. If unset, will align this.transform. 
        /// </summary>
        /// <remarks>
        /// This transform must be identity at startup, and must not be modified
        /// by anything but this AlignSubtree component.
        /// </remarks>
        public Transform subTree = null;

        #endregion Inspector fields

        #region Internal members

        /// <summary>
        /// Owned independent AlignmentManager.
        /// </summary>
        private AlignmentManager alignmentManager = null;

        /// <summary>
        /// Owned independent AlignmentManager.
        /// </summary>
        public AlignmentManager AlignmentManager => alignmentManager;

        private bool needLoad = false;

        #endregion Internal members

        #region Public APIs

        /// <summary>
        /// Explicit command to save the alignment manager to store.
        /// </summary>
        /// <returns>True on successful save.</returns>
        public bool Save()
        {
            if (alignmentManager != null)
            {
                return alignmentManager.Save();
            }
            return false;
        }

        /// <summary>
        /// Explicit command to load the alignment manager from store.
        /// </summary>
        /// <returns>True on successful load.</returns>
        public bool Load()
        {
            if (alignmentManager != null)
            {
                return alignmentManager.Load();
            }
            return false;
        }

        /// <summary>
        /// Explicitly add a pin to the owned pins list.
        /// </summary>
        /// <param name="pin">THe pin to add.</param>
        /// <returns>True if added, false if it was already there.</returns>
        public bool AddOwnedPin(SpacePin pin)
        {
            if (!ownedPins.Contains(pin))
            {
                ownedPins.Add(pin);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove a specific pin from the owned pins list.
        /// </summary>
        /// <param name="pin">The pin to remove.</param>
        /// <returns>True if removed, else false (probably not found).</returns>
        public bool RemoveOwnedPin(SpacePin pin)
        {
            return ownedPins.Remove(pin);
        }

        /// <summary>
        /// Clear the entire list of owned space pins.
        /// </summary>
        /// <remarks>
        /// This removes all pins in the list, whether added dynamically or added in the inspector.
        /// </remarks>
        public void ClearOwnedPins()
        {
            ownedPins.Clear();
        }

        /// <summary>
        /// This should be called whenever pins are added to the owned list.
        /// </summary>
        /// <remarks>
        /// It's only necessary to call this when adding pins to the owned list dynamically
        /// from script. It is called from OnEnable for all pins added in the inspector or
        /// collected from the scene graph subtree.
        /// </remarks>
        public void ClaimPinOwnership()
        {
            CheckInternalWiring();
            if (CollectFromTree)
            {
                var spacePins = GetComponentsInChildren<SpacePin>();
                foreach (var pin in spacePins)
                {
                    AddOwnedPin(pin);
                }
            }
            foreach (var pin in ownedPins)
            {
                pin.AlignmentManager = alignmentManager;
            }
        }

        /// <summary>
        /// Fired when a new AlignmentManager has been created throughout CheckInternalWiring
        /// </summary>
        public event EventHandler<IAlignmentManager> OnAlignManagerCreated;

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

                OnAlignManagerCreated?.Invoke(this,alignmentManager);
            }
            if (subTree == null)
            {
                subTree = transform;
            }
        }

        /// <summary>
        /// Check that all internal wiring is complete.
        /// </summary>
        private void Start()
        {
            CheckInternalWiring();
            needLoad = WorldLockingManager.GetInstance().AutoLoad;
        }

        /// <summary>
        /// Prompt the AlignmentManager to compute a new alignment pose, then apply it to the target subtree.
        /// </summary>
        private void Update()
        {
            Debug.Assert(alignmentManager != null);

            CheckLoad();

            var wltMgr = WorldLockingManager.GetInstance();
            Debug.Assert(alignmentManager != wltMgr.AlignmentManager);

            Pose lockedHeadPose = wltMgr.LockedFromPlayspace.Multiply(wltMgr.PlayspaceFromSpongy.Multiply(wltMgr.SpongyFromCamera));
            alignmentManager.ComputePinnedPose(lockedHeadPose);
            var pinnedFromLocked = alignmentManager.PinnedFromLocked;
            var lockedFromPinned = pinnedFromLocked.Inverse();

            subTree.SetGlobalPose(lockedFromPinned);
        }

        private void CheckLoad()
        {
            if (needLoad)
            {
                needLoad = false;
                Load();
            }
        }

        /// <summary>
        /// Check that all internal wiring is complete. Assign our independent alignmentManager
        /// to all space pins beneath us.
        /// Load state from previous session if available and so configured.
        /// </summary>
        private void OnEnable()
        {
            ClaimPinOwnership();
        }

        #endregion Internal AlignmentManager management
    }
}
