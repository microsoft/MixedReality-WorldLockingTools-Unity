// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using NUnit.Framework;
using UnityEngine.TestTools;

namespace Microsoft.MixedReality.WorldLocking.Tests.Core
{
    public class TestLoadHelpers
    {
        public bool Setup()
        {
            if (!FindTestRootPath())
            {
                return false;
            }

            return true;
        }

        public void TearDown()
        {
            testRootPath = "";
        }

        public GameObject LoadBasicSceneRig()
        {
            var prefabRig = AssetDatabase.LoadMainAssetAtPath(testRootPath + "/Prefabs/CoreTestBasicSceneRig.prefab");
            GameObject rig = GameObject.Instantiate<GameObject>(prefabRig as GameObject);
            return rig;
        }

        public GameObject LoadGameObject(string goPath)
        {
            var prefab = AssetDatabase.LoadMainAssetAtPath(testRootPath + "/" + goPath);
            GameObject go = GameObject.Instantiate<GameObject>(prefab as GameObject);
            return go;
        }
        public T LoadComponentOnGameObject<T>(string goPath) where T : Object
        {
            GameObject go = LoadGameObject(goPath);
            return go.GetComponent<T>();
        }

        public void UnloadAll()
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; ++i)
            {
                GameObject.Destroy(roots[i]);
            }
        }

        private string testRootPath = "Assets/WorldLocking.Core/Tests";

        private bool FindTestRootPath()
        {
#if UNITY_EDITOR
            string[] paths = AssetDatabase.FindAssets("CoreTestBasicSceneRig");
            Assert.AreEqual(paths.Length, 1);
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            /// Get the folder the asset is in (Prefabs).
            path = Path.GetDirectoryName(path);
            /// Get the folder the Prefabs are in (Tests)
            path = Path.GetDirectoryName(path);
            /// Switch dir separator char for Unity.
            testRootPath = path.Replace('\\', '/');
#endif // UNITY_EDITOR
            return !string.IsNullOrEmpty(testRootPath);
        }


    }
}