﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControlTypes;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Experimental.UI.BoundsControl
{
    /// <summary>
    /// Links that are rendered in between the corners of <see cref="BoundsControl"/>
    /// </summary>
    public class Links
    {
        /// <summary>
        /// defines a bounds control link - the line between 2 corners of a box
        /// it keeps track of the transform and the axis the link is representing
        /// </summary>
        private class Link
        {
            public Transform transform;
            public CardinalAxisType axisType;
            public Link(Transform linkTransform, CardinalAxisType linkAxis)
            {
                transform = linkTransform;
                axisType = linkAxis;
            }
        }

        private List<Link> links = new List<Link>();

        private LinksConfiguration config;
        private Vector3 cachedExtents;
        private FlattenModeType cachedFlattenAxis;

        internal Links(LinksConfiguration configuration)
        {
            Debug.Assert(configuration != null, "Can't create BoundsControlLinks without valid configuration");
            config = configuration;
            config.wireFrameChanged.AddListener(UpdateWireframe);
        }

        ~Links()
        {
            config.wireFrameChanged.RemoveListener(UpdateWireframe);
        }

        private void UpdateWireframe(LinksConfiguration.WireframeChangedEventType changedType)
        {
            switch (changedType)
            {
                case LinksConfiguration.WireframeChangedEventType.Visibility:
                    Reset(config.ShowWireFrame, cachedFlattenAxis);
                    break;
                case LinksConfiguration.WireframeChangedEventType.Radius:
                    UpdateLinkScales(cachedExtents);
                    break;
                case LinksConfiguration.WireframeChangedEventType.Shape:
                    UpdateLinkPrimitive();
                    break;
                case LinksConfiguration.WireframeChangedEventType.Material:
                    UpdateMaterial();
                    break;
            }
        }

        internal void Clear()
        {
            if (links != null)
            {
                foreach (Link link in links)
                {
                    Object.Destroy(link.transform.gameObject);
                }
                links.Clear();
            }
        }

        internal void UpdateMaterial()
        {
            if (links != null && config.WireframeMaterial != null)
            {
                for (int i = 0; i < links.Count; ++i)
                {
                    Renderer linkRenderer = links[i].transform.gameObject.GetComponent<Renderer>();
                    if (linkRenderer != null)
                    {
                        linkRenderer.material = config.WireframeMaterial;
                    }
                }
            }
        }

        internal void UpdateVisibilityInInspector(HideFlags flags)
        {
            if (links != null)
            {
                foreach (var link in links)
                {
                    link.transform.hideFlags = flags;
                }
            }
        }

        internal void Reset(bool isVisible, FlattenModeType flattenAxis)
        {
            cachedFlattenAxis = flattenAxis;
            if (links != null)
            {
                for (int i = 0; i < links.Count; ++i)
                {
                    links[i].transform.gameObject.SetActive(isVisible && config.ShowWireFrame);
                }

                int[] flattenedHandles = VisualUtils.GetFlattenedIndices(cachedFlattenAxis);
                if (flattenedHandles != null)
                {
                    for (int i = 0; i < flattenedHandles.Length; ++i)
                    {
                        links[flattenedHandles[i]].transform.gameObject.SetActive(false);
                    }
                }
            }
        }

        private Vector3 GetLinkDimensions(Vector3 currentBoundsExtents)
        {
            float wireframeEdgeRadius = config.WireframeEdgeRadius;
            float linkLengthAdjustor = config.WireframeShape == WireframeType.Cubic ? 2.0f : 1.0f - (6.0f * wireframeEdgeRadius);
            return (currentBoundsExtents * linkLengthAdjustor) + new Vector3(wireframeEdgeRadius, wireframeEdgeRadius, wireframeEdgeRadius);
        }

        internal void UpdateLinkPositions(ref Vector3[] boundsCorners)
        {
            if (boundsCorners != null)
            {
                for (int i = 0; i < links.Count; ++i)
                {
                    links[i].transform.position = VisualUtils.GetLinkPosition(i, ref boundsCorners);
                }
            }
        }

        internal void UpdateLinkScales(Vector3 currentBoundsExtents)
        {
            if (links != null)
            {
                for (int i = 0; i < links.Count; ++i)
                {
                    Transform parent = links[i].transform.parent;
                    Vector3 rootScale = parent.lossyScale;
                    Vector3 invRootScale = new Vector3(1.0f / rootScale[0], 1.0f / rootScale[1], 1.0f / rootScale[2]);
                    // Compute the local scale that produces the desired world space dimensions
                    Vector3 linkDimensions = Vector3.Scale(GetLinkDimensions(currentBoundsExtents), invRootScale);

                    float wireframeEdgeRadius = config.WireframeEdgeRadius;
                    if (links[i].axisType == CardinalAxisType.X)
                    {
                        links[i].transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.x, wireframeEdgeRadius);
                    }
                    else if (links[i].axisType == CardinalAxisType.Y)
                    {
                        links[i].transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.y, wireframeEdgeRadius);
                    }
                    else//Z
                    {
                        links[i].transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.z, wireframeEdgeRadius);
                    }
                }
                cachedExtents = currentBoundsExtents;
            }
        }

        private void UpdateLinkPrimitive()
        {
            GameObject sharedMeshPrimitive;
            if (config.WireframeShape == WireframeType.Cubic)
            {
                sharedMeshPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            else
            {
                sharedMeshPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            }
            var sharedMeshFilter = sharedMeshPrimitive.GetComponent<MeshFilter>();
            if (sharedMeshFilter)
            {
                foreach (var link in links)
                {
                    GameObject linkObj = link.transform.gameObject;
                    // replace shared mesh - note that we don't have a collider to replace in case of wireframe
                    linkObj.GetComponent<MeshFilter>().sharedMesh = sharedMeshFilter.sharedMesh;
                }
            }

            Object.Destroy(sharedMeshPrimitive);
            UpdateLinkScales(cachedExtents);
        }

        internal void CreateLinks(RotationHandles rotationHandles, Transform parent, Vector3 currentBoundsExtents)
        {
            // create links
            if (links != null)
            {
                GameObject link;
                Vector3 linkDimensions = GetLinkDimensions(currentBoundsExtents);
                for (int i = 0; i < RotationHandles.NumEdges; ++i)
                {
                    if (config.WireframeShape == WireframeType.Cubic)
                    {
                        link = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Object.Destroy(link.GetComponent<BoxCollider>());
                    }
                    else
                    {
                        link = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Object.Destroy(link.GetComponent<CapsuleCollider>());
                    }
                    link.name = "link_" + i.ToString();

                    CardinalAxisType axisType = rotationHandles.GetAxisType(i);
                    float wireframeEdgeRadius = config.WireframeEdgeRadius;
                    if (axisType == CardinalAxisType.Y)
                    {
                        link.transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.y, wireframeEdgeRadius);
                        link.transform.Rotate(new Vector3(0.0f, 90.0f, 0.0f));
                    }
                    else if (axisType == CardinalAxisType.Z)
                    {
                        link.transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.z, wireframeEdgeRadius);
                        link.transform.Rotate(new Vector3(90.0f, 0.0f, 0.0f));
                    }
                    else//X
                    {
                        link.transform.localScale = new Vector3(wireframeEdgeRadius, linkDimensions.x, wireframeEdgeRadius);
                        link.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
                    }

                    link.transform.position = rotationHandles.GetEdgeCenter(i);
                    link.transform.parent = parent;
                    Renderer linkRenderer = link.GetComponent<Renderer>();

                    if (config.WireframeMaterial != null)
                    {
                        linkRenderer.material = config.WireframeMaterial;
                    }

                    link.SetActive(config.ShowWireFrame);
                    links.Add(new Link(link.transform, axisType));
                }
            }
        }
    }
}
