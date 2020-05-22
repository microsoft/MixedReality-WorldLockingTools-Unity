﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Object that represents a cursor comprised of sprites and colors for each state
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/SDK/SpriteCursor")]
    public class SpriteCursor : BaseCursor
    {
        [Serializable]
        public struct SpriteCursorDatum
        {
            public string Name;
            public CursorStateEnum CursorState;
            public Sprite CursorSprite;
            public Color CursorColor;
        }

        [SerializeField]
        public SpriteCursorDatum[] CursorStateData;

        /// <summary>
        /// Sprite renderer to change.  If null find one in children
        /// </summary>
        public SpriteRenderer TargetRenderer;

        /// <summary>
        /// On enable look for a sprite renderer on children
        /// </summary>
        protected override void OnEnable()
        {
            if (CursorStateData == null)
            {
                CursorStateData = Array.Empty<SpriteCursorDatum>();
            }

            if (TargetRenderer == null)
            {
                TargetRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            base.OnEnable();
        }

        /// <summary>
        /// Override OnCursorState change to set the correct sprite
        /// state for the cursor
        /// </summary>
        public override void OnCursorStateChange(CursorStateEnum state)
        {
            base.OnCursorStateChange(state);

            if (state != CursorStateEnum.Contextual)
            {
                for (int i = 0; i < CursorStateData.Length; i++)
                {
                    if (CursorStateData[i].CursorState == state)
                    {
                        SetCursorState(CursorStateData[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Based on the type of state info pass it through to the sprite renderer
        /// </summary>
        private void SetCursorState(SpriteCursorDatum stateDatum)
        {
            // Return if we do not have an animator
            if (TargetRenderer != null)
            {
                TargetRenderer.sprite = stateDatum.CursorSprite;
                TargetRenderer.color = stateDatum.CursorColor;
            }
        }
    }
}
