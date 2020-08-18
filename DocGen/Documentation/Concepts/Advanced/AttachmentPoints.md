# Attachment points

First and foremost, World Locking Tools provides a stable world-locked coordinate system, the world locked space. This space remains as fixed as possible relative to the physical world. And objects within world locked space enjoy capabilities requiring such a stable frame of reference, such as maintaining relative placement to other virtual objects, simulation of natural physics laws, kinematics, and other animation techniques.

In fact, depending on the needs of the application, world locked space may be sufficient for some or all scene content.

But while world locked space will remain optimally aligned with physical space, there are situations to be described later in which it is not possible for multiple points in world locked space to remain both fixed in their common coordinate space and fixed relative to reference points in the physical world.

For a trivial but illuminating example, suppose that the sensor maps one anchor to the position (3,0,0) and another to the position (-3,0,0). Later, as sensor refinements are processed, it is established that the two coordinates should have been (3,0,0) and (-2,0,0). There is clearly no rotation and offset that can be applied to the camera which will transform a 6 meter distance between the two anchors into a 5 meter offset.

Using Unity's WorldAnchor system, the two anchors would just silently move into their newly scanned positions.

But World Locking Tools guarantees that in world locked space, non-moving objects will "mostly" never move. And in fact, any motion is up to the owning application.

Another common "abnormal" condition is **loss of tracking**. When tracking is lost in one environment (e.g. room) and regained in another environment, then at first there is no information linking the two spaces. The coordinates in one space are meaningless relative to coordinates in the other space. The attachment point paradigm allows the application to gracefully handle the initial phase when spatial information about the old space is unknown (e.g. by hiding the objects in that old space), as well as recovering when the spatial relationship between the two spaces does become known.

More discussion can be found of these special conditions and the [refit operations](RefitOperations.md) which WLT performs to handle them. The discussion here is focused on the contract between WLT and the application on smoothly resolving such conditions.

Attachment points are the codification of that contract between World Locking Tools and the application. An application creates and positions attachment points using World Locking Tools APIs. When a correction in the position of an attachment point is determined by a [refit operation](RefitOperations.md), the application is notified via callback of the new position in world locked space that will keep the attachment point at its old position in physical space.

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
2) Its coordinates in world locked space change due to a refit operation, which may be either a [fragment merge](RefitOperations.md#fragment-merge) or a [refreeze](RefitOperations.md#refreeze-operations).

These notifications are broadcast through delegates which the application hands to the WorldLockingManager on creation of the attachment point.

How to best handle these notifications is left to the application, as each will have its own considerations. Sample handlers, which are used internally and may be either used as is or used as a starting point for custom implementations, are provided.

## Sample implementations

For an attachment point that is to remain fixed in the physical world, and which should hide its contents when its tracking is not valid, [AdjusterFixed](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed) implements the [AdjustStateDelegate](xref:Microsoft.MixedReality.WorldLocking.Core.AdjustStateDelegate) with its HandleAdjustState member,and the [AdjustLocationDelegate](xref:Microsoft.MixedReality.WorldLocking.Core.AdjustLocationDelegate) with its HandleAdjustLocation member. A similar component for moving objects is in [AdjusterMoving](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterMoving).

It is worth noting that supplying either or both these delegates is optional, and in fact reactions to state and location changes may be implemented based on polling rather than events. But unless their use is impossible due to specifics of the application, the event based system using delegates forms a much more efficient implementation.

The recommendation is that you start with the AdjusterFixed component (or very similar AdjusterMoving), and modify the handlers [HandleAdjustLocation](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed.HandleAdjustLocation*) and [HandleAdjustState](xref:Microsoft.MixedReality.WorldLocking.Tools.AdjusterFixed.HandleAdjustState*) to suit your applications needs.

## See also

* [Fragments](Fragments.md)
* [Refit operations](RefitOperations.md)