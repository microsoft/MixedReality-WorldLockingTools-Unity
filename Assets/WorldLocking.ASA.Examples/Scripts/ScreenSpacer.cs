// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Adjust a menu based on whether camera is in portrait or landscape mode.
    /// </summary>
    public class ScreenSpacer : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        public Transform Target
        {
            get
            {
                Transform t = target == null ? transform : target;
                return t;
            }
            set
            {
                target = value;
                CaptureLocalPose();
            }
        }

        private Pose localPose = Pose.identity;

        private void CaptureLocalPose()
        {
            localPose = Target.GetLocalPose();
        }

        [SerializeField]
        private MeshFilter meshFilter;

        private void Awake()
        {
            transform.SetParent(CameraCache.Main.transform, false);
        }

        private void Update()
        {
            SetDistance();
        }

        /// <summary>
        /// See remarks.
        /// </summary>
        /// <returns>The adjusted FOV in radians</returns>
        /// <remarks>
        /// This function does two things.
        /// 1. We want to always use the horizontal FOV of portrait mode, so that the menu
        ///    doesn't change size (Horizontal extent) on the screen when we change orientation.
        ///    I.e. we always want it to fill the same screen width as when the phone is in portrait mode.
        /// 2. The camera is currently, on Android (Pixel3a), returning the same vertical FOV when in landscape as portrait.
        ///    This is clearly incorrect, as the scene doesn't flatten when the 60 degree vertical FOV is spread over half as many
        ///    pixels in landscape as in portrait.
        /// The seen behavior is that the vertical and horizontal FOVs are swapped when switching between portrait and landscape,
        /// which is reasonable behavior. It's just not what is reported by CameraCache.Main.fieldOfView.
        /// The third thing this function does is convert from degrees to radians.
        /// </remarks>
        private Vector2 GetAdjustedFOV()
        {
            float verticalFOV = Mathf.Deg2Rad * CameraCache.Main.fieldOfView;
            float widthOverHeight = CameraCache.Main.aspect;
            bool isPortrait = true;
            if (widthOverHeight > 1.0f)
            {
                isPortrait = false;
                widthOverHeight = 1.0f / widthOverHeight;
            }
            float horizontalFOV = verticalFOV * widthOverHeight;
            if (!isPortrait)
            {
                verticalFOV = horizontalFOV;
            }

            return new Vector2(horizontalFOV, verticalFOV);
        }

        private void SetDistance()
        {
            if (meshFilter == null || meshFilter.mesh == null)
            {
                return;
            }
            Vector2 adjustedFOV = GetAdjustedFOV();
            Vector2 tanHalfFOV;
            tanHalfFOV.x = Mathf.Tan(adjustedFOV.x * 0.5f);
            tanHalfFOV.y = Mathf.Tan(adjustedFOV.y * 0.5f);

            Vector3 localScale = meshFilter.transform.localScale;
            float width = meshFilter.mesh.bounds.size.x * localScale.x;
            float height = meshFilter.mesh.bounds.size.y * localScale.y;

            float distance = width / (2.0f * tanHalfFOV.x);
            distance = Mathf.Max(distance, meshFilter.transform.localPosition.z);

            float offsetHeight = distance * tanHalfFOV.y - height * 0.5f;
            offsetHeight = Mathf.Max(offsetHeight, meshFilter.transform.localPosition.y);

            Vector3 localPosition = transform.localPosition;
            localPosition.z = distance - meshFilter.transform.localPosition.z;
            localPosition.y = offsetHeight - meshFilter.transform.localPosition.y;
            transform.localPosition = localPosition;

        }
    }

}