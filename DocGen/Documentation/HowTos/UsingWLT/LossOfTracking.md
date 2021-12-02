---
title: Handling exceptional conditions
description: Fail-safes in place for when things temporarily go wrong.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Handling exceptional conditions

For the most part, WLT can detect and fix tracking errors quietly without the application's involvement.

But some exceptional conditions lead to errors which the application might want to adjust to.

Loss of tracking is an example of such a condition.

Tracking might be lost at any time, for any of a number of reasons. The sensors might be covered, the lighting might be inadequate, or there might not be any visible features around the camera for it to track. 

Fuller discussions of these [exceptional conditions on a conceptual level](../../Concepts/Advanced/RefitOperations.md), including [WLT's features aimed at mitigating them](../../Concepts/Advanced/AttachmentPoints.md), are contained elsewhere in this documentation.

Here, we'll dig into how the application developer can (optionally) take advantage of those features to customize the application's behavior during these exceptional conditions.

## AttachmentPoints

As discussed more fully [here](../../Concepts/Advanced/AttachmentPoints.md), an attachment point is the contract between WLT and the application, for notification that exceptional conditions have occurred, along with appropriate data which the application may use to respond.

## Adjuster components

An implementation of such application responses is available in the form of the "adjuster" components. The primary of those is the [AdjusterFixed](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed) component.

The AdjusterFixed can be used as-is, but understanding what it does can be instructive, especially for a developer wanting to further customize the behavior.

It is important to recognize that the Adjuster components serve two roles:

1. They manage the underlying AttachmentPoint.
2. They provide implementations of the application's responses to exceptional conditions.  

### AttachmentPoint management

Examining the `Start()` and `OnDestroy()` members captures most of the management of the AttachmentPoint required.

On `Start()`, the underlying AttachmentPoint is created, giving the AdjusterFixed's member functions as callbacks (see below).

In `OnDestroy()`, these callback connections are severed and the AttachmentPoint released.

### Condition handling callbacks

The two callbacks implement the application's desired behavior during these exceptional conditions.

#### Handling tracking state

In `HandleStateAdjust()`, the AdjusterFixed component disables objects contained in a fragment which isn't currently being tracked.

```csharp
        protected virtual void HandleAdjustState(AttachmentPointStateType state)
        {
            bool visible = state == AttachmentPointStateType.Normal;
            if (visible != gameObject.activeSelf)
            {
                gameObject.SetActive(visible);
            }
        }
```

While this simple behavior is perfect for many applications, it is easy to imagine cases when it would not be sufficient.

1. The object should be hidden, but not disabled (should continue updating).
2. An alternate method of hiding the object is preferred (e.g. moving it beyond the far clipping plane).
3. Rather than hiding the object, it should be rendered with a different material (e.g. X-ray material).
4. Rather than hiding the object, an alternate object should be rendered.
5. Etc.

Fortunately, the application developer is free to implement any of these behaviors, or other behaviors only limited by imagination.

The simplest means of specifying custom behavior is to implement a custom component deriving from AdjusterFixed. The AttachmentPoint management can then be inherited, and the handlers overridden to create the custom behavior.

#### Handling repositioning

As described in the [conceptual documentation](../../Concepts/Advanced/RefitOperations.md), the WLT system may decide that an object can be best held in its position in the physical world by repositioning it in frozen space. It will inform the application of that situation via the AttachmentPoint mechanism.

The application is, of course, free to ignore such adjustments. However, the behavior provided by the AdjusterFixed (and AdjusterMoving) component is to apply that repositioning transform immediately.

```csharp
        protected virtual void HandleAdjustLocation(Pose adjustment)
        {
            Pose pose = gameObject.transform.GetGlobalPose();
            pose = adjustment.Multiply(pose);
            gameObject.transform.SetGlobalPose(pose);
        }
```

That is almost always what the application wants. The question might be asked, then, of why anyone would want to override the AdjusterFixed's `HandlePositionAdjust()` function.

The answer, of course, is that the application might want to perform other actions in addition to correcting the position. A temporary material effect might help notify the user that a change has been made. The repositioning might be spread out over a few seconds. Or if a repositioning is too drastic, the application might prefer to discard the object, rather than moving it.

## AdjusterFixed vs AdjusterMoving

A closer look at the [AdjusterMoving](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterMoving) component shows it to be nearly identical to the [AdjusterFixed](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed) component it derives from.

The difference between the two is that the AdjusterMoving assumes that its target is constantly being moved around the environment. Therefore, each update it notifies the WLT system of its new Pose.

The cost of the AdjusterMoving comes mostly from the addition of an [Update()](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html) function, rather than the work done within the function. However, for an object that is "mostly" stationary, and is only moved infrequently from script, it can be advantageous to use an AdjusterFixed component, and call [AdjusterFixed.UpdatePosition()](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed.UpdatePosition) after each time the object is moved.

## Customize the behavior, but only if you want to

Again, the pattern here is hopefully consistent throughout the World Locking Tools. WLT provides simple but generally useful baseline behavior. It is hoped that this implementation will either:

1. Satisfy the needs of your application.
2. Provide a baseline implementation for you to enhance.
3. Give a sample implementation from which you can go wild.

## See also

* [Before You Start](BeforeGettingStarted.md)
* [Most Basic Setup](JustWorldLock.md)
* [Across Sessions](PersistenceTricks.md)
* [Pinning It Down](AlignMyCoordinates.md)
