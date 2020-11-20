﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Flags used by MixedRealityControllerAttribute.
    /// </summary>
    [System.Flags]
    public enum MixedRealityControllerConfigurationFlags : byte
    {
        /// <summary>
        /// Controllers with custom interaction mappings can have their mappings be added / removed to the
        /// controller mapping profile in the property inspector.
        /// </summary>
        UseCustomInteractionMappings = 1 << 0,
    }
}
