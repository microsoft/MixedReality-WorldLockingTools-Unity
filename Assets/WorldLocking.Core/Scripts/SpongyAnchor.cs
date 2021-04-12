// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity spatial anchors, facilitating creation and persistence.
    /// </summary>
    public abstract class SpongyAnchor : MonoBehaviour
    {

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public abstract bool IsLocated { get; }

        /// <summary>
        /// Return the anchor's pose in spongy space.
        /// </summary>
        public abstract Pose SpongyPose { get; }

        /// <summary>
        /// Diagnostic only - to be removed.
        /// </summary>
        public virtual Vector3 Delta { get; set; }
    }
}
