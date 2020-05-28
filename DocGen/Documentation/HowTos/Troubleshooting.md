
# Trouble shooting

Annoying issues will be noted here.

## World Locking issues

### Low frame rate

World Locking Tools should have no discernable impact on your framerate. (An exception is that the visualizations will eventually drag down your framerate after many anchors have been created, but the visualizers are just for diagnostics, not for shipping with your application.)

If you see a framerate drop after adding World Locking Tools to your application, check your Unity logs. That usually means an exception is being repeatedly generated.

### Missing dll's etc.

This has been seen from the Frozen World Engine dll. Go to NuGet for Unity:
 
>  `NuGet > Manage NuGet Packages > Installed`
 
uninstall and re-install the latest FrozenWorld.Engine package.

### It's not working

Check the Unity logs for errors and exceptions. 

Check that your scene camera is attached to at least one other object. See the setup in WorldLocking.Examples.WorldLockingPhysicsSample for example. If you are doing dynamic camera manipulation, you may need to keep the WorldLockingManager informed of the current camera. See [WorldLockingManager.AdjustmentFrame](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager.AdjustmentFrame) and [WorldLockingManager.CameraParent](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager.CameraParent).

## More general Unity/AR problems

### "DirectoryNotFoundException: Could not find a part of the path"

The path has grown too long. See fuller [explanation here](InitialSetup.md#a-warning-note-on-installation-path-length).

### "A remote operation is taking longer than expected" message box then failure to deploy.

Check your USB connection. A bad cable, a bad port, missing IPOverUSB, can all cause this. But it's probably somewhere on the communication path from your PC to your device.

### Missing Windows SDK components.

Mismatch between Visual Studio version indicated in Unity versus Visual Studio version you're trying to build with. Check:

> `Unity > File > Build Settings > Visual Studio Version` 

Especially dangerous is if that's set to `Latest Installed` and you have multiple versions of Visual Studio installed.

### On HoloLens, application starts up as a slate, rather than an AR experience.

For Unity version 2019.3 and earlier, not sure about later), check: 

> `Unity > Project Settings > Player > XR Settings`  

You must have Virtual Reality Supported checked, and the Windows Mixed Reality in Virtual Reality SDKs.

### When building for ARM on HoloLens2, app stops at startup. ARM64 works fine.

[Known issue](https://issuetracker.unity3d.com/issues/enabling-graphics-jobs-in-2019-dot-3-x-results-in-a-crash-or-nothing-rendering-on-hololens-2). 

The fix is either disable Graphics Jobs under 

> `Project Settings > Player > Other Settings > Graphics Jobs`

or just build for ARM64.