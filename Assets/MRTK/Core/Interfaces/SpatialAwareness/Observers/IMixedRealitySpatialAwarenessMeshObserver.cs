﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.SpatialAwareness
{
    /// <summary>
    /// The interface for defining an <see cref="IMixedRealitySpatialAwarenessObserver"/> which provides mesh data.
    /// </summary>
    public interface IMixedRealitySpatialAwarenessMeshObserver : IMixedRealitySpatialAwarenessObserver
    {
        /// <summary>
        /// Gets or sets a value indicating how the mesh subsystem is to display surface meshes within the application.
        /// </summary>
        /// <remarks>
        /// Applications that wish to process the <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>es should set this value to None.
        /// </remarks>
        SpatialAwarenessMeshDisplayOptions DisplayOption { get; set; }

        /// <summary>
        /// Gets or sets the level of detail, as a MixedRealitySpatialAwarenessMeshLevelOfDetail value, for the returned spatial mesh.
        /// Setting this value to Custom, implies that the developer is specifying a custom value for MeshTrianglesPerCubicMeter. 
        /// </summary>
        /// <remarks>
        /// Specifying any other value will cause <see cref="TrianglesPerCubicMeter"/> to be overwritten.
        /// </remarks>
        SpatialAwarenessMeshLevelOfDetail LevelOfDetail { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessMeshObject"/>s being managed by the observer.
        /// </summary>
        IReadOnlyDictionary<int, SpatialAwarenessMeshObject> Meshes { get; }

        /// <summary>
        /// Get or sets the desired Unity Physics Layer on which to set the spatial mesh.
        /// </summary>
        /// <remarks>
        /// If not explicitly set, it is recommended that implementations return <see cref="IMixedRealitySpatialAwarenessObserver.DefaultPhysicsLayer"/>.
        /// </remarks>
        int MeshPhysicsLayer { get; set; }

        /// <summary>
        /// Gets the bit mask that corresponds to the value specified in <see cref="MeshPhysicsLayer"/>.
        /// </summary>
        int MeshPhysicsLayerMask { get; }

        /// <summary>
        /// Indicates whether or not mesh normals should be recalculated by the observer.
        /// </summary>
        bool RecalculateNormals { get; set; }

        /// <summary>
        /// Gets or sets the level of detail, in triangles per cubic meter, for the returned spatial mesh.
        /// </summary>
        /// <remarks>
        /// When specifying a <see cref="LevelOfDetail"/> other than Custom, this value will be automatically overwritten with system default values.
        /// </remarks>
        int TrianglesPerCubicMeter { get; set; }

        /// <summary>
        /// Gets or sets the <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to be used when spatial <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>es should occlude other objects.
        /// </summary>
        Material OcclusionMaterial { get; set; }

        /// <summary>
        /// Gets or sets the <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to be used when displaying <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">Mesh</see>es.
        /// </summary>
        Material VisibleMaterial { get; set; }
    }
}