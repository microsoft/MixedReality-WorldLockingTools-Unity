// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    /// <summary>
    /// Component to handle frozen world adjustments for fixed (stationary) objects.
    /// </summary>
    /// <remarks>
    /// For dynamic objects, use <see cref="AdjusterMoving"/>
    /// </remarks>
    public class AdjusterFixed : AdjusterBase
    {
        protected IAttachmentPointManager Manager { get { return WorldLockingManager.GetInstance().AttachmentPointManager; } }

        protected IAttachmentPoint AttachmentPoint { get; private set; }

        private void Start()
        {
            // Ask the manager for an attachment point, passing delegates for update
            AttachmentPoint = Manager.CreateAttachmentPoint(gameObject.transform.position, null,
                HandleAdjustLocation,   // Handle adjustments to position
                HandleAdjustState  // Handle connectedness of fragment
                );
            AttachmentPoint.Name = string.Format($"{gameObject.name}=>{AttachmentPoint.Name}");
        }

        private void OnDestroy()
        {
            // Let the manager know the attachment point is freed.   
            Manager.ReleaseAttachmentPoint(AttachmentPoint);
            AttachmentPoint = null;
        }

        // mafinc - should have attach point as parameter?
        protected virtual void HandleAdjustLocation(Pose adjustment)
        {
            Pose pose = gameObject.transform.GetGlobalPose();
            pose = adjustment.Multiply(pose);
            gameObject.transform.SetGlobalPose(pose);
        }

        protected virtual void HandleAdjustState(AttachmentPointStateType state)
        {
            bool visible = state == AttachmentPointStateType.Normal;
            if (visible != gameObject.activeSelf)
            {
                gameObject.SetActive(visible);
            }
        }

    }
}