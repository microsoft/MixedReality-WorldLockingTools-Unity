
# World Locking Tools (WLT) combined with Azure Spatial Anchors (ASA)

Azure Spatial Anchors (ASA), provide a powerful cross-platform method for persistence across sessions and sharing across devices of spatial coordinates of physical features.

World Locking Tools (WLT), on the other hand, leverage a device's local tracking system to stabilize Unity's global coordinate space relative to the physical environment.

Combining the two gives stationary coordinate space(s) which are maintained across sessions, and sharable between different devices.

## Supported configurations

WLT supports:

* Unity 2018.4, Unity 2019.4, Unity 2020.3
* HoloLens, HoloLens2, Android, and iOS
* Unity Legacy XR, XR SDK, or OpenXR
* Azure Spatial Anchors v2.9.0-v2.10.2

Use of ASA v2.9+ imposes these additional restrictions:

* Unity 2020.3 or later
* XR SDK or OpenXR
* At the time of this writing, ASA only targets ARM64 (not ARM32) on HoloLens2. Check the latest [ASA documentation](https://docs.microsoft.com/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal).

## Setup for using ASA with WLT

Before deploying and running the samples leveraging ASA with WLT, some external software and configuration is necessary. In particular, enabling coarse relocation requires some non-intuitive setup. These additional steps are detailed in the [WLT+ASA Sample documentation](Samples/WLT_ASA_Sample.md).

Additionally, [notes on the software bridging the two systems](Samples/WLT_ASA_Software.md), ASA and WLT, may prove helpful in understanding what's going on.

## See also

* [WLT+ASA software overview](Samples/WLT_ASA_Software.md)
* [WLT_ASA Samples Walkthrough](Samples/WLT_ASA_Sample.md)
* [Azure Spatial Anchors Quick Start](https://docs.microsoft.com/azure/spatial-anchors/unity-overview)
* [World Locking Tools for Unity](https://microsoft.github.io/MixedReality-WorldLockingTools-Unity/README.html)
