// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class SpongyAnchorARCore : SpongyAnchor
    {
        public static float TrackingStartDelayTime = 0.3f;

        private float lastNotLocatedTime = float.NegativeInfinity;

#if WLT_ARCORE_SDK_INCLUDED
        private GoogleARCore.Anchor internalAnchor = null;
#endif // WLT_ARCORE_SDK_INCLUDED

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public override bool IsLocated =>
             IsReliablyLocated && Time.unscaledTime > lastNotLocatedTime + TrackingStartDelayTime;

        private bool IsReliablyLocated
        {
            get
            {
#if WLT_ARCORE_SDK_INCLUDED
                return internalAnchor != null && internalAnchor.TrackingState == GoogleARCore.TrackingState.Tracking;
#else // WLT_ARCORE_SDK_INCLUDED
                return false;
#endif // WLT_ARCORE_SDK_INCLUDED
            }
        }

        public override Pose SpongyPose
        {
            get
            {
                return transform.GetGlobalPose();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (!IsReliablyLocated)
            {
                lastNotLocatedTime = Time.unscaledTime;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
#if WLT_ARCORE_SDK_INCLUDED
            internalAnchor = Session.CreateAnchor(transform.GetGlobalPose());
#endif // WLT_ARCORE_SDK_INCLUDED
        }

    }
}