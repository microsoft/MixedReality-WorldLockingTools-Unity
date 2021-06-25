// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Simple script to select between equivalent menus at build time based on platform.
    /// </summary>
    public class PlatformMenuSelector : MonoBehaviour
    {
        [SerializeField]
        private GameObject HoloLensMenu;

        [SerializeField]
        private GameObject AndroidMenu;

        [SerializeField]
        private GameObject iOSMenu;

        private void Awake()
        {
            // Set all to disabled state.
            SetMenuActive(AndroidMenu, false);
            SetMenuActive(HoloLensMenu, false);
            SetMenuActive(iOSMenu, false);

            // Now enable the right one.
#if UNITY_ANDROID
        SetMenuActive(AndroidMenu, true);
#endif // UNITY_ANDROID

#if UNITY_WSA
        SetMenuActive(HoloLensMenu, true);
#endif // UNITY_WSA

#if UNITY_IOS
        SetMenuActive(iOSMenu, true);
#endif // UNITY_IOS
        }

        private void SetMenuActive(GameObject menu, bool active)
        {
            if (menu != null)
            {
                menu.SetActive(active);
            }
        }
    }
}