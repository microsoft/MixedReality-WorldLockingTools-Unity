// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;


namespace Microsoft.MixedReality.WorldLocking.Examples
{
    /// <summary>
    /// Callback for when the user has finished positioning and/or orienting the target.
    /// </summary>
    public delegate void ManipulationEndedDelegate();

    /// <summary>
    /// Helper class to add MRTK object manipulation controls to an object.
    /// </summary>
    public class PinManipulator
    {
        #region Public fields

        /// <summary>
        /// Whether to enable user orientation of the object. If false, only positioning enabled.
        /// </summary>
        private bool userOriented = false;

        /// <summary>
        /// Whether to enable user orientation of the object. If false, only positioning enabled.
        /// </summary>
        /// <remarks>
        /// May be toggled from script during runtime.
        /// </remarks>
        public bool UserOriented
        {
            get { return userOriented; }
            set
            {
                if (value != userOriented)
                {
                    userOriented = value;
                    if (manipulationHandler != null)
                    {
                        SetupManipulation();
                    }
                }
            }
        }
        #endregion Public fields

        #region Internal fields

        /// <summary>
        /// The object to position and/or orient.
        /// </summary>
        private readonly Transform owner;

        /// <summary>
        /// The prefab for the visualization rays.
        /// </summary>
        private readonly GameObject prefabFeelerRay;

        /// <summary>
        /// The callback when the user finishes a manipulation.
        /// </summary>
        private readonly ManipulationEndedDelegate manipulationEnded;

        /// <summary>
        /// Bounding box to assist in manipulation.
        /// </summary>
        private BoundingBox boundingBox;

        /// <summary>
        /// Event handler for manipulations.
        /// </summary>
        private ObjectManipulator manipulationHandler;

        /// <summary>
        /// Make the target grabbable.
        /// </summary>
        private NearInteractionGrabbable nearGrabbable;

        /// <summary>
        /// Instance of the feeler ray visualization prefab (if any). May be null for no axis visualization.
        /// </summary>
        private GameObject feelerRays;

        /// <summary>
        ///  A common node to hang feeler rays off of. This is a (temporary?) workaround for issues around
        ///  the MRTK BoundingBox's bounds computation and BoundsOverride.
        /// </summary>
        private static Transform feelerRayParent = null;

        #endregion Internal fields

        /// <summary>
        /// Constructor accepts readonly dependencies.
        /// </summary>
        /// <param name="owner">The object to manipulate.</param>
        /// <param name="prefab">The visualization prefab to instantiate.</param>
        /// <param name="del">The manipulation ended callback.</param>
        public PinManipulator(Transform owner, GameObject prefab, ManipulationEndedDelegate del)
        {
            this.owner = owner;
            prefabFeelerRay = prefab;
            manipulationEnded = del;

            SetupFeelerRays();
        }

        /// <summary>
        /// Get set up.
        /// </summary>
        public void Startup()
        {
            SetupManipulation();
        }

