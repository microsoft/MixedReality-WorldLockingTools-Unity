// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    public class AlignmentControl : MonoBehaviour
    {
        [SerializeField]
        private List<SpacePin> spacePins = new List<SpacePin>();

        // Start is called before the first frame update
        void Start()
        {

        }

        public void Clear()
        {
            for (int i = 0; i < spacePins.Count; ++i)
            {
                spacePins[i].Reset();
            }
            WorldLockingManager.GetInstance().AlignmentManager.ClearAlignmentAnchors();
            WorldLockingManager.GetInstance().AlignmentManager.SendAlignmentAnchors();

        }
    }
}