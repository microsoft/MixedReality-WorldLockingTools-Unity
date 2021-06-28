
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;

/// <summary>
/// NOTE DANGER OF RACE CONDITION ON REFIT EVENTS
/// If we are receiving system updates/refinements on cloud anchor poses,
///    If tracker tracker makes correction (e.g. due to loop closure) to cloud anchor pose
///    Then later FW issues a refreeze correction suitable for previous cloud anchor pose
///       FW correction will be applied to already corrected cloud anchor pose.
/// </summary>

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    using CloudAnchorId = System.String;

    /// <summary>
    /// Implementation of the IBinder interface, managing the relationship between space pins and cloud anchors.
    /// </summary>
    [RequireComponent(typeof(PublisherASA))]
    public partial class SpacePinBinder : MonoBehaviour, IBinder
    {
        #region Inspector members
        [Tooltip("List of space pins to manage. These may also be added from script using AddSpacePin()")]
        [SerializeField]
        private List<SpacePinASA> spacePins = new List<SpacePinASA>();

        public IReadOnlyCollection<SpacePinASA> SpacePins { get { return spacePins; } }

        [Tooltip("Distance (roughly) to search from device when looking for cloud anchors using coarse relocation.")]
        [SerializeField]
        private float searchRadius = 25.0f; // meters

        /// <summary>
        /// Distance (roughly) to search from device when looking for cloud anchors using coarse relocation.
        /// </summary>
        public float SearchRadius { get { return searchRadius; } set { searchRadius = value; } }

        #endregion // Inspector members

        #region Internal types
        /// <summary>
        /// Convenience bundle of a space pin and associated local peg and its properties. Some redundancy there.
        /// </summary>
        private class SpacePinPegAndProps
        {
            public SpacePinASA spacePin;
            public LocalPegAndProperties pegAndProps;
        };

        #endregion // Internal types

        #region Internal members

        /// <summary>
        /// The list of bindings. Could be a Dictionary or something.
        /// </summary>
        private readonly List<SpacePinCloudBinding> bindings = new List<SpacePinCloudBinding>();

        /// <summary>
        /// The publisher used to access cloud anchors.
        /// </summary>
        private IPublisher publisher = null;

        private readonly int ConsoleHigh = 8;
        private readonly int ConsoleLow = 3;
        #endregion // Internal members

        #region Public APIs

        /// <inheritdoc/>
        public string Name { get { return name; } }

        /// <summary>
        /// The key for the key-value pair in the space pin/cloud anchor properties identifying the space pin id in the value.
        /// </summary>
        public static readonly string SpacePinIdKey = "SpacePinId";

        /// <inheritdoc/>
        public bool IsReady
        {
            get { return PublisherStatus.readiness == PublisherReadiness.Ready; }
        }

        /// <inheritdoc/>
        public ReadinessStatus PublisherStatus { get { return publisher != null ? publisher.Status : new ReadinessStatus(); } }

        #region Create and maintain bindings between space pins and cloud anchors
        /// <inheritdoc/>
        public IReadOnlyList<SpacePinCloudBinding> GetBindings()
        {
            return bindings;
        }

        /// <inheritdoc/>
        public bool CreateBinding(string spacePinId, string cloudAnchorId)
        {
            int spacePinIdx = FindSpacePinById(spacePinId);
            if (spacePinIdx < 0)
            {
                Debug.LogError($"Trying to bind a space pin that Binder doesn't know about. Check inspector or add from script.");
                return false;
            }
            SetBinding(spacePinId, cloudAnchorId);
            return true;
        }

        /// <inheritdoc/>
        public bool RemoveBinding(string spacePinId)
        {
            int bindingIdx = FindBindingBySpacePinId(spacePinId);
            if (bindingIdx < 0)
            {
                Debug.LogError($"Trying to remove unknown binding for space pin {spacePinId}");
                return false;
            }
            bindings.RemoveAt(bindingIdx);
            return true;
        }
        #endregion // Create and maintain bindings between space pins and cloud anchors

        #region Space pin list control from script
        /// <summary>
        /// Add a space pin to the list of managed pins.
        /// </summary>
        /// <param name="spacePin">Pin to add.</param>
        /// <returns>True if not already there but added.</returns>
        public bool AddSpacePin(SpacePinASA spacePin)
        {
            // mafish - make sure it's not already in there.
            int idx = FindSpacePin(spacePin);
            if (idx <= 0)
            {
                spacePins.Add(spacePin);
                spacePin.Publisher = publisher;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the space pin binding associated with this SpacePin.
        /// </summary>
        /// <param name="spacePinId">Space pin id of binding to remove.</param>
        /// <returns>True if found and removed.</returns>
        /// <remarks>
        /// Any binding between this pin and a cloud anchor is also severed.
        /// </remarks>
        public bool RemoveSpacePin(string spacePinId)
        {
            int idx = FindSpacePinById(spacePinId);
            if (idx < 0)
            {
                Debug.Assert(FindBindingBySpacePinId(spacePinId) < 0, $"Space pin id {spacePinId} not found in list of space pins, but found in bindings");
                return false;
            }
            spacePins[idx].Publisher = null;
            spacePins.RemoveAt(idx);
            int bindingIdx = FindBindingBySpacePinId(spacePinId);
            if (bindingIdx >= 0)
            {
                bindings.RemoveAt(bindingIdx);
            }
            return true;
        }
        #endregion Space pin list control from script

        #region Publish to cloud
        /// <inheritdoc/>
        public async Task<bool> Publish()
        {
            bool allSuccessful = true;
            foreach (var spacePin in spacePins)
            {
                if (IsReadyForPublish(spacePin))
                {
                    bool success = await Publish(spacePin);
                    if (!success)
                    {
                        Debug.LogError($"Failed to publish {spacePin.SpacePinId}, continuing.");
                        allSuccessful = false;
                    }
                }
            }
            return allSuccessful;
        }

        /// <summary>
        /// Publish the spacePin.
        /// </summary>
        /// <param name="spacePin">SpacePinASA to publish</param>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// It may be this should be a private member.
        /// </remarks>
        public async Task<bool> Publish(SpacePinASA spacePin)
        {
            if (!IsReady)
            {
                // mafinc - Should we wait until it is ready? Maybe as a binder option?
                return false;
            }

            int idx = FindSpacePin(spacePin);
            if (idx < 0)
            {
                Debug.LogError($"Trying to publish unknown space pin. Must be added in inspector or AddSpacePin() first.");
                return false;
            }

            int cloudIdx = FindBindingBySpacePinId(spacePin.SpacePinId);
            if (cloudIdx >= 0)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"Publishing previously published space pin={spacePin.SpacePinId}, deleting from cloud first.");
                await publisher.Delete(bindings[cloudIdx].cloudAnchorId);
                RemoveBinding(spacePin.SpacePinId);
            }

            var obj = ExtractForPublisher(spacePin);
            if (obj == null)
            {
                return false;
            }
            CloudAnchorId cloudAnchorId = await publisher.Create(obj);
            if (string.IsNullOrEmpty(cloudAnchorId))
            {
                Debug.LogError($"Failed to create cloud anchor for {spacePin.SpacePinId}");
                return false;
            }
            SetBinding(spacePin.SpacePinId, cloudAnchorId);
            return true;
        }

        #endregion // Publish to cloud

        #region Download from cloud
        /// <inheritdoc/>
        public async Task<bool> Download()
        {
            if (!IsReady)
            {
                return false;
            }

            bool allSuccessful = true;
            List<SpacePinPegAndProps> readObjects = new List<SpacePinPegAndProps>();
            List<CloudAnchorId> cloudAnchorList = new List<CloudAnchorId>();
            Dictionary<CloudAnchorId, SpacePinASA> spacePinByCloudId = new Dictionary<CloudAnchorId, SpacePinASA>();
            foreach (var spacePin in spacePins)
            {
                int bindingIdx = FindBindingBySpacePinId(spacePin.SpacePinId);
                if (bindingIdx >= 0)
                {
                    string cloudAnchorId = bindings[bindingIdx].cloudAnchorId;
                    cloudAnchorList.Add(cloudAnchorId);
                    spacePinByCloudId[cloudAnchorId] = spacePin;
                }
            }
            if (cloudAnchorList.Count > 0)
            {
                var found = await publisher.Read(cloudAnchorList);
                if (found != null)
                {
                    foreach (var keyVal in found)
                    {
                        var cloudAnchorId = keyVal.Key;
                        var spacePin = spacePinByCloudId[cloudAnchorId];
                        var pegAndProps = keyVal.Value;
                        Debug.Assert(pegAndProps.localPeg != null);
                        readObjects.Add(new SpacePinPegAndProps() { spacePin = spacePin, pegAndProps = pegAndProps });
                    }
                }
                else
                {
                    SimpleConsole.AddLine(ConsoleHigh, $"publisher Read returned null looking for {cloudAnchorList.Count} ids");
                }
            }
            var wltMgr = WorldLockingManager.GetInstance();
            foreach (var readObj in readObjects)
            {
                Pose lockedPose = wltMgr.LockedFromFrozen.Multiply(readObj.pegAndProps.localPeg.GlobalPose);
                SimpleConsole.AddLine(ConsoleLow, $"Dwn: {lockedPose.ToString("F3")}");
                readObj.spacePin.SetLockedPose(lockedPose);
                readObj.spacePin.SetLocalPeg(readObj.pegAndProps.localPeg);
            }
            return allSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> Search()
        {
            if (!IsReady)
            {
                return false;
            }

            Dictionary<CloudAnchorId, LocalPegAndProperties> found = await publisher.Find(searchRadius);

            var wltMgr = WorldLockingManager.GetInstance();

            bool foundAny = false;
            foreach (var keyval in found)
            {
                string spacePinId = keyval.Value.properties[SpacePinIdKey];
                string cloudAnchorId = keyval.Key;
                var pegAndProps = keyval.Value;
                int idx = FindSpacePinById(spacePinId);
                if (idx >= 0)
                {
                    CreateBinding(spacePinId, cloudAnchorId);
                    foundAny = true;
                    SpacePinASA spacePin = spacePins[idx];

                    Pose lockedPose = wltMgr.LockedFromFrozen.Multiply(pegAndProps.localPeg.GlobalPose);
                    spacePin.SetLockedPose(lockedPose);
                    spacePin.SetLocalPeg(pegAndProps.localPeg);
                }
                else
                {
                    SimpleConsole.AddLine(ConsoleHigh, $"Found anchor for unknown SpacePin={spacePinId}.");
                }
            }
            return foundAny;
        }
        #endregion // Download from cloud

        #region Cleanup

        /// <inheritdoc/>
        public async Task<bool> Purge()
        {
            if (!IsReady)
            {
                return false;
            }
            await publisher.PurgeArea(searchRadius);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> Clear()
        {
            if (!IsReady)
            {
                return false;
            }
            foreach (var binding in bindings)
            {
                await publisher.Delete(binding.cloudAnchorId);
            }
            bindings.Clear();
            return true;
        }

        public void UnPin()
        {
            foreach (var spacePin in spacePins)
            {
                if (spacePin.PinActive)
                {
                    spacePin.Reset();
                }
            }
        }
        #endregion // Cleanup

        #endregion // Public APIs

        #region Unity
        /// <summary>
        /// Establish relationship with the publisher.
        /// </summary>
        private void Awake()
        {
            var publisherASA = GetComponent<PublisherASA>();
            // When Setup is complete, publisher.IsReady will be true.
            publisherASA.Setup();
            SetSpacePinsPublisher(publisherASA);
        }

        #endregion // Unity

        #region Internal helpers

        /// <summary>
        /// Capture the publisher we'll be using, and pass it on to all managed space pins.
        /// </summary>
        /// <param name="publisherASA">The publisher to capture.</param>
        /// <remarks>
        /// SpacePinASA needs a reference to the publisher for the management of its ILocalPeg.
        /// </remarks>
        private void SetSpacePinsPublisher(PublisherASA publisherASA)
        {
            publisher = publisherASA;
            foreach (var spacePin in spacePins)
            {
                spacePin.Publisher = publisher;
            }
        }

        /// <summary>
        /// Determine whether a space pin has necessary setup to be published.
        /// </summary>
        /// <param name="spacePin">The space pin to check.</param>
        /// <returns>True if the space pin can be published.</returns>
        private bool IsReadyForPublish(SpacePinASA spacePin)
        {
            if (spacePin == null)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"Getting null space pin to check ready for publish.");
                return false;
            }
            if (spacePin.Publisher != publisher)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"SpacePin={spacePin.SpacePinId} has different publisher than binder={name}.");
                return false;
            }
            return spacePin.IsReadyForPublish;
        }

        /// <summary>
        /// Create or update (by space pin id) a binding to a cloud anchor.
        /// </summary>
        /// <param name="spacePinId">Id of the space pin.</param>
        /// <param name="cloudAnchorId">Id of the cloud anchor.</param>
        private void SetBinding(string spacePinId, CloudAnchorId cloudAnchorId)
        {
            Debug.Log($"Setting binding between sp={spacePinId} ca={cloudAnchorId}");
            int bindingIdx = FindBindingBySpacePinId(spacePinId);
            var binding = new SpacePinCloudBinding() { spacePinId = spacePinId, cloudAnchorId = cloudAnchorId };
            if (bindingIdx < 0)
            {
                Debug.Log($"Adding new binding sp={spacePinId} ca={cloudAnchorId}");
                bindings.Add(binding);
            }
            else
            {
                Debug.Log($"Updating existing binding sp={spacePinId} from ca={bindings[bindingIdx].cloudAnchorId} to ca={cloudAnchorId}");
                bindings[bindingIdx] = binding;
            }
        }

        /// <summary>
        /// Pull generic data from a space pin to pass to a publisher.
        /// </summary>
        /// <param name="spacePin">The space pin to extract from.</param>
        /// <returns>Null on failure, else a valid local peg and associated properties.</returns>
        private LocalPegAndProperties ExtractForPublisher(SpacePinASA spacePin)
        {
            if (!spacePin.IsReadyForPublish)
            {
                Debug.LogError($"Trying to publish a space pin with no native anchor. Place it first.");
                return null;
            }

            LocalPegAndProperties ret = new LocalPegAndProperties(spacePin.LocalPeg, spacePin.Properties);

            return ret;
        }

        /// <summary>
        /// Find the index in the spacePins list to a managed space pin by its id.
        /// </summary>
        /// <param name="spacePinId">Id of the pin to find.</param>
        /// <returns>The index of the pin if found, else -1.</returns>
        private int FindSpacePinById(string spacePinId)
        {
            return FindByPredicate(spacePins, x => x.SpacePinId == spacePinId);
        }

        /// <summary>
        /// Find the index of a space pin in the space pins list.
        /// </summary>
        /// <param name="spacePin">The pin to find.</param>
        /// <returns>Index in the list if found, else -1.</returns>
        private int FindSpacePin(SpacePin spacePin)
        {
            return FindByPredicate(spacePins, x => x == spacePin);
        }

        /// <summary>
        /// Find the index of a binding by its cloud anchor id.
        /// </summary>
        /// <param name="cloudAnchorId">Cloud anchor id to search for.</param>
        /// <returns>Index if found, else -1.</returns>
        private int FindBindingByCloudAnchorId(string cloudAnchorId)
        {
            return FindByPredicate(bindings, x => x.cloudAnchorId == cloudAnchorId);
        }

        /// <summary>
        /// Find the index of a binding by its space pin id.
        /// </summary>
        /// <param name="spacePinId">Space pin id to search for.</param>
        /// <returns>Index if found, else -1.</returns>
        private int FindBindingBySpacePinId(string spacePinId)
        {
            return FindByPredicate(bindings, x => x.spacePinId == spacePinId);
        }

        /// <summary>
        /// Search a list according to predicate.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list.</typeparam>
        /// <param name="searchList">List to search.</param>
        /// <param name="pred">Predicate to search by.</param>
        /// <returns></returns>
        private static int FindByPredicate<T>(List<T> searchList, Predicate<T> pred)
        {
            int idx = searchList.FindIndex(x => pred(x));
            return idx;
        }

        #endregion // Internal helpers
    }

}

