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

        public float lookAngle = 0.0f;

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

            transform.position = position;
            Vector3 forward = new Vector3(-posZ, 0.0f, posX);
            Quaternion ahead = Quaternion.LookRotation(forward, Vector3.up);
            Quaternion rotation = Quaternion.Euler(0.0f, -lookAngle, 0.0f);
            transform.rotation = rotation * ahead;
        }
    }
}