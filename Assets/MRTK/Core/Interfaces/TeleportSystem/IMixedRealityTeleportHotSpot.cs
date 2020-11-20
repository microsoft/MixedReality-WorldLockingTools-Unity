﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Teleport
{
    public interface IMixedRealityTeleportHotSpot
    {
        /// <summary>
        /// The position the teleport will end at.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The normal of the teleport raycast.
        /// </summary>
        Vector3 Normal { get; }

        /// <summary>
        /// Is the teleport target active?
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Should the target orientation be overridden?
        /// </summary>
        bool OverrideTargetOrientation { get; }

        /// <summary>
        /// Should the destination orientation be overridden?
        /// Useful when you want to orient the user in a specific direction when they teleport to this position.
        /// </summary>
        /// <remarks>
        /// Override orientation is the transform forward of the GameObject this component is attached to.
        /// </remarks>
        float TargetOrientation { get; }

        /// <summary>
        /// Returns the <see href="https://docs.unity3d.com/ScriptReference/GameObject.html">GameObject</see> reference for this teleport target.
        /// </summary>
        GameObject GameObjectReference { get; }
    }
}