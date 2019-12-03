﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// The associated GameObject will rotate when RotateTarget() is called based on a given direction and speed.
    /// </summary>
    public class RotateWithConstSpeedDir : MonoBehaviour
    {
        #region Serialized variables

        [Tooltip("Euler angles by which the object should be rotated by.")]
        [SerializeField]
        private Vector3 RotateByEulerAngles = Vector3.zero;

        [Tooltip("Rotation speed factor.")]
        [SerializeField]
        private float speed = 1f;

        #endregion

        /// <summary>
        /// Rotate game object based on specified rotation speed and Euler angles.
        /// </summary>
        public void RotateTarget()
        {
            transform.eulerAngles = transform.eulerAngles + RotateByEulerAngles * speed;
        }
    }
}