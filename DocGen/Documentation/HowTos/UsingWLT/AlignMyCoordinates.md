---
title: Align my coordinates
description: Explanation of why an application might want to align its coordinate space to the physical world, and how WLT accomplishes that.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Align my coordinates

A brief summary of World Locking Tools capabilities presented so far is in order.

1. With [drag and drop ease](JustWorldLock.md), WLT will provide a coordinate space which is stationary relative to the physical world.
2. That space can be made [optionally persistent](PersistenceTricks.md), so that physical features around a point in space in this session are the same as the physical features around the point in previous sessions.
3. The application can opt in to callbacks allowing it to [adjust to larger scale tracking corrections](LossOfTracking.md). The [Adjuster](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed) scripts can be used as is or as examples for this.

Having gotten all of those benefits, your application might have a further requirement, to align the coordinate system with physical space at a small number of discrete points.

The usual reason for this is that there is a large virtual feature (or system of objects) in your application which needs to match up at physical features. Because of distortions in tracker space caused by tracker error, this is actually impossible. But an approximation can be made by matching virtual to physical points on a perceptually driven priority. In essence, the pin closest to you matches corresponding point in the physical world best.

In order to do that, the system needs more information from your application. The SpacePin component is the managing object for AlignmentAnchors. The correspondences are made in the form of pairs of virtual and physical poses.

[Motivation for SpacePins](../../Concepts/Advanced/SpacePins.md) and their usage are detailed elsewhere in this documentation. There are also samples of their usage both [in this repo](../Samples/SpacePin.md) and in the sibling [Samples repo](https://microsoft.github.io/MixedReality-WorldLockingTools-Samples/README.html).

## See also

* [Before You Start](BeforeGettingStarted.md)
* [Most Basic Setup](JustWorldLock.md)
* [Loss of Tracking](LossOfTracking.md)
* [Across Sessions](PersistenceTricks.md)
