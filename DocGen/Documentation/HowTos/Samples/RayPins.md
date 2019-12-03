# Ray Pins Example

## Related samples

The [Space Pins](SpacePin.md) sample demonstrates setting up Space Pins by manually manipulating marker objects into position using MRTK affordances. More abstract discussion about the [Space Pin feature](../../Concepts/Advanced/SpacePins.md) is also relevant background for this sample.

Rather than manual manipulation of objects, this sample uses ray cast tests onto the spatial reconstruction meshes to set the world alignment.

Significantly, this sample also creates all required World Locking Tools components from script, rather than needing assets setup in the editor.

## Scene contents

There are eight (8) Space Pin virtual marker objects in the RayPins scene. Four (4) are floor level, at the northeast, southeast, northwest, and southwest corners of a square four meters (4m) per side.

Another four (4) pins are one meter up, suggesting they are points on walls six meters (6m) apart.

## Running the sample

### Physical setup

Find a physical space with some clearance. Place markers on the floor and walls at the same separations as the virtual markers in the scene. It's not necessary to have a physical marker for every virtual marker.

It is helpful to either label the physical markers with the name of the corresponding virtual marker (e.g. "NW"), or draw a map with their placement labeled.

### App setup

Build and deploy the RayPins scene to device.

### Running the app

#### Startup
On startup, the coordinate system is based on the head position, and the virtual grid and markers placement is arbitrary.

#### First marker

On the radio selection, pick one of the markers for which there is a corresponding physical marker in the room. Click on the physical marker in the room. The scene will shift to align the selected virtual marker to the ray hit physical marker.

If the alignment is unsatisfactory, for example because of a slip at the moment of selection, simply repeat selecting, with the appropriate radio button still selected, until satisfactory alignment is achieved.

#### Second marker

Move to another physical marker in the room, and select its virtual marker in the radio selection. Click on that physical marker. The grid and markers now rotate to align with both markers aligned.

#### More markers

When near either of the first two pins placed, alignment should be quite close between the physical and virtual markers.

For other markers, however, there may be significant misalignments between physical and virtual. These may arise from a number of sources, but primarily from inexact placement of physical markers, or from tracker error.

Repeat the radio selection and ray hit placement of virtual markers for any further physical markers placed in the room. After this placement process, any such marker should show good alignment when near it.

#### Verification

A physical tape measure may be used to verify the interpolated alignment between markers. The grid lines are spaced one meter (1m) apart, and the lines are one centimeter (1cm) wide.

### Persistence

Note that since AutoSave and AutoLoad are enabled on the WorldLockingContext in the RayPins scene, after aligning the content to a physical room and exiting the app, on running the application again the virtual grid and markers will resume their alignment with the physical room.

To clear the alignment and start over, either select the Reset radio button, or uninstall and reinstall the application.
