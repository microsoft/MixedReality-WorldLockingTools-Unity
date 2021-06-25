// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    /// <summary>
    /// Interface for a magical oracle that communicates IBinder bindings across space and time.
    /// </summary>
    /// <remarks>
    /// Note that the IBindingOracle only transmits bindings, which are string pairs of SpacePinId and CloudAnchorId.
    /// It does not cause the binder to do anything with those bindings.
    /// </remarks>
    public interface IBindingOracle
    {
        /// <summary>
        /// The name of this oracle.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Broadcast the bindings.
        /// </summary>
        /// <param name="binder">The binder whose bindings should be broadcast.</param>
        /// <returns>True on success.</returns>
        bool Put(IBinder binder);

        /// <summary>
        /// Retrieve bindings from the ethereal plane.
        /// </summary>
        /// <param name="binder">The binder to add the bindings to.</param>
        /// <returns>True on success.</returns>
        bool Get(IBinder binder);
    }
}