        /// <summary>
        /// If active, position and orient the visualization.
        /// </summary>
        public virtual void Update()
        {
            if (feelerRays != null && feelerRays.gameObject.activeSelf)
            {
                feelerRays.transform.SetGlobalPose(owner.GetGlobalPose());
            }
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public virtual void Shutdown()
        {
            TakeDownFeelerRays();
            TakeDownManipulation();
        }

        #region Display

        /// <summary>
        /// Get the common feeler ray parent (workaround), creating if necessary.
        /// </summary>
        /// <returns>The parent GameObject.</returns>
        private static Transform GetFeelerRayParent()
        {
            if (feelerRayParent == null)
            {
                var go = new GameObject("Feeler Rays Group Parent");
                feelerRayParent = go.transform;
            }
            return feelerRayParent;
        }

        /// <summary>
        /// Create the visualization if it doesn't exist.
        /// </summary>
        /// <remarks>
        /// If necessary, an attachment node (feelerRayParent) will be created to add the visualization objects to.
        /// </remarks>
        private void SetupFeelerRays()
        {
            if (feelerRays == null && prefabFeelerRay != null)
            {
                feelerRays = GameObject.Instantiate(prefabFeelerRay, GetFeelerRayParent());
            }
            if (feelerRays != null)
            {
                feelerRays.SetActive(false);
            }
        }

        /// <summary>
        /// Cleanup the visualization if it exists.
        /// </summary>
        /// <remarks>
        /// If cleanup of this visualization leaves the feelerRayParent without children, it is disposed of too.
        /// </remarks>
        private void TakeDownFeelerRays()
        {
            if (feelerRays != null)
            {
                // Looks like there is a known Unity bug with an incorrect error message
                // when destroying an object "while" changing its hierarchy. 
                // https://issuetracker.unity3d.com/issues/transform-component-being-destroyed-on-application-quit-when-its-gameobjects-parent-is-being-changed
                // At any rate, destroying the gameobject will also remove it from its parent.
                GameObject.Destroy(feelerRays);
            }
            if (feelerRayParent != null)
            {
                // Note that even if this is the last feeler rays hanging off of feelerRayParent,
                // feelerRayParent.transform.childCount won't drop to zero until next frame after
                // the above GaemObject.Destroy() actually happens.
                // That's okay, the parent will get cleaned up when the scene is unloaded.
                if (feelerRayParent.transform.childCount == 0)
                {
                    GameObject.Destroy(feelerRayParent.gameObject);
                    feelerRayParent = null;
                }
            }
        }

        /// <summary>
        /// Display the feeler rays.
        /// </summary>
        private void ShowFeelerRays()
        {
            if (feelerRays != null)
            {
                feelerRays.transform.SetGlobalPose(owner.GetGlobalPose());
                feelerRays.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the feeler rays.
        /// </summary>
        private void HideFeelerRays()
        {
            if (feelerRays != null)
            {
                feelerRays.SetActive(false);
            }
        }


#endregion Display

#region Manipulation callbacks

        /// <summary>
        /// Create all necessary resources.
        /// </summary>
        private void SetupManipulation()
        {
            TakeDownManipulation();
            if (userOriented)
            {
                boundingBox = owner.gameObject.AddComponent<BoundingBox>();

                boundingBox.HideElementsInInspector = false;
                boundingBox.BoundingBoxActivation = BoundingBox.BoundingBoxActivationType.ActivateByProximityAndPointer;
                boundingBox.RotateStarted.AddListener(BeginManipulation);
                boundingBox.RotateStopped.AddListener(FinishManipulation);
                boundingBox.ScaleStarted.AddListener(BeginManipulation);
                boundingBox.ScaleStopped.AddListener(FinishManipulation);
                float maxScaleFactor = 8.0f;
                float minScaleFactor = 0.2f;
                MinMaxScaleConstraint scaleHandler = owner.GetComponent<MinMaxScaleConstraint>();
                if (scaleHandler == null)
                {
                    scaleHandler = owner.gameObject.AddComponent<MinMaxScaleConstraint>();
                }
                scaleHandler.RelativeToInitialState = true;
                scaleHandler.ScaleMaximum = maxScaleFactor;
                scaleHandler.ScaleMinimum = minScaleFactor;

            }

            manipulationHandler = owner.gameObject.AddComponent<ObjectManipulator>();

            var rotationAxisConstraint = owner.gameObject.AddComponent<RotationAxisConstraint>();
            rotationAxisConstraint.HandType = Toolkit.Utilities.ManipulationHandFlags.OneHanded | Toolkit.Utilities.ManipulationHandFlags.TwoHanded;
            rotationAxisConstraint.ProximityType = Toolkit.Utilities.ManipulationProximityFlags.Near | Toolkit.Utilities.ManipulationProximityFlags.Far;
            rotationAxisConstraint.ConstraintOnRotation = 0;

            var fixedRotationToWorldConstraint = owner.gameObject.AddComponent<FixedRotationToWorldConstraint>();
            fixedRotationToWorldConstraint.HandType = Toolkit.Utilities.ManipulationHandFlags.OneHanded;
            fixedRotationToWorldConstraint.ProximityType = Toolkit.Utilities.ManipulationProximityFlags.Near | Toolkit.Utilities.ManipulationProximityFlags.Far;

            manipulationHandler.OnManipulationStarted.AddListener(BeginManipulation);
            manipulationHandler.OnManipulationEnded.AddListener(FinishManipulation);

            nearGrabbable = owner.gameObject.AddComponent<NearInteractionGrabbable>();
        }

        /// <summary>
        /// Clean up all resources.
        /// </summary>
        private void TakeDownManipulation()
        {
            boundingBox = null;
            manipulationHandler = null;
            nearGrabbable = null;
        }

        /// <summary>
        /// Enable visuals etc. to assist manipulation.
        /// </summary>
        private void BeginManipulation()
        {
            ShowFeelerRays();
        }
        /// <summary>
        /// Different signature for BeginManipulation() required by MRTK.
        /// </summary>
        /// <param name="eventData">ignored.</param>
        private void BeginManipulation(ManipulationEventData eventData)
        {
            BeginManipulation();
        }

        /// <summary>
        /// Hide manipulation visualizations, and process the manipulation data.
        /// </summary>
        private void FinishManipulation()
        {
            HideFeelerRays();

            if (manipulationEnded != null)
            {
                manipulationEnded();
            }
        }

        /// <summary>
        /// Different signature for FinishManipulation(), required by MRTK.
        /// </summary>
        /// <param name="eventData">ignored</param>
        private void FinishManipulation(ManipulationEventData eventData)
        {
            FinishManipulation();
        }

#endregion Manipulation callbacks

    }
}