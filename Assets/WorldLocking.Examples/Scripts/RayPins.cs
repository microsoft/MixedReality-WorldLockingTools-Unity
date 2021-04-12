// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    public class RayPins : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
    {
        /// <summary>
        /// An orienter to infer orientation from position of pins. Shared over all pins.
        /// </summary>
        private IOrienter orienter;

        /// <summary>
        /// The radio set to clear back to Idle (0).
        /// </summary>
        private InteractableToggleCollection radioSet = null;

        /// <summary>
        /// One pin created for each spacePinPoint.
        /// </summary>
        private readonly List<SpacePinOrientable> spacePins = new List<SpacePinOrientable>();

        /// <summary>
        /// Global position of each of the space pins points can be matched to a ray cast against the environment.
        /// </summary>
        public List<Transform> spacePinPoints = new List<Transform>();

        /// <summary>
        /// The pin that ray hits will be routed to. If -1, then no pins will receive hits.
        /// </summary>
        private int activePin = -1;

        /// <summary>
        /// Accessor for currently active pin.
        /// </summary>
        public int ActivePin { get { return activePin; } set { activePin = value; } }

        /// <summary>
        /// Function for setting active pin from MRTK GUI callbacks.
        /// </summary>
        /// <param name="i">The new current pin.</param>
        public void SetActivePin(int i) { ActivePin = i; }


        /// <summary>
        /// Create a shared orienter, and create space pins for any spacePinPoints set in the inspector.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            orienter = gameObject.AddComponent<Orienter>();

            CreateSpacePins();

            radioSet = gameObject.GetComponentInChildren<InteractableToggleCollection>();

        }

        /// <summary>
        /// Destroy any existing pins, and create new pins, one for each spacePinPoint.
        /// </summary>
        /// <returns>True on success.</returns>
        /// <remarks>
        /// If the spacePinPoint list is modified from script, CreateSpacePins should be called to resynchronize.
        /// </remarks>
        public bool CreateSpacePins()
        {
            ClearAll();
            spacePins.Clear();
            for (int i = 0; i < spacePinPoints.Count; ++i)
            {
                spacePins.Add(CreateSpacePin(spacePinPoints[i]));
            }
            return spacePins.Count > 0;
        }

        private SpacePinOrientable CreateSpacePin(Transform t)
        {
            Debug.Assert(orienter != null);
            SpacePinOrientable pin = t.gameObject.AddComponent<SpacePinOrientable>();
            pin.Orienter = orienter;
            return pin;
        }

        /// <summary>
        /// Disable the effects of all pins, as if they had never been set.
        /// </summary>
        public void ClearAll()
        {
            for (int i = 0; i < spacePins.Count; ++i)
            {
                spacePins[i].Reset();
            }
            // Could optionally also reset all existing anchors for a true reset.
            // If wanted, uncomment this line.
            WorldLockingManager.GetInstance().Reset();

            // Also go back to idle mode.
            activePin = -1;
        }
#region Convenience wrapper for ray hit information

        private struct RayHit
        {
            public readonly Vector3 rayStart;
            public readonly Vector3 hitPosition;
            public readonly Vector3 hitNormal;
            public readonly GameObject gameObject;

            public RayHit(Vector3 rayStart, RaycastHit hitInfo)
            {
                this.rayStart = rayStart;
                this.hitPosition = hitInfo.point;
                this.hitNormal = hitInfo.normal;
                this.gameObject = hitInfo.collider?.gameObject;
            }

            public RayHit(IPointerResult pointerResult)
            {
                this.rayStart = pointerResult.StartPoint;
                this.hitPosition = pointerResult.Details.Point;
                this.hitNormal = pointerResult.Details.Normal;
                this.gameObject = pointerResult.CurrentPointerTarget;
            }
        };

#endregion Convenience wrapper for ray hit information

#region Handle hits

        private void HandleHit(RayHit rayHit)
        {
            Debug.Assert(spacePins.Count == spacePinPoints.Count);
            if (activePin >= 0 && activePin < spacePins.Count)
            {
                spacePins[activePin].SetFrozenPosition(rayHit.hitPosition);
            }
        }

#endregion Handle hits


#region InputSystemGlobalHandlerListener Implementation

        /// <inheritdocs />
        protected override void RegisterHandlers()
        {
            MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>()?.RegisterHandler<IMixedRealityPointerHandler>(this);
        }

        /// <inheritdocs />
        protected override void UnregisterHandlers()
        {
            MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>()?.UnregisterHandler<IMixedRealityPointerHandler>(this);
        }

#endregion InputSystemGlobalHandlerListener Implementation

#region IMixedRealityPointerHandler

        /// <summary>
        /// Process pointer clicked event if ray cast has result.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            var pointerResult = eventData.Pointer.Result;
            var rayHit = new RayHit(pointerResult);
            int uiLayer = LayerMask.GetMask("UI");
            if (rayHit.gameObject == null || ((1 << rayHit.gameObject.layer) & uiLayer) == 0)

                HandleHit(rayHit);
        }

        /// <summary>
        /// No-op on pointer up.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {

        }

        /// <summary>
        /// No-op on pointer down.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {

        }

        /// <summary>
        /// No-op on pointer drag.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {

        }

#endregion IMixedRealityPointerHandler

    }
}