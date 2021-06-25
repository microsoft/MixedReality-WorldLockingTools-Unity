// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Simple object for interaction. Not to be taken seriously.
    /// </summary>
    public class ActionCube : MonoBehaviour
    {
        private List<Material> materials = new List<Material>();
        private List<Color> originals = new List<Color>();

        private static readonly string colorParamName = "_Color";

        private void Awake()
        {
            CaptureMaterials();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        protected async virtual void DoThing()
        {
            await ChangeColorForSeconds(3.0f, Color.white);
        }

        protected async Task<bool> ChangeColorForSeconds(float seconds, Color color)
        {
            Debug.Log($"ChangeColorForSeconds({seconds}, {color}");
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].SetColor(colorParamName, color);
            }
            Debug.Log($"Colors set, waiting...");
            int milliSeconds = (int)(seconds * 1000.0f);
            await Task.Delay(milliSeconds);

            Debug.Log($"Waited, restoring colors");
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].SetColor(colorParamName, originals[i]);
            }

            Debug.Log($"ChangeColorForSeconds complete");

            return true;
        }

        protected void SetColors(Color color)
        {
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].SetColor(colorParamName, color);
            }
        }
        protected void RestoreColors()
        {
            for (int i = 0; i < materials.Count; ++i)
            {
                materials[i].SetColor(colorParamName, originals[i]);
            }
        }

        public void OnSelect()
        {
            DoThing();
        }

        private void CaptureMaterials()
        {
            materials.Clear();
            var renderers = GetComponentsInChildren<Renderer>();
            Debug.Log($"Got {renderers.Length} renderers from {name}");
            foreach (var rend in renderers)
            {
                materials.Add(rend.material);
                originals.Add(rend.material.GetColor(colorParamName));
                Debug.Log($"Got {rend.material.name} from {rend.name}");
            }
        }

    }

}
