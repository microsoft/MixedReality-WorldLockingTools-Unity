---
title: Cross Platform using Unity's XR SDK Plugin system
description: Building HoloLens and mobile applications leveraging World Locking Tools using Unity's XR SDK Plugin system.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Cross Platform using Unity's XR SDK Plugin system

Initial development of the World Locking Tools targeted the HoloLens family of devices via Unity's XR.WSA (VR/AR Windows Store App) APIs. This is part of what has become commonly known as Unity's Legacy XR interface, or Unity's built-in VR support.

Unity has since introduced its [XR Plugin architecture](https://docs.unity3d.com/Manual/XRPluginArchitecture.html), whose goal is to provide cross platform abstractions giving developers access to common features across available VR and AR devices.

During this transition phase, WLT supports both the Legacy XR interface for HoloLens, and the AR Subsystems for cross-platform. It should be noted that the Legacy XR interface is deprecated since Unity 2019, and will no longer be supported as of Unity 2020.

WLT currently supports **AR Subsystems versioned 2.X.Y for Unity 2019.4 (LTS)**. Further version support will be rolled out in subsequent releases.

## Switching WLT to target XR SDK

Retargeting WLT for XR SDK is exceedingly simple. First, configure your project to use the [XR Plugin system](https://docs.unity3d.com/Manual/configuring-project-for-xr.html), or AR Foundation which sits on top of the XR Plugin system. Once the necessary resources have been installed, change the Anchor Subsystem type in the World Locking Context in you

1. Go to the WorldLockingManager GameObject in your initial (or global) scene.
2. In the inspector, find the [WorldLockingContext](WorldLockingContext.md).
3. Open Anchor Management settings.
4. Make sure the "Use Defaults" checkbox is unchecked.
5. Change the Anchor Subsystem type to XRSDK.
6. NOTE: Do not use the ARF Anchor Subsystem type. If using AR Foundation for your app, still select XRSDK subsystem.

![The anchor management section of the context settings](~/DocGen/Images/Screens/Context/WLTContextAnchor.JPG)

## Using MRTK with WLT on XR SDK

[MRTK](https://microsoft.github.io/MixedRealityToolkit-Unity/README.html), in addition to the incredible value it provides for abstracting user interactions in VR and AR, simplifies targeting devices via the XR SDK greatly. The following are notes that might prove helpful when setting up MRTK to target specific devices.

These all assume that the WLT Anchor Management Anchor Subsystem has been set appropriately as described in the previous section.

Be sure to run the `Mixed Reality Toolkit => Utilities => UnityAR => Update Scripting Defines` when changing plugin targets.

### Setup for Windows XR Plugin (HoloLens)

See full instructions at [Getting started with MRTK and XR SDK](https://microsoft.github.io/MixedRealityToolkit-Unity/version/releases/2.5.1/Documentation/GettingStartedWithMRTKAndXRSDK.html?q=2020).

If working in the WLT project, I suggest using the WLT provided “XRSDKMixedRealityToolkitConfigurationProfile” to start.

WLT using Windows XR Plugin has been tested on Unity 2019.4, using the following versions:

> * XR Plugin Management: v3.2.16 (expected to work with v3.X.Y, X >= 2)
> * Windows XR Plugin: v2.4.1 (prior to v2.4.1 anchors were broken. Expected to work v2.4.Y, Y>=1, v2.X.Y, X>=1). 

### Setup for ARCore XR Plugin (Android)

To get an Android XR Plugin driving an MRTK AR application, follow the instructions at [How to configure MRTK for iOS and Android](https://microsoft.github.io/MixedRealityToolkit-Unity/version/releases/2.5.1/Documentation/CrossPlatform/UsingARFoundation.html?q=2020)

> NOTE: You need ALL of:
>
> * XR Plugin Management (tested v3.2.16, expected v3.X.Y, X>=2).
> * AR Subsystems (tested v2.1.3, expected v2.X.Y, X>1).
> * AR Core XR Plugin (tested v2.1.12, expected v2.X.Y, X>1).
> * AR Foundation (tested v2.1.10, expected v2.X.Y, X>1).

I suggest using the WLT provided “AR MixedRealityToolkitConfigurationProfile” to start.

> NOTE: If you hit a build error about **manifest-merger failed**, then follow the appropriate instructions here:
https://developers.google.com/ar/develop/unity/android-11-build

### Setup for other XR Plugins (ARKit, Oculus, etc.)

Setup for other platforms should be analogous to setup for ARCore, but have not been tested. If you have access to such devices and a chance to try them, any [feedback](~/DocGen/Documentation/Howtos/Contributing.md) would help the community and be greatly appreciated.
