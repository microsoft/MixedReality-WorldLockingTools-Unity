// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Optional visualizer of anchors and edges
    /// </summary>
    public class AnchorGraphVisual : MonoBehaviour
    {
        public FrameVisual Prefab_FrameViz;
        public SpongyAnchorVisual Prefab_SpongyAnchorViz;
        public FrozenAnchorVisual Prefab_FrozenAnchorViz;

        private WorldLockingManager manager { get { return WorldLockingManager.GetInstance(); } }

        private FrameVisual spongyWorldViz;
        private GameObject worldLockingVizRoot;
        private Dictionary<FragmentId, FrameVisual> frozenFragmentVizs = new Dictionary<FragmentId, FrameVisual>();

        //private Dictionary<AnchorId, SpongyAnchorVisual> spongyAnchorVizs = new Dictionary<AnchorId, SpongyAnchorVisual>();
        private List<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>> spongyResources = new List<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>>();
        //private Dictionary<AnchorId, FrozenAnchorVisual> frozenAnchorVizs = new Dictionary<AnchorId, FrozenAnchorVisual>();
        private List<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>> frozenResources = new List<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>>();
        //private Dictionary<AnchorEdge, ConnectingLine> edgeVizs = new Dictionary<AnchorEdge, ConnectingLine>();
        private List<SyncLists.IdPair<AnchorEdge, ConnectingLine>> edgeResources = new List<SyncLists.IdPair<AnchorEdge, ConnectingLine>>();
        private Dictionary<AnchorId, ConnectingLine> displacementVizs = new Dictionary<AnchorId, ConnectingLine>();

        private void Reset()
        {
            if (spongyWorldViz != null)
            {
                Destroy(spongyWorldViz.gameObject);
            }
            spongyWorldViz = null;

            if (worldLockingVizRoot != null)
            {
                Destroy(worldLockingVizRoot);
            }
            worldLockingVizRoot = null;

            foreach (var p in frozenFragmentVizs.Values)
            {
                if (p != null)
                {
                    Destroy(p.gameObject);
                }
            }
            frozenFragmentVizs.Clear();

            // mafinc spongyAnchorVizs.Clear();
            spongyResources.Clear();
            // mafinc frozenAnchorVizs.Clear();
            frozenResources.Clear();
            // mafinc edgeVizs.Clear();
            edgeResources.Clear();
            displacementVizs.Clear();
        }

        private void RefitHandler(FragmentId mergedId, FragmentId[] combined)
        {
            Reset();
        }

        private void OnEnable()
        {
            manager.FragmentManager.RegisterForRefitNotifications(RefitHandler);
        }

        private void OnDisable()
        {
            Reset();
            manager.FragmentManager.UnregisterForRefitNotifications(RefitHandler);
        }

        private void Update()
        {
            if (!manager.Enabled)
            {
                Reset();
            }
            else
            {
                UpdateSpongy();
                UpdateFrozen();
            }
        }

        private void UpdateSpongy()
        {
#if false
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            AnchorManager anchorManager = manager.AnchorManager as AnchorManager;
            Debug.Assert(anchorManager != null, "This should not be called without a valid AnchorManager");
            // The spongyWorldViz object is hung off the SpongyFrame.
            if (!spongyWorldViz)
            {
                spongyWorldViz = Instantiate(Prefab_FrameViz, manager.AdjustmentFrame.transform);
                spongyWorldViz.name = "Spongy";
                spongyWorldViz.color = Color.green;
            }

            // Delete any spongy visuals that don't have a corresponding anchor in the anchor manager.
            foreach (var staleId in spongyAnchorVizs.Keys.Except(anchorManager.SpongyAnchors.Keys).ToArray())
            {
                Destroy(spongyAnchorVizs[staleId]);
                spongyAnchorVizs.Remove(staleId);
            }

            // Create any visualizations we're missing.
            foreach (var newId in anchorManager.SpongyAnchors.Keys.Except(spongyAnchorVizs.Keys))
            {
                spongyAnchorVizs[newId] = Prefab_SpongyAnchorViz.Instantiate(
                    spongyWorldViz, 
                    anchorManager.SpongyAnchors[newId].GetComponent<WorldAnchor>());
                break; // create at most one spongy anchor per frame to avoid performance spike
            }

            // Visualize the support relevances.
            var supportRelevances = manager.Plugin.GetSupportRelevances();

            foreach (var id in spongyAnchorVizs.Keys)
            {
                float relevance;
                if (supportRelevances.TryGetValue(id, out relevance))
                {
                    spongyAnchorVizs[id].SetSupportRelevance(relevance);
                }
                else
                {
                    spongyAnchorVizs[id].SetNoSupport();
                }
            }
#else
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            AnchorManager anchorManager = manager.AnchorManager as AnchorManager;
            Debug.Assert(anchorManager != null, "This should not be called without a valid AnchorManager");

            CheckSpongyRoot(manager);

            var spongyCurrentDict = anchorManager.SpongyAnchors;
            List<SyncLists.IdPair<AnchorId, SpongyAnchor>> spongyCurrent = new List<SyncLists.IdPair<AnchorId, SpongyAnchor>>();
            foreach (var item in spongyCurrentDict)
            {
                spongyCurrent.Add(new SyncLists.IdPair<AnchorId, SpongyAnchor>() { id = item.Key, target = item.Value });
            }
            spongyCurrent.Sort(SyncLists.IdPair<AnchorId, SpongyAnchor>.CompareById);

#if false // mafinc
            var resourceDict = spongyAnchorVizs;
            List<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>> spongyResources = new List<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>>();
            foreach (var item in resourceDict)
            {
                spongyResources.Add(new SyncLists.IdPair<AnchorId, SpongyAnchorVisual>() { id = item.Key, target = item.Value });
            }
            spongyResources.Sort(SyncLists.IdPair<AnchorId, SpongyAnchorVisual>.CompareById);
#endif // mafinc

            SpongyVisualCreator spongyCreator = new SpongyVisualCreator(Prefab_SpongyAnchorViz, spongyWorldViz);
            SyncLists.Sync(
                spongyCurrent,
                spongyResources,
                (item, res) => item.id.CompareTo(res.id), 
                spongyCreator.CreateSpongyVisual, 
                spongyCreator.UpdateSpongyVisual,
                spongyCreator.DestroySpongyVisual);


            // Visualize the support relevances.
            var supportRelevances = manager.Plugin.GetSupportRelevances();

            for (int i = 0; i < spongyResources.Count; ++i)
            {
                AnchorId id = spongyResources[i].id;
                float relevance;
                if (supportRelevances.TryGetValue(id, out relevance))
                {
                    spongyResources[i].target.SetSupportRelevance(relevance);
                }
                else
                {
                    spongyResources[i].target.SetNoSupport();
                }
            }
#endif
        }

        private static AnchorEdge RegularizeEdge(AnchorEdge edge)
        {
            if (edge.anchorId2 < edge.anchorId1)
            {
                var id = edge.anchorId2;
                edge.anchorId2 = edge.anchorId1;
                edge.anchorId1 = id;
            }
            return edge;
        }

        private Comparer<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>> spongyAnchorVisualById
            = Comparer<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>>.Create((x, y) => x.id.CompareTo(y.id));
        private Comparer<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>> frozenAnchorVisualById
            = Comparer<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>>.Create((x, y) => x.id.CompareTo(y.id));

        private static int CompareAnchorEdges(AnchorEdge lhs, AnchorEdge rhs)
        {
            Debug.LogError(lhs.anchorId1 < lhs.anchorId2);
            Debug.LogError(rhs.anchorId1 < rhs.anchorId2);
            if (lhs.anchorId1 < rhs.anchorId1)
            {
                return -1;
            }
            if (lhs.anchorId1 > rhs.anchorId1)
            {
                return 1;
            }
            if (lhs.anchorId2 < rhs.anchorId2)
            {
                return -1;
            }
            if (lhs.anchorId2 > rhs.anchorId2)
            {
                return 1;
            }
            return 0;
        }
        private Comparer<AnchorEdge> anchorEdgeComparer = Comparer<AnchorEdge>.Create((x, y) => CompareAnchorEdges(x, y));

        private static SyncLists.IdPair<S, T> FindInSortedList<S, T>(S key, List<SyncLists.IdPair<S, T>> list, IComparer<SyncLists.IdPair<S, T>> comparer)
        {
            SyncLists.IdPair<S, T> item = new SyncLists.IdPair<S, T>() { id = key };
            int idx = list.BinarySearch(item, comparer);
            if (idx < 0)
            {
                Debug.LogError("Item not found in sorted list");
                return null;
            }
            return list[idx];
        }

        private static SyncLists.IdPair<S, T> FindInSortedList<S, T>(S key, List<SyncLists.IdPair<S, T>> list, Comparison<S> comparison)
        {
            var comparer = Comparer<SyncLists.IdPair<S, T>>.Create((lhs, rhs) => comparison(lhs.id, rhs.id));
            return FindInSortedList(key, list, comparer);
        }

        private class SpongyVisualCreator
        {
            private readonly SpongyAnchorVisual Prefab_SpongyAnchorVisual;
            private readonly FrameVisual spongyWorldVisual;

            public SpongyVisualCreator(SpongyAnchorVisual prefab, FrameVisual spongyWorldVisual)
            {
                this.Prefab_SpongyAnchorVisual = prefab;
                this.spongyWorldVisual = spongyWorldVisual;
            }

            public SyncLists.IdPair<AnchorId, SpongyAnchorVisual> CreateSpongyVisual(SyncLists.IdPair<AnchorId, SpongyAnchor> source)
            {
                var spongyAnchorVisual = Prefab_SpongyAnchorVisual.Instantiate(
                    spongyWorldVisual,
                    source.target.GetComponent<WorldAnchor>());

                return new SyncLists.IdPair<AnchorId, SpongyAnchorVisual>()
                {
                    id = source.id,
                    target = spongyAnchorVisual
                };
            }

            public void UpdateSpongyVisual(SyncLists.IdPair<AnchorId, SpongyAnchor> source, SyncLists.IdPair<AnchorId, SpongyAnchorVisual> target)
            {

            }

            public void DestroySpongyVisual(SyncLists.IdPair<AnchorId, SpongyAnchorVisual> target)
            {
                Destroy(target.target);
            }
        }

        private class FrozenAnchorVisualCreator
        {
            private readonly FrozenAnchorVisual Prefab_FrozenAnchorViz;
            private readonly HashSet<FragmentId> activeFragmentIds;
            private readonly Dictionary<FragmentId, FrameVisual> frozenFragmentVisuals;
            private readonly Pose frozenFromLocked;

            public FrozenAnchorVisualCreator(
                FrozenAnchorVisual prefab,
                HashSet<FragmentId> activeFragmentIds,
                Dictionary<FragmentId, FrameVisual> fragmentVisuals,
                Pose frozenFromLocked)
            {
                this.Prefab_FrozenAnchorViz = prefab;
                this.activeFragmentIds = activeFragmentIds;
                this.frozenFragmentVisuals = fragmentVisuals;
                this.frozenFromLocked = frozenFromLocked;
            }

            public SyncLists.IdPair<AnchorId, FrozenAnchorVisual> CreateFrozenVisual(SyncLists.IdPair<AnchorId, FragmentPose> source)
            {
                FragmentId fragmentId = source.target.fragmentId;
                if (!activeFragmentIds.Contains(fragmentId))
                {
                    return null;
                }

                AnchorId anchorId = source.id;
                Pose localPose = source.target.pose;
                localPose = frozenFromLocked.Multiply(localPose);


                // Already ensured this exists in above block.
                FrameVisual frozenFragmentViz = frozenFragmentVisuals[fragmentId];

                // If there isn't a visualization for this anchor, add one.
                FrozenAnchorVisual frozenAnchorVisual;
                frozenAnchorVisual = Prefab_FrozenAnchorViz.Instantiate(anchorId.FormatStr(), frozenFragmentViz);
                frozenAnchorVisual.gameObject.AddComponent<AdjusterMoving>();

                // Put the frozen anchor vis at the world locked transform of the anchor
                frozenAnchorVisual.transform.SetLocalPose(localPose);

                return new SyncLists.IdPair<AnchorId, FrozenAnchorVisual>()
                {
                    id = source.id,
                    target = frozenAnchorVisual
                };
            }

            public void UpdateFrozenVisual(SyncLists.IdPair<AnchorId, FragmentPose> source, SyncLists.IdPair<AnchorId, FrozenAnchorVisual> target)
            {
                Pose localPose = source.target.pose;
                localPose = frozenFromLocked.Multiply(localPose);

                // Put the frozen anchor vis at the world locked transform of the anchor
                target.target.transform.SetLocalPose(localPose);
            }

            public void DestroyFrozenVisual(SyncLists.IdPair<AnchorId, FrozenAnchorVisual> target)
            {
                Destroy(target.target);
            }

        }

        private class FrozenEdgeVisualCreator
        {
            private readonly AnchorGraphVisual owner;
            private readonly List<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>> frozenResources;

            public FrozenEdgeVisualCreator(AnchorGraphVisual owner, List<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>> frozenResources)
            {
                this.owner = owner;
                this.frozenResources = frozenResources;
            }


            public SyncLists.IdPair<AnchorEdge, ConnectingLine> CreateFrozenEdge(AnchorEdge edge)
            {
                var anchorId1 = edge.anchorId1;
                var anchorId2 = edge.anchorId2;

                var frozenAnchor1 = FindInSortedList(anchorId1, frozenResources, (x, y) => x.CompareTo(y));
                var frozenAnchor2 = FindInSortedList(anchorId2, frozenResources, (x, y) => x.CompareTo(y));
                if (frozenAnchor1 == null || frozenAnchor2 == null)
                {
                    return null;
                }

                Color color = Color.blue;
                float width = 0.002f;
                Transform parent = frozenAnchor1.target.transform;

                var edgeVisual = ConnectingLine.Create(parent,
                    frozenAnchor1.target.transform, frozenAnchor2.target.transform,
                    width, color);

                return new SyncLists.IdPair<AnchorEdge, ConnectingLine>()
                {
                    id = edge,
                    target = edgeVisual
                };
            }

            public void UpdateFrozenEdge(AnchorEdge source, SyncLists.IdPair<AnchorEdge, ConnectingLine> target)
            {

            }

            public void DestroyFrozenEdge(SyncLists.IdPair<AnchorEdge, ConnectingLine> target)
            {
                Destroy(target.target);
            }
        }

        private void CheckSpongyRoot(WorldLockingManager manager)
        {
            // The spongyWorldViz object is hung off the SpongyFrame.
            if (!spongyWorldViz)
            {
                spongyWorldViz = Instantiate(Prefab_FrameViz, manager.AdjustmentFrame.transform);
                spongyWorldViz.name = "Spongy";
                spongyWorldViz.color = Color.green;
            }
        }

        private void UpdateFrozen()
        {
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            var plugin = manager.Plugin;

            // mafinc - holder
            var frozenAnchorDict = plugin.GetFrozenAnchors();
            List<SyncLists.IdPair<AnchorId, FragmentPose>> frozenCurrent = new List<SyncLists.IdPair<AnchorId, FragmentPose>>();
            foreach (var item in frozenAnchorDict)
            {
                frozenCurrent.Add(new SyncLists.IdPair<AnchorId, FragmentPose>() { id = item.Key, target = item.Value });
            }
            frozenCurrent.Sort((x, y) => x.id.CompareTo(y.id));

            var activeFragmentIds = UpdateFragmentVisuals();

            /// The "frozen" coordinates here are ignoring the rest of the transform up the camera tree.
            Pose frozenFromLocked = manager.FrozenFromLocked;


            var frozenCreator = new FrozenAnchorVisualCreator(Prefab_FrozenAnchorViz, activeFragmentIds, frozenFragmentVizs, frozenFromLocked);
            SyncLists.Sync(
                frozenCurrent,
                frozenResources, 
                (item, res) => item.id.CompareTo(res.id),
                frozenCreator.CreateFrozenVisual,
                frozenCreator.UpdateFrozenVisual,
                frozenCreator.DestroyFrozenVisual);

#if false // mafinc
            // DisplacementVizs is lines from frozen to spongy anchors. Cull any that we don't have frozen anchors for.
            foreach (var staleId in displacementVizs.Keys.Except(uptodateFrozenAnchors.Keys).ToArray())
            {
                Destroy(displacementVizs[staleId]);
                displacementVizs.Remove(staleId);
            }

            // kv.Key == anchorId
            // kv.Value.fragmentId == fragmentId
            // kv.Value.pose == WorldLockingAnchors[anchorId].tranform, so world locked transform of the anchor
            foreach (var kv in frozenCurrent)
            {
                FragmentId fragmentId = kv.target.fragmentId;
                if (!activeFragmentIds.Contains(fragmentId))
                    continue;

                AnchorId anchorId = kv.id;
                Pose localPose = kv.Value.pose;
                localPose = frozenFromLocked.Multiply(localPose);

                bool breakLoop = false;

                // We just made sure we have all spongy anchor visualizations in UpdateSpongy().

                // if we have both a frozen anchor (assured above) and a spongy anchor (assured in UpdateSpongy() above), 
                // but no connecting line, add one now.
#if false // mafinc
                if (spongyAnchorVizs.ContainsKey(anchorId) && !displacementVizs.ContainsKey(anchorId))
                {
                    var newLine = ConnectingLine.Create(frozenFragmentViz.transform,
                                                        frozenAnchorViz.transform,
                                                        spongyAnchorVizs[anchorId].transform,
                                                        0.01f, Color.red);
                    displacementVizs[anchorId] = newLine;
                    breakLoop = true;
                }
#else
                var spongyResource = FindInSortedList(anchorId, spongyResources, spongyAnchorVisualById);
                if (spongyResource != null && !displacementVizs.ContainsKey(anchorId))
                {
                    var newLine = ConnectingLine.Create(frozenFragmentViz.transform,
                                                        frozenAnchorViz.transform,
                                                        spongyResource.target.transform,
                                                        0.01f, Color.red);
                    displacementVizs[anchorId] = newLine;
                    breakLoop = true;
                }
#endif // mafinc

                if (breakLoop)
                {
                    break; // create at most one frozen anchor per frame to avoid performance spike
                }
            }
#endif // mafinc

#if true // mafinc
            // mafinc - holder
            var uptodateEdges = plugin.GetFrozenEdges();
            List<AnchorEdge> edgeCurrent = new List<AnchorEdge>();
            foreach (var edge in uptodateEdges)
            {
                edgeCurrent.Add(RegularizeEdge(edge));
            }
            edgeCurrent.Sort(anchorEdgeComparer);

            var frozenEdgeCreator = new FrozenEdgeVisualCreator(this, frozenResources);
            SyncLists.Sync(
                edgeCurrent,
                edgeResources,
                (x, y) => CompareAnchorEdges(x, y.id),
                frozenEdgeCreator.CreateFrozenEdge,
                frozenEdgeCreator.UpdateFrozenEdge,
                frozenEdgeCreator.DestroyFrozenEdge);

#endif
        }

        private HashSet<FragmentId> UpdateFragmentVisuals()
        {
            // Go through and find all fragment GameObjects that are active (not disabled).
            var activeFragmentIds = new HashSet<FragmentId>();

            GameObject worldLockingRoot = EnsureWorldLockingVizRoot();
            var fragmentManager = manager.FragmentManager;
            var fragmentIds = fragmentManager.FragmentIds;
            foreach (var fragmentId in fragmentIds)
            {
                if (fragmentManager.GetFragmentState(fragmentId) == AttachmentPointStateType.Normal)
                {
                    activeFragmentIds.Add(fragmentId);
                    FrameVisual frozenFragmentViz;
                    if (!frozenFragmentVizs.TryGetValue(fragmentId, out frozenFragmentViz))
                    {
                        frozenFragmentViz = Instantiate(Prefab_FrameViz, worldLockingRoot.transform);
                        frozenFragmentViz.name = fragmentId.ToString();
                        frozenFragmentVizs[fragmentId] = frozenFragmentViz;
                        frozenFragmentViz.gameObject.AddComponent<AdjusterMoving>();
                    }
                    frozenFragmentViz.color = fragmentId == fragmentManager.CurrentFragmentId ? Color.blue : Color.gray;
                }
            }
            return activeFragmentIds;
        }

        private GameObject EnsureWorldLockingVizRoot()
        {
            if (!worldLockingVizRoot)
            {
                worldLockingVizRoot = new GameObject("WorldLockingViz");
            }
            return worldLockingVizRoot;
        }
    }
}
