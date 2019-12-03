// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    public class FallOut : MonoBehaviour
    {
        /// <summary>
        /// The height below the camera at which a falling object disappears. Object's position is based on local space origin, not bounds.
        /// </summary>
        [Tooltip("The height below the camera at which a falling object disappears. Object's position is based on local space origin, not bounds.")]
        public float KillHeight = -20.0f;

        // Update is called once per frame
        void Update()
        {
            float objHeight = transform.position.y;
            float camHeight = Camera.main.transform.position.y;
            if (objHeight < camHeight + KillHeight)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}