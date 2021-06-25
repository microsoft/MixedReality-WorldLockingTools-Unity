// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    public interface ILocalPeg
    {
        /// <summary>
        /// The name for this peg.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is there enough information to publish this anchor to the cloud?
        /// </summary>
        bool IsReadyForPublish { get; }

        /// <summary>
        /// The current global pose for the blob.
        /// </summary>
        Pose GlobalPose { get; }
    }

    /// <summary>
    /// A local peg, with an associated properties dictionary.
    /// </summary>
    public class LocalPegAndProperties
    {
        public readonly ILocalPeg localPeg;
        public readonly IDictionary<string, string> properties;

        public LocalPegAndProperties(ILocalPeg lp, IDictionary<string, string> props)
        {
            this.localPeg = lp;
            this.properties = props;
        }
    };

}
