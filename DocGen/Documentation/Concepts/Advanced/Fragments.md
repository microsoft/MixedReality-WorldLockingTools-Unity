# Fragments

As mentioned, in World Locking Tools terminology, a *fragment* is a collection of things which exist in known relation with each other in the same coordinate space. However, there is generally no meaningful spatial relationship between different fragments.

A simple example might help clarify. 

Imagine two well lit rooms, connected by a long dark hallway. The head-tracked session begins in the first room. The room being well lit and with appropriate furnishings, the user quickly and easily scans and maps it. Objects in the room, as well as any anchors created, are all in known positions relative to the head and relative to each other.

Since the second room hasn't even been visited yet, there is still no knowledge about its contents.

Now the user proceeds into the dark hallway. There, tracking is lost immediately because of the poor lighting. The user passes through the hallway to the second room.

In the second room, tracking is again restored and the user quickly scans the room, adding some anchors for good measure.

At this time, both rooms have been scanned, and the contents of each room is known relative to the other contents of the same room, but there is no knowledge about one room relative to the other. The hallway could have been of any length, and it may have curved.

These two rooms, then, are forming isolated islands of spatial relationship. The group of inter-related objects in each room are herein called "fragments". And in our hypothetical situation, our session now contains two fragments, one for each room. Because no tracking data was acquired in the hallway, there is no corresponding hallway fragment.

All of the objects in both rooms have coordinates, but the two coordinate systems are unrelated. When the camera is in the second room, the head is placed in the same coordinate system as all the other objects in the second room. This allows those second room objects to be rendered appropriately relative to the user's perspective.

However, the objects in the first room are in an unrelated coordinate system. Depending on the length of the unmapped hallway, they might be meters or tens of meters away, or off to the side if the hallway bends. Therefore, without further info connecting the two spaces, the system doesn't have enough information to meaningfully place the first room's objects in the users view. But the system does know it hasn't enough info to render those objects correctly, and through the [attachment point](AttachmentPoints.md) mechanism can inform the application of that condition.

## See also

* [Attachment points](AttachmentPoints.md)
* [Refit operations](RefitOperations.md)