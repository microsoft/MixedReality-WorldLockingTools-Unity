// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Perform one off actions at startup.
    /// </summary>
    public class Startup : MonoBehaviour
    {
        private void Awake()
        {
#if WINDOWS_UWP
            var display = Windows.Graphics.Holographic.HolographicDisplay.GetDefault();
            var view = display.TryGetViewConfiguration(Windows.Graphics.Holographic.HolographicViewConfigurationKind.PhotoVideoCamera);
            if (view != null)
            {
                view.IsEnabled = true;
            }
#endif

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
