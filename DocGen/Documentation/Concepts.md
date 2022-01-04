---
title: World Locking Tools concepts
description: Explains the problems World Locking Tools solves, and how it solves them.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.prod: mixed-reality
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# World Locking Tools concepts

## The problem

In the day-to-day physical world, space is well described by a stationary coordinate system. A motionless object in a stationary coordinate system will continue having the same coordinates forever. A group of objects laid out in a specific configuration will maintain that configuration. Two objects moving with identical velocities will remain at a fixed offset from each other.

These and similar laws are such a basic part of existence that when they no longer hold, intuition about the world becomes unreliable.

## Previous solutions

Unity's *global coordinate space* and *spatial anchors* each address different aspects of the problems caused by sensor inaccuracies and drift.

Unity's global coordinate space provides a stable frame of reference in which holographic objects remain fixed relative to one another.  While objects in this space will behave consistently relative to each other, consistency with the physical world is not guaranteed nor generally provided. Inconsistencies will develop especially when the user is moving around.

Unity's spatial anchors can maintain a hologram's position in the physical world when the user is mobile, but at the sacrifice of self-consistency within the virtual world. Different anchors are constantly moving relative to one another. They are also moving through the global coordinate space, making simple tasks like layout difficult, and physics simulation problematic.

## The source of the problem

Discussion here will center around HoloLens technology, but these concepts apply generally to inside-out markerless tracking techniques, especially as augmented by inertial systems.

The HoloLens is amazing at determining where it is relative to visible features in its surroundings. By extension, it's also amazing at positioning other virtual objects based on those same visible features. When the user is sitting or standing in a roughly constant position, the device is great at keeping virtual objects registered with visible physical reference points. A virtual cup placed on a physical table will mostly stay in the same spot on the surface of the table.  

That's when the HoloLens is confined to the same small volume, with a constant set of visible features in view for reference. But there are other interesting scenarios.

When the user gets up and moves about the room, or possibly even between rooms, the HoloLens must switch between old features that are leaving view and new features that are coming into view. Without getting into implementation details, it's clear to see that while in transit, tracking accuracy is going to be very much degraded.

Here's a simplistic scenario for context.

### Illustration

The user is at point A. Looking around, there are many good visible reference features, so the head tracking quality is excellent, and any holograms placed will stay put.

The user then walks 10 meters in physical space to point B. But tracking in transit has lower fidelity, so as a result, after the user reaches point B, the device registers that it has traveled only 9 meters. This is a large even amount for illustration, but it's consistent with the device specifications, which allow a +-10% distance error in such a case.

As the device looks around at point B, good visible features are recorded. Tracking and stability of holograms at point B is excellent, too.

While the user is at a particular point, things around that point look great. But there's an inconsistency. The 10 meters between points A and B in physical space are only 9 meters in virtual space. That's often referred to as "the scale problem", although "the distance problem" might be more accurate. We'll look into that problem soon.

Back to our scenario: for the next action, the user walks back to point A. This time the tracking errors make the 10-meter walk from B to A in physical space add up to 10.5 meters in virtual space. That means that the full walk from A to B to A adds up to a net distance of 1.5 meters, when it should be 0.0 meters. This is an obvious problem. A hologram placed at point A before the walk will now appear 1.5 meters away from point A.

This is where spatial anchors can help. After walking to B and back, the system recognizes that it's back at point A, yet the head's Unity coordinates have changed by 1.5 meters. But if the hologram at point A has a spatial anchor attached, the spatial anchor can think "I'm at point A, the head is at point A, but my coordinates differ from the head's coordinates by 1.5 meters. I'll just change my coordinates by 1.5 meters so that we're in agreement again." And a spatial anchor at point C, a meter to the left of the user, is going through the same process. In essence, the spatial anchor constantly redefines where point A is in Unity space so that the head's coordinates are always right. And each spatial anchor does this adjustment independently for its place in the physical world.  

## World Locking Tools for Unity

World Locking Tools keeps an internal supply of spatial anchors it spreads as the user moves around. It analyses the coordinates of the camera and those spatial anchors every frame. It detects when all of those spatial anchors are moving over 1.5 meters to match the coordinates of the head, and says "Hmm, instead of changing the coordinates of everything in the world to compensate for the head having different coordinates than the last time it was here, I'll just fix the head's coordinates instead."

That means that, rather than having to have a spatial anchor drag a hologram through Unity space so that it will remain fixed in physical space, the entire Unity world space is locked to physical space. If a hologram is motionless in Unity space, it will remain motionless relative to the physical world features around it. And just as importantly, it will remain fixed relative to the virtual features around it.

Obviously it's more complicated under the hood than that. For example, remember that a problem with the spatial anchors is that they move independently, so they don't always agree with each other. The underlying FrozenWorld engine arbitrates those disagreements to come up with the most perceptually correct camera correction, and does that every frame.

## The scale problem again

If the user walks from point A to point B and back to point A, the system has enough information to fix the drift that occurred in transit. It may not know where point B is (and generally doesn't know exactly where any point B is relative to point A), but it knows whether it is at point A or not. When it gets back to point A, it expects things to be pretty much as it left them. If they aren't, the system can make it so.

But what about at point B? It thought the 10-meter walk was only 9 meters. And it has no way of knowing whether that 9 meters is correct, and if it isn't, how much it's off. Spatial anchors don't help here. Spatial anchors have the same problem the head tracker does; each knows where it is in the physical world (relative to visible features), but one spatial anchor doesn't know anything about another spatial anchor. Specifically, spatial anchors don't know how far apart they are.

This can be inconvenient in many forms, but it becomes a blocking issue when objects, or systems of objects, are larger in size than a meter or so. Consider a model of a room, or a building, or a set of desks, or even a car. While a spatial anchor can keep one end of the model registered with a physical world feature, by the time the other end of the model is reached, significant error might have accumulated. The other end won't be lined up correctly. And the error will be different from device to device, and possibly even between runs on the same device.

And so far in this discussion, the minimum information required to fix the problem hasn't been introduced.

World Locking Tools addresses that problem with the [Space Pins](Concepts/Advanced/SpacePins.md) API, which allows the application to supply enough information relating to the physical world and holographic world to correct for the errors in distance traveled. This allows large holograms to appear aligned with the physical world all over.

## Next

These advanced mechanisms will be covered in later sections, but it's useful to first look in detail at World Locking Tools' baseline operation. Understanding what services the baseline operation does and doesn't supply will help you to determine the proper use of advanced concepts later, and whether those advanced techniques are even required for a specific application.

## See also

* [FAQ](IntroFAQ.md)
* [The basic system](Concepts/BasicConcepts.md)
* [Advanced topics](Concepts/AdvancedConcepts.md)
