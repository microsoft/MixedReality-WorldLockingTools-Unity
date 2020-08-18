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
    /// For dynamic objects, use <see cref="AdjusterMoving"/>.
    /// 
    /// This component is appropriate for inheriting from, to let it take care of
    /// lifetime management and book-keeping, then just override <see cref="HandleAdjustLocation(Pose)"/>
    /// and/or <see cref="HandleAdjustState(AttachmentPointStateType)"/> with actions more suitable
    /// for your application.
    /// </remarks>
    public class AdjusterFixed : AdjusterBase
    {
        /// <summary>
        /// The attachment point manager interface which this component subscribes to.
        /// </summary>
        protected IAttachmentPointManager Manager { get { return WorldLockingManager.GetInstance().AttachmentPointManager; } }

        /// <summary>
        /// The attachment point which this component wraps.
        /// </summary>
        protected IAttachmentPoint AttachmentPoint { get; private set; }

        /// <summary>
        /// Ask the manager for an attachment point, passing delegates for update
        /// </summary>
        private void Start()
        {
            AttachmentPoint = Manager.CreateAttachmentPoint(gameObject.transform.position, null,
                HandleAdjustLocation,   // Handle adjustments to position
                HandleAdjustState  // Handle connectedness of fragment
                );
            AttachmentPoint.Name = string.Format($"{gameObject.name}=>{AttachmentPoint.Name}");
        }

        /// <summary>
        /// Let the manager know the attachment point is freed.   
        /// </summary>
        private void OnDestroy()
        {
            Manager.ReleaseAttachmentPoint(AttachmentPoint);
            AttachmentPoint = null;
        }

        /// <summary>
        /// For infrequent moves under script control, UpdatePosition notifies the system that the
        /// object has relocated. It should be called after any scripted movement of the object
        /// (but **not** after movement triggered by WLT, such as in <see cref="HandleAdjustLocation(Pose)"/>).
        /// </summary>
        public void UpdatePosition()
        {
            if (AttachmentPoint != null)
            {
                Manager.MoveAttachmentPoint(AttachmentPoint, gameObject.transform.position);
            }
        }

        /// <summary>
        /// Handle a pose adjustment due to a refit operation.
        /// </summary>
        /// <param name="adjustment">The pose adjustment to apply/</param>
        /// <remarks>
        /// This simple implementation folds the adjustment into the current pose.
        /// </remarks>
        protected virtual void HandleAdjustLocation(Pose adjustment)
        {
            Pose pose = gameObject.transform.GetGlobalPose();
            pose = adjustment.Multiply(pose);
            gameObject.transform.SetGlobalPose(pose);
        }

        /// <summary>
        /// Handle a change in associated fragment state.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <remarks>
        /// The only state under which the visual location can be regarded as reliable
        /// is the Normal state.
        /// This simple implementation disables the object tree when its location is unreliable,
        /// and enables it when its location is reliable.
        /// Actual appropriate behavior is highly application dependent. Some questions to ask:
        /// * Is there a more appropriate way to hide the object (e.g. move it far away)?
        /// * Should the update pause, or just stop rendering? (Disabling pauses update **and** render).
        /// * Is it better to hide the object, or to display it in alternate form?
        /// * Etc.
        /// </remarks>
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