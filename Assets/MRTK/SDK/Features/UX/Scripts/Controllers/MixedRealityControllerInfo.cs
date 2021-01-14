﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// This script keeps track of the GameObjects representations for each button on the Mixed Reality Controllers.
    /// It also keeps track of the animation Transforms in order to properly animate according to user input.
    /// </summary>
    public class MixedRealityControllerInfo
    {
        public readonly GameObject ControllerParent;
        public readonly Handedness Handedness;

        private GameObject home;
        private Transform homePressed;
        private Transform homeUnpressed;
        private GameObject menu;
        private Transform menuPressed;
        private Transform menuUnpressed;
        private GameObject grasp;
        private Transform graspPressed;
        private Transform graspUnpressed;
        private GameObject thumbstickPress;
        private Transform thumbstickPressed;
        private Transform thumbstickUnpressed;
        private GameObject thumbstickX;
        private Transform thumbstickXMin;
        private Transform thumbstickXMax;
        private GameObject thumbstickY;
        private Transform thumbstickYMin;
        private Transform thumbstickYMax;
        private GameObject select;
        private Transform selectPressed;
        private Transform selectUnpressed;
        private GameObject touchpadPress;
        private Transform touchpadPressed;
        private Transform touchpadUnpressed;
        private GameObject touchpadTouchX;
        private Transform touchpadTouchXMin;
        private Transform touchpadTouchXMax;
        private GameObject touchpadTouchY;
        private Transform touchpadTouchYMin;
        private Transform touchpadTouchYMax;
        private GameObject touchpadTouchVisualizer = null;
        private GameObject pointingPose;

        // These values are used to determine if a button's state has changed.
        private bool wasGrasped;
        private bool wasMenuPressed;
        private bool wasHomePressed;
        private bool wasThumbstickPressed;
        private bool wasTouchpadPressed;
        private bool wasTouchpadTouched;
        private Vector2 lastThumbstickPosition;
        private Vector2 lastTouchpadPosition;
        private double lastSelectPressedAmount;

        public MixedRealityControllerInfo(GameObject controllerParent, Handedness handedness)
        {
            ControllerParent = controllerParent;
            Handedness = handedness;
        }

        public enum ControllerElementEnum
        {
            // Controller button elements
            Home,
            Menu,
            Grasp,
            Thumbstick,
            Select,
            Touchpad,
            // Controller body elements & poses
            PointingPose
        }

        public bool TryGetElement(ControllerElementEnum element, out Transform elementTransform)
        {
            switch (element)
            {
                // control elements
                case ControllerElementEnum.Home:
                    if (home != null)
                    {
                        elementTransform = home.transform;
                        return true;
                    }
                    break;
                case ControllerElementEnum.Menu:
                    if (menu != null)
                    {
                        elementTransform = menu.transform;
                        return true;
                    }
                    break;
                case ControllerElementEnum.Select:
                    if (select != null)
                    {
                        elementTransform = select.transform;
                        return true;
                    }
                    break;
                case ControllerElementEnum.Grasp:
                    if (grasp != null)
                    {
                        elementTransform = grasp.transform;
                        return true;
                    }
                    break;
                case ControllerElementEnum.Thumbstick:
                    if (thumbstickPress != null)
                    {
                        elementTransform = thumbstickPress.transform;
                        return true;
                    }
                    break;
                case ControllerElementEnum.Touchpad:
                    if (touchpadPress != null)
                    {
                        elementTransform = touchpadPress.transform;
                        return true;
                    }
                    break;
                // body elements & poses
                case ControllerElementEnum.PointingPose:
                    if (pointingPose != null)
                    {
                        elementTransform = pointingPose.transform;
                        return true;
                    }
                    break;
            }

            elementTransform = null;
            return false;
        }

        /// <summary>
        /// Iterates through the Transform array to find specifically named GameObjects.
        /// These GameObjects specify the animation bounds and the GameObject to modify for button,
        /// thumbstick, and touchpad animation.
        /// </summary>
        /// <param name="childTransforms">The transforms of the glTF model.</param>
        public void LoadInfo(Transform[] childTransforms, MixedRealityControllerVisualizer motionControllerVisualizer)
        {
            foreach (Transform child in childTransforms)
            {
                // Animation bounds are named in two pairs:
                // pressed/unpressed and min/max. There is also a value
                // transform, which is the transform to modify to
                // animate the interactions. We also look for the
                // touch transform, in order to spawn the touchpadTouched
                // visualizer.
                switch (child.name.ToLower())
                {
                    case "pointing_pose":
                        pointingPose = child.gameObject;
                        break;
                    case "pressed":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                homePressed = child;
                                break;
                            case "menu":
                                menuPressed = child;
                                break;
                            case "grasp":
                                graspPressed = child;
                                break;
                            case "select":
                                selectPressed = child;
                                break;
                            case "thumbstick_press":
                                thumbstickPressed = child;
                                break;
                            case "touchpad_press":
                                touchpadPressed = child;
                                break;
                        }
                        break;
                    case "unpressed":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                homeUnpressed = child;
                                break;
                            case "menu":
                                menuUnpressed = child;
                                break;
                            case "grasp":
                                graspUnpressed = child;
                                break;
                            case "select":
                                selectUnpressed = child;
                                break;
                            case "thumbstick_press":
                                thumbstickUnpressed = child;
                                break;
                            case "touchpad_press":
                                touchpadUnpressed = child;
                                break;
                        }
                        break;
                    case "min":
                        switch (child.parent.name.ToLower())
                        {
                            case "thumbstick_x":
                                thumbstickXMin = child;
                                break;
                            case "thumbstick_y":
                                thumbstickYMin = child;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchXMin = child;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchYMin = child;
                                break;
                        }
                        break;
                    case "max":
                        switch (child.parent.name.ToLower())
                        {
                            case "thumbstick_x":
                                thumbstickXMax = child;
                                break;
                            case "thumbstick_y":
                                thumbstickYMax = child;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchXMax = child;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchYMax = child;
                                break;
                        }
                        break;
                    case "value":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                home = child.gameObject;
                                break;
                            case "menu":
                                menu = child.gameObject;
                                break;
                            case "grasp":
                                grasp = child.gameObject;
                                break;
                            case "select":
                                select = child.gameObject;
                                break;
                            case "thumbstick_press":
                                thumbstickPress = child.gameObject;
                                break;
                            case "thumbstick_x":
                                thumbstickX = child.gameObject;
                                break;
                            case "thumbstick_y":
                                thumbstickY = child.gameObject;
                                break;
                            case "touchpad_press":
                                touchpadPress = child.gameObject;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchX = child.gameObject;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchY = child.gameObject;
                                break;
                        }
                        break;
                }
            }
        }

        public void AnimateGrasp(bool isGrasped)
        {
            if (grasp != null && graspPressed != null && graspUnpressed != null && isGrasped != wasGrasped)
            {
                SetLocalPositionAndRotation(grasp, isGrasped ? graspPressed : graspUnpressed);
                wasGrasped = isGrasped;
            }
        }

        public void AnimateMenu(bool isMenuPressed)
        {
            if (menu != null && menuPressed != null && menuUnpressed != null && isMenuPressed != wasMenuPressed)
            {
                SetLocalPositionAndRotation(menu, isMenuPressed ? menuPressed : menuUnpressed);
                wasMenuPressed = isMenuPressed;
            }
        }

        public void AnimateHome(bool isHomePressed)
        {
            if (home != null && homePressed != null && homeUnpressed != null && isHomePressed != wasHomePressed)
            {
                SetLocalPositionAndRotation(home, isHomePressed ? homePressed : homeUnpressed);
                wasHomePressed = isHomePressed;
            }
        }

        public void AnimateSelect(float newSelectPressedAmount)
        {
            if (select != null && selectPressed != null && selectUnpressed != null && !newSelectPressedAmount.Equals((float)lastSelectPressedAmount))
            {
                select.transform.localPosition = Vector3.Lerp(selectUnpressed.localPosition, selectPressed.localPosition, newSelectPressedAmount);
                select.transform.localRotation = Quaternion.Lerp(selectUnpressed.localRotation, selectPressed.localRotation, newSelectPressedAmount);
                lastSelectPressedAmount = newSelectPressedAmount;
            }
        }

        public void AnimateThumbstick(bool isThumbstickPressed, Vector2 newThumbstickPosition)
        {
            if (thumbstickPress != null && thumbstickPressed != null && thumbstickUnpressed != null && isThumbstickPressed != wasThumbstickPressed)
            {
                SetLocalPositionAndRotation(thumbstickPress, isThumbstickPressed ? thumbstickPressed : thumbstickUnpressed);
                wasThumbstickPressed = isThumbstickPressed;
            }

            if (thumbstickX != null && thumbstickY != null && thumbstickXMin != null && thumbstickXMax != null && thumbstickYMin != null && thumbstickYMax != null && newThumbstickPosition != lastThumbstickPosition)
            {
                Vector2 thumbstickNormalized = (newThumbstickPosition + Vector2.one) * 0.5f;

                thumbstickX.transform.localPosition = Vector3.Lerp(thumbstickXMin.localPosition, thumbstickXMax.localPosition, thumbstickNormalized.x);
                thumbstickX.transform.localRotation = Quaternion.Lerp(thumbstickXMin.localRotation, thumbstickXMax.localRotation, thumbstickNormalized.x);

                thumbstickY.transform.localPosition = Vector3.Lerp(thumbstickYMax.localPosition, thumbstickYMin.localPosition, thumbstickNormalized.y);
                thumbstickY.transform.localRotation = Quaternion.Lerp(thumbstickYMax.localRotation, thumbstickYMin.localRotation, thumbstickNormalized.y);

                lastThumbstickPosition = newThumbstickPosition;
            }
        }

        public void AnimateTouchpad(bool isTouchpadPressed, bool isTouchpadTouched, Vector2 newTouchpadPosition)
        {
            if (touchpadPress != null && touchpadPressed != null && touchpadUnpressed != null && isTouchpadPressed != wasTouchpadPressed)
            {
                SetLocalPositionAndRotation(touchpadPress, isTouchpadPressed ? touchpadPressed : touchpadUnpressed);
                wasTouchpadPressed = isTouchpadPressed;
            }

            if (touchpadTouchVisualizer != null && isTouchpadTouched != wasTouchpadTouched)
            {
                touchpadTouchVisualizer.SetActive(isTouchpadTouched);
                wasTouchpadTouched = isTouchpadTouched;
            }

            if (touchpadTouchX != null && touchpadTouchY != null && touchpadTouchXMin != null && touchpadTouchXMax != null && touchpadTouchYMin != null && touchpadTouchYMax != null && newTouchpadPosition != lastTouchpadPosition)
            {
                Vector2 touchpadNormalized = (newTouchpadPosition + Vector2.one) * 0.5f;

                touchpadTouchX.transform.localPosition = Vector3.Lerp(touchpadTouchXMin.localPosition, touchpadTouchXMax.localPosition, touchpadNormalized.x);
                touchpadTouchX.transform.localRotation = Quaternion.Lerp(touchpadTouchXMin.localRotation, touchpadTouchXMax.localRotation, touchpadNormalized.x);

                touchpadTouchY.transform.localPosition = Vector3.Lerp(touchpadTouchYMax.localPosition, touchpadTouchYMin.localPosition, touchpadNormalized.y);
                touchpadTouchY.transform.localRotation = Quaternion.Lerp(touchpadTouchYMax.localRotation, touchpadTouchYMin.localRotation, touchpadNormalized.y);

                lastTouchpadPosition = newTouchpadPosition;
            }
        }

        private void SetLocalPositionAndRotation(GameObject buttonGameObject, Transform newTransform)
        {
            buttonGameObject.transform.localPosition = newTransform.localPosition;
            buttonGameObject.transform.localRotation = newTransform.localRotation;
        }

        public void SetRenderersVisible(bool visible)
        {
            MeshRenderer[] renderers = ControllerParent.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = visible;
            }
        }
    }
}