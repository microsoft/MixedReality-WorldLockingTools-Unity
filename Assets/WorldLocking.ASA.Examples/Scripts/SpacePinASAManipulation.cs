// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.ASA;
using Microsoft.MixedReality.WorldLocking.Examples;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Component that adds MRTK object manipulation capabilities on top of the auto-orienting SpacePinOrientable.
    /// </summary>
    public class SpacePinASAManipulation : SpacePinASA
    {
        #region Inspector fields

        [SerializeField]
        [Tooltip("Proxy renderable to show axis alignment during manipulations.")]
        private GameObject prefab_FeelerRay = null;

        /// <summary>
        /// Proxy renderable to show axis alignment during manipulations.
        /// </summary>
        public GameObject Prefab_FeelerRay { get { return prefab_FeelerRay; } set { prefab_FeelerRay = value; } }

        [SerializeField]
        [Tooltip("Whether to show the MRTK rotation gizmos.")]
        private bool allowRotation = true;

        /// <summary>
        /// Whether to show the MRTK rotation gizmos.
        /// </summary>
        /// <remarks>
        /// Rotating the SpacePinOrientableManipulation object only has any effect when the first
        /// pin is manipulated. Once the second object is manipulated, and ever after, the orientation
        /// is implied by the alignment of the pin objects, and actual orientation of the objects is ignored.
        /// </remarks>
        public bool AllowRotation { get { return allowRotation; } set { allowRotation = value; } }
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
            pinManipulator.UserOriented = AllowRotation;
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

        /// <summary>
        /// Callback for when the user has finished positioning the target.
        /// </summary>
        private void OnFinishManipulation()
        {
            SetFrozenPose(ExtractModelPose());
            ConfigureLocalPeg();
        }
    }
}

