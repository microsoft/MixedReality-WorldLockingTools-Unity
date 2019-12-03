// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

public class DashboardTagalong : MonoBehaviour
{

    public float maxAngle = 20.0f;
    public float lerpTime = 0.1f;

    Vector3 originalPosition;
    Quaternion originalRotation;

    Quaternion currentRotation;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        currentRotation = Quaternion.identity;
    }

    void Update()
    {
        Vector3 camPosition = Camera.main.transform.position; // GazeManager.Instance.Stabilizer.StablePosition;
        Quaternion camRotation = Camera.main.transform.rotation; // GazeManager.Instance.Stabilizer.StableRotation;

        float cameraAngle = camRotation.eulerAngles.y;
        float currentAngle = currentRotation.eulerAngles.y;

        float diffAngle = currentAngle - cameraAngle;
        while (diffAngle > 180) diffAngle -= 360;
        while (diffAngle < -180) diffAngle += 360;

        diffAngle = Math.Min(diffAngle, maxAngle);
        diffAngle = Math.Max(diffAngle, -maxAngle);

        float targetAngle = cameraAngle + diffAngle;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.unscaledDeltaTime / lerpTime);

        transform.position = camPosition + currentRotation * originalPosition;
        transform.rotation = currentRotation * originalRotation;
    }
}
