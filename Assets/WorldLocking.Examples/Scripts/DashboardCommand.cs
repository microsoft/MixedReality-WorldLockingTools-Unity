// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// The DashboardCommand class provides a proxy layer between interactive elements (e.g. buttons)
    /// and the actions that need to be performed (e.g. WorldLockingManager perform a merge).
    /// </summary>
    public class DashboardCommand : MonoBehaviour
    {
        /// <summary>
        /// Use current instance of WorldLockingManager.
        /// </summary>
        private WorldLockingManager worldLockingManager { get { return WorldLockingManager.GetInstance(); } }

        /// <summary>
        /// The text manager keeps text display fields up to date, including showing and hiding them as desired.
        /// This may be null if no debug display is required.
        /// </summary>
        [SerializeField]
        [Tooltip("Text manager controlling display of diagnostics.")]
        private StatusToText textManager = null;

        /// <summary>
        /// The anchor visualizer provides a visualization of all existing anchors, including maintenance
        /// such as hiding and showing the anchors.
        /// </summary>
        [SerializeField]
        [Tooltip("Manager controlling display of anchor visualizations.")]
        private AnchorGraphVisual anchorVisualizer = null;

        /// <summary>
        /// Optional component to control spatial mapping visualization.
        /// </summary>
        [SerializeField]
        [Tooltip("Spatial Mapping component.")]
        private FrozenSpatialMapping spatialMapping = null;

        [SerializeField]
        [Tooltip("The root of the GUI to be shown/hidden")]
        private Transform guiRoot = null;

        public void ToggleGui()
        {
            if (guiRoot != null)
            {
                guiRoot.gameObject.SetActive(!guiRoot.gameObject.activeSelf);
            }
        }

        public void ToggleManager()
        {
            var settings = worldLockingManager.Settings;
            settings.Enabled = !settings.Enabled;
            worldLockingManager.Settings = settings;
        }

        /// <summary>
        /// Toggle anchor visualization.
        /// </summary>
        public bool AnchorVisualization
        {
            get
            {
                if (anchorVisualizer != null)
                {
                    return anchorVisualizer.enabled;
                }
                return false;
            }
            set
            {
                if (anchorVisualizer != null)
                {
                    anchorVisualizer.enabled = value;
                }
            }
        }

        /// <summary>
        /// Toggle info display.
        /// </summary>
        public bool InfoEnabled
        {
            get
            {
                if (textManager!= null)
                {
                    return textManager.InfoEnabled;
                }
                return false;
            }
            set
            {
                if (textManager != null)
                {
                    textManager.InfoEnabled = value;
                }
            }
        }

        /// <summary>
        /// Toggle Metrics display
        /// </summary>
        public bool MetricsEnabled
        {
            get
            {
                if (textManager != null)
                {
                    return textManager.MetricsEnabled;
                }
                return false;
            }
            set
            {
                if (textManager != null)
                {
                    textManager.MetricsEnabled = value;
                }
            }
        }

        /// <summary>
        /// Toggle status display
        /// </summary>
        public bool StatusEnabled
        {
            get
            {
                if (textManager != null)
                {
                    return textManager.ErrorStatusEnabled;
                }
                return false;
            }
            set
            {
                if (textManager != null)
                {
                    textManager.ErrorStatusEnabled = value;
                }
            }
        }

        /// <summary>
        /// Toggle state display
        /// </summary>
        public bool StateEnabled
        {
            get
            {
                if (textManager != null)
                {
                    return textManager.StateIndicatorEnabled;
                }
                return false;
            }
            set
            {
                if (textManager != null)
                {
                    textManager.StateIndicatorEnabled = value;
                }
            }
        }

        /// <summary>
        /// Toggle whether the spatial mapping mesh is displayed.
        /// </summary>
        public bool SpatialMapDisplayEnabled
        {
            get
            {
                if (spatialMapping != null)
                {
                    return spatialMapping.Display;
                }
                return false;
            }
            set
            {
                if (spatialMapping != null)
                {
                    spatialMapping.Display = value;
                }
            }
        }

        /// <summary>
        /// Return whether there is an available frozen spatial mapping setup and attached.
        /// </summary>
        public bool HasSpatialMap
        {
            get
            {
                return spatialMapping != null;
            }
        }

        /// <summary>
        /// Whether the WorldLockingManager is actively stabilizing space or being bypassed.
        /// </summary>
        public bool ManagerEnabled
        {
            get
            {
                return worldLockingManager.Enabled;
            }
            set
            {
                var config = worldLockingManager.Settings;
                config.Enabled = value;
                worldLockingManager.Settings = config;
            }
        }
        /// <summary>
        /// Toggle automatic saving of state for later restore.
        /// </summary>
        public bool AutoSave
        {
            get
            {
                return worldLockingManager.AutoSave;
            }
            set
            {
                var config = worldLockingManager.Settings;
                config.AutoSave = value;
                worldLockingManager.Settings = config;
            }
        }

        /// <summary>
        /// Toggle automatic merging whenever indicated by underlying system.
        /// </summary>
        public bool AutoMerge
        {
            get
            {
                return worldLockingManager.AutoMerge;
            }
            set
            {
                var config = worldLockingManager.Settings;
                config.AutoMerge = value;
                worldLockingManager.Settings = config;
            }
        }

        /// <summary>
        /// Toggle automatic refreezing whenever indicated by underlying system.
        /// </summary>
        public bool AutoRefreeze
        {
            get
            {
                return worldLockingManager.AutoRefreeze;
            }
            set
            {
                var config = worldLockingManager.Settings;
                config.AutoRefreeze = value;
                worldLockingManager.Settings = config;
            }
        }

        /// <summary>
        /// Perform a refreeze
        /// </summary>
        public void Refreeze()
        {
            worldLockingManager.FragmentManager.Refreeze();
        }

        /// <summary>
        /// Perform a merge.
        /// </summary>
        public void Merge()
        {
            worldLockingManager.FragmentManager.Merge();
        }

        /// <summary>
        /// Save current frozen world state.
        /// </summary>
        public void Save()
        {
            worldLockingManager.Save();
        }

        /// <summary>
        /// Load the last frozen world state, overwriting current state.
        /// </summary>
        public void Load()
        {
            worldLockingManager.Load();
        }

        /// <summary>
        /// Reset the frozen world state to a starting condition.
        /// </summary>
        public void Reset()
        {
            worldLockingManager.Reset();
        }

    }
}