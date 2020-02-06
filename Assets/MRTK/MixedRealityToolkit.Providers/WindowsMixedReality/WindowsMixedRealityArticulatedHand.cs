﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;

#if UNITY_WSA
using UnityEngine.XR.WSA.Input;
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
using System;
using System.Collections.Generic;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Foundation.Metadata;
using Windows.Perception;
using Windows.Perception.People;
using Windows.UI.Input.Spatial;
#elif DOTNETWINRT_PRESENT
using Microsoft.Windows.Foundation.Metadata;
using Microsoft.Windows.Perception;
using Microsoft.Windows.Perception.People;
using Microsoft.Windows.UI.Input.Spatial;
#endif
#endif // WINDOWS_UWP || DOTNETWINRT_PRESENT
#endif // UNITY_WSA

namespace Microsoft.MixedReality.Toolkit.WindowsMixedReality.Input
{
    /// <summary>
    /// A Windows Mixed Reality Controller Instance.
    /// </summary>
    [MixedRealityController(
        SupportedControllerType.ArticulatedHand,
        new[] { Handedness.Left, Handedness.Right })]
    public class WindowsMixedRealityArticulatedHand : BaseWindowsMixedRealitySource, IMixedRealityHand
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WindowsMixedRealityArticulatedHand(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
                : base(trackingState, controllerHandedness, inputSource, interactions)
        {
#if (UNITY_WSA && DOTNETWINRT_PRESENT) || WINDOWS_UWP
            articulatedHandApiAvailable = ApiInformation.IsMethodPresent("Windows.UI.Input.Spatial.SpatialInteractionSourceState", "TryGetHandPose");
#endif
        }

        /// <summary>
        /// The Windows Mixed Reality articulated hands default interactions.
        /// </summary>
        /// <remarks>A single interaction mapping works for both left and right articulated hands.</remarks>
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Spatial Grip", AxisType.SixDof, DeviceInputType.SpatialGrip, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(2, "Select", AxisType.Digital, DeviceInputType.Select, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(3, "Grab", AxisType.SingleAxis, DeviceInputType.TriggerPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(4, "Index Finger Pose", AxisType.SixDof, DeviceInputType.IndexFinger, MixedRealityInputAction.None)
        };

        #region IMixedRealityHand Implementation

        /// <inheritdoc/>
        public bool TryGetJoint(TrackedHandJoint joint, out MixedRealityPose pose)
        {
#if (UNITY_WSA && DOTNETWINRT_PRESENT) || WINDOWS_UWP
            return unityJointPoses.TryGetValue(joint, out pose);
#else
            pose = MixedRealityPose.ZeroIdentity;
            return false;
#endif
        }

        #endregion IMixedRealityHand Implementation

        public override bool IsInPointingPose
        {
            get
            {
                bool valid = true;
#if (UNITY_WSA && DOTNETWINRT_PRESENT) || WINDOWS_UWP
                Vector3 palmNormal = unityJointOrientations[(int)HandJointKind.Palm] * (-1 * Vector3.up);
                if (CursorBeamBackwardTolerance >= 0)
                {
                    Vector3 cameraBackward = -CameraCache.Main.transform.forward;
                    if (Vector3.Dot(palmNormal.normalized, cameraBackward) > CursorBeamBackwardTolerance)
                    {
                        valid = false;
                    }
                }
                if (valid && CursorBeamUpTolerance >= 0)
                {
                    if (Vector3.Dot(palmNormal, Vector3.up) > CursorBeamUpTolerance)
                    {
                        valid = false;
                    }
                }
#endif // (UNITY_WSA && DOTNETWINRT_PRESENT) || WINDOWS_UWP
                return valid;
            }
        }

#if UNITY_WSA
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
        private MixedRealityPose currentIndexPose = MixedRealityPose.ZeroIdentity;

