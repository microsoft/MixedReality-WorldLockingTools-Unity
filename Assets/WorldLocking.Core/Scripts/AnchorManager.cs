// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_EXTRA_LOGGING

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
#if UNITY_WSA
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
#endif // UNITY_WSA

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Encapsulation of spongy world (raw input) state. Its primary duty is the creation and maintenance
    /// of the graph of (spongy) anchors built up over the space traversed by the camera.
    /// </summary>
    /// <remarks>
    /// Anchor and Edge creation algorithm:
    /// 
    /// Goal: a simple and robust algorithm that guarantees an even distribution of anchors, fully connected by
    /// edges between nearest neighbors with a minimum of redundant edges
    ///
    /// For simplicity, the algorithm should be stateless between time steps
    ///
    /// Rules
    /// * two parameters define spheres MIN and MAX around current position
    /// * whenever MIN does not contain any anchors, a new anchor is created
    /// * when a new anchor is created is is linked by edges to all anchors within MAX
    /// * the MAX radius is 20cm larger than MIN radius which would require 12 m/s beyond world record sprinting speed to cover in one frame
    /// * whenever MIN contains more than one anchor, the anchor closest to current position is connected to all others within MIN 
    /// </remarks>
    public abstract class AnchorManager : IAnchorManager
    {
        /// <summary>
        /// minimum distance that can occur in regular anchor creation.
        /// </summary>
        private float minNewAnchorDistance = 1.0f;

        /// <inheritdoc/>
        public float MinNewAnchorDistance { get { return minNewAnchorDistance; } set { minNewAnchorDistance = value; } }

        /// <summary>
        /// maximum distance to be considered for creating edges to new anchors
        /// </summary>
        private float maxAnchorEdgeLength = 1.2f;

        /// <inheritdoc/>
        public float MaxAnchorEdgeLength { get { return maxAnchorEdgeLength; } set { maxAnchorEdgeLength = value; } }

        private static readonly float AnchorAddOutTime = 0.4f;

        protected abstract float TrackingStartDelayTime { get; }

        // UNITY_WSA - abstract?
        protected abstract bool IsTracking();

        // mafinc - this ErrorStatus would be well refactored.
        /// <summary>
        /// Error string for last error, cleared at beginning of each update.
        /// </summary>
        public string ErrorStatus { get; private set; } = "";

        /// <summary>
        /// Return the current number of spongy anchors.
        /// </summary>
        public int NumAnchors => spongyAnchors.Count;

        public int NumEdges => plugin.GetNumFrozenEdges();

        private readonly IPlugin plugin;
        private readonly IHeadPoseTracker headTracker = null;
        private readonly Transform worldAnchorParent;

        // New anchor creation:
        // 
        // When a new WorldAnchor component is created, it is sometimes reported as isLocated==true within the same frame
        // only to become isLocated==false in the very next frame and then never to become located again.
        // 
        // To avoid bogus fragments from being created and then hang around indefinitely, whenever the Update routine creates
        // a new anchor, its data is only stored temporarily in the following fields. Then in the following time step, it is finalized
        // only if the isLocated is still true.
        // 
        // newAnchorId is static, and never reset, so that even if a new AnchorManager is created, a new anchor is
        // never reusing the id from an old anchor (from the same session).
        private static AnchorId newAnchorId = AnchorId.FirstValid;
        private SpongyAnchor newSpongyAnchor;
        private List<AnchorId> newAnchorNeighbors;

        public struct SpongyAnchorWithId
        {
            public AnchorId anchorId;
            public SpongyAnchor spongyAnchor;
        }
        private readonly List<SpongyAnchorWithId> spongyAnchors = new List<SpongyAnchorWithId>();
        public List<SpongyAnchorWithId> SpongyAnchors => spongyAnchors;

        private float lastAnchorAddTime;
        private float lastTrackingInactiveTime;

        /// <summary>
        /// Set up an anchor manager.
        /// </summary>
        /// <param name="plugin">The engine interface to update with the current anchor graph.</param>
        public AnchorManager(IPlugin plugin, IHeadPoseTracker headTracker)
        {
            this.plugin = plugin;
            this.headTracker = headTracker;

            worldAnchorParent = new GameObject("SpongyWorldAnchorRoot").transform;

            lastAnchorAddTime = float.NegativeInfinity;
            lastTrackingInactiveTime = float.NegativeInfinity;
        }

        /// <summary>
        /// GC release of resources.
        /// </summary>
        ~AnchorManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Explicit dispose to release resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Implement disposal of resources.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reset();
                if (worldAnchorParent != null)
                {
                    GameObject.Destroy(worldAnchorParent.gameObject);
                }
            }
        }

        /// <summary>
        /// Delete all spongy anchor objects and reset internal state
        /// </summary>
        public void Reset()
        {
            foreach (var anchor in spongyAnchors)
            {
                DestroyAnchor(AnchorId.Invalid, anchor.spongyAnchor);
            }
            spongyAnchors.Clear();

            newSpongyAnchor = DestroyAnchor(AnchorId.Invalid, newSpongyAnchor);
            headTracker.Reset();
        }

        /// <summary>
        /// Create missing spongy anchors/edges and feed plugin with up-to-date input
        /// </summary>
        /// <returns>Boolean: Has the plugin received input to provide an adjustment?</returns>
        public virtual bool Update()
        {
            ErrorStatus = "";

            if (!IsTracking())
            {
                return LostTrackingCleanup("Lost Tracking");
            }

            // To communicate spongyHead and spongyAnchor poses to the FrozenWorld engine, they must all be expressed
            // in the same coordinate system. Here, we do not care where this coordinate
            // system is defined and how it fluctuates over time, as long as it can be used to express the
            // relative poses of all the spongy objects within each time step.
            // 
            Pose spongyHead = headTracker.GetHeadPose();

            // place new anchors 1m below head
            Pose newSpongyAnchorPose = spongyHead;
            newSpongyAnchorPose.position.y -= 1;
            newSpongyAnchorPose.rotation = Quaternion.identity;

            var activeAnchors = new List<AnchorPose>();
            var innerSphereAnchorIds = new List<AnchorId>();
            var outerSphereAnchorIds = new List<AnchorId>();

            float minDistSqr = float.PositiveInfinity;
            AnchorId minDistAnchorId = 0;

            List<AnchorEdge> newEdges;
            AnchorId newId = FinalizeNewAnchor(out newEdges);

            float innerSphereRadSqr = MinNewAnchorDistance * MinNewAnchorDistance;
            float outerSphereRadSqr = MaxAnchorEdgeLength * MaxAnchorEdgeLength;

            foreach (var keyval in spongyAnchors)
            {
                var id = keyval.anchorId;
                var a = keyval.spongyAnchor;
                if (a.IsLocated)
                {
                    Pose aSpongyPose = a.SpongyPose;
                    float distSqr = (aSpongyPose.position - newSpongyAnchorPose.position).sqrMagnitude;
                    var anchorPose = new AnchorPose() { anchorId = id, pose = aSpongyPose };
                    activeAnchors.Add(anchorPose);
                    if (distSqr < minDistSqr)
                    {
                        minDistSqr = distSqr;
                        minDistAnchorId = id;
                    }
                    if (distSqr <= outerSphereRadSqr && id != newId)
                    {
                        outerSphereAnchorIds.Add(id);
                        if (distSqr <= innerSphereRadSqr)
                        {
                            innerSphereAnchorIds.Add(id);
                        }
                    }
                }
            }

            if (newId == 0 && innerSphereAnchorIds.Count == 0)
            {
                if (Time.unscaledTime <= lastTrackingInactiveTime + TrackingStartDelayTime)
                {
#if WLT_EXTRA_LOGGING
                    // Tracking has become active only recently. We suppress creation of new anchors while
                    // new anchors may still be in transition due to SpatialAnchor easing.
                    Debug.Log($"Skip new anchor creation because only recently gained tracking {Time.unscaledTime - lastTrackingInactiveTime}");
#endif // WLT_EXTRA_LOGGING
                }
                else if (Time.unscaledTime < lastAnchorAddTime + AnchorAddOutTime)
                {
#if WLT_EXTRA_LOGGING
                    // short timeout after creating one anchor to prevent bursts of new, unlocatable anchors
                    // in case of problems in the anchor generation
                    Debug.Log($"Skip new anchor creation because waiting on recently made anchor "
                        + $"{Time.unscaledTime - lastAnchorAddTime} "
                        + $"- {(newSpongyAnchor != null ? newSpongyAnchor.name : "null")}");
#endif // WLT_EXTRA_LOGGING
                }
                else
                {
                    PrepareNewAnchor(newSpongyAnchorPose, outerSphereAnchorIds);
                    lastAnchorAddTime = Time.unscaledTime;
                }
            }

            if (activeAnchors.Count == 0)
            {
                ErrorStatus = "No active anchors";
                return false;
            }

            // create edges between nearby existing anchors
            if (innerSphereAnchorIds.Count >= 2)
            {
                foreach (var i in innerSphereAnchorIds)
                {
                    if (i != minDistAnchorId)
                    {
                        newEdges.Add(new AnchorEdge() { anchorId1 = i, anchorId2 = minDistAnchorId });
                    }
                }
            }

            plugin.ClearSpongyAnchors();
            plugin.Step_Init(spongyHead);
            plugin.AddSpongyAnchors(activeAnchors);
            plugin.SetMostSignificantSpongyAnchorId(minDistAnchorId);
            plugin.AddSpongyEdges(newEdges);
            plugin.Step_Finish();

            return true;
        }

        private bool LostTrackingCleanup(string message)
        {
            Debug.Log($"Lost Tracking Frame {Time.frameCount}");
            lastTrackingInactiveTime = Time.unscaledTime;

            if (newSpongyAnchor)
            {
                newSpongyAnchor = DestroyAnchor(AnchorId.Invalid, newSpongyAnchor);
            }

            ErrorStatus = message;
            return false;
        }

        // UNITY_WSA - abstract?
        protected abstract SpongyAnchor CreateAnchor(AnchorId id, Transform parent, Pose initialPose);

        protected abstract SpongyAnchor DestroyAnchor(AnchorId id, SpongyAnchor spongyAnchor);

        protected void RemoveSpongyAnchorById(AnchorId id)
        {
            if (id != AnchorId.Invalid)
            {
                int index = SpongyAnchors.FindIndex(anchorWithId => anchorWithId.anchorId == id);
                if (index >= 0)
                {
                    Debug.Assert(index < SpongyAnchors.Count);
                    SpongyAnchors.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// prepare potential new anchor, which will only be finalized in a later time step
        /// when isLocated is actually found to be true (see code before)
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="neighbors"></param>
        private void PrepareNewAnchor(Pose pose, List<AnchorId> neighbors)
        {
            if (newSpongyAnchor)
            {
                Debug.Log($"Discarding {newSpongyAnchor.name} (located={newSpongyAnchor.IsLocated}) because still not located");
                newSpongyAnchor = DestroyAnchor(AnchorId.Invalid, newSpongyAnchor);
            }

            newSpongyAnchor = CreateAnchor(NextAnchorId(), worldAnchorParent, pose);
            newAnchorNeighbors = neighbors;
        }

        /// <summary>
        /// If a potential new anchor was prepared (in a previous time step) and is now found to be
        /// located, this routine finalizes it and prepares its edges to be added
        /// </summary>
        /// <param name="newEdges">List that will have new edges appended by this routine</param>
        /// <returns>new anchor id (or Invalid if none was finalized)</returns>
        private AnchorId FinalizeNewAnchor(out List<AnchorEdge> newEdges)
        {
            newEdges = new List<AnchorEdge>();

            if ((newSpongyAnchor == null) || !newSpongyAnchor.IsLocated)
            {
#if WLT_EXTRA_LOGGING
                if (newSpongyAnchor != null)
                {
                    Debug.Log($"Can't finalize {newSpongyAnchor.name} because it's still not located.");
                }
#endif // WLT_EXTRA_LOGGING
                return AnchorId.Invalid;
            }

            AnchorId newId = ClaimAnchorId();
            foreach (var id in newAnchorNeighbors)
            {
                newEdges.Add(new AnchorEdge() { anchorId1 = id, anchorId2 = newId });
            }
            spongyAnchors.Add(new SpongyAnchorWithId()
            {
                anchorId = newId,
                spongyAnchor = newSpongyAnchor
            });
            newSpongyAnchor = null;

            return newId;
        }

        /// <summary>
        /// Return the next available anchor id.
        /// </summary>
        /// <returns>Next available id</returns>
        /// <remarks>
        /// This function doesn't claim the id, only returns what the next will be.
        /// Use ClaimAnchorId() to obtain the next id and keep any other caller from claiming it.
        /// </remarks>
        private AnchorId NextAnchorId()
        {
            return newAnchorId;
        }

        /// <summary>
        /// Claim a unique anchor id.
        /// </summary>
        /// <returns>The exclusive anchor id</returns>
        private AnchorId ClaimAnchorId()
        {
            return newAnchorId++;
        }

        /// <summary>
        /// Save the spongy anchors to persistent storage
        /// </summary>
        public async Task SaveAnchors()
        {
            await SaveAnchors(spongyAnchors);
        }

        // UNITY_WSA - abstract?
        protected virtual async Task SaveAnchors(List<SpongyAnchorWithId> spongyAnchors)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Load the spongy anchors from persistent storage
        /// </summary>
        /// <remarks>
        /// The set of spongy anchors loaded by this routine is defined by the frozen anchors
        /// previously loaded into the plugin.
        /// 
        /// Likewise, when a spongy anchor fails to load, this routine will delete its frozen
        /// counterpart from the plugin.
        /// </remarks>
        /// UNITY_WSA
        public async Task LoadAnchors()
        {
            await LoadAnchors(plugin, newAnchorId, worldAnchorParent, spongyAnchors);

            foreach (var spongyAnchor in spongyAnchors)
            {
                if (newAnchorId <= spongyAnchor.anchorId)
                {
                    newAnchorId = spongyAnchor.anchorId + 1;
                }
            }
        }

        // UNITY_WSA - abstract?
        protected virtual async Task LoadAnchors(IPlugin plugin, AnchorId firstId, Transform parent, List<SpongyAnchorWithId> spongyAnchors)
        {
            await Task.CompletedTask;
        }
    }
}
