﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Sample for allowing the game object that this script is attached to follow the user's eye gaze
    /// at a given distance of "DefaultDistanceInMeters". 
    /// </summary>
    public class FollowEyeGazeGazeProvider : MonoBehaviour
    {
        [Tooltip("Display the game object along the eye gaze ray at a default distance (in meters).")]
        [SerializeField]
        private float defaultDistanceInMeters = 2f;

        private IMixedRealityInputSystem inputSystem = null;

        /// <summary>
        /// The active instance of the input system.
        /// </summary>
        private IMixedRealityInputSystem InputSystem
        {
            get
            {
                if (inputSystem == null)
                {
                    MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out inputSystem);
                }
                return inputSystem;
            }
        }

        private void Update()
        {
            if (InputSystem?.GazeProvider != null)
            {
                gameObject.transform.position = InputSystem.GazeProvider.GazeOrigin + InputSystem.GazeProvider.GazeDirection.normalized * defaultDistanceInMeters;
            }
        }
    }
} 