        private SpatialInteractionManager spatialInteractionManager = null;
        private SpatialInteractionManager SpatialInteractionManager
        {
            get
            {
                if (spatialInteractionManager == null)
                {
                    UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                    {
                        spatialInteractionManager = SpatialInteractionManager.GetForCurrentView();
                    }, true);
                }

                return spatialInteractionManager;
            }
        }

#if WINDOWS_UWP
        private HandMeshObserver handMeshObserver = null;
        private int[] handMeshTriangleIndices = null;
        private bool hasRequestedHandMeshObserver = false;
        private Vector2[] handMeshUVs;
#endif // WINDOWS_UWP

        private readonly float CursorBeamBackwardTolerance = 0.5f;
        private readonly float CursorBeamUpTolerance = 0.8f;

        private readonly bool articulatedHandApiAvailable = false;
#endif // WINDOWS_UWP || DOTNETWINRT_PRESENT

        #region Update data functions

        /// <summary>
        /// Update the controller data from the provided platform state
        /// </summary>
        /// <param name="interactionSourceState">The InteractionSourceState retrieved from the platform</param>
        public override void UpdateController(InteractionSourceState interactionSourceState)
        {
            if (!Enabled) { return; }

            base.UpdateController(interactionSourceState);

            UpdateHandData(interactionSourceState);

            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.IndexFinger:
                        UpdateIndexFingerData(interactionSourceState, Interactions[i]);
                        break;
                }
            }
        }

#if WINDOWS_UWP
        protected void InitializeUVs(Vector3[] neutralPoseVertices)
        {
            if (neutralPoseVertices.Length == 0)
            {
                Debug.LogError("Loaded 0 verts for neutralPoseVertices");
            }

            float minY = neutralPoseVertices[0].y;
            float maxY = minY;

            float maxMagnitude = 0.0f;

            for (int ix = 1; ix < neutralPoseVertices.Length; ix++)
            {
                Vector3 p = neutralPoseVertices[ix];

                if (p.y < minY)
                {
                    minY = p.y;
                }
                else if (p.y > maxY)
                {
                    maxY = p.y;
                }
                float d = p.x * p.x + p.y * p.y;
                if (d > maxMagnitude) maxMagnitude = d;
            }

            maxMagnitude = Mathf.Sqrt(maxMagnitude);
            float scale = 1.0f / (maxY - minY);

            handMeshUVs = new Vector2[neutralPoseVertices.Length];

            for (int ix = 0; ix < neutralPoseVertices.Length; ix++)
            {
                Vector3 p = neutralPoseVertices[ix];

                handMeshUVs[ix] = new Vector2(p.x * scale + 0.5f, (p.y - minY) * scale);
            }
        }

        private async void SetHandMeshObserver(SpatialInteractionSourceState sourceState)
        {
            handMeshObserver = await sourceState.Source.TryCreateHandMeshObserverAsync();
        }
