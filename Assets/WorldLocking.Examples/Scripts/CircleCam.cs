// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Examples
{

    public class CircleCam : MonoBehaviour
    {
        public float rpm = 10.0f;

        public float distance = 5.0f;

        private float revolutions = 0.0f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float deltaSecs = Time.deltaTime;
            float deltaMinutes = deltaSecs / 60.0f;

            revolutions += rpm * deltaMinutes;
            revolutions = (float)(revolutions - (int)revolutions);
            float fracRevRads = revolutions * 2.0f * Mathf.PI; ;
            float posX = Mathf.Cos(fracRevRads);
            float posZ = Mathf.Sin(fracRevRads);

            Vector3 position = new Vector3(posX, 0.0f, posZ) * distance;
            Vector3 forward = new Vector3(-posX, 0.0f, -posZ);

            transform.position = position;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }
    }
}