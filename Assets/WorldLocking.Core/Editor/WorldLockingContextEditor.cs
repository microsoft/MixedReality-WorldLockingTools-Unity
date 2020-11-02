// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Custom editor for the collections of settings managed by the WorldLockingManager.
    /// </summary>
    [CustomEditor(typeof(WorldLockingContext))]
    public class WorldLockingContextEditor : Editor
    {
        bool showWorld = true;
        bool showLinkage = true;
        bool showAnchor = false;
        bool showDiagnostics = false;

        /// <summary>
        /// Put up the GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            string versionInfo = WorldLockingManager.Version;
            EditorGUILayout.LabelField("Version: ", versionInfo);

            var context = target as WorldLockingContext;

            showWorld = EditorGUILayout.Foldout(showWorld, "Automation settings", true);
            if (showWorld)
            {
                string mgrPath = "shared.settings.";

                SerializedProperty mgrUseDefaultsProp = AddProperty(mgrPath, "useDefaults");

                bool mgrUseDefault = mgrUseDefaultsProp.boolValue;
                context.SharedSettings.settings.UseDefaults = mgrUseDefault;

                using (new EditorGUI.DisabledScope(mgrUseDefault))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(mgrPath, "Enabled");

                        AddProperty(mgrPath, "AutoMerge");

                        AddProperty(mgrPath, "AutoRefreeze");

                        AddProperty(mgrPath, "AutoLoad");

                        AddProperty(mgrPath, "AutoSave");
                    }
                }

            } 

            EditorGUILayout.Space();

            showLinkage = EditorGUILayout.Foldout(showLinkage, "Camera Transform Links", true);
            if (showLinkage)
            {
                string mgrPath = "shared.linkageSettings.";

                SerializedProperty mgrUseExisting = AddProperty(mgrPath, "useExisting");

                bool useExisting = mgrUseExisting.boolValue;

                using (new EditorGUI.DisabledScope(useExisting))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(mgrPath, "AdjustmentFrame");

                        AddProperty(mgrPath, "CameraParent");
                    }
                }

            }

            EditorGUILayout.Space();

            showAnchor = EditorGUILayout.Foldout(showAnchor, "Anchor Management", true);
            if (showAnchor)
            {
                string mgrPath = "shared.anchorSettings.";

                SerializedProperty anchorUseDefaults = AddProperty(mgrPath, "useDefaults");

                bool useDefaults = anchorUseDefaults.boolValue;
                context.SharedSettings.anchorSettings.UseDefaults = useDefaults;

                using (new EditorGUI.DisabledScope(useDefaults))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(mgrPath, "ARSessionSource");

                        AddProperty(mgrPath, "ARSessionOriginSource");

                        AddProperty(mgrPath, "MinNewAnchorDistance");

                        AddProperty(mgrPath, "MaxAnchorEdgeLength");
                    }
                }

            }

            EditorGUILayout.Space();

            showDiagnostics = EditorGUILayout.Foldout(showDiagnostics, "Diagnostics settings", true);
            if (showDiagnostics)
            {
                string diagPath = "diagnosticsSettings.settings.";

                var diagnostics = context.DiagnosticsSettings;

                SerializedProperty diagUseDefaultsProp = AddProperty(diagPath, "useDefaults");

                bool diagUseDefaults = diagUseDefaultsProp.boolValue;

                using (new EditorGUI.DisabledScope(diagUseDefaults))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        AddProperty(diagPath, "Enabled");

                        AddProperty(diagPath, "StorageSubdirectory");

                        AddProperty(diagPath, "StorageFileTemplate");

                        AddProperty(diagPath, "MaxKilobytesPerFile");

                        AddProperty(diagPath, "MaxNumberOfFiles");
                    }
                }

            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Find a property, possibly with a path down from the serializedObject.
        /// </summary>
        /// <remarks>
        /// Path seems to work like so:
        ///   struct MySubStruct { int myIntField; }
        ///   struct MyStruct { MySubStruct mySubStruct; }
        ///   class myObj : Monobehavior { MyStruct myStruct; }
        ///   var intProp = serializedObject.FindProperty("myStruct.mySubStruct.myIntField");
        /// </remarks>
        /// <param name="path">Path including trailing '.' (or empty).</param>
        /// <param name="name">Field name.</param>
        /// <returns></returns>
        private SerializedProperty FindProperty(string path, string name)
        {
            return serializedObject.FindProperty(path + name);
        }

        /// <summary>
        /// Find a property and add to the GUI.
        /// </summary>
        /// <param name="path">Path including trailing '.' (or empty).</param>
        /// <param name="name">Field name.</param>
        /// <returns></returns>
        private SerializedProperty AddProperty(string path, string name)
        {
            SerializedProperty prop = FindProperty(path, name);
            EditorGUILayout.PropertyField(prop);
            return prop;
        }
    }
}

#endif // UNITY_EDITOR