#endif // WINDOWS_UWP

        /// <summary>
        /// Update the hand data from the device.
        /// </summary>
        /// <param name="interactionSourceState">The InteractionSourceState retrieved from the platform.</param>
        private void UpdateHandData(InteractionSourceState interactionSourceState)
        {
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
            // Articulated hand support is only present in the 18362 version and beyond Windows
            // SDK (which contains the V8 drop of the Universal API Contract). In particular,
            // the HandPose related APIs are only present on this version and above.
            if (!articulatedHandApiAvailable)
            {
                return;
            }

            PerceptionTimestamp perceptionTimestamp = PerceptionTimestampHelper.FromHistoricalTargetTime(DateTimeOffset.Now);
            IReadOnlyList<SpatialInteractionSourceState> sources = SpatialInteractionManager?.GetDetectedSourcesAtTimestamp(perceptionTimestamp);
            foreach (SpatialInteractionSourceState sourceState in sources)
            {
                if (sourceState.Source.Id.Equals(interactionSourceState.source.id))
                {
                    HandPose handPose = sourceState.TryGetHandPose();

#if WINDOWS_UWP
                    if (CoreServices.InputSystem.InputSystemProfile.HandTrackingProfile.EnableHandMeshVisualization)
                    {
                        // Accessing the hand mesh data involves copying quite a bit of data, so only do it if application requests it.
                        if (handMeshObserver == null && !hasRequestedHandMeshObserver)
                        {
                            SetHandMeshObserver(sourceState);
                            hasRequestedHandMeshObserver = true;
                        }

                        if (handMeshObserver != null && handMeshTriangleIndices == null)
                        {
                            uint indexCount = handMeshObserver.TriangleIndexCount;
                            ushort[] indices = new ushort[indexCount];
                            handMeshObserver.GetTriangleIndices(indices);
                            handMeshTriangleIndices = new int[indexCount];
                            Array.Copy(indices, handMeshTriangleIndices, (int)handMeshObserver.TriangleIndexCount);

                            // Compute neutral pose
                            Vector3[] neutralPoseVertices = new Vector3[handMeshObserver.VertexCount];
                            HandPose neutralPose = handMeshObserver.NeutralPose;
                            var vertexAndNormals = new HandMeshVertex[handMeshObserver.VertexCount];
                            HandMeshVertexState handMeshVertexState = handMeshObserver.GetVertexStateForPose(neutralPose);
                            handMeshVertexState.GetVertices(vertexAndNormals);

                            for (int i = 0; i < handMeshObserver.VertexCount; i++)
                            {
                                neutralPoseVertices[i] = WindowsMixedRealityUtilities.SystemVector3ToUnity(vertexAndNormals[i].Position);
                            }

                            // Compute UV mapping
                            InitializeUVs(neutralPoseVertices);
                        }

                        if (handPose != null && handMeshObserver != null && handMeshTriangleIndices != null)
                        {
                            var vertexAndNormals = new HandMeshVertex[handMeshObserver.VertexCount];
                            var handMeshVertexState = handMeshObserver.GetVertexStateForPose(handPose);
                            handMeshVertexState.GetVertices(vertexAndNormals);

                            var meshTransform = handMeshVertexState.CoordinateSystem.TryGetTransformTo(WindowsMixedRealityUtilities.SpatialCoordinateSystem);
                            if (meshTransform.HasValue)
                            {
                                System.Numerics.Vector3 scale;
                                System.Numerics.Quaternion rotation;
                                System.Numerics.Vector3 translation;
                                System.Numerics.Matrix4x4.Decompose(meshTransform.Value, out scale, out rotation, out translation);

                                var handMeshVertices = new Vector3[handMeshObserver.VertexCount];
                                var handMeshNormals = new Vector3[handMeshObserver.VertexCount];

                                for (int i = 0; i < handMeshObserver.VertexCount; i++)
                                {
                                    handMeshVertices[i] = WindowsMixedRealityUtilities.SystemVector3ToUnity(vertexAndNormals[i].Position);
                                    handMeshNormals[i] = WindowsMixedRealityUtilities.SystemVector3ToUnity(vertexAndNormals[i].Normal);
                                }

                                HandMeshInfo handMeshInfo = new HandMeshInfo
                                {
                                    vertices = handMeshVertices,
                                    normals = handMeshNormals,
                                    triangles = handMeshTriangleIndices,
                                    uvs = handMeshUVs,
                                    position = WindowsMixedRealityUtilities.SystemVector3ToUnity(translation),
                                    rotation = WindowsMixedRealityUtilities.SystemQuaternionToUnity(rotation)
                                };

                                CoreServices.InputSystem?.RaiseHandMeshUpdated(InputSource, ControllerHandedness, handMeshInfo);
                            }
                        }
                    }
                    else
                    {
                        // if hand mesh visualization is disabled make sure to destroy our hand mesh observer if it has already been created
                        if (handMeshObserver != null)
                        {
                            // notify that hand mesh has been updated (cleared)
                            HandMeshInfo handMeshInfo = new HandMeshInfo();
                            CoreServices.InputSystem?.RaiseHandMeshUpdated(InputSource, ControllerHandedness, handMeshInfo);
                            hasRequestedHandMeshObserver = false;
                            handMeshObserver = null;
                        }
                    }
#endif // WINDOWS_UWP

                    if (handPose != null && handPose.TryGetJoints(WindowsMixedRealityUtilities.SpatialCoordinateSystem, jointIndices, jointPoses))
                    {
                        for (int i = 0; i < jointPoses.Length; i++)
                        {
                            unityJointOrientations[i] = WindowsMixedRealityUtilities.SystemQuaternionToUnity(jointPoses[i].Orientation);
                            unityJointPositions[i] = WindowsMixedRealityUtilities.SystemVector3ToUnity(jointPoses[i].Position);

                            // We want the controller to follow the Playspace, so fold in the playspace transform here to 
                            // put the controller pose into world space.
                            unityJointPositions[i] = MixedRealityPlayspace.TransformPoint(unityJointPositions[i]);
                            unityJointOrientations[i] = MixedRealityPlayspace.Rotation * unityJointOrientations[i];

                            if (jointIndices[i] == HandJointKind.IndexTip)
                            {
                                lastIndexTipRadius = jointPoses[i].Radius;
                            }

                            TrackedHandJoint handJoint = ConvertHandJointKindToTrackedHandJoint(jointIndices[i]);

                            if (!unityJointPoses.ContainsKey(handJoint))
                            {
                                unityJointPoses.Add(handJoint, new MixedRealityPose(unityJointPositions[i], unityJointOrientations[i]));
                            }
                            else
                            {
                                unityJointPoses[handJoint] = new MixedRealityPose(unityJointPositions[i], unityJointOrientations[i]);
                            }
                        }
                        CoreServices.InputSystem?.RaiseHandJointsUpdated(InputSource, ControllerHandedness, unityJointPoses);
                    }
                }
            }
#endif // WINDOWS_UWP || DOTNETWINRT_PRESENT
        }

        private void UpdateIndexFingerData(InteractionSourceState interactionSourceState, MixedRealityInteractionMapping interactionMapping)
        {
#if WINDOWS_UWP || DOTNETWINRT_PRESENT
            UpdateCurrentIndexPose();

            // Update the interaction data source
            interactionMapping.PoseData = currentIndexPose;

            // If our value changed raise it.
            if (interactionMapping.Changed)
            {
                // Raise input system event if it's enabled
                CoreServices.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, currentIndexPose);
            }
