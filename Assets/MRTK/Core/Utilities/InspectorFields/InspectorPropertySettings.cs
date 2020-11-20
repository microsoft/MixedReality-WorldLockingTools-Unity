﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Utilities.Editor
{
    /// <summary>
    /// A InspectorField property definition and value.
    /// </summary>
    [System.Serializable]
    public struct InspectorPropertySetting
    {
        public InspectorField.FieldTypes Type;
        public string Label;
        public string Name;
        public string Tooltip;
        public int IntValue;
        public string StringValue;
        public float FloatValue;
        public bool BoolValue;
        public GameObject GameObjectValue;
        public ScriptableObject ScriptableObjectValue;
        public UnityEngine.Object ObjectValue;
        public Material MaterialValue;
        public Texture TextureValue;
        public Color ColorValue;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public Vector4 Vector4Value;
        public AnimationCurve CurveValue;
        public AudioClip AudioClipValue;
        public Quaternion QuaternionValue;
        public UnityEvent EventValue;
        public string[] Options;
    }
}
