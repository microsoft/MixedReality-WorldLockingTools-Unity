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

        private Dictionary<AnchorId, SpongyAnchorVisual> spongyAnchorVizs = new Dictionary<AnchorId, SpongyAnchorVisual>();
        private Dictionary<AnchorId, FrozenAnchorVisual> frozenAnchorVizs = new Dictionary<AnchorId, FrozenAnchorVisual>();
        private Dictionary<AnchorEdge, ConnectingLine> edgeVizs = new Dictionary<AnchorEdge, ConnectingLine>();
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

            spongyAnchorVizs.Clear();
            frozenAnchorVizs.Clear();
            edgeVizs.Clear();
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
        }

        private void UpdateFrozen()
        {
            Debug.Assert(manager != null, "This should not be called without a valid manager");
            var plugin = manager.Plugin;
            var uptodateFrozenAnchors = plugin.GetFrozenAnchors();
            var uptodateEdges = plugin.GetFrozenEdges();

            // Cull out any frozen anchors the DLL doesn't know about
            foreach (var staleId in frozenAnchorVizs.Keys.Except(uptodateFrozenAnchors.Keys).ToArray())
            {
                Destroy(frozenAnchorVizs[staleId]);
                frozenAnchorVizs.Remove(staleId);
            }

            // Cull out any edges the DLL has forgotten about.
            foreach (var staleEdge in edgeVizs.Keys.Except(uptodateEdges).ToArray())
            {
                Destroy(edgeVizs[staleEdge]);
                edgeVizs.Remove(staleEdge);
            }

            // DisplacementVizs is lines from frozen to spongy anchors. Cull any that we don't have frozen anchors for.
            foreach (var staleId in displacementVizs.Keys.Except(uptodateFrozenAnchors.Keys).ToArray())
            {
                Destroy(displacementVizs[staleId]);
                displacementVizs.Remove(staleId);
            }

            // Now go through and find all fragment GameObjects that are active (not disabled).
            // The visualization is still enabled, even though everything in the fragment is disabled.
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
                        frozenFragmentViz = Instantiate(Prefab_FrameViz, worldLockingVizRoot.transform);
                        frozenFragmentViz.name = fragmentId.ToString();
                        frozenFragmentVizs[fragmentId] = frozenFragmentViz;
                        frozenFragmentViz.gameObject.AddComponent<AdjusterMoving>();
                    }
                    frozenFragmentViz.color = fragmentId == fragmentManager.CurrentFragmentId ? Color.blue : Color.gray;
                }
            }

            /// The "frozen" coordinates here are ignoring the rest of the transform up the camera tree.
            Pose frozenFromLocked = manager.FrozenFromLocked;

            // kv.Key == anchorId
            // kv.Value.fragmentId == fragmentId
            // kv.Value.pose == WorldLockingAnchors[anchorId].tranform, so world locked transform of the anchor
            foreach (var kv in uptodateFrozenAnchors)
            {
                FragmentId fragmentId = kv.Value.fragmentId;
                if (!activeFragmentIds.Contains(fragmentId))
                    continue;

                AnchorId anchorId = kv.Key;
                Pose localPose = kv.Value.pose;
                localPose = frozenFromLocked.Multiply(localPose);

                bool breakLoop = false;

                // Already ensured this exists in above block.
                FrameVisual frozenFragmentViz = frozenFragmentVizs[fragmentId];

                // If there isn't a visualization for this anchor, add one.
                FrozenAnchorVisual frozenAnchorViz;
                if (!frozenAnchorVizs.TryGetValue(anchorId, out frozenAnchorViz))
                {
                    frozenAnchorViz = Prefab_FrozenAnchorViz.Instantiate(anchorId.FormatStr(), frozenFragmentViz);
                    frozenAnchorVizs[anchorId] = frozenAnchorViz;
                    frozenAnchorViz.gameObject.AddComponent<AdjusterMoving>();
                    breakLoop = true;
                }

                // Put the frozen anchor vis at the world locked transform of the anchor
                frozenAnchorViz.transform.SetLocalPose(localPose);

                // We just made sure we have all spongy anchor visualizations in UpdateSpongy().

                // if we have both a frozen anchor (assured above) and a spongy anchor (assured in UpdateSpongy() above), 
                // but no connecting line, add one now.
                if (spongyAnchorVizs.ContainsKey(anchorId) && !displacementVizs.ContainsKey(anchorId))
                {
                    var newLine = ConnectingLine.Create(frozenFragmentViz.transform,
                                                        frozenAnchorViz.transform,
                                                        spongyAnchorVizs[anchorId].transform,
                                                        0.01f, Color.red);
                    displacementVizs[anchorId] = newLine;
                    breakLoop = true;
                }

                if (breakLoop)
                {
                    break; // create at most one frozen anchor per frame to avoid performance spike
                }
            }

            foreach (var edge in uptodateEdges.Except(edgeVizs.Keys))
            {
                var anchorId1 = edge.anchorId1;
                var anchorId2 = edge.anchorId2;

                FrozenAnchorVisual frozenAnchorViz1;
                FrozenAnchorVisual frozenAnchorViz2;
                if (!frozenAnchorVizs.TryGetValue(anchorId1, out frozenAnchorViz1) || !frozenAnchorVizs.TryGetValue(anchorId2, out frozenAnchorViz2))
                {
                    continue;
                }

                var fragmentId1 = uptodateFrozenAnchors[anchorId1].fragmentId;
                var fragmentId2 = uptodateFrozenAnchors[anchorId2].fragmentId;

                Color color;
                float width;
                Transform parent;

                if (fragmentId1 == fragmentId2)
                {
                    // regular edges (within a fragment)
                    color = Color.blue;
                    width = 0.001f;
                    parent = frozenAnchorViz1.transform;
                }
                else
                {
                    // inter-fragment edge
                    color = Color.yellow;
                    width = 0.004f;
                    GameObject root = EnsureWorldLockingVizRoot();
                    parent = root.transform;
                }

                edgeVizs[edge] = ConnectingLine.Create(parent,
                    frozenAnchorViz1.transform, frozenAnchorViz2.transform,
                    width, color);

                break; // create at most one edge visualization per frame to avoid performance spike
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
