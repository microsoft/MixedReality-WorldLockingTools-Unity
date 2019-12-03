﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using UnityEngine;
using UnityEditor;

namespace Microsoft.MixedReality.Toolkit.Editor
{
    /// <summary>
    /// Custom property drawer to show an optionally collapsible foldout help section in the Inspector
    /// </summary>
    /// <example>
    /// <code>
    /// [Help("This is a multiline optionally collapsable help section.\n • Great for providing simple instructions in Inspector.\n • Easy to use.\n • Saves space.")]
    /// </code>
    /// </example>
    [CustomPropertyDrawer(typeof(HelpAttribute))]
    public class HelpDrawer : DecoratorDrawer
    {
        /// <summary>
        /// Unity calls this function to draw the GUI
        /// </summary>
        /// <param name="position">Rectangle to display the GUI in</param>
        public override void OnGUI(Rect position)
        {
            HelpAttribute help = attribute as HelpAttribute;

            if (help.Collapsible)
            {
                HelpFoldOut = EditorGUI.Foldout(position, HelpFoldOut, help.Header);
                if (HelpFoldOut)
                {
                    EditorGUI.HelpBox(position, help.Text, MessageType.Info);
                }
            }
            else
            {
                EditorGUI.HelpBox(position, help.Text, MessageType.Info);
            }
            cachedPosition = position;
        }

        /// <summary>
        /// Gets the height of the decorator
        /// </summary>
        public override float GetHeight()
        {
            HelpAttribute help = attribute as HelpAttribute;

            // Computing the actual height requires the cachedPosition because
            // CalcSize doesn't factor in word-wrapped height, and CalcHeight
            // requires a pre-determined width.
            GUIStyle helpStyle = EditorStyles.helpBox;
            GUIContent helpContent = new GUIContent(help.Text);
            float wrappedHeight = helpStyle.CalcHeight(helpContent, cachedPosition.width);

            // The height of the help box should be the content if expanded, or
            // just the header text if not expanded.
            float contentHeight = !help.Collapsible || HelpFoldOut ?
                wrappedHeight :
                helpStyle.lineHeight;

            return helpStyle.margin.top + helpStyle.margin.bottom + contentHeight;
        }

        #region Private

        /// <summary>
        /// The "help" foldout state
        /// </summary>
        private bool HelpFoldOut = false;
        private Rect cachedPosition = new Rect();

        #endregion
    }
}