# Attachment points

First and foremost, World Locking Tools provides a stable world-locked coordinate system, the frozen space. This space remains as fixed as possible relative to the physical world. And objects within frozen space enjoy capabilities requiring such a stable frame of reference, such as maintaining relative placement to other virtual objects, simulation of natural physics laws, kinematics, and other animation techniques.

In fact, depending on the needs of the application, frozen space may be sufficient for some or all scene content.

But while frozen space will remain optimally aligned with physical space, there are situations to be described later in which it is not possible for multiple points in frozen space to remain both fixed in their common coordinate space and fixed relative to reference points in the physical world.

For a trivial but illuminating example, suppose that the sensor maps one anchor to the position (3,0,0) and another to the position (-3,0,0). Later, as sensor refinements are processed, it is established that the two coordinates should have been (3,0,0) and (-2,0,0). There is clearly no rotation and offset that can be applied to the camera which will transform a 6 meter distance between the two anchors into a 5 meter offset.

Using Unity's WorldAnchor system, the two anchors would just silently move into their newly scanned positions.

But World Locking Tools guarantees that in frozen space, non-moving objects will "mostly" never move. And in fact, any motion is up to the owning application.

Attachment points are the codification of that contract between World Locking Tools and the application. An application creates and positions attachment points using World Locking Tools APIs. When a correction in the position of an attachment point is determined by a refit operation, the application is notified via callback of the new position in frozen space that will keep the attachment point at its old position in physical space.

Some scenarios in which World Locking Tools attachment points might be the solution:

* It is more important to remain fixed relative to features in the physical world than relative to other virtual objects. 
* Objects are placed in the world at runtime rather than in Unity at design time, and it may be important to reconcile relative positions separated by disruptions in tracking (see discussion of [fragments](Fragments.md)).
* It is important to manage an object's visibility based on the validity of its physical space positioning.

## Using attachment points

Use of attachment points is fairly straightforward.

### Client responsibilities

For each attachment point required, the client must:

1) Request attachment points from the system. See [CreateAttachmentPoint](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager.CreateAttachmentPoint*)
2) Dispose of attachment points that are no longer needed. See [ReleaseAttachmentPoint](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager.ReleaseAttachmentPoint*)
3) Apprise the system of the attachment point's initial position and movement. See [CreateAttachmentPoint](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager.CreateAttachmentPoint*), [MoveAttachmentPoint](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager.MoveAttachmentPoint*), and [TeleportAttachmentPoint](xref:Microsoft.MixedReality.WorldLocking.Core.IAttachmentPointManager.TeleportAttachmentPoint*)
4) Handle refit operation events. See below.

### World Locking Tools responsibilities

World Locking Tools will notify the application, for each affected attachment point, when either of the following occurs:

1) The validity of the attachment point's physical world tracking changes. 
2) Its coordinates in frozen space change due to a refit operation, which may be either a [fragment merge](RefitOperations.md#fragment-merge) or a [refreeze](RefitOperations.md#refreeze-operations).

These notifications are broadcast through delegates which the application hands to the WorldLockingManager on creation of the attachment point.

How to best handle these notifications is left to the application, as each will have its own considerations. Sample handlers, which are used internally and may be either used as is or used as a starting point for custom implementations, are provided.

For an attachment point that is to remain fixed in the physical world, and which should hide its contents when its tracking is not valid, [AdjusterFixed](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed) implements the [AdjustStateDelegate](xref:Microsoft.MixedReality.WorldLocking.Core.AdjustStateDelegate) with its HandleAdjustState member,and the [AdjustLocationDelegate](xref:Microsoft.MixedReality.WorldLocking.Core.AdjustLocationDelegate) with its HandleAdjustLocation member. A similar component for moving objects is in [AdjusterMoving](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterMoving).

It is worth noting that supplying either or both these delegates is optional, and in fact reactions to state and location changes may be implemented based on polling rather than events. But unless their use is impossible due to specifics of the application, the event based system using delegates forms a much more efficient implementation.

## See also

* [Fragments](Fragments.md)
* [Refit operations](RefitOperations.md)