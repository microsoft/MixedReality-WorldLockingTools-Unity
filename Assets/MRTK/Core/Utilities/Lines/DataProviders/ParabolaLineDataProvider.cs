﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities
{
    /// <summary>
    /// Base Parabola line data provider.
    /// </summary>
    public abstract class ParabolaLineDataProvider : BaseMixedRealityLineDataProvider
    {
        [SerializeField]
        private MixedRealityPose startPoint = MixedRealityPose.ZeroIdentity;

        /// <summary>
        /// The Starting point of this line.
        /// </summary>
        /// <remarks>Always located at this <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see>'s <see href="https://docs.unity3d.com/ScriptReference/Transform-position.html">Transform.position</see></remarks>
        public MixedRealityPose StartPoint => startPoint;

        #region Line Data Provider Implementation

        /// <inheritdoc />
        protected override float GetUnClampedWorldLengthInternal()
        {
            float distance = 0f;
            Vector3 last = GetUnClampedPoint(0f);
            for (int i = 1; i < UnclampedWorldLengthSearchSteps; i++)
            {
                Vector3 current = GetUnClampedPoint((float)i / UnclampedWorldLengthSearchSteps);
                distance += Vector3.Distance(last, current);
            }

            return distance;
        }

        /// <inheritdoc />
        protected override Vector3 GetUpVectorInternal(float normalizedLength)
        {
            return transform.up;
        }

        #endregion Line Data Provider Implementation
    }
}