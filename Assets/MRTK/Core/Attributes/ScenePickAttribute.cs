﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit
{
    /// <summary>
    /// Attribute to mark up an int field to be drawn using the
    /// ScenePickPropertyDrawer
    /// This allows the UI to display a dropdown instead of a
    /// numeric entry field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ScenePickAttribute : PropertyAttribute
    {
        // Nothing to see Here, This only acts as a marker to help the editor.
    }
}
