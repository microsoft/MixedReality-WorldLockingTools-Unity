// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;


namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Make the <see cref="Microsoft.MixedReality.WorldLocking.Core.SpacePin"/> manually manipulable, using MRTK controls.
    /// </summary>
    public class SpacePinManipulation : SpacePin
    {
        #region Inspector fields

        [SerializeField]
        [Tooltip("Proxy renderable to show axis alignment during manipulations.")]
        private GameObject prefab_FeelerRay = null;

        /// <summary>
        /// Proxy renderable to show axis alignment during manipulations.
        /// </summary>
        public GameObject Prefab_FeelerRay { get { return prefab_FeelerRay; } set { prefab_FeelerRay = value; } }
        #endregion Inspector fields

        #region Internal fields

        /// <summary>
        /// Utility helper for setting up MRTK manipulation controls.
        /// </summary>
        PinManipulator pinManipulator;

        #endregion Internal fields

        #region Unity methods

        /// <summary>
        /// Start(), and set up MRTK manipulation controls.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            pinManipulator = new PinManipulator(transform, Prefab_FeelerRay, OnFinishManipulation);
            pinManipulator.UserOriented = true;
            pinManipulator.Startup();
        }

        /// <summary>
        /// Give the manipulation controls an update pulse. 
        /// </summary>
        private void Update()
        {
            pinManipulator.Update();
        }

        /// <summary>
        /// Shutdown the manipulation controls.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            pinManipulator.Shutdown();
        }

        #endregion Unity methods

        #region Manipulation callback
        /// <summary>
        /// Callback for when the user has finished positioning the target.
        /// </summary>
        private void OnFinishManipulation()
        {
            SetFrozenPose(ExtractModelPose());
        }

        #endregion Manipulation callback
    }
}