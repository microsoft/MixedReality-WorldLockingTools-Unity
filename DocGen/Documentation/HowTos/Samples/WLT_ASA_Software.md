
# WLT+ASA: Overview of supporting software

## IBinder - binding SpacePins to Azure Spatial Anchors

The [IBinding](xref:Microsoft.MixedReality.WorldLocking.ASA.IBinder) interface is at the center. It is implemented here by the [SpacePinBinder class](xref:Microsoft.MixedReality.WorldLocking.ASA.SpacePinBinder). It is a Unity Monobehaviour, and may be configured either from Unity's Inspector or from script.

Each IBinder is [named](xref:Microsoft.MixedReality.WorldLocking.ASA.IBinder.Name), so a single [IBindingOracle](xref:Microsoft.MixedReality.WorldLocking.ASA.IBindingOracle) can manage bindings for multiple IBindings.

## IPublisher - reading and writing spatial anchors to the cloud

The [IPublisher](xref:Microsoft.MixedReality.WorldLocking.ASA.IPublisher) interface handles publishing spatial anchors to the cloud, and then retrieving them in later sessions or on other devices. It is implemented here with the [PublisherASA class](xref:Microsoft.MixedReality.WorldLocking.ASA.PublisherASA). Pose data in the current physical space is captured and retrieved using Azure Spatial Anchors.

When a spatial anchor is published, a cloud anchor id is obtained. This id may be used in later sessions or on other devices to retrieve the cloud anchor's pose in the current coordinate system, along with any properties stored with it. The system always adds a property identifying the cloud anchor's associated SpacePin.

It should be noted that the IPublisher, and the PublisherASA, don't know anything about SpacePins. IPublisher doesn't know or care what will be done with the cloud anchor data. It simply provides a simplified awaitable interface for publishing and retrieving cloud anchors.

### Read versus Find

If a cloud anchor's id is known, the cloud anchor may be retrieved by its id. This is the most robust way to retrieve a cloud anchor. This is [Read](xref:Microsoft.MixedReality.WorldLocking.ASA.IPublisher.Read*).

However, there are interesting scenarios in which the ids for the cloud anchors within an area aren't known by a device, but if they cloud anchors could be retrieved, their spatial data and properties would combine to provide enough information to make them useful.

[Find](xref:Microsoft.MixedReality.WorldLocking.ASA.IPublisher.Find*) searches the area around a device for cloud anchors, and returns any that it was able to identify. This process is known as [coarse relocation](https://docs.microsoft.com/azure/spatial-anchors/how-tos/set-up-coarse-reloc-unity).

## IBindingOracle - sharing cloud anchor ids

The [IBindingOracle interface](xref:Microsoft.MixedReality.WorldLocking.ASA.IBindingOracle) provides a means of persisting and sharing bindings between SpacePins and specific cloud anchors. Specifically, it persists space-pin-id/cloud-anchor-id pairs, along with the name of the IBinder.

The oracle's interface is extremely simple. Given an IBinder, it can either [Put](xref:Microsoft.MixedReality.WorldLocking.ASA.IBindingOracle.Put*) the IBinder's bindings, or it can [Get](xref:Microsoft.MixedReality.WorldLocking.ASA.IBindingOracle.Get*) them. Put stores them, and Get retrieves them. The mechanism of storage and retrieval is left to the implementation of the concrete class implementing the IBindingOracle interface.

This sample implements possibly the simplest possible IBindingOracle, in the form of the [SpacePinBinderFile class](xref:Microsoft.MixedReality.WorldLocking.ASA.SpacePinBinder). On Put, it writes the IBinder's bindings to a text file. On Get, it reads them from the text file (if available) and feeds them into the IBinder.

## ILocalPeg - blob marking a position in physical space

The [ILocalPeg interface](xref:Microsoft.MixedReality.WorldLocking.ASA.ILocalPeg) is an abstraction of a device local anchor. In a more perfect world, the required ILocalPegs would be internally managed by the IPublisher. However, device local anchors work much better when created while the device is in the vicinity of the anchor's pose. The IPublisher only knows where the device local anchors should be placed when they are needed, not at the optimal time of creating them.

The [SpacePinASA](xref:Microsoft.MixedReality.WorldLocking.ASA.SpacePinASA) does know when the best time to create its local anchor is. When the manipulation of the SpacePin ends and its pose set, the SpacePinASA requests the IPublisher to [create an opaque local peg](xref:Microsoft.MixedReality.WorldLocking.ASA.IPublisher.CreateLocalPeg*) at the desired pose. The SpacePinBinder then pulls the ILocalPeg off the SpacePinASA, and passes it to the IPublisher to be used in [creating a cloud spatial anchor](xref:Microsoft.MixedReality.WorldLocking.ASA.IPublisher.Create*).

## See also

* [WLT+ASA Samples Setup and Walkthrough](WLT_ASA_Sample.md)
* [Space Pins Concepts](~/DocGen/Documentation/Concepts/Advanced/SpacePins.md)
* [Space Pins Sample](SpacePin.md)
