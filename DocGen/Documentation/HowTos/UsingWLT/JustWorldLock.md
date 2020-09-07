
# Just world-lock everything

Achieving baseline world-locked behavior for your application requires no code and very little setup. Before getting to the setup, which is described below, let's look at that baseline behavior in more detail.

## Basic locking of the global Unity coordinate system to the physical world.

Integrating WLT into your application provides a number of features straight out of the box, with no additional code or interaction with your app.

Simply put, with WLT, a point in Unity's global coordinate system will maintain its position relative to physical world features.

That means that if you place a hologram in your global space, it will stay where it is in the physical world. No application use of anchors is needed.

Furthermore, if WLT's persistence feature is enabled, then that same point in Unity's global coordinate space will have the same relationship to physical features in subsequent runs of the application.

Do you want to save where a hologram is in the physical world and have it appear there the next time you run your application? Just save its global pose, and restore it next run.

In addition to the simplicity provided, there are a [number of advantages to using WLT](../../Concepts/BasicConcepts.md#world-locked-space) rather than anchors for world-locking your scene.

## How to set it up

Setup for gaining the advantages of baseline WLT behavior is very simple, and can be broken into four (4) steps:

1. Import the [Frozen World Engine NuGet package](../InitialSetup.md#nuget-setup) into your project.
2. Import the [World Locking Tools unity package](../InitialSetup.md#world-locking-tools-assets) into your project.
3. Drop the [WorldLockingManager prefab](../InitialSetup.md#the-core-experience) into your scene.
4. Add an ["adjustment" game object](../InitialSetup.md#adding-world-locking-tools-to-a-unity-scene) to your camera hierarchy.

![](~/DocGen/Images/Screens/Simplest.jpg)

A [walk-through of this basic setup](https://microsoft.github.io/MixedReality-WorldLockingTools-Samples/Tutorial/01_Minimal/01_Minimal.html) con be found in the [World Locking Tools Samples](https://microsoft.github.io/MixedReality-WorldLockingTools-Samples/README.html), a sibling repository devoted to more specialized examples of WLT use.

