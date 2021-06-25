// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    /// <summary>
    /// A data blob with enough information to be saved to the cloud and 
    /// reconstructed from the cloud in a later session or on a different device.
    /// </summary>
    /// <remarks>
    /// In a better world, this construct would be hidden in the internals of the
    /// IPublisher interface. Unfortunately, the IPublisher doesn't know when the best
    /// time to create a local peg is, and so has to leave that to the application.
    /// See <see cref="IPublisher.CreateLocalPeg(string, Pose)"/>.
    /// In general, a local peg will be of better quality if it is created when
    /// the tracker is near the local peg's location, and the area has been adequately
    /// scanned.
    /// </remarks>
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
