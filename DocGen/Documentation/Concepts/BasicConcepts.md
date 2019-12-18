
# The basic idea

Inside-out head tracking systems are an amazing new technology. At their strengths they are almost magical. But they have their weaknesses too.

Inside-out head tracking systems, like those in the HoloLens, are very good at telling where the head is relative to nearby physical features. Equivalently, they are very good at telling where real-world features are relative to the head.

But they are not as good at telling where the head is relative to where the head was. When the head moves from point A to point B, the tracking system will generally be slightly wrong about how far the head is traveled. That means the tracking system will be incorrect about the distance between points A and B. This is frequently and confusingly called "the scale problem".

Then when the head travels from point B back to point A, it will again be wrong about the distance traveled. It will be impressively close to correct, but noticeably incorrect. This is also referred to as "drift".

These problems are discussed more in this [FAQ](../IntroFAQ.md#why-are-the-virtual-and-real-world-markers-inconsistent).

What matters here is that the World Locking Tools can fix these problems. In the latter case, of drift, the World Locking Tools can recognize that the head is back near point A, from the physical features around point A, and correct the head's position.

In the former case, of the scale problem, the World Locking Tools can take additional input from the application to know where point B is relative to point A, and correct that distance travelled as well.

To understand further _how_ the World Locking Tools accomplish this, some additional terminology will be helpful.

# Spongy and world locked spaces

## Spongy space
At the core of World Locking Tools is an optimization engine. It takes as inputs a graph of currently active spatial anchors in the world, along with the current head tracking information. This is commonly referred to, within this and related documentation and code, as the **Spongy state**. The spongy state is so named because it is constantly in flux. The spatial anchors are always in motion relative to each other, and within their native *spongy* coordinate space, as incoming sensor data refines their state. 

This spongy space is the only coordinate system previously available in which the mixed reality application developer could work.

## World locked space

From the spongy state, the World Locking Tools engine computes a stable space which optimally aligns the spongy space with the physical world. This stable space is referred to as **World locked space**, and its full state as the *frozen state*.

It is important to realize that both spongy space and world locked space are rigid cartesian coordinate systems, and in fact differ from each other by only a rotation and offset. However, the transform from spongy space to world locked space changes each frame, as new sensor data is processed.

The difference between the two spaces is that, while incoming sensor data is free to refine (i.e. move) spatial anchors relative to each other and the head in spongy space, world locked space is chosen to minimize such movements. This allows scene objects placed in world locked space to appear fixed in the physical world without being attached to individual spatial anchors. Each frame the engine computes the world locked space in which the underlying anchors are most stable. That is, the world locked space in which virtual objects stay optimally aligned with real world features. 

This transform is applied to the scene each frame by adjusting the local transform of a parent of the camera in the scene graph. Since the camera defines the original spongy space, inserting this "world-locked from spongy" transform into the camera's hierarchy establishes the root space of the scene to be world locked space.

## Persistence

The Frozen State can optionally be persisted across sessions. There are manual controls for both saving the current state and for loading from a saved state. Additionally, flags on the World Locking Tools Manager enable or disable automatic periodic saving of Frozen State, and automatic loading of the last saved state at startup.

Using these features allows the scanning and stabilization of a real space to persist over multiple sessions. 

Additionally, if the Space Pin feature is used to align modeling space to the real space, that alignment can be persisted. In that case, after an initial alignment session to set up the Space Pins to align the modeled scene to a physical space, subsequent sessions can automatically load the modeled scene into the physical space with virtual and real features aligned.

See [Persistence](Advanced/Persistence.md) and [Space Pin feature](Advanced/SpacePins.md) for further details.

## Camera movement implications

A subtle but important thing to note here is that, by applying the correction transform to the camera, the native Unity "stationary frame of reference" has been converted into the optimal world locked frame of reference. Since no objects in the scene were moved, this will not interfere with physics simulation or other dynamics calculations.

However, the camera being moved within the stationary frame of reference does have implications. Specifically, any sub-systems which assume that the head transform is the only transform between the stationary frame of reference and the camera space will be incorrect. 

This is generally not a problem, as such capabilities as teleport already rely on the ability to place a transform between the camera and root space.

Also, the MRTK already factors in the need for such transforms, so for users of MRTK services this will "just work".

For users requiring direct access to lower level systems that are unable to take advantage of MRTK, samples are provided for building adapters. A few such examples are listed below:

* [A world anchor adapter](xref:Microsoft.MixedReality.WorldLocking.Tools.WorldAnchorAdapter)
* [Tap event adapting](xref:Microsoft.MixedReality.WorldLocking.Tools.FrozenTapToAdd)
* [Spatial mapping adapter](xref:Microsoft.MixedReality.WorldLocking.Tools.FrozenSpatialMapping)

## See also

* [Advanced topics](AdvancedConcepts.md)
* [How-to articles](../HowTos.md)
* [Samples](../HowTos/SampleApplications.md)

