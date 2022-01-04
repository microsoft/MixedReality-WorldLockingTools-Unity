---
title: Before getting started
description: A few notes on WLT to consider before diving in.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.prod: mixed-reality
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Before getting started

The World Locking Tools for Unity offers a very powerful API for fine control over the services it offers. On first look it can be quite overwhelming.

A reasonable question might be: How much code should I expect to write to use WLT?

The answer, which may surprise you, is "None."

WLT has been carefully structured to handle the vast majority of usage cases with a simple drag and drop interface. Some slight modifications to your scene, [as described here](JustWorldLock.md), and your application is world-locked and anchor free.

![Screenshot of Unity with most basic WLT setup](~/DocGen/Images/Screens/BasicSetup.jpg)

## Customizing behavior through code

There are a small number of cases where you might want to do additional coding against the WLT APIs.

First, you might want to customize your customers' experiences, especially in extraordinary circumstances, such as loss of tracking. Some such bespoke behavior is described in the [Handling exceptional conditions](LossOfTracking.md) section.

Secondly, you might want to do your WLT setup at runtime. Any configuration of WLT which can be done in the Unity Inspector can be done by script calls. Likewise, any WLT object or component that can be added to the scene and deployed at build time, can instead be added to the scene at runtime from script.

The final case is where additional input is required from your application in order to perform a service for you. For example, in order to [align your coordinate system with physical world features](AlignMyCoordinates.md) in a desired way, you must give an indication of how you want the coordinate system aligned. This additional input comes in the form of pairs of virtual and tracking space poses.

## Start off easy

WLT strives to maintain this pattern throughout. To get the most commonly desired functionality requires no coding and minimal setup. Default behavior is implemented as available components. Customizing the default behavior requires only enough code to override the provided behavior with the behavior you want. Additional features require only enough interaction with your application to indicate your intentions.

There is a lot of API surface in WLT. Those are growth opportunities for farther down the road, so that WLT never boxes you in. But start off simple. You may find WLT does everything you require from it without a line of code.

First, read and understand this conceptual documentation. At appropriate places you will find links to the API documentation, as a reference for exact calling syntax. Direct links to the overall API documentation are included below.

## See also

* [Most Basic Setup](JustWorldLock.md)
* [Loss of Tracking](LossOfTracking.md)
* [Across Sessions](PersistenceTricks.md)
* [Pinning It Down](AlignMyCoordinates.md)

## API Documentation

* [WLT Core](xref:Microsoft.MixedReality.WorldLocking.Core)
* [WLT Tools](xref:Microsoft.MixedReality.WorldLocking.Tools)
* [WLT with ASA](xref:Microsoft.MixedReality.WorldLocking.ASA)
