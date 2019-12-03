# IAnchorManager

The Frozen World engine relies on a network of spatial anchors surrounding the user, from which to calculate the ideal frozen space.

The IAnchorManager maintains that network, and supplies it to the engine for processing each frame.

The current implementation of IAnchorManager is in Assets/WorldLocking.Core/Scripts/AnchorManager.cs. AnchorManager is built on Unity's WorldAnchor spatial anchors. The Frozen World engine itself is agnostic to the type of anchors used.

As the user moves around the environment, AnchorManager grows the graph of anchors according to the following simple but effective algorithm:

* If the nearest existing anchor is more than X meters from the user then:
  * Add a new anchor.
  * Add edges from the new anchor to all existing anchors less than Y meters from the new anchor.

The values of 'X' and 'Y' above are constants in the AnchorManager, as MinAnchorDistance and MaxAnchorDistance respectively. These could be converted to properties, giving more flexibility at the cost of complexity in the WorldLockingContext API surface. However, the current values of 1 meter and 1.2 meters, respectively, have been satisfactory to date.

The above algorithm grows the graph of anchors in the space traversed by the user during the initial exploration, eventually settling into a static network.

The anchor manager may also persist the anchor graph across multiple sessions. 

AnchorManager has options to automatically save its anchor graph during the session, load the previously saved graph at startup, or save and load on demand.

## See also

* [WorldLockingManager](WorldLockingManager.md)
* [IFragmentManager](IFragmentManager.md)
* [IAttachmentPointManager](IAttachmentPointManager.md)

And in API reference:

* [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager)
* [IAnchorManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAnchorManager)
* [IFragmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IFragmentManager)
* [IAttachmentPointManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager)