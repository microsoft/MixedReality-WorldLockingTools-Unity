﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// A game object with this script attached will follow the main camera's position. 
    /// This is particularly useful for secondary cameras or sound sources to follow the user around.
    /// </summary>
    public class MoveWithCamera : MonoBehaviour
    {
        /// <summary>
        /// The GameObject mimics the camera's movement while keeping a given offset.
        /// </summary>
        [SerializeField]
        private Vector3 offsetToCamera = Vector3.zero;

        private void Update()
        {
            gameObject.transform.position = CameraCache.Main.transform.position + offsetToCamera;
            gameObject.transform.rotation = CameraCache.Main.transform.rotation;
        }
    }
}