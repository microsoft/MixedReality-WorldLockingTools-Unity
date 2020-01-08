﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
        //private Dictionary<AnchorId, ConnectingLine> displacementVizs = new Dictionary<AnchorId, ConnectingLine>();
        private List<SyncLists.IdPair<AnchorId, ConnectingLine>> displacementResources = new List<SyncLists.IdPair<AnchorId, ConnectingLine>>();

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

            spongyResources.Clear();
            frozenResources.Clear();
            edgeResources.Clear();
            displacementResources.Clear();
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

            SpongyVisualCreator spongyCreator = new SpongyVisualCreator(Prefab_SpongyAnchorViz, spongyWorldViz);
            SyncLists.Sync(
                spongyCurrent,
                spongyResources,
                (item, res) => item.id.CompareTo(res.id),
                spongyCreator.CreateSpongyVisual,
                spongyCreator.UpdateSpongyVisual,
                spongyCreator.DestroySpongyVisual);

            // mafinc - no dictionary from plugin
            // Visualize the support relevances.
            var supportRelevancesSource = manager.Plugin.GetSupportRelevances();
            List<SyncLists.IdPair<AnchorId, float>> supportRelevances = new List<SyncLists.IdPair<AnchorId, float>>();
            foreach (var support in supportRelevancesSource)
            {
                supportRelevances.Add(
                    new SyncLists.IdPair<AnchorId, float>()
                    {
                        id = support.anchorId,
                        target = support.relevance
                    }
                );
            }
            supportRelevances.Sort((x, y) => x.id.CompareTo(y.id));

            int iSupport = 0;
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                // Skip any supports with a lower id, these have no corresponding spongy resource.
                while (iSupport < supportRelevances.Count && supportRelevances[iSupport].id < spongyResources[iSpongy].id)
                {
                    ++iSupport;
                }

                if (iSupport < supportRelevances.Count && supportRelevances[iSupport].id == spongyResources[iSpongy].id)
                {
                    spongyResources[iSpongy].target.SetSupportRelevance(supportRelevances[iSupport++].target);
                }
                else
                {
                    spongyResources[iSpongy].target.SetNoSupport();
                }
            }
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
            Debug.Assert(lhs.anchorId1 < lhs.anchorId2);
            Debug.Assert(rhs.anchorId1 < rhs.anchorId2);
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
            private readonly Dictionary<FragmentId, FrameVisual> frozenFragmentVisuals;
            private readonly Pose frozenFromLocked;

            public FrozenAnchorVisualCreator(
                FrozenAnchorVisual prefab,
                Dictionary<FragmentId, FrameVisual> fragmentVisuals,
                Pose frozenFromLocked)
            {
                this.Prefab_FrozenAnchorViz = prefab;
                this.frozenFragmentVisuals = fragmentVisuals;
                this.frozenFromLocked = frozenFromLocked;
            }

            public SyncLists.IdPair<AnchorId, FrozenAnchorVisual> CreateFrozenVisual(SyncLists.IdPair<AnchorId, FragmentPose> source)
            {
                // Already ensured this fragment exists.
                FragmentId fragmentId = source.target.fragmentId;

                AnchorId anchorId = source.id;

                FrameVisual frozenFragmentViz = frozenFragmentVisuals[fragmentId];

                // If there isn't a visualization for this anchor, add one.
                FrozenAnchorVisual frozenAnchorVisual;
                frozenAnchorVisual = Prefab_FrozenAnchorViz.Instantiate(anchorId.FormatStr(), frozenFragmentViz);
                frozenAnchorVisual.gameObject.AddComponent<AdjusterMoving>();

                // Put the frozen anchor vis at the world locked transform of the anchor
                SetPose(source, frozenAnchorVisual);

                return new SyncLists.IdPair<AnchorId, FrozenAnchorVisual>()
                {
                    id = source.id,
                    target = frozenAnchorVisual
                };
            }

            private void SetPose(SyncLists.IdPair<AnchorId, FragmentPose> source, FrozenAnchorVisual target)
            {
                Pose localPose = source.target.pose;
                localPose = frozenFromLocked.Multiply(localPose);
                localPose.position.y += 0.25f;
                target.transform.SetLocalPose(localPose);
            }

            public void UpdateFrozenVisual(SyncLists.IdPair<AnchorId, FragmentPose> source, SyncLists.IdPair<AnchorId, FrozenAnchorVisual> target)
            {
                SetPose(source, target.target);
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

                Transform parent1 = frozenAnchor1.target.transform.parent;
                Transform parent2 = frozenAnchor2.target.transform.parent;
                bool sameFragment = parent1 == parent2;
                Color color = Color.blue;
                float width = 0.002f;
                Transform parent = parent1;

                if (!sameFragment)
                {
                    color = Color.yellow;
                    width = 0.004f;
                }

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

        private class DisplacementCreator
        {
            public DisplacementCreator()
            {
            }

            public SyncLists.IdPair<AnchorId, ConnectingLine> CreateDisplacement(AnchorId id, FrozenAnchorVisual frozen, SpongyAnchorVisual spongy)
            {
                var newLine = ConnectingLine.Create(spongy.transform.parent,
                                    frozen.transform,
                                    spongy.transform,
                                    0.01f, Color.red);

                return new SyncLists.IdPair<AnchorId, ConnectingLine>()
                {
                    id = id,
                    target = newLine
                };
            }

            public void DestroyDisplacement(SyncLists.IdPair<AnchorId, ConnectingLine> target)
            {
                Destroy(target.target);
            }

            public bool ShouldConnect(
                SyncLists.IdPair<AnchorId, FrozenAnchorVisual> frozen, 
                SyncLists.IdPair<AnchorId, SpongyAnchorVisual> spongy)
            {
                if (frozen.id != spongy.id)
                {
                    return false;
                }
                float MinDistanceSquared = 0.01f; // one centimeter.
                float distanceSq = (frozen.target.transform.position - spongy.target.transform.position).sqrMagnitude;
                if (distanceSq < MinDistanceSquared)
                {
                    return false;
                }
                return true;
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

            var frozenAnchorSource = plugin.GetFrozenAnchors();
            List<SyncLists.IdPair<AnchorId, FragmentPose>> frozenItems = new List<SyncLists.IdPair<AnchorId, FragmentPose>>();
            foreach (var item in frozenAnchorSource)
            {
                frozenItems.Add(
                    new SyncLists.IdPair<AnchorId, FragmentPose>()
                    {
                        id = item.anchorId,
                        target = item.fragmentPose
                    }
                );
            }
            frozenItems.Sort((x, y) => x.id.CompareTo(y.id));

            UpdateFragmentVisuals();

            /// The "frozen" coordinates here are ignoring the rest of the transform up the camera tree.
            Pose frozenFromLocked = manager.FrozenFromLocked;

            var frozenCreator = new FrozenAnchorVisualCreator(Prefab_FrozenAnchorViz, frozenFragmentVizs, frozenFromLocked);
            SyncLists.Sync(
                frozenItems,
                frozenResources, 
                (item, res) => item.id.CompareTo(res.id),
                frozenCreator.CreateFrozenVisual,
                frozenCreator.UpdateFrozenVisual,
                frozenCreator.DestroyFrozenVisual);

            // Connect frozen anchors with corresponding spongy anchors with a line.
            DisplacementCreator displacementCreator = new DisplacementCreator();
            SyncDisplacements(displacementCreator,
                frozenResources,
                spongyResources,
                displacementResources);

            var edgesSource = plugin.GetFrozenEdges();
            List<AnchorEdge> edgeItems = new List<AnchorEdge>();
            foreach (var edge in edgesSource)
            {
                edgeItems.Add(RegularizeEdge(edge));
            }
            edgeItems.Sort(anchorEdgeComparer);

            var frozenEdgeCreator = new FrozenEdgeVisualCreator(this, frozenResources);
            SyncLists.Sync(
                edgeItems,
                edgeResources,
                (x, y) => CompareAnchorEdges(x, y.id),
                frozenEdgeCreator.CreateFrozenEdge,
                frozenEdgeCreator.UpdateFrozenEdge,
                frozenEdgeCreator.DestroyFrozenEdge);

        }

        private void SyncDisplacements(
            DisplacementCreator displacementCreator,
            List<SyncLists.IdPair<AnchorId, FrozenAnchorVisual>> frozenResources,
            List<SyncLists.IdPair<AnchorId, SpongyAnchorVisual>> spongyResources,
            List<SyncLists.IdPair<AnchorId, ConnectingLine>> displacementResources)
        {
            int iFrozen = 0;
            int iDisplace = 0;
            for (int iSpongy = 0; iSpongy < spongyResources.Count; ++iSpongy)
            {
                while (iFrozen < frozenResources.Count && frozenResources[iFrozen].id < spongyResources[iSpongy].id)
                {
                    iFrozen++;
                }
                /// If we've reached the end of the frozen resources, we're finished creating.
                if (iFrozen >= frozenResources.Count)
                {
                    break;
                }
                if (displacementCreator.ShouldConnect(frozenResources[iFrozen], spongyResources[iSpongy]))
                {
                    AnchorId id = frozenResources[iFrozen].id;
                    Debug.Assert(id == spongyResources[iSpongy].id);
                    while (iDisplace < displacementResources.Count && displacementResources[iDisplace].id < id)
                    {
                        displacementCreator.DestroyDisplacement(displacementResources[iDisplace]);
                        displacementResources.RemoveAt(iDisplace);
                    }
                    Debug.Assert(iDisplace <= displacementResources.Count);
                    Debug.Assert(iDisplace == displacementResources.Count || displacementResources[iDisplace].id >= id);
                    if (iDisplace == displacementResources.Count || displacementResources[iDisplace].id > id)
                    {
                        displacementResources.Insert(iDisplace,
                            displacementCreator.CreateDisplacement(
                                id,
                                frozenResources[iFrozen].target,
                                spongyResources[iSpongy].target));
                    }
                    ++iDisplace;
                }
            }
            // Finished creating. Now destroy any displacements further in the list, as they no longer have matching
            // frozen/spongy pairs.
            Debug.Assert(iDisplace <= displacementResources.Count);
            int displacementCount = iDisplace;
            while (iDisplace < displacementResources.Count)
            {
                displacementCreator.DestroyDisplacement(displacementResources[iDisplace++]);
            }
            displacementResources.RemoveRange(displacementCount, displacementResources.Count - displacementCount);
            Debug.Assert(displacementResources.Count == displacementCount);
        }

        private void UpdateFragmentVisuals()
        {
            GameObject worldLockingRoot = EnsureWorldLockingVizRoot();
            var fragmentManager = manager.FragmentManager;
            var fragmentIds = fragmentManager.FragmentIds;
            foreach (var fragmentId in fragmentIds)
            {
                FrameVisual frozenFragmentViz;
                if (!frozenFragmentVizs.TryGetValue(fragmentId, out frozenFragmentViz))
                {
                    frozenFragmentViz = Instantiate(Prefab_FrameViz, worldLockingRoot.transform);
                    frozenFragmentViz.name = fragmentId.ToString();
                    frozenFragmentVizs[fragmentId] = frozenFragmentViz;
                    frozenFragmentViz.gameObject.AddComponent<AdjusterMoving>();
                }
                Color fragmentColor = Color.gray;
                if (fragmentId == fragmentManager.CurrentFragmentId)
                {
                    fragmentColor = Color.blue;
                }
                if (fragmentManager.GetFragmentState(fragmentId) != AttachmentPointStateType.Normal)
                {
                    fragmentColor = Color.red;
                }
                frozenFragmentViz.color = fragmentColor;
            }
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
