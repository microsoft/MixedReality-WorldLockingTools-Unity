﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions.SceneTransitions
{
    /// <summary>
    /// Quad-based implementation if ICameraFader. Instantiates quads in front of cameras to achieve fade out / in effect.
    /// </summary>
    public class CameraFaderQuad : ICameraFader
    {
        const string QuadMaterialShaderName = "Sprites/Default";
        const string QuadMaterialColorName = "_Color";

        /// <summary>
        /// Simple struct for keeping track of quad properties
        /// </summary>
        private struct Quad
        {
            public Renderer Renderer;
            // Eventually we want to be able to have different quads use different colors
            // Using property blocks now keeps our options open
            public MaterialPropertyBlock PropertyBlock;
        }

        /// <inheritdoc />
        public CameraFaderState State { get; private set; }

        private Dictionary<Camera, Quad> quads = new Dictionary<Camera, Quad>();
        private Color fadeOutColor;
        private Color fadeInColor;
        private Color currentColor;
        private Material quadMaterial;

        /// <inheritdoc />
        public async Task FadeOutAsync(float fadeOutTime, Color color, IEnumerable<Camera> targets)
        {
            switch (State)
            {
                case CameraFaderState.Clear:
                    break;

                default:
                    Debug.LogWarning("Can't fade out in state " + State + " - not proceeding.");
                    return;
            }

            State = CameraFaderState.FadingOut;

            fadeOutColor = color;
            fadeInColor = fadeOutColor;
            fadeInColor.a = 0;

            if (fadeOutColor.a < 1)
            {
                Debug.LogWarning("Target color is not fully opaque.");
            }

            // Create our material
            if (quadMaterial == null)
            {
                try
                {
                    quadMaterial = new Material(Shader.Find(QuadMaterialShaderName));
                }
                catch (Exception e)
                {
                    Debug.LogError("Error when trying to create quad material in CameraFaderQuad");
                    Debug.LogException(e);
                    return;
                }
            }

            quadMaterial.SetColor(QuadMaterialColorName, currentColor);

            // Create our quads
            foreach (Camera camera in targets)
            {
                // Can't target the same camera twice
                if (quads.ContainsKey(camera))
                    continue;

                Quad quad = new Quad();
                quad.PropertyBlock = new MaterialPropertyBlock();
                quad.Renderer = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Renderer>();
                quad.Renderer.sharedMaterial = quadMaterial;
                // Parent the quad under the camera
                quad.Renderer.transform.parent = camera.transform;
                quad.Renderer.transform.localScale = camera.orthographic ? Vector3.one * camera.orthographicSize : Vector3.one * camera.fieldOfView;
                quad.Renderer.transform.localPosition = Vector3.forward * camera.nearClipPlane * 1.01f;
                quad.Renderer.transform.localRotation = Quaternion.identity;

                // Set the quad's layer to something the camera sees
                for (int layer = 0; layer < 32; layer++)
                {
                    if (camera.cullingMask == (camera.cullingMask | (1 << layer)))
                    {
                        quad.Renderer.gameObject.layer = layer;
                        break;
                    }
                }

                quads.Add(camera, quad);
            }

            // Perform our fade
            float fadeAmount = 0;
            while (fadeAmount < 1)
            {
                fadeAmount += Time.unscaledDeltaTime;
                currentColor = Color.Lerp(fadeInColor, fadeOutColor, fadeAmount);

                foreach (Quad quad in quads.Values)
                {
                    // Must have been destroyed - just continue
                    if (quad.Renderer == null)
                        continue;

                    quad.PropertyBlock.SetColor(QuadMaterialColorName, currentColor);
                    quad.Renderer.SetPropertyBlock(quad.PropertyBlock);
                }

                await Task.Yield();
            }

            await Task.Yield();

            State = CameraFaderState.Opaque;
        }

        /// <inheritdoc />
        public async Task FadeInAsync(float fadeInTime)
        {
            if (quads.Count == 0)
            {
                Debug.LogError("No camera targets found - are you trying to fade in before you've faded out?");
                return;
            }

            switch (State)
            {
                case CameraFaderState.Opaque:
                    break;

                default:
                    Debug.LogWarning("Can't fade in in state " + State + " - not proceeding.");
                    break;
            }

            State = CameraFaderState.FadingIn;

            // Perform our fade
            float fadeAmount = 0;
            while (fadeAmount < 1)
            {
                fadeAmount += Time.unscaledDeltaTime;
                currentColor = Color.Lerp(fadeOutColor, fadeInColor, fadeAmount);

                foreach (Quad quad in quads.Values)
                {
                    // Must have been destroyed - just continue
                    if (quad.Renderer == null)
                        continue;

                    quad.PropertyBlock.SetColor(QuadMaterialColorName, currentColor);
                    quad.Renderer.SetPropertyBlock(quad.PropertyBlock);
                }

                await Task.Yield();
            }

            await Task.Yield();

            DestroyQuads();

            State = CameraFaderState.Clear;
        }

        /// <inheritdoc />
        public void OnDestroy()
        {
            DestroyQuads();
        }

        private void DestroyQuads()
        {
            foreach (Quad quad in quads.Values)
            {
                if (quad.Renderer != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(quad.Renderer.gameObject);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(quad.Renderer.gameObject);
                    }
                }
            }

            quads.Clear();
        }
    }
}