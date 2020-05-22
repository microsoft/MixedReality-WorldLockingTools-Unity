﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Utilities
{
    /// <summary>
    /// Utility component to animate and visualize a light that can be used with 
    /// the "MixedRealityToolkit/Standard" shader "_HoverLight" feature.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Rendering/HoverLight.html")]
    [AddComponentMenu("Scripts/MRTK/Core/HoverLight")]
    public class HoverLight : MonoBehaviour
    {
        // Two hover lights are supported at this time.
        private const int hoverLightCount = 2;
        private const int hoverLightDataSize = 2;
        private static List<HoverLight> activeHoverLights = new List<HoverLight>(hoverLightCount);
        private static Vector4[] hoverLightData = new Vector4[hoverLightCount * hoverLightDataSize];
        private static int _HoverLightDataID;
        private static int lastHoverLightUpdate = -1;

        [Tooltip("Specifies the radius of the HoverLight effect")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float radius = 0.15f;

        /// <summary>
        /// Specifies the Radius of the HoverLight effect
        /// </summary>
        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        [Tooltip("Specifies the highlight color")]
        [SerializeField]
        private Color color = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        /// <summary>
        /// Specifies the highlight color
        /// </summary>
        public Color Color
        {
            get => color;
            set => color = value;
        }

        private void OnEnable()
        {
            AddHoverLight(this);
        }

        private void OnDisable()
        {
            RemoveHoverLight(this);
            UpdateHoverLights(true);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Initialize();
            UpdateHoverLights();
        }
#endif // UNITY_EDITOR

        private void LateUpdate()
        {
            UpdateHoverLights();
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled)
            {
                return;
            }

            Gizmos.color = Color;
            Gizmos.DrawWireSphere(transform.position, Radius);
            Gizmos.DrawIcon(transform.position + Vector3.right * Radius, string.Empty, false);
            Gizmos.DrawIcon(transform.position + Vector3.left * Radius, string.Empty, false);
            Gizmos.DrawIcon(transform.position + Vector3.up * Radius, string.Empty, false);
            Gizmos.DrawIcon(transform.position + Vector3.down * Radius, string.Empty, false);
            Gizmos.DrawIcon(transform.position + Vector3.forward * Radius, string.Empty, false);
            Gizmos.DrawIcon(transform.position + Vector3.back * Radius, string.Empty, false);
        }

        private void AddHoverLight(HoverLight light)
        {
            if (activeHoverLights.Count >= hoverLightCount)
            {
                Debug.LogWarningFormat("Max hover light count ({0}) exceeded.", hoverLightCount);
            }

            activeHoverLights.Add(light);
        }

        private void RemoveHoverLight(HoverLight light)
        {
            activeHoverLights.Remove(light);
        }

        private void Initialize()
        {
            _HoverLightDataID = Shader.PropertyToID("_HoverLightData");
        }

        private void UpdateHoverLights(bool forceUpdate = false)
        {
            if (lastHoverLightUpdate == -1)
            {
                Initialize();
            }

            if (!forceUpdate && (Time.frameCount == lastHoverLightUpdate))
            {
                return;
            }

            for (int i = 0; i < hoverLightCount; ++i)
            {
                HoverLight light = (i >= activeHoverLights.Count) ? null : activeHoverLights[i];
                int dataIndex = i * hoverLightDataSize;

                if (light)
                {
                    hoverLightData[dataIndex] = new Vector4(light.transform.position.x,
                                                            light.transform.position.y,
                                                            light.transform.position.z,
                                                            1.0f);
                    hoverLightData[dataIndex + 1] = new Vector4(light.Color.r,
                                                                light.Color.g,
                                                                light.Color.b,
                                                                1.0f / Mathf.Clamp(light.Radius, 0.001f, 1.0f));
                }
                else
                {
                    hoverLightData[dataIndex] = Vector4.zero;
                }
            }

            Shader.SetGlobalVectorArray(_HoverLightDataID, hoverLightData);

            lastHoverLightUpdate = Time.frameCount;
        }
    }
}