#endif // WINDOWS_UWP || DOTNETWINRT_PRESENT
        }

        #endregion Update data functions

#if WINDOWS_UWP || DOTNETWINRT_PRESENT
        private static readonly HandJointKind[] jointIndices = new HandJointKind[]
        {
            HandJointKind.Palm,
            HandJointKind.Wrist,
            HandJointKind.ThumbMetacarpal,
            HandJointKind.ThumbProximal,
            HandJointKind.ThumbDistal,
            HandJointKind.ThumbTip,
            HandJointKind.IndexMetacarpal,
            HandJointKind.IndexProximal,
            HandJointKind.IndexIntermediate,
            HandJointKind.IndexDistal,
            HandJointKind.IndexTip,
            HandJointKind.MiddleMetacarpal,
            HandJointKind.MiddleProximal,
            HandJointKind.MiddleIntermediate,
            HandJointKind.MiddleDistal,
            HandJointKind.MiddleTip,
            HandJointKind.RingMetacarpal,
            HandJointKind.RingProximal,
            HandJointKind.RingIntermediate,
            HandJointKind.RingDistal,
            HandJointKind.RingTip,
            HandJointKind.LittleMetacarpal,
            HandJointKind.LittleProximal,
            HandJointKind.LittleIntermediate,
            HandJointKind.LittleDistal,
            HandJointKind.LittleTip
        };

        private readonly JointPose[] jointPoses = new JointPose[jointIndices.Length];
        private readonly Vector3[] unityJointPositions = new Vector3[jointIndices.Length];
        private readonly Quaternion[] unityJointOrientations = new Quaternion[jointIndices.Length];
        private readonly Dictionary<TrackedHandJoint, MixedRealityPose> unityJointPoses = new Dictionary<TrackedHandJoint, MixedRealityPose>();
        private float lastIndexTipRadius = 0;

        private TrackedHandJoint ConvertHandJointKindToTrackedHandJoint(HandJointKind handJointKind)
        {
            switch (handJointKind)
            {
                case HandJointKind.Palm: return TrackedHandJoint.Palm;

                case HandJointKind.Wrist: return TrackedHandJoint.Wrist;

                case HandJointKind.ThumbMetacarpal: return TrackedHandJoint.ThumbMetacarpalJoint;
                case HandJointKind.ThumbProximal: return TrackedHandJoint.ThumbProximalJoint;
                case HandJointKind.ThumbDistal: return TrackedHandJoint.ThumbDistalJoint;
                case HandJointKind.ThumbTip: return TrackedHandJoint.ThumbTip;

                case HandJointKind.IndexMetacarpal: return TrackedHandJoint.IndexMetacarpal;
                case HandJointKind.IndexProximal: return TrackedHandJoint.IndexKnuckle;
                case HandJointKind.IndexIntermediate: return TrackedHandJoint.IndexMiddleJoint;
                case HandJointKind.IndexDistal: return TrackedHandJoint.IndexDistalJoint;
                case HandJointKind.IndexTip: return TrackedHandJoint.IndexTip;

                case HandJointKind.MiddleMetacarpal: return TrackedHandJoint.MiddleMetacarpal;
                case HandJointKind.MiddleProximal: return TrackedHandJoint.MiddleKnuckle;
                case HandJointKind.MiddleIntermediate: return TrackedHandJoint.MiddleMiddleJoint;
                case HandJointKind.MiddleDistal: return TrackedHandJoint.MiddleDistalJoint;
                case HandJointKind.MiddleTip: return TrackedHandJoint.MiddleTip;

                case HandJointKind.RingMetacarpal: return TrackedHandJoint.RingMetacarpal;
                case HandJointKind.RingProximal: return TrackedHandJoint.RingKnuckle;
                case HandJointKind.RingIntermediate: return TrackedHandJoint.RingMiddleJoint;
                case HandJointKind.RingDistal: return TrackedHandJoint.RingDistalJoint;
                case HandJointKind.RingTip: return TrackedHandJoint.RingTip;

                case HandJointKind.LittleMetacarpal: return TrackedHandJoint.PinkyMetacarpal;
                case HandJointKind.LittleProximal: return TrackedHandJoint.PinkyKnuckle;
                case HandJointKind.LittleIntermediate: return TrackedHandJoint.PinkyMiddleJoint;
                case HandJointKind.LittleDistal: return TrackedHandJoint.PinkyDistalJoint;
                case HandJointKind.LittleTip: return TrackedHandJoint.PinkyTip;

                default: return TrackedHandJoint.None;
            }
        }

        #region Protected InputSource Helpers

        protected void UpdateCurrentIndexPose()
        {
            currentIndexPose.Rotation = unityJointOrientations[(int)HandJointKind.IndexTip];

            var skinOffsetFromBone = (currentIndexPose.Rotation * Vector3.forward * lastIndexTipRadius);
            currentIndexPose.Position = (unityJointPositions[(int)HandJointKind.IndexTip] + skinOffsetFromBone);
        }

        #endregion Private InputSource Helpers

#endif // WINDOWS_UWP || DOTNETWINRT_PRESENT
#endif // UNITY_WSA
    }
}
