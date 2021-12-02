---
title: Persisting spatial coordinate systems
description: Persisting local spatial tracking state across sessions.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Persisting spatial coordinate systems

In general, the benefits of World Locking Tools' persistence capabilities are of more interest than the details of their implementation.

This article will therefore begin with a discussion of the experiences that World Locking Tools persistence enables. After that comes a look at how World Locking Tools State persistence may be managed. It will then close with a brief sketch of what data is saved and loaded.

## World Locking Tools across sessions

As defined [elsewhere](../BasicConcepts.md), the Frozen State is all data necessary to resume the current world-locked stable space.

The primary utility of World Locking Tools' persistence is in allowing the benefits of a preliminary session's work, scanning an area and aligning virtual space to the real world, to be used in subsequent sessions.

Restoration of this state allows subsequent sessions to forego tedious or time consuming setup and get straight to the focal experience.

## Saving World Locking Tools State

Before it can be loaded, the Frozen State must be saved. 

The most straightforward way to save the Frozen State is to enable AutoSave on the World Locking Tools Manager, either in the Unity inspector on the [World Locking Tools Context](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingContext), or at runtime via script.

Setting World Locking Tools Manager state via script is performed by first getting the state, changing it in any desired way, and then setting the state back as a block. For example, to toggle the AutoSave feature:

```
var settings = WorldLockingManger.GetInstance().Settings;
settings.AutoSave = !settings.AutoSave;
WorldLockingManager.GetInstance().Settings = settings;
```

If the AutoSave feature goes from enabled to disabled during a session, no further periodic saves will be attempted. If it goes from disabled to enabled, periodic saves will be begun or resumed.

The AutoSave feature will keep an up-to-date saved state by periodically saving the current state asynchronously.

If more control over the timing of the saving of state is required, then the AutoSave may be set to false, and manual saving done via script. The asynchronous save is easily triggered, as:

```
WorldLockingManager.GetInstance().Save();
```

As the save is asynchronous, other attempts to invoke a `Save()` while a previous is still under way will be ignored.

## Loading Frozen State

Having saved off a Frozen State, it might be desired to reload World Locking Tools back into that state, either in a subsequent session or even later in the same session.

As in saving Frozen State, there are two paths for loading state.

If the AutoLoad flag on the World Locking Tools Manager is enabled, then any previous saved state will be loaded at startup time. If there is no saved state to load, no error is generated and startup proceeds as if the flag wasn't set.

Setting the AutoLoad flag from false to true (for example, via script) at runtime will have no effect. The AutoLoad either happens at initial load, or doesn't happen at all.

However, a load may be initiated from script at any time through the World Locking Tools Manager's Load function:

```
WorldLockingManager.GetInstance().Load();
```

As with the Save, the Load is performed asynchronously. Any subsequent calls to Load while one is still ongoing will be ignored.

## What is saved?

The data required to reconstruct the World Locking Tools mapping, that is the alignment of the virtual world to the real world, can be broken into three groups.

* **Spatial Anchors** - The underlying network of spatial anchors created and maintained internally by World Locking Tools' [Anchor Manager](xref:Microsoft.MixedReality.WorldLocking.Core.IAnchorManager), supply the requisite binding to the real world. Those anchors are persisted via the platforms underlying storage mechanism.

* **Engine State** - Engine state is persisted to allow the engine to resume its current mapping. Restoring this state removes such indeterminacies as the initial pose of the head in the previous session(s).

* **Space Pinning** - If the application has applied any further Space Pins to force alignment of modeling coordinates to the real world at a discrete set of points, that mapping is also persisted.

## What is not saved?

Only state is saved. In particular, settings are not saved. Any configuration changes by the application, for example changes made through the WorldLockingManager API, are reset each time the application starts up to their values as set in the Unity Inspector, or if they aren't set in the Inspector then to their default values in code.

For example, say the application wants to present the user with the option to AutoSave World Locking state, and have the user's preference persist across sessions until changed. Then the application must:

1) Present the user with UX for setting AutoSave preference (presumably with other application settings).
2) Forward the user's preference to the WorldLockingManager
3) Record the preference to file (presumably with other application settings).
4) On application startup, load the saved preference (if any has been saved) and forward to WorldLockingManager.

See notes in [WorldLockingContext](../../HowTos/WorldLockingContext.md#all-settings-may-be-applied-from-script) regarding timing issues when mixing state setting between assets and script.

## See also

* [Space Pins](SpacePins.md)
* [Attachment points](AttachmentPoints.md)
* [Fragments](Fragments.md)
* [Refit operations](RefitOperations.md)
