# Space Pins

## The problems to solve

### The scale error

While the traditional WorldAnchor approach to aligning Holograms with real world features works great on a small scale, it struggles as the scale grows to encompass more than a meter or so.

Scale error in head tracking space means that even if a WorldAnchor keeps one end of a virtual object, sized only a few meters long, perfectly aligned with a real world feature, the other end is likely to be misaligned with a corresponding real world feature. This is because the distance traveled through head tracked space tends to differ from the distance traveled through physical space with an error bound of +-10%. The actual error is often less (it depends on a number of environment and device characteristics), but will generally be significant, and grow without bounds as the scale of the project grows.

Put another way, if a user wearing a HoloLens walks ten meters in the real world, the distance travelled in virtual space, as reported by the head tracker, will be between 9 and 11 meters. If the user walks 100 meters, the error grows to +-10 meters.

Thus, a 10 meter beam (in modeling space) with one end point perfectly aligned to the zero end of a tape measure in real space will have the other end registered to the tape measure at somewhere between 9 and 11 meters.

For the same reasons, multiple objects, each world locked using WorldAnchors, will be different distances apart in virtual space than in real space.

### The arbitrary coordinate system

There is an additional concern. The Unity coordinate system in HoloLens is indeterminate. It is based solely on the head pose at the start of the application.

This is not an issue for many tasks. If the goal is to cast a ray into the spatial mapping of the room and place a Hologram at the hit position, then the numerical values of the hit position are irrelevant.

Likewise, when popping up UX elements around the user, the absolute coordinates to place a UX element at don't matter, only the coordinates relative to the user.

However, more involved scenarios can be complicated by the unpredictable coordinate system. To load a large collection of objects, for example a user's desktop or an entire office room, into virtual space with a fixed relation to physical space, requires some compensating transform to align the modeling space objects with the head based coordinate frame. 

That compensation is often done by attaching all objects to a single Unity transform, and adjusting that single transform to position and orient the virtual objects in alignment with the real world.

Equivalently, a single transform in the camera's hierarchy can be used to realign the camera so that when the user is seeing a real world reference point, a virtual object with the desired modeling coordinates will appear overlaid on that feature.

## The solution

The Space Pinning feature addresses both of these issues at once. It does so by leveraging both the world-locked nature of the World Locking Tools global space, and the arbitrariness of that space.

### Aligning Unity space wth the real world

World Locking Tools at its core provides a stable world locked coordinate system. This means that a virtual object placed into Frozen Space registered with a real world feature will remain registered with that real world feature over time.

But there are an infinite number of spaces that satisfy that goal. In fact, given one world-locked space, transforming it by any arbitrary position and rotation produces another equally valid world-locked space.

The Space Pin feature applies an additional constraint that removes the indeterminate nature of the World Locking Tools world-locking transform.

That constraint is that when "near" a Space Pin, the pose of that Space Pin in world-locked space will be the same as the pose of the Space Pin in modeling space.

Consider a cube in a Unity scene modeled at global coordinates of (0, 0, 1). When the scene is loaded into HoloLens, the cube will appear 1 meter in front of the initial head pose. Depending on the initial head pose, that might be anywhere in the physical room.

The Space Pin allows that cube to be locked to a real world feature in the room, e.g. the corner of a specific desk. Unlike locking the cube with a WorldAnchor, the Space Pin moves the entirety of Unity space such that the cube is aligned with the desk corner. So, for example, other desktop items modeled relative to the cube in Unity will be dispersed properly across the real desktop.

### Addressing the scale error

While a single Space Pin removes the indeterminacy of the relation between virtual coordinate and the real world, it doesn't address scale error.

That is, while it may have moved the origin to a physical world aligned position and orientation, walking 10 meters in the real world might still only move the user 9 meters in virtual space.

For this, multiple Space Pins provide the complete solution. When near any specific Space Pin, the world will be aligned according to that Space Pin. The other Space Pins will be off, but being more distant, that generally proves to be acceptable, and often imperceptible. 

As the user moves between Space Pins, a smooth interpolation minimizes the scale error at any given point in space. With an adequate density of Space Pins as reference points, misalignment of real world and virtual features is reduced to the order of head tracker error.

While the required density of Space Pins depends on both the tracking quality the environment supports and the precision requirements of the application, some numbers here might help set expectations. In an office environment, with adequate lighting and visible features to track, a spacing of 10 meters between Space Pins reduces error from an accumulation of 10-20 cm over 10 meters, down to millimeter errors (max error l.t. 0.5cm, 0.0 error at endpoints).

## Persistence

The Space Pin feature works in tandem with the rest of World Locking Tools' persistence. There are both manual calls for invoking saving and loading from script, and flags for automated saving and loading per session.

When enabled, the AutoSave/AutoLoad feature on the World Locking Tools Manager will allow the full spatial alignment of the virtual world to the real world to be restored on subsequent sessions.

In practice, this means that a single or small number of preliminary sessions may be used to establish an adequate scan of the physical environment, and alignment of that physical environment with Unity's modeling coordinate space. Subsequent sessions will then load the virtual environment correctly aligned with the real world without further user action required.

### See also

* [Space Pin Sample](../../HowTos/Samples/SpacePin.md)
* [Ray Pins Sample](../../HowTos/Samples/RayPins.md)

### Also see

* [Attachment points](AttachmentPoints.md)
* [Fragments](Fragments.md)
* [Refit operations](RefitOperations.md)
