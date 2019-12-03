﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAwareness
{

    public partial class BaseSpatialAwarenessObject : IMixedRealitySpatialAwarenessObject
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public GameObject GameObject { get; set; }

        /// <inheritdoc />
        public MeshRenderer Renderer { get; set; }

        public MeshFilter Filter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void CleanObject()
        {
            // todo: consider if this should be virtual, and what params it should contain
        }

        /// <summary>
        /// constructor
        /// </summary>
        protected BaseSpatialAwarenessObject()
        {
            //empty for now
        }
    }
}
