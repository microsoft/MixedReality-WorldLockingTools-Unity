// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    public class WorldLockingSetup 
    {
        private static Transform CheckWorldLockingRoot()
        {
            Transform root = null;
            var wltContext = GameObject.FindObjectOfType<WorldLockingContext>();
            if (wltContext != null)
            {
                root = wltContext.transform.parent;
            }
            if (root == null)
            {
                var wltGO = GameObject.Find("WorldLocking");
                if (wltGO != null)
                {
                    root = wltGO.transform;
                }
            }
            if (root == null)
            {
                root = new GameObject("WorldLocking").transform;
            }
            return root;
        }

        private static GameObject InstantiatePrefab(string pathFilter, string name)
        {
            string[] assetGuids = AssetDatabase.FindAssets(name);
            for (int i = 0; i < assetGuids.Length; ++i)
            {
                Debug.Log($"{i}: {AssetDatabase.GUIDToAssetPath(assetGuids[i])}");
            }
            foreach (var guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(pathFilter))
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                    GameObject found = GameObject.Instantiate(obj) as GameObject;
                    found.name = obj.name;
                    return found;
                }
            }
            return null;
        }

        private static WorldLockingContext CheckWorldLockingManager(Transform worldLockingRoot)
        {
            // Look for a WorldLockingContext component in the scene. 
            var wltContext = GameObject.FindObjectOfType<WorldLockingContext>();

            // If not found, instantiate the WorldLockingManager prefab, and attach to WorldLocking root
            if (wltContext == null)
            {
                GameObject wltObject = InstantiatePrefab("WorldLocking.Core/Prefabs", "WorldLockingManager");
                Debug.Assert(wltObject != null, "Missing WorldLockingManager from WorldLocking.Core/Prefabs");
                wltContext = wltObject.GetComponent<WorldLockingContext>();
                Debug.Assert(wltContext != null, "WorldLockingManager prefab corrupt?");
            }
            // Now we definitely have a WorldLockingContext. Make sure it is attached to WorldLocking root object.
            wltContext.transform.parent = worldLockingRoot;
            return wltContext;
        }

        private static void CheckCamera(WorldLockingContext worldLockingContext)
        {
            // Find main camera. If not found, issue warning but we are done.
            if (Camera.main == null)
            {
                Debug.Log($"Scene has no main camera, camera linkage will not be configured.");
                return;
            }
            Transform mainCamera = Camera.main.transform;

            // If the camera doesn't have a parent
            //      Add MRTKPlayspace object, and attach camera to it.
            // If MRTKPlayspace object doesn't have a parent
            //      Add WLTAdjustment object, and attach MRTKPlayspace to it.
            // Set WorldLockingContext CameraParent to MRTKPlayspace object.
            // Set WorldLockingContext Adjustment to WLTAdjustment object.
            if (mainCamera.parent == null)
            {
                mainCamera.parent = new GameObject("MixedRealityPlayspace").transform;
            }
            Transform mrtkPlayspace = mainCamera.parent;
            if (mrtkPlayspace.parent == null)
            {
                mrtkPlayspace.parent = new GameObject("WLT_Adjustment").transform;
            }
            Transform wltAdjustment = mrtkPlayspace.parent;

            var sharedSettings = worldLockingContext.SharedSettings;
            sharedSettings.linkageSettings.CameraParent = mrtkPlayspace;
            sharedSettings.linkageSettings.AdjustmentFrame = wltAdjustment;
        }

        [MenuItem("Mixed Reality Toolkit/Utilities/World Locking Tools/Add to scene")]
        private static void AddWorldLockingToScene()
        {
            // Look for WorldLocking root object in scene.
            // If not found, add one.
            Transform worldLockingRoot = CheckWorldLockingRoot();

            WorldLockingContext worldLockingContext = CheckWorldLockingManager(worldLockingRoot);

            CheckCamera(worldLockingContext);

            Selection.activeObject = worldLockingContext.gameObject;
        }

        private static void AddAnchorVisualizer(Transform wltRoot)
        {
            AnchorGraphVisual anchorVisual = GameObject.FindObjectOfType<AnchorGraphVisual>();
            if (anchorVisual == null)
            {
                GameObject anchorVisualObject = InstantiatePrefab("WorldLocking.Tools/Prefabs", "AnchorGraphVisual");
                anchorVisual = anchorVisualObject.GetComponent<AnchorGraphVisual>();
            }
            Debug.Assert(anchorVisual != null, "Missing AnchorGraphVisual prefab?");
            anchorVisual.transform.parent = wltRoot;
        }

        private static void AddGlobalSpacePinVisualizer(Transform wltRoot, SpacePinMeshVisualizer[] visualizers)
        {
            List<SpacePinMeshVisualizer> globalVisualizers = new List<SpacePinMeshVisualizer>();
            foreach (var vis in visualizers)
            {
                if (vis.TargetSubtree == null)
                {
                    globalVisualizers.Add(vis);
                }
            }
            if (globalVisualizers.Count > 1)
            {
                // We have too many, there should be exactly one when we're done, zero or one right now.
                Debug.LogError($"Found too many global space pin visualizers in the scene, deleting all but one.");
                for (int i = 1; i < globalVisualizers.Count; ++i)
                {
                    Debug.Log($"Deleting global space pin visualizer {globalVisualizers[i].name}");
                    GameObject.DestroyImmediate(globalVisualizers[i]);
                }
            }
            else if (globalVisualizers.Count == 0)
            {
                GameObject newVis = InstantiatePrefab("WorldLocking.Tools/Prefabs", "SpacePinVisualizer");
                newVis.name = $"{newVis.name} (Global)";
                newVis.transform.parent = wltRoot;
            }

        }

        private static void AddSubtreeSpacePinVisualizers(Transform wltRoot, SpacePinMeshVisualizer[] visualizers)
        {
            AlignSubtree[] subtrees = GameObject.FindObjectsOfType<AlignSubtree>();

            foreach (var subtree in subtrees)
            {
                bool found = false;
                foreach(var vis in visualizers)
                {
                    if (vis.TargetSubtree == subtree)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    GameObject newVis = InstantiatePrefab("WorldLocking.Tools/Prefabs", "SpacePinVisualizer");
                    newVis.name = $"{newVis.name} ({subtree.name})";
                    newVis.transform.parent = wltRoot;
                    var visualizer = newVis.GetComponent<SpacePinMeshVisualizer>();
                    visualizer.TargetSubtree = subtree;
                }
                     
            }
        }

        private static void AddSpacePinVisualizers(Transform wltRoot)
        {
            SpacePinMeshVisualizer[] visualizers = GameObject.FindObjectsOfType<SpacePinMeshVisualizer>();

            AddGlobalSpacePinVisualizer(wltRoot, visualizers);

            AddSubtreeSpacePinVisualizers(wltRoot, visualizers);
        }

        [MenuItem("Mixed Reality Toolkit/Utilities/World Locking Tools/Add visualizers")]
        private static void AddWorldLockingVisualizers()
        {
            Transform worldLockingRoot = CheckWorldLockingRoot();

            AddAnchorVisualizer(worldLockingRoot);

            AddSpacePinVisualizers(worldLockingRoot);

            Selection.activeObject = worldLockingRoot.gameObject;
        }

        [MenuItem("Mixed Reality Toolkit/Utilities/World Locking Tools/Remove visualizers")]
        private static void RemoveWorldLockingVisualisers()
        {
            AnchorGraphVisual[] anchorVisuals = GameObject.FindObjectsOfType<AnchorGraphVisual>();
            foreach( var vis in anchorVisuals)
            {
                GameObject.DestroyImmediate(vis.gameObject);
            }

            SpacePinMeshVisualizer[] visualizers = GameObject.FindObjectsOfType<SpacePinMeshVisualizer>();
            foreach(var vis in visualizers)
            {
                GameObject.DestroyImmediate(vis.gameObject);
            }
        }

    }
}
