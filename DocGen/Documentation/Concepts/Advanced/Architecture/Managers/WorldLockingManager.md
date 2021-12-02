# WorldLockingManager

The World Locking Tools for Unity is managed by the [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager). There is only one WorldLockingManager in an application.

The manager may be configured by setting of properties in the Unity editor's inspector, or at runtime. See [WorldLockingContext](~/DocGen/Documentation/HowTos/WorldLockingContext.md).

In addition to managing user options, the WorldLockingManager provides access to three interfaces:

* *IAnchorManager* - Build and maintain the network of spatial anchors used as input to the Frozen World engine, feeding them into the engine every frame.
* *IFragmentManager* - Maintain grouping of attachment points into fragments, effecting refit operations.
* *IAttachmentPointManager* - Creating, releasing, and moving attachment points.
* *IAlignmentManager* - Specifying alignment of World Locked Space with physical space.

These are described in detail in subsequent sections.

## See also

* [IAnchorManager](IAnchorManager.md)
* [IFragmentManager](IFragmentManager.md)
* [IAttachmentPointManager](IAttachmentPointManager.md)
* [IAlignmentManager](IAlignmentManager.md)

And in API reference:

* [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager)
* [IAnchorManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAnchorManager)
* [IFragmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IFragmentManager)
* [IAttachmentPointManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager)
* [IAlignmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAlignmentManager)