// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    using CloudAnchorId = System.String;

    /// <summary>
    /// Readiness states. 
    /// </summary>
    /// <remarks>
    /// The publisher is only able to process requested tasks when its state is "Ready".
    /// </remarks>
    public enum PublisherReadiness
    {
        NotSetup, // Setup hasn't been completed yet.
        NoManager, // There is no manager created, probably an installation/setup error.
        Starting, // Waiting on internal systems to boot.
        NotReadyToCreate, // System running, but not scanned enough to reliably create cloud anchors.
        NotReadyToLocate, // System running, but still searching for relocation signals. Only ever in this state when coarse relocation is enabled.
        Ready, // Ready to process requests.
        Busy // Currently processing a request.
    };

    /// <summary>
    /// Class wrapping the readiness state, along with the progress to readiness to create cloud anchors
    /// </summary>
    /// <remarks>
    /// The floating point progress indicators are a bleed-through of the internal implementation,
    /// but are very useful to the application/user when establishing tracking.
    /// </remarks>
    public class ReadinessStatus
    {
        /// <summary>
        /// Readiness state.
        /// </summary>
        public PublisherReadiness readiness = PublisherReadiness.NotSetup;

        /// <summary>
        /// Progress to recommended for create. Recommended when recommendedForCreate >= 1.0f.
        /// </summary>
        public float recommendedForCreate = 0;

        /// <summary>
        /// Progress to ready for create. Ready (but not necessarily recommended) when readyForCreate >= 1.0f;
        /// </summary>
        public float readyForCreate = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReadinessStatus()
        {
        }

        /// <summary>
        /// Constructor setting readiness, leaving progress indicators at defaults.
        /// </summary>
        /// <param name="r"></param>
        public ReadinessStatus(PublisherReadiness r)
        {
            readiness = r;
        }

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="r">Readiness to set.</param>
        /// <param name="recommended">Recommended for create progress value.</param>
        /// <param name="ready">Ready for create progress value.</param>
        public ReadinessStatus(PublisherReadiness r, float recommended, float ready)
        {
            readiness = r;
            recommendedForCreate = recommended;
            readyForCreate = ready;
        }
    }


    /// <summary>
    /// The IPublisher abstracts the process of publishing and downloading cloud anchors.
    /// </summary>
    public interface IPublisher
    {
        /// <summary>
        /// Get the current status, including progress to readiness to create.
        /// </summary>
        /// <remarks>
        /// The publisher processes requests when its ReadinessStatus.readiness == Readiness.Ready.
        /// If that is not the current status, methods will return an error, unless otherwise noted. 
        /// </remarks>
        ReadinessStatus Status { get; }

        /// <summary>
        /// Create a local position holder blob.
        /// </summary>
        /// <param name="id">Unique name to give the local peg.</param>
        /// <param name="lockedPose">The position in WLT.LockedSpace to capture</param>
        /// <returns>Awaitable created blob.</returns>
        /// <remarks>
        /// This can be called anytime, regardless of the ReadinessStatus.
        /// See <see cref="ILocalPeg"/> for more details.
        /// </remarks>
        Task<ILocalPeg> CreateLocalPeg(string id, Pose lockedPose);

        /// <summary>
        /// Free up the resources from an ILocalPeg.
        /// </summary>
        /// <param name="peg">The peg to clean up.</param>
        /// <remarks>
        /// Param "peg" will be invalid after this call.
        /// </remarks>
        void ReleaseLocalPeg(ILocalPeg peg);

        /// <summary>
        /// Create a cloud anchor corresponding to the input local peg and its properties.
        /// </summary>
        /// <param name="pegAndProps">Peg and properties to be captured to the cloud.</param>
        /// <returns>Awaitable identifier for the cloud anchor.</returns>
        Task<CloudAnchorId> Create(LocalPegAndProperties pegAndProps);

        /// <summary>
        /// Download a cloud anchor with the given identifier.
        /// </summary>
        /// <param name="cloudAnchorId">Identifier for the desired cloud anchor.</param>
        /// <returns>Awaitable local peg and its properties that were used to create the cloud anchor are reconstructed and returned.</returns>
        Task<LocalPegAndProperties> Read(CloudAnchorId cloudAnchorId);

        /// <summary>
        /// Download a list of cloud anchors by id.
        /// </summary>
        /// <param name="cloudAnchorIds">List of ids to download.</param>
        /// <returns>Dictionary of LocalPegAndProperties by cloudAnchorId.</returns>
        /// <remarks>
        /// If any cloud anchor ids have already been downloaded this session, and are still retained, those cached records will be refreshed and returned.
        /// </remarks>
        Task<Dictionary<CloudAnchorId, LocalPegAndProperties>> Read(IReadOnlyCollection<CloudAnchorId> cloudAnchorIds);

        /// <summary>
        /// Delete a cloud anchor, and create a new one based on input local peg and its properties.
        /// </summary>
        /// <param name="cloudAnchorId">Cloud anchor to delete.</param>
        /// <param name="pegAndProps">Local anchor and properties to create new cloud anchor from.</param>
        /// <returns>Awaitable identifier for the new cloud anchor.</returns>
        Task<CloudAnchorId> Modify(CloudAnchorId cloudAnchorId, LocalPegAndProperties pegAndProps);

        /// <summary>
        /// Search the area around the device for cloud anchors.
        /// </summary>
        /// <param name="radiusFromDevice">Distance (roughly) from device to search.</param>
        /// <returns>Awaitable dictionary of cloud anchor ids and corresponding local peg and properties.</returns>
        Task<Dictionary<CloudAnchorId, LocalPegAndProperties>> Find(float radiusFromDevice);

        /// <summary>
        /// Search the area around the device, and destroy any anchors found.
        /// </summary>
        /// <param name="radius">Distance (roughly) from device to search.</param>
        /// <returns>Awaitable task.</returns>
        Task PurgeArea(float radius);

        /// <summary>
        /// Delete the indicated cloud anchor from the cloud database.
        /// </summary>
        /// <param name="cloudAnchorId">Cloud anchor to destroy.</param>
        /// <returns>Awaitable task.</returns>
        Task Delete(CloudAnchorId cloudAnchorId);
    };

}
