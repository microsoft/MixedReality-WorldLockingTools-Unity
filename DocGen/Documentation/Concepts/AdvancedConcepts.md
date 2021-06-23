# Advanced capabilities of World Locking Tools

Some definition of terms might be helpful before diving deeper into World Locking Tools' more advanced usage. 

These concepts are discussed more thoroughly in subsequent sections of this documentation, including actions necessary for a developer wanting to manually leverage these capabilities.

* **Persistence** is the saving of World Locking Tools state from earlier sessions to enhance subsequent sessions, by leveraging the state collected and computed in the earlier sessions, rather than requiring it to be collected or computed again. 

* **Space Pins** are a discrete and sparse set of points at which the World Locked Space may be mapped to modeling coordinates. In addition to allowing objects modeled in Unity's coordinate space to be aligned with the real world, they provide a solution to the tracker's "scale problem". 

* **Attachment points** are special markers in the world which remain optimally registered to the physical world through infrequent adjustment events.

* A **fragment** is a collection of things which exist in known relation with each other in the same coordinate space. In contrast, there is generally no meaningful spatial relationship between different fragments. These are elsewhere referred to as "tracker islands".

* **Refit operations** are infrequent adjustment events, at times when additional sensor data enables an improved registration of virtual objects with the physical world, but only at the cost of moving them within world locked space.

There are two types of refit operations.

* **Merge operations** are the repositioning of the entire contents of a fragment uniformly into a common coordinate space with another fragment.
* **Refreeze operations** are the repositioning of individual objects within a fragment to account for movement of the supporting anchors.

## See also

* [Coordinate spaces](Advanced/CoordinateSpaces.md)
* [SessionOrigin](Advanced/SessionOrigin.md)
* [Persistence](Advanced/Persistence.md)
* [Space Pins](Advanced/SpacePins.md)
* [Attachment points](Advanced/AttachmentPoints.md)
* [Fragments](Advanced/Fragments.md)
* [Refit operations](Advanced/RefitOperations.md)
