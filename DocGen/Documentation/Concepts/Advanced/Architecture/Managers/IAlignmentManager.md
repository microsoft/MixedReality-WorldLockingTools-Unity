# IAlignmentManager

The IAlignmentManager interface abstracts the service of translating and rotating the world-locked space to align with the physical world at a discrete set of Space Pins.

The alignment concepts are described in detail in an article dedicated to [Space Pins](~/DocGen/Documentation/Concepts/Advanced/SpacePins.md).

Application interactions with the IAlignmentManager are not generally required. Rather, the helper component class [Space Pin](xref:Microsoft.MixedReality.WorldLocking.Core.SpacePin), and components derived from it, is provided to handle the minimal bookkeeping involved.

## Persistence considerations

The IAlignmentManager provides a callback notification on post load. Any object registering for load notifications should un-register at the latest before its destruction. For Unity objects, that can be done in OnDestroy.

In addition to explicit Save() and Load() member functions, the IAlignmentManager interface provides a persistence hook in the form of [RestoreAlignmentAnchor](xref:Microsoft.MixedReality.WorldLocking.Core.IAlignmentManager.RestoreAlignmentAnchor*).

RestoreAlignmentAnchor searches its database for enough information to recreate the named Alignment Anchor, which has presumably been created and saved in an earlier session. If successful, an AnchorId valid for this session is returned, and the caller can claim ownership for the session. If for any reason the Alignment Anchor cannot be restored, an invalid AnchorId will be returned, and the caller should assume that the named anchor has not been created (nor saved) yet.

The post load callback is useful for suggesting an appropriate time to check with the database whether a named Alignment Anchor is now available.

## See also

* [IAlignmentManager](xref:Microsoft.MixedReality.WorldLocking.Core.IAlignmentManager)
* [SpacePin](xref:Microsoft.MixedReality.WorldLocking.Core.SpacePin)
* [SpacePinManipulation](xref:Microsoft.MixedReality.WorldLocking.Examples.SpacePinManipulation)
* [SpacePinOrientable](xref:Microsoft.MixedReality.WorldLocking.Core.SpacePinOrientable)
* [SpacePinOrientableManipulation](xref:Microsoft.MixedReality.WorldLocking.Examples.SpacePinOrientableManipulation)
* [PinManipulator](xref:Microsoft.MixedReality.WorldLocking.Examples.PinManipulator)
