// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    /// <summary>
    /// A binding between a space pin and a cloud anchor, by their respective id's.
    /// </summary>
    public struct SpacePinCloudBinding
    {
        public string spacePinId;
        public string cloudAnchorId;
    };

    /// <summary>
    /// Interface for a binding layer between space pins and cloud anchors.
    /// </summary>
    /// <remarks>
    /// This abstraction isn't so much for multiple implementations, as for
    /// providing a clean API. The IBinder is responsible for managing the mapping
    /// between the cloud anchors, which persist and transmit pose data, and the 
    /// space pins, which use that data to create shared spaces.
    /// </remarks>
    public interface IBinder
    {
        /// <summary>
        /// The name of this binder.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If the binder is ready to execute tasks.
        /// </summary>
        /// <remarks>
        /// Reasons not be be ready include initialization not complete, or already busy on another task.
        /// </remarks>
        bool IsReady { get; }

        /// <summary>
        /// The current status of the publisher.
        /// </summary>
        ReadinessStatus PublisherStatus { get; }

        /// <summary>
        /// Known bindings between space pins and cloud anchors.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<SpacePinCloudBinding> GetBindings();

        /// <summary>
        /// Set the cloud anchor id associated with this space pin.
        /// </summary>
        /// <param name="spacePinId">Name of the space pin to be bound to this cloud id.</param>
        /// <param name="cloudAnchorId">Cloud id to be bound to the space pin.</param>
        /// <returns>False if space pin is unknown. Space pin must be registered before being bound.</returns>
        /// <remarks>
        /// A space pin must be bound to a cloud anchor id before it can be downloaded.
        /// </remarks>
        bool CreateBinding(string spacePinId, string cloudAnchorId);

        /// <summary>
        /// Erase a binding between a space pin and its corresponding cloud anchor.
        /// </summary>
        /// <param name="spacePinId">Space pin to unbind.</param>
        /// <returns>True if found and unbound.</returns>
        /// <remarks>
        /// Neither the space pin nor the cloud anchor are affected by this, but will be independent of one another after.
        /// </remarks>
        bool RemoveBinding(string spacePinId);

        /// <summary>
        /// Publish all active space pins to the cloud.
        /// </summary>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// Space pins which are previously published in this session, i.e. that have a binding to a cloud anchor, will have that cloud anchor deleted
        /// first, and a new binding created to the newly published cloud anchor.
        /// To be publishable, a SpacePin must have an <see cref="ILocalPeg"/> created from <see cref="IPublisher.CreateLocalPeg(string, Pose)"/>. The <see cref="SpacePinASA"/>
        /// manages this automatically.
        /// </remarks>
        Task<bool> Publish();

        /// <summary>
        /// Pull down cloud anchors for all known bindings, and apply them to the bound space pins.
        /// </summary>
        /// <returns>True on success.</returns>
        Task<bool> Download();

        /// <summary>
        /// Search for cloud anchors in the area, download them, and apply to associated space pins.
        /// </summary>
        /// <returns>True on success</returns>
        Task<bool> Search();

        /// <summary>
        /// Delete all known (bound) cloud anchors from the cloud, and erase their bindings.
        /// </summary>
        /// <returns>True on success.</returns>
        /// <remarks> 
        /// Bound space pins are unaffected, but will no longer be bound.
        /// </remarks>
        Task<bool> Clear();

        /// <summary>
        /// Find all cloud anchors in the area, delete them, and release their bindings.
        /// </summary>
        /// <returns>True on success.</returns>
        Task<bool> Purge();
    }
}
