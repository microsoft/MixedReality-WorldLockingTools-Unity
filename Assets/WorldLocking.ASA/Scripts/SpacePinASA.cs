// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;

namespace Microsoft.MixedReality.WorldLocking.ASA
{
    public class SpacePinASA : SpacePinOrientable
    {
        /// <summary>
        /// The local peg created (hopefully) when the camera was close to this pin.
        /// </summary>
        private ILocalPeg localPeg = null;

        /// <summary>
        /// Accessor for local peg.
        /// </summary>
        public ILocalPeg LocalPeg { get { return localPeg; } }

        /// <summary>
        /// Accessor for publisher. This is managed by the binder.
        /// </summary>
        public IPublisher Publisher { get; set; }

        /// <summary>
        /// Unique identifier for this space pin.
        /// </summary>
        public string SpacePinId { get { return name; } }

        [Serializable]
        public class KeyValPair
        {
            public string key;
            public string val;
        };

        [Tooltip("Key value pairs become property list on any cloud anchor generated from this space pin.")]
        [SerializeField]
        private List<KeyValPair> propertyList = new List<KeyValPair>();
        
        /// <summary>
        /// <see cref="propertyList"/> will autopopulate this dictionary at Awake, getting around inability to serialize Dictionary.
        /// </summary>
        private readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        /// <summary>
        /// Runtime access of properties.
        /// </summary>
        public Dictionary<string, string> Properties => properties;

        private readonly int ConsoleLow = 3;
        private readonly int ConsoleHigh = 8;

        /// <summary>
        /// Populate the properties dictionary from the serialized list.
        /// </summary>
        private void Awake()
        {
            foreach (var keyval in propertyList)
            {
                Debug.Assert(!string.IsNullOrEmpty(keyval.key));
                Debug.Assert(!string.IsNullOrEmpty(keyval.val));
                properties[keyval.key] = keyval.val;
            }
            if (!properties.ContainsKey(SpacePinBinder.SpacePinIdKey))
            {
                properties[SpacePinBinder.SpacePinIdKey] = SpacePinId;
            }
        }

        /// <summary>
        /// Ready to publish when we have a local peg and it is ready to publish.
        /// </summary>
        public bool IsReadyForPublish
        {
            get
            {
                if (LocalPeg == null)
                {
                    return false;
                }
                return LocalPeg.IsReadyForPublish;
            }
        }

        /// <summary>
        /// Accept the local peg assigned by the binder after it's been downloaded from the cloud.
        /// </summary>
        /// <param name="peg">The local peg to take.</param>
        public void SetLocalPeg(ILocalPeg peg)
        {
            if (peg?.Name == localPeg?.Name)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"Redundant SLP: {name} {peg.Name}");
                return;
            }
            if (localPeg != null)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"SLP release {localPeg?.Name} take {peg?.Name}");
                Publisher.ReleaseLocalPeg(localPeg);
            }
            localPeg = peg;
            SimpleConsole.AddLine(ConsoleLow, $"SLP: {name} - {localPeg.GlobalPose.position.ToString("F3")}");
        }

        /// <summary>
        /// Create a local peg based on current state (LockedPose).
        /// </summary>
        /// <remarks>
        /// This typically happens when the SpacePinASA is locally manipulated into a new pose.
        /// </remarks>
        public async void ConfigureLocalPeg()
        {
            if (Publisher == null)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"Publisher hasn't been set on SpacePin={name}");
                return;
            }
            if (localPeg != null)
            {
                SimpleConsole.AddLine(ConsoleHigh, $"Releasing existing peg {name}");
                Publisher.ReleaseLocalPeg(localPeg);
            }
            localPeg = await Publisher.CreateLocalPeg($"{SpacePinId}_peg", LockedPose);
            SimpleConsole.AddLine(ConsoleLow, $"CLP: {name} - {localPeg.GlobalPose.position.ToString("F3")}");
        }
    }
}

