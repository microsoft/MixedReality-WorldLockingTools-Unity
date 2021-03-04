﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAwareness
{
    /// <summary>
    /// Object encapsulating the components of a spatial awareness mesh object.
    /// </summary>
    public class SpatialAwarenessMeshObject : BaseSpatialAwarenessObject
    {
        /// <summary>
        /// When a mesh is created we will need to create a game object with a minimum 
        /// set of components to contain the mesh.  These are the required component types.
        /// </summary>
        private static readonly Type[] RequiredMeshComponents =
        {
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(MeshCollider)
        };

        /// <summary>
        /// The collider for the mesh object.
        /// </summary>
        public MeshCollider Collider { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        private SpatialAwarenessMeshObject() : base() { }

        /// <summary>
        /// Creates a <see cref="SpatialAwarenessMeshObject"/>.
        /// </summary>
        /// <returns>
        /// SpatialMeshObject containing the fields that describe the mesh.
        /// </returns>
        public static SpatialAwarenessMeshObject Create(
            Mesh mesh,
            int layer,
            string name,
            int meshId,
            GameObject meshParent = null)
        {
            SpatialAwarenessMeshObject newMesh = new SpatialAwarenessMeshObject
            {
                Id = meshId,
                GameObject = new GameObject(name, RequiredMeshComponents)
                {
                    layer = layer
                }
            };

            /// Preserve local transform when attaching to parent.
            newMesh.GameObject.transform.SetParent(meshParent != null ? meshParent.transform : null, false);

            newMesh.Filter = newMesh.GameObject.GetComponent<MeshFilter>();
            newMesh.Filter.sharedMesh = mesh;

            newMesh.Renderer = newMesh.GameObject.GetComponent<MeshRenderer>();

            // Reset the surface mesh collider to fit the updated mesh. 
            // Unity tribal knowledge indicates that to change the mesh assigned to a
            // mesh collider, the mesh must first be set to null.  Presumably there
            // is a side effect in the setter when setting the shared mesh to null.
            newMesh.Collider = newMesh.GameObject.GetComponent<MeshCollider>();
            newMesh.Collider.sharedMesh = null;
            newMesh.Collider.sharedMesh = newMesh.Filter.sharedMesh;

            return newMesh;
        }

        /// <summary>
        /// Clean up the resources associated with the surface.
        /// </summary>
        /// <param name="meshObject">The <see cref="SpatialAwarenessMeshObject"/> whose resources will be cleaned up.</param>
        public static void Cleanup(SpatialAwarenessMeshObject meshObject, bool destroyGameObject = true, bool destroyMeshes = true)
        {
            if (meshObject.GameObject == null)
            {
                return;
            }

            if (destroyGameObject)
            {
                UnityEngine.Object.Destroy(meshObject.GameObject);
                meshObject.GameObject = null;
                return;
            }

            if (destroyMeshes)
            {
                Mesh filterMesh = meshObject.Filter.sharedMesh;
                Mesh colliderMesh = meshObject.Collider.sharedMesh;

                if (filterMesh != null)
                {
                    UnityEngine.Object.Destroy(filterMesh);
                    meshObject.Filter.sharedMesh = null;
                }

                if ((colliderMesh != null) && (colliderMesh != filterMesh))
                {
                    UnityEngine.Object.Destroy(colliderMesh);
                    meshObject.Collider.sharedMesh = null;
                }
            }
        }

    }
}