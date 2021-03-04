// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

using System.Collections.Generic;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Tools;

namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Example component using Frozen World to facilitate physics simulation.
    /// </summary>
    /// <remarks>
    /// This MRTK based component uses MRTK for inputs to abstract out device.
    /// 
    /// In all modes, the ray cast intersection as reported by MRTK is further filtered
    /// to specifically exclude UI elements. Along with collidable objects in the scene,
    /// a collidable spatial map is also included as ray cast target. The "hit point" is
    /// the intersection of the currently active pointer ray with those collidables, along with 
    /// information about that object and the intersection (e.g. surface normal at hit point).
    /// If there is no current hit point, after excluding UI elements, no operation is performed.
    /// 
    /// The component itself has 5 modes of operation:
    /// 1) Idle - ignore inputs, do nothing
    /// 2) Throw darts - compute and display an ballistic arc to toss a physics rigid body at
    /// the current hit point.
    /// 3) Place pillar - Place an upright pillar at hit point. If the hit object is not a beam or 
    /// pillar (e.g. is the spatial map), then a static pillar is added, else a physically simulated pillar.
    /// 4) Place beam - A two part operation. The first select of a hit point establishes the first end point
    /// of a beam, and the second hit point the other beam's end point. A physically simulated beam stretched to have those two
    /// endpoints is generated and added to the scene.
    /// 5) Remove object - Clicking on an object added to the scene in one of the above modes will remove it from the scene.
    /// 
    /// Mode selection is done via the MRTK radio buttons (see Microsoft.MixedReality.Toolkit.UI.InteractableToggleCollection) 
    /// included in the scene as an addition to the dashboard.
    /// </remarks>
    public class PhysicsBeamSample : InputSystemGlobalHandlerListener, IMixedRealityPointerHandler
    {
#region Public Fields
        [SerializeField]
        [Tooltip("The subroot to attach created objects to.")]
        private Transform attachRoot = null;
        /// <summary>
        /// The subroot to attach created objects to.
        /// </summary>
        public Transform AttachRoot => attachRoot;

        [SerializeField]
        [Tooltip("The prefab of the dart to place in the world at gaze position on air taps.")]
        private GameObject prefabDart = null;
        /// <summary>
        /// The prefab of the dart to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabDart => prefabDart;

        [SerializeField]
        [Tooltip("The prefab of fixed pillars to place in the world at gaze position on air taps.")]
        private GameObject prefabPillarFixed = null;
        /// <summary>
        /// The prefab of fixed pillars to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabPillarFixed => prefabPillarFixed;

        [SerializeField]
        [Tooltip("The prefab of dynamic pillars to place in the world at gaze position on air taps.")]
        private GameObject prefabPillarDynamic = null;
        /// <summary>
        /// The prefab of dynamic pillars to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabPillarDynamic => prefabPillarDynamic;

        [SerializeField]
        [Tooltip("The prefab of the beam to place in the world at gaze position on air taps.")]
        private GameObject prefabBeam = null;
        /// <summary>
        /// The prefab of the beam to place in the world at gaze position on air taps.
        /// </summary>
        public GameObject PrefabBeam => prefabBeam;

        [SerializeField]
        [Tooltip("The prefab of the world-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.")]
        private GameObject prefabWorldLockedSphere = null;
        /// <summary>
        /// The prefab of the world-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.
        /// </summary>
        public GameObject PrefabWorldLockedSphere => prefabWorldLockedSphere;

        [SerializeField]
        [Tooltip("The prefab of the non-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.")]
        private GameObject prefabUnlockedSphere = null;
        /// <summary>
        /// The prefab of the non-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.
        /// </summary>
        public GameObject PrefabUnlockedSphere => prefabUnlockedSphere;

        [SerializeField]
        [Tooltip("The prefab of the hybrid-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.")]
        private GameObject prefabHybridLockedSphere = null;
        /// <summary>
        /// The prefab of the hybrid-locked 'sphere' to place in the world at gaze position on air taps in Pin Sphere Mode.
        /// </summary>
        public GameObject PrefabHybridLockedSphere => prefabHybridLockedSphere;

        [SerializeField]
        [Tooltip("Radio popup for selecting world lock mode when in Pin Sphere mode.")]
        private InteractableToggleCollection worldLockSelector = null;

        [SerializeField]
        [Tooltip("Material to use when rendering line for beam placement.")]
        private Material lineMaterial = null;
        /// <summary>
        /// Material to use when rendering line for beam placement.
        /// </summary>
        public Material LineMaterial => lineMaterial;

#endregion Public Fields

#region Private Fields
        private enum BuildMode
        {
            Idle = 0,
            Dart,
            DropPillars,
            StartBeam,
            EndBeam,
            PinSphere,
            RemoveObject
        };

        private int ModeToIndex(BuildMode mode)
        {
            switch (mode)
            {
                case BuildMode.Idle:
                    return 0;
                case BuildMode.Dart:
                    return 1;
                case BuildMode.DropPillars:
                    return 2;
                case BuildMode.StartBeam:
                case BuildMode.EndBeam:
                    return 3;
                case BuildMode.PinSphere:
                    return 4;
                case BuildMode.RemoveObject:
                    return 5;
            }
            return 0;
        }

        private InteractableToggleCollection radioSet = null;

        private BuildMode mode = BuildMode.Idle;

        private Vector3 beamStartPosition = Vector3.zero;

        private LineRenderer lineRenderer;

#endregion Private Fields

#region Unity Methods

        /// <summary>
        /// Override InputSystemGlobalListener Start() method for additional one-time setup.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            radioSet = gameObject.GetComponent<InteractableToggleCollection>();

            SetupLineRenderer();

            SyncRadioSet();

            int pillarLayer = LayerMask.NameToLayer("Pillared");
            int spatialMappingLayer = LayerMask.NameToLayer("SpatialMapping");
            if (pillarLayer == -1)
            {
                Debug.LogWarning("Undefined layer 'Pillared', spatial mapping objects might interfere with pillars and beams");
            }
            if (spatialMappingLayer == -1)
            {
                Debug.LogWarning("Undefined layer 'SpatialMapping', spatial mapping objects might interfere with pillars and beams");
            }
            if (pillarLayer != -1 && spatialMappingLayer != -1)
            {
                Physics.IgnoreLayerCollision(pillarLayer, spatialMappingLayer, true);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            switch (mode)
            {
                case BuildMode.EndBeam:
                    ShowBeam();
                    break;
                case BuildMode.Dart:
                    ShowDartArc();
                    break;
                default:
                    break;
            }
        }

#endregion Unity Methods

#region Internal state handlers

        private void SyncRadioSet()
        {
            radioSet.CurrentIndex = ModeToIndex(mode);
            radioSet.SetSelection(radioSet.CurrentIndex);
        }

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

        private void HandleHit(RayHit rayHit)
        {
            switch (mode)
            {
                case BuildMode.Dart:
                    DropDart(rayHit);
                    break;
                case BuildMode.DropPillars:
                    DropPillar(rayHit);
                    break;
                case BuildMode.StartBeam:
                    StartBeam(rayHit);
                    break;
                case BuildMode.EndBeam:
                    EndBeam(rayHit);
                    break;
                case BuildMode.PinSphere:
                    PinSphere(rayHit);
                    break;
                case BuildMode.RemoveObject:
                    RemoveObject(rayHit);
                    break;

                case BuildMode.Idle:
                default:
                    break;
            }

        }

        private void DropDart(RayHit rayHit)
        {
            if (PrefabDart != null)
            {
                Vector3 source = rayHit.rayStart;

                Vector3 target = rayHit.hitPosition;

                VelocityTime vt = ComputeArc(source, target);
                Vector3 velocity = vt.v;
                float t = vt.t;

                if (t > 0)
                {
                    Vector3 startDir = target - source;
                    startDir.y = 0;
                    startDir.Normalize();
                    Vector3 right = Vector3.Cross(Vector3.up, startDir);
                    Quaternion startRot = Quaternion.LookRotation(startDir, right);

                    var newObj = GameObject.Instantiate(PrefabDart, source, startRot, AttachRoot);
                    var rigidBody = newObj.GetComponent<Rigidbody>();
                    if (rigidBody != null)
                    {
                        rigidBody.AddForce(velocity, ForceMode.VelocityChange);
                        //rigidBody.velocity = velocity;

                        newObj.AddComponent<AdjusterMoving>();
                    }
                    else
                    {
                        newObj.transform.position = target;
                        newObj.AddComponent<AdjusterFixed>();
                    }

                }
            }
        }

        private List<IMixedRealityPointer> FindActivePointers()
        {
            List<IMixedRealityPointer> activePointers = new List<IMixedRealityPointer>();
            var inputSystem = MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>();
            if (inputSystem != null)
            {
                var detectedInputSources = inputSystem.DetectedInputSources;
                foreach (var inputSource in detectedInputSources)
                {
                    foreach (var pointer in inputSource.Pointers)
                    {
                        if (pointer.IsActive && pointer.Result != null)
                        {
                            activePointers.Add(pointer);
                        }
                    }
                }
            }
            return activePointers;
        }

        private void ShowDartArc()
        {
            bool show = false;
            var activePointers = FindActivePointers();
            if (activePointers.Count > 0)
            {
                var focusPointer = activePointers[0];

                int uiLayer = LayerMask.GetMask("UI");
                if (focusPointer.Result.CurrentPointerTarget == null || ((1 << focusPointer.Result.CurrentPointerTarget.layer) & uiLayer) == 0)
                {
                    Vector3 rayStart = focusPointer.Rays[0].Origin;
                    Vector3 endPoint = focusPointer.Result.Details.Point;
                    VelocityTime vt = ComputeArc(rayStart, endPoint);
                    if (vt.t > 0)
                    {
                        ShowArc(rayStart, vt.v, vt.t);
                        show = true;
                    }
                }
            }
            lineRenderer.enabled = show;

        }

        private struct VelocityTime
        {
            public Vector3 v;
            public float t;
        }
        private VelocityTime ComputeArc(Vector3 source, Vector3 target)
        {
            Vector3 horizontal = target - source;
            float dy = horizontal.y;
            horizontal.y = 0;
            float dx = horizontal.magnitude;
            Vector3 horizontalDirection = Vector3.Normalize(horizontal);

            float g = Physics.gravity.y;

            float t = 0.0f;

            float vx = 0;
            float vy = 0;
            if (dy > 0)
            {
                vy = Mathf.Sqrt(-2.0f * g * dy);

                t = -vy / g;

                vx = dx / t;
            }
            else if (dy < 0)
            {
                t = Mathf.Sqrt(2.0f * dy / g);

                vx = dx / t;
            }

            Vector3 velocity = new Vector3(horizontalDirection.x * vx, vy, horizontalDirection.z * vx);

            return new VelocityTime() { v = velocity, t = t };
        }

        private void ShowArc(Vector3 startPos, Vector3 startVel, float maxAge)
        {
            lineRenderer.enabled = true;
            int numPoints = 20;
            if (lineRenderer.positionCount != numPoints)
            {
                lineRenderer.positionCount = numPoints;
            }
            float g = Physics.gravity.y;
            float tstep = maxAge / (numPoints - 2);
            Vector3[] points = new Vector3[numPoints];
            for (int i = 0; i < points.Length; ++i)
            {
                float age = tstep * i;

                Vector3 position = startPos + age * startVel + 0.5f * new Vector3(0.0f, g, 0.0f) * age * age;
                points[i] = position;
            }
            lineRenderer.SetPositions(points);
        }

        private void DropPillar(RayHit rayHit)
        {
            if (PrefabPillarFixed != null || PrefabPillarDynamic != null)
            {
                var hitPos = rayHit.hitPosition;
                var toRay = rayHit.rayStart - hitPos;
                var hitDirProj = toRay;
                hitDirProj.y = 0;
                hitDirProj.Normalize();
                var hitUp = new Vector3(0.0f, 1.0f, 0.0f);
                var hitRot = Quaternion.LookRotation(hitDirProj, hitUp);

                bool isStack = (PrefabPillarDynamic != null) && (rayHit.gameObject?.GetComponentInParent<RemovableGroup>() ?? false);

                if (isStack || PrefabPillarFixed == null)
                {
                    var newObj = GameObject.Instantiate(PrefabPillarDynamic, hitPos, hitRot, AttachRoot);

                    newObj.AddComponent<AdjusterMoving>();
                }
                else
                {
                    var newObj = GameObject.Instantiate(PrefabPillarFixed, hitPos, hitRot, AttachRoot);

                    newObj.AddComponent<AdjusterFixed>();
                }
            }

        }

        private void StartBeam(RayHit rayHit)
        {
            beamStartPosition = rayHit.hitPosition;
            lineRenderer.enabled = true;
            mode = BuildMode.EndBeam;
            SyncRadioSet();
        }

        private void EndBeam(RayHit rayHit)
        {
            Vector3 beamEndPosition = rayHit.hitPosition;
            Vector3 midPosition = (beamStartPosition + beamEndPosition) * 0.5f;
            float epsilon = 0.001f; // Slight offset to keep from starting interpenetrating.
            midPosition.y = Mathf.Max(beamStartPosition.y, beamEndPosition.y) + epsilon;
            Vector3 beamDirection = beamEndPosition - beamStartPosition;
            beamDirection.y = 0;
            float beamLength = beamDirection.magnitude;
            beamDirection = Vector3.Normalize(beamDirection);
            Quaternion rot = Quaternion.LookRotation(beamDirection, new Vector3(0.0f, 1.0f, 0.0f));

            var newObj = GameObject.Instantiate(PrefabBeam, midPosition, rot, AttachRoot);
            newObj.transform.localScale = new Vector3(1.0f, 1.0f, beamLength);

            newObj.AddComponent<AdjusterMoving>();

            lineRenderer.enabled = false;
            mode = BuildMode.StartBeam;
            SyncRadioSet();
        }

        private void SetupLineRenderer()
        {
            GameObject lineObject = new GameObject("Beam Ray");
            lineObject.transform.SetParent(AttachRoot);
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.material.color = Color.grey;
            float width = 0.01f;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.cyan;
        }

        private void ShowBeam()
        {
            if (lineRenderer.positionCount != 2)
            {
                lineRenderer.positionCount = 2;
            }
            bool show = false;
            var activePointers = FindActivePointers();
            if (activePointers.Count > 0)
            {
                var focusPointer = activePointers[0];
                int uiLayer = LayerMask.GetMask("UI");
                if (((1 << focusPointer.Result.CurrentPointerTarget.layer) & uiLayer) == 0)
                {
                    lineRenderer.SetPosition(0, beamStartPosition);
                    lineRenderer.SetPosition(1, focusPointer.Result.Details.Point);
                    show = true;
                }
            }
            lineRenderer.enabled = show;
        }

        private enum WorldLockType
        {
            Unlocked = 0,
            WorldLock,
            HybridLock
        };

        private WorldLockType worldLock = WorldLockType.HybridLock;

        private WorldLockType WorldLock { get { return worldLock; } set { worldLock = value; } }

        private void PinSphere(RayHit rayHit)
        {
            var hitPos = rayHit.hitPosition;
            var toRay = rayHit.rayStart - hitPos;
            var hitDirProj = toRay;
            hitDirProj.y = 0;
            hitDirProj.Normalize();
            var hitUp = new Vector3(0.0f, 1.0f, 0.0f);
            var hitRot = Quaternion.LookRotation(hitDirProj, hitUp);

            switch(WorldLock)
            {
                case WorldLockType.Unlocked:
                    if (PrefabUnlockedSphere != null)
                    {
                        var newObj = GameObject.Instantiate(PrefabUnlockedSphere, hitPos, hitRot, AttachRoot);
                    }
                    else
                    {
                        Debug.LogError("Missing prefab for Unlocked Sphere");
                    }
                    break;
                case WorldLockType.WorldLock:
                    if (PrefabWorldLockedSphere != null)
                    {
                        var newObj = GameObject.Instantiate(PrefabWorldLockedSphere, hitPos, hitRot, AttachRoot);

                        var twa = newObj.AddComponent<ToggleWorldAnchor>();
                        twa.AlwaysLock = true;
                    }
                    else
                    {
                        Debug.LogError("Missing prefab for World Locked Sphere");
                    }
                    break;
                case WorldLockType.HybridLock:
                    if (PrefabHybridLockedSphere != null)
                    {
                        var newObj = GameObject.Instantiate(PrefabHybridLockedSphere, hitPos, hitRot, AttachRoot);

                        var twa = newObj.AddComponent<ToggleWorldAnchor>();
                        twa.AlwaysLock = false;
                    }
                    else
                    {
                        Debug.LogError("Missing prefab for Hybrid Locked Sphere");
                    }
                    break;
            }
        }

        private void RemoveObject(RayHit rayHit)
        {
            if (rayHit.gameObject != null)
            {
                RemovableGroup removal = rayHit.gameObject.GetComponentInParent<RemovableGroup>();
                if (removal != null)
                {
                    GameObject.Destroy(removal.gameObject);
                }
            }
        }

#endregion Internal state handlers

#region Mode transitions

        /// <summary>
        /// Switch into idle mode.
        /// </summary>
        public void EnterIdleMode()
        {
            FinishCurrentAction();
            mode = BuildMode.Idle;
        }

        /// <summary>
        /// Switch into dart tossing mode.
        /// </summary>
        public void EnterDartMode()
        {
            FinishCurrentAction();
            mode = BuildMode.Dart;
        }

        /// <summary>
        /// Switch into pillar placement mode.
        /// </summary>
        public void EnterPillarMode()
        {
            FinishCurrentAction();
            mode = BuildMode.DropPillars;
        }

        private WorldLockType NextWorldLock()
        {
            switch (WorldLock)
            {
                case WorldLockType.Unlocked:
                    return WorldLockType.WorldLock;
                case WorldLockType.WorldLock:
                    return WorldLockType.HybridLock;
                case WorldLockType.HybridLock:
                    return WorldLockType.Unlocked;
                default:
                    Debug.Assert(false, $"Unhandled WorldLockType {WorldLock.ToString()}");
                    break;
            }
            return WorldLockType.HybridLock;
        }
        private void SetupWorldLockSelect(bool activate)
        {
            if (activate)
            {
                worldLockSelector.gameObject.SetActive(true);
                worldLockSelector.SetSelection((int)WorldLock);
            }
            else
            {
                worldLockSelector.gameObject.SetActive(false);
            }
        }

        public void EnterWorldLockUnlocked()
        {
            WorldLock = WorldLockType.Unlocked;
        }
        public void EnterWorldLockWorldLocked()
        {
            WorldLock = WorldLockType.WorldLock;
        }
        public void EnterWorldLockHybridLocked()
        {
            WorldLock = WorldLockType.HybridLock;
        }

        public void EnterPinSphereMode()
        {
            FinishCurrentAction();
            mode = BuildMode.PinSphere;
            SetupWorldLockSelect(true);
        }

        /// <summary>
        /// Switch into cross-beam placement mode.
        /// </summary>
        public void EnterBeamMode()
        {
            if (mode != BuildMode.StartBeam && mode != BuildMode.EndBeam)
            {
                FinishCurrentAction();
                mode = BuildMode.StartBeam;
            }
        }

        /// <summary>
        /// Switch into object removal mode.
        /// </summary>
        public void EnterRemoveMode()
        {
            FinishCurrentAction();
            mode = BuildMode.RemoveObject;
        }

        private void FinishCurrentAction()
        {
            lineRenderer.enabled = false;
            SetupWorldLockSelect(false);
        }

#endregion Mode transitions

#region InputSystemGlobalHandlerListener Implementation

        protected override void RegisterHandlers()
        {
            MixedRealityToolkit.Instance.GetService<IMixedRealityInputSystem>()?.RegisterHandler<IMixedRealityPointerHandler>(this);
        }

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