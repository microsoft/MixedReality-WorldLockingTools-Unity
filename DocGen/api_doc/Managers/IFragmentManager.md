
# IFragmentManager

The fragment manager maintains the mapping of which attachment points are associated with which fragments.

While the concept of fragments is important in understanding Frozen World, fragments themselves are generally not interesting outside of the Frozen World library. 

A client application might want to know whether the fragment an object belongs to is currently being tracked, but that information is supplied directly to the attachment point through its AdjustStateDelegate.

Likewise, a fragment merge operation will affect the position of an attachment point, but the attachment point will be notified directly of the location modification irrespective of the containing fragment.

The fragment manager handles the book-keeping of which fragment an attachment point is created into, and which fragment it might move into because of refit operations. It also implements the refit operation notifications.

However, all these operations are behind the scenes. The client application will generally not interact directly with the IFragmentManager.

The current implementation of IFragmentManager is in Assets/WorldLocking.Core/Scripts/FragmentManager.cs, which also implements the IAttachmentPointManager interface.

## See also

* [WorldLockingManager](WorldLockingManager.md)
* [IAnchorManager](IAnchorManager.md)
* [IAttachmentPointManager](IAttachmentPointManager.md)

And in API reference:

* [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager)
* [IAnchorManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAnchorManager)
* [IFragmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IFragmentManager)
* [IAttachmentPointManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager)