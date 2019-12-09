// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.Toolkit.UI;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Simple class to manage synchronizing state up to UI elements.
    /// </summary>
    public class DashboardUI : MonoBehaviour
    {
        /// <summary>
        /// Dashboard command dispatcher.
        /// </summary>
        public DashboardCommand dashboardCommand;

        /// <summary>
        /// Button to perform a refreeze, only enabled when a refreeze is available.
        /// </summary>
        public Interactable ButtonRefreeze;
        /// <summary>
        /// Button to perform a merge, only enabled when a refreeze is available.
        /// </summary>
        public Interactable ButtonMerge;
        /// <summary>
        /// Toggle diagnostic display of anchors.
        /// </summary>
        public Interactable CheckBoxShowAnchors;
        /// <summary>
        /// Toggle display of basic information.
        /// </summary>
        public Interactable CheckBoxShowInfo;
        /// <summary>
        /// Toggle display of detailed metrics.
        /// </summary>
        public Interactable CheckBoxShowMetrics;
        /// <summary>
        /// Toggle display of spatial map.
        /// </summary>
        public Interactable CheckBoxShowSpatMap;
        /// <summary>
        /// Toggle Frozen World Manager.
        /// </summary>
        public Interactable CheckBoxManagerEnabled;
        /// <summary>
        /// Toggle automatic merge operation when indicated by engine.
        /// </summary>
        public Interactable CheckBoxAutoMerge;
        /// <summary>
        /// Toggle periodic automatic saves of anchor state.
        /// </summary>
        public Interactable CheckBoxAutoSave;
        /// <summary>
        /// Toggle automatic refreeze operations when indicated by engine.
        /// </summary>
        public Interactable CheckBoxAutoRefreeze;
        /// <summary>
        /// Manual perform save of current anchor state.
        /// </summary>
        public Interactable ButtonSave;
        /// <summary>
        /// Manual perform load of last saved anchor state, overwriting current state.
        /// </summary>
        public Interactable ButtonLoad;

        private void Start()
        {

            //Ignore the collisions between layer 0 (default) and layer 5 (UI)
            Physics.IgnoreLayerCollision(0, 5, true);

            if (dashboardCommand == null)
            {
                throw new System.Exception("DashboardUI missing required DashboardCommand");
            }
            if (WorldLockingManager.GetInstance() == null)
            {
                throw new System.Exception("DashboardUI missing required WorldLockingManager");
            }

            if (!dashboardCommand.HasSpatialMap)
            {
                CheckBoxShowSpatMap.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            var worldLockingManager = WorldLockingManager.GetInstance();
            ButtonRefreeze.IsEnabled = worldLockingManager.RefreezeIndicated;
            ButtonMerge.IsEnabled = worldLockingManager.MergeIndicated;
            ButtonSave.IsEnabled = !dashboardCommand.AutoSave;
            ButtonLoad.IsEnabled = !dashboardCommand.AutoSave;

            CheckBoxShowAnchors.CurrentDimension = dashboardCommand.AnchorVisualization ? 1 : 0;
            CheckBoxShowInfo.CurrentDimension = dashboardCommand.InfoEnabled ? 1 : 0;
            CheckBoxShowMetrics.CurrentDimension = dashboardCommand.MetricsEnabled ? 1 : 0;
            CheckBoxShowSpatMap.CurrentDimension = dashboardCommand.SpatialMapDisplayEnabled ? 1 : 0;
            CheckBoxManagerEnabled.CurrentDimension = dashboardCommand.ManagerEnabled ? 1 : 0;
            CheckBoxAutoMerge.CurrentDimension = dashboardCommand.AutoMerge ? 1 : 0;
            CheckBoxAutoSave.CurrentDimension = dashboardCommand.AutoSave ? 1 : 0;
            CheckBoxAutoRefreeze.CurrentDimension = dashboardCommand.AutoRefreeze ? 1 : 0;
        }
    }
}