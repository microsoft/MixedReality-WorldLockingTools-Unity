using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;


namespace Microsoft.MixedReality.WorldLocking.Examples
{
    public class PlantPlacard : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
    {
        public GameObject placardPrefab = null;

        // Start is called before the first frame update
        protected override void Start()
        {
            uiLayer = LayerMask.GetMask("UI");
            pillarLayer = LayerMask.GetMask("Pillared");
        }

        // Update is called once per frame
        void Update()
        {

        }

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

        #region Convenience wrapper for ray hit information

        private int uiLayer = 0;
        private int pillarLayer = 0;

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

        public static bool TestLayer(GameObject go, int layerTest)
        {
            if (go == null)
            {
                return false;
            }
            int layerMask = (1 << go.layer);
            return (layerMask & layerTest) != 0;
        }

        #endregion Convenience wrapper for ray hit information

        #region Handle hits

        private void HandleDelete(RayHit rayHit)
        {
            /// Climb to the sub-root of the pillar. That will be the parent-most object
            /// with the Pillared layer.
            var trans = rayHit.gameObject.transform;
            while ((trans.parent != null)
                && (TestLayer(trans.parent.gameObject, pillarLayer)))
            {
                trans = trans.parent;
            }
            GameObject.Destroy(trans.gameObject);
        }

        private void HandleAdd(RayHit rayHit)
        {
            var position = rayHit.hitPosition;
            Vector3 dir = rayHit.hitPosition - rayHit.rayStart;
            dir.y = 0;
            dir.Normalize();
            Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);

            var go = GameObject.Instantiate(placardPrefab, position, rotation);
            go.SetActive(true);
        }

        #endregion Handle Hits

        #region IMixedRealityPointerHandler

        /// <summary>
        /// Process pointer clicked event if ray cast has result.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            var pointerResult = eventData.Pointer.Result;
            var rayHit = new RayHit(pointerResult);
            if (TestLayer(rayHit.gameObject, uiLayer))
            {
                return;
            }
            if (TestLayer(rayHit.gameObject, pillarLayer))
            {
                HandleDelete(rayHit);
            }
            else
            {
                HandleAdd(rayHit);
            }

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
