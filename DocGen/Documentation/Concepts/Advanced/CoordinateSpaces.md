
# Coordinate Spaces in World Locking Tools for Unity

The World Locking Tools for Unity (WLT) ultimately provide a stable world-locked coordinate system, with configurable mapping to the physical world.

This transformation from the shifting, non-persistent, and arbitrary native Unity global coordinate space, to the world-locked space happens in steps. Each intermediate coordinate space has a name. 

To some degree, all names are somewhat arbitrary. These are the names of the intermediate spaces as used in the WLT documentation and code.

`Spongy Space` - The Unity global coordinate space you would get without WLT. A stationary object in Spongy Space (one whose coordinates are unchanging) will drift relative to the physical world.

`Play Space` - A position/rotation transformation of Spongy Space, it can be used to implement features like teleport.

`Locked Space` - The world-locked space as computed by the FrozenWorld Engine and implemented by WLT. A stationary object in Locked Space will remain fixed relative to features in the physical world. However, the numeric values of its coordinates are arbitrary.

`Pinned Space` - A transformation of Locked Space to give coordinates a desired mapping to the physical world. An object with position (X,Y,Z) will appear at a known predetermined position relative to physical world features.

`Frozen Space` - A position/rotation transformation of Pinned Space, allowing the application to apply an arbitrary transform to the camera hierarchy.

As a convenience, the WorldLockingManager supplies transformations between all of these spaces. For example, the most useful of these is [FrozenFromSpongy](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager.FrozenFromSpongy), a [Pose](https://docs.unity3d.com/ScriptReference/Pose.html) which transforms from Spongy Space to Frozen Space. This is useful when converting coordinates returned by native APIs, which have no notion of WLT and so operate in Spongy Space, into Frozen Space. 

Note that when using [MRTK](https://microsoft.github.io/MixedRealityToolkit-Unity/README.html), no such translations are needed. Its coordinate space is already Frozen Space.

Other conversions between the various spaces are available on the [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager), but are not generally needed.

## See also

* [A primer on Unity coordinate spaces (without WLT) as relates to AR/MR](https://docs.microsoft.com/en-us/windows/mixed-reality/coordinate-systems).