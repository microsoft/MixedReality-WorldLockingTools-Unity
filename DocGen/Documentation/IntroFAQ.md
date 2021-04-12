
# Frequently asked questions

## Definitions used in this documentation:

*Pose* – a position and orientation.

*Hologram* - a visible virtual object.

*Real world* - the physical world.

*Physical world* - the real world.

*Virtual world* - synthetically generated and simulated world presented via electronic media. 

*Virtual world marker* – a pose in the virtual (modeling) coordinate system. That’s it, just a reference position and orientation.

*Real world marker* – a pose relative to the real-world environment and camera corresponding to a virtual world marker. The real-world marker’s pose is established by some combination of sensed data.

*Visible features* - Features of the physical world that are visually recognizable.

## What does World Locking Tools do?

Given inconsistencies between virtual and real world markers, World Locking Tools provides a stable coordinate system and camera adjustment that minimizes the visible inconsistencies.

Put another way, it world-locks the entire scene with a shared pool of anchors, rather than locking each group of objects with the group's own individual anchor. 

## Why are the virtual and real world markers inconsistent?

Among the many amazing technologies behind Microsoft Mixed Reality, the ability to track the headset's pose in the physical world in real time, without the aid of external devices, is especially amazing.

The head tracking system is remarkably accurate at determining the head's pose relative to known virtual reference points in the physical environment. For this discussion, those virtual reference points will be called "visible features". 

On leaving a position known relative to surrounding visible features, for instance if the user walks around the room, and then returning to that same position, the system will recognize many of those same visible features. It can also determine the  poses of those visible features relative to the current head pose, and do so with surprising accuracy.

Now the tracker system knows where these features and the head are relative to each other, but it doesn't know in absolute terms what the coordinates are for either head or features. As the physical world has no absolute coordinate system, there is no right answer. So the tracking system assigns coordinates that are consistent with recent history, but may be inconsistent over all history. That is, on returning to the exact same pose, the head may now have different coordinates than when it left. This is a form of sensor drift.

But if the new head virtual pose coordinates have shifted toward positive X, for example, then all hologram objects that are stationary in that virtual coordinate system are now shifted toward negative X relative to the head. That means that to the observer wearing the head tracker, they will be shifted relative to the real world as compared with their placement previous to the walk.

## Can Unity handle this?

Yes, with limitations. Unity provides an excellent mechanism for dealing with this, known as spatial anchors. If the virtual space has shifted relative to physical space, by keeping track of underlying visible features, a spatial anchor knows to shift itself in virtual space to remain locked in physical space. Anything attached to the spatial anchor will likewise be dragged through Unity's virtual space to remain stationary in physical space.

The limitations are related to the fact that visible features become unreliable when they are far from the head tracker's cameras. This is not surprising. Visible features that aren't visible make poor reference points. 

A spatial anchor's useful range is therefore limited to 3 meters. Depending on the accuracy requirements of the application, the usable range might be less.

## That seems pretty good, what's the problem?

It's beyond good, it's simply amazing. But there are situations, important situations, where spatial anchors do not provide a satisfactory solution.

First, each spatial anchor moves through Unity's virtual coordinate space independently attempting to remain stationary in the physical world. This means that objects anchored independently will move relative to each other as they try to remain in their physical positions. For an application trying to maintain a precise layout, this can be a large problem.

Second, with its limited range, a single spatial anchor will not provide good results for single objects which are larger than the usable range of that spatial anchor. While the points on the object near the spatial anchor will remain well world-locked, because of the lever arm effect, points farther and farther from the spatial anchor will suffer ever increasing errors. This leaves an object, or a collection of objects, larger than a meter or so without a robust world-locking solution.

## What else can go wrong?

On returning to a previously occupied pose, World Locking Tools has enough information to restore the virtual coordinate system back where it was relative to the physical world. This keeps holograms that are stationary in the virtual world also stationary in the physical world.

But drift may occur on a one way trip, as well as a round trip.

As a concrete example, consider measuring 10 meters between two QR codes placed in a physical room, and therefore modeling two boxes in the virtual room as 10 meters apart. But at runtime, because of drift of the head pose in the virtual space, walking the 10 meters between the QR codes moves 11 meters through virtual space. 

The application may opt into a feature of World Locking Tools to address this by providing information calibrating distance in virtual space to distance in physical space. The behavior that World Locking Tools provides is that, standing over the first QR code and looking down will see the first box. As the 10 meters are walked through the physical world, the extra meter in virtual space is quietly absorbed, leaving the head moved 10 meters in virtual space as well. So on reaching the second physical QR code and looking down, the second virtual box will be there as expected.

Note that corresponding adjustments will be made to all of the anchors which don’t have ground truth data, as they are passed on the path between the two QR codes. That adjustment is, of course, applied smoothly to minimize its perception.

## What if the real-world markers aren’t stable?

Furthermore, if the real-world markers are being dynamically updated, World Locking Tools can adjust its spatial frame and camera adjustment to optimally match the current configuration.

For example, on HoloLens if the real-world markers are spatial anchors, then they will drift over time. They will also move on re-establishment (e.g. loop closure), and in other circumstances. As their poses are updating, World Locking Tools compensates by adjusting the camera as before to minimize the perceived inconsistencies between the sensed spatial anchors and their virtual counterparts.

Note that this compensation for updates in anchor positions can happen even in the absence of ground truth data about the markers. The implied ground truth data is that the current relationships between real-world markers is (more) correct.

## What if the inconsistencies get really bad?

World Locking Tools can detect several scenarios in which the inconsistencies between real and virtual markers are large and can be improved upon. For example, more information may be obtained which establishes the spatial relationship between two previously isolated pools of markers. Or loop closure might suggest a shift of markers along the route to allow the endpoints to meet. 

In these cases, World Locking Tools notifies the client of the potential fix, and on the client's bequest performs the fix and notifies the client of adjustments it should make in its objects which track virtual markers. Until the client requests such a fix (if ever), World Locking Tools continues to minimize the perceived inconsistencies.



