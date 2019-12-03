﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.XR.WSA;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Component for controlling location, visual appearance and ID text of a spongy anchor visualization.
    /// </summary>
    /// <remarks>
    /// Each spongy anchor is paired with a WorldAnchor component connected to a different GameObject.
    /// The WorldAnchor component has its globalPose controlled by Unity, which may not appear correct when
    /// the camera position is adjusted by FrozenWorld.
    /// Therefore this component allows connecting a separate visualization GameObject located in the camera's frame
    /// of reference while the WorldAnchor remains invisible in the top-level Unity frame of reference, keeping the
    /// localPose of both objects in sync.
    /// 
    /// Spongy anchors are visualized by a concentric pair of an outer ring and an inner disc
    /// The outer ring of fixed size indicates the state of the WorldAnchor by its color:
    /// 
    /// green: support(area of inner circle indicating relevance)
    /// red: support with zero relevance
    /// yellow: not a support 
    /// gray: WorldAnchor not located(i.e.currently not part of spongy world)
    /// 
    /// The inner disc indicates the relevance of the spongy anchor (0..100%) by its area.
    /// </remarks>
    public class SpongyAnchorVisual : MonoBehaviour
    {
        /// <summary>
        /// The WorldAnchor on a separate GameObject for syncing this GameObject's localPose")]
        /// </summary>
        private WorldAnchor worldAnchor = null;

        [SerializeField]
        [Tooltip("The child object that will have its color controlled")]
        private Renderer ringObject = null;

        [SerializeField]
        [Tooltip("The child object that will have its scale controlled")]
        private GameObject discObject = null;

        [SerializeField]
        [Tooltip("The child Text object that will have its color and text controlled")]
        private TextMesh textObject = null;

        private Color color;

        /// <summary>
        /// Create a visualizer for a spongy anchor.
        /// </summary>
        /// <param name="parent">Coordinate space to create the visualizer in</param>
        /// <param name="worldAnchor">The worldanchor component assigned to some other object that this object is supposed to sync with</param>
        /// <returns></returns>
        public SpongyAnchorVisual Instantiate(FrameVisual parent, WorldAnchor worldAnchor)
        {
            var res = Instantiate(this, parent.transform);
            res.name = worldAnchor.name;
            res.textObject.text = res.name;
            res.worldAnchor = worldAnchor;
            res.color = Color.gray;
            return res;
        }

        private void Update()
        {
            var color = this.color;

            // Unity's implementation of WorldAnchor adjusts its global transform to track the
            // SpatialAnchor coordinate system. Here we want to keep the local transform of the visualization
            // towards its parent to track the SpatialAnchor, so we copy the global pose of the worldanchor to the
            // local pose of the visualization object.
            // This is because the visualizations tree is rooted at the SpongyFrame, which also contains the camera (and MRTK Playspace).
            // The SpongyFrame is adjusted by FrozeWorld every frame. This means that giving a transform M relative to the SpongyFrame,
            // as done here, will put the object _relative to the camera_ in the same place as setting M as the world transform
            // if SpongyFrame wasn't there, i.e. Unity World Space.
            transform.SetLocalPose(worldAnchor.transform.GetGlobalPose());

            if (!worldAnchor.isLocated)
                color = Color.gray;

            ringObject.material.color = color;
            textObject.color = color;

        }

        /// <summary>
        /// Set the relevance, which sets the color.
        /// </summary>
        /// <param name="relevance">The new relevance</param>
        public void SetSupportRelevance(float relevance)
        {
            if(relevance > 0.0f)
            {
                color = Color.green;
                var rad = (float)Math.Sqrt(relevance);
                discObject.SetActive(true);
                discObject.transform.localScale = new Vector3(rad, 1.0f, rad);
            }
            else
            {
                color = Color.red;
                discObject.SetActive(false);
            }
        }

        /// <summary>
        /// Declare as not being a support.
        /// </summary>
        public void SetNoSupport()
        {
            color = Color.yellow;
            discObject.SetActive(false);
        }
    }
}
