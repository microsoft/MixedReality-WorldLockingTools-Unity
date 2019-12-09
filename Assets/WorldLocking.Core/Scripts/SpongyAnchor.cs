// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Wrapper class for Unity WorldAnchor, facilitating creation and persistence.
    /// </summary>
    public class SpongyAnchor : MonoBehaviour
    {
        /// <summary>
        /// Timeout that protects against SpatialAnchor easing
        /// </summary>
        /// <remark>
        /// The Unity WorldAnchor component is based on the API property Windows.Perception.Spatial.SpatialAnchor.CoordinateSystem
        /// (see https://docs.microsoft.com/en-us/uwp/api/windows.perception.spatial.spatialanchor.coordinatesystem)
        /// 
        /// In contrast to its companion property RawCoordinateSystem, this value is smoothed out over a time of 300ms
        /// (determined experimentally) whenever the correct anchor position is re-established after a tracking loss.
        /// 
        /// Since Unity does not offer access to the raw value, we here introduce a delay after each time isLocated switches back
        /// to true to avoid feeding the FrozenWorld Engine with incorrect initial data.
        /// 
        /// Note: It would be worth trying direct access to SpatialAnchor is possible (COM-5081). First attempts
        /// failed to do this in some straightforward way from Unity-C# code. Further research would be required.
        /// </remark>
        public static readonly float TrackingStartDelayTime = 0.3f;

        private WorldAnchor worldAnchor;
        private float lastNotLocatedTime = float.NegativeInfinity;
        private bool isSaved = false;

        /// <summary>
        /// Returns true if the anchor is reliably located. False might mean loss of tracking or not fully initialized.
        /// </summary>
        public bool isLocated => worldAnchor.isLocated && Time.unscaledTime > lastNotLocatedTime + TrackingStartDelayTime;

        // Start is called before the first frame update
        private void Start()
        {
            if (worldAnchor == null)
            {
                worldAnchor = gameObject.AddComponent<WorldAnchor>();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (!worldAnchor.isLocated)
            {
                lastNotLocatedTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Save to the anchor store, replacing any existing anchor (by name).
        /// </summary>
        /// <param name="store"></param>
        public void Save(WorldAnchorStore store)
        {
            if (isSaved)
            {
                return;
            }

            store.Delete(name);
            bool success = store.Save(name, worldAnchor);
            Debug.Assert(success);
            isSaved = true;
        }

        /// <summary>
        /// Load from the anchor store, losing whatever state was before load.
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        public bool Load(WorldAnchorStore store)
        {
            if (worldAnchor)
            {
                Destroy(worldAnchor);
            }
            worldAnchor = store.Load(name, gameObject);
            isSaved = true;
            return worldAnchor != null;
        }
    }
}
