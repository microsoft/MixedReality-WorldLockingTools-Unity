# IAttachmentPointManager

The IAttachmentPointManager is the most common World Locking Tools interface for a client application to access.

But the attachment point interface is extremely small. It gives opportunity for the client application to perform the four operations available on attachment points.

* Create
* Release
* Move 
* Teleport

### CreateAttachmentPoint

The interesting thing to note here is the delegates passed in as arguments to the create function.

Either or both of these arguments may be null, in which case the created attachment point will not receive any notifications corresponding to that delegate.

These delegates may not be changed after creation. This should not be a burden, as the delegate itself may fork behavior based on current state. If even that is not possible, then the attachment point must be released and a new attachment point with the desired new delegates created.

### ReleaseAttachmentPoint

When the attachment point is no longer needed, the client application should notify the system via the release API.

### MoveAttachmentPoint

Unlike most spatial anchors, attachment points may move freely through the world, automatically binding to the most relevant anchors at their new positions.

As an attachment point moves, its owner should notify the system of its new position. This movement might be from physics simulation, or any other animation technique.

### TeleportAttachmentPoint

Confusingly, teleporting is more closely related to creation than movement. Rather than thinking of teleport as moving to a new location, it is helpful to think of it as ceasing to exist, then beginning existence again in a (possibly) new location.

The rule of thumb is, if the object moved continuously from old location to new location, use MoveAttachmentPoint. If it popped into existence in the new location, use TeleportAttachmentPoint.

## Contexts for creation and teleporting

Creation and the conceptually similar teleport take an optional (may be null) parameter of a creation context. The context is an already existing attachment point that gives the system several hints about where in the anchor graph (which may not be fully connected in the case of multiple fragments) to find the best anchor to base this attachment point on.

The current implementation of IAttachmentPointManager is in Assets/WorldLocking.Core/Scripts/FragmentManager.cs, which also implements the IFragmentManager interface.

## Move and teleport via manager or attachment point APIs

It may be noticed that, in addition to the move and teleport interfaces described here, there are corresponding methods on the IAttachmentPoint interface. These are equivalent, and whichever is more convenient to the caller may be used.

## See also

* [WorldLockingManager](WorldLockingManager.md)
* [IFragmentManager](IFragmentManager.md)
* [IAnchorManager](IAnchorManager.md)

And in API reference:

* [WorldLockingManager](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager)
* [IAnchorManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAnchorManager)
* [IFragmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IFragmentManager)
* [IAttachmentPointManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager)

