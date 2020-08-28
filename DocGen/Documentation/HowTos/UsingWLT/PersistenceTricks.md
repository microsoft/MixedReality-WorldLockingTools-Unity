
# Persistence tricks

## Basic persistence

Basic persistence for World Locking Tools comes enabled by default. This enabling comes in two parts.

![](~/DocGen/Images/Screens/PersistSaveLoad.jpg)

The relevant checkboxes here are the "Auto Load" and "Auto Save", which are checked. You might notice they are greyed out. That's because they are part of the "Use Defaults" choice. Disabling "Use Defaults" enables the selection of arbitrary combinations of the Automation options. 

Further reading is available on [these settings](xref:Microsoft.MixedReality.WorldLocking.Core.ManagerSettings), and on [manipulating them from script](../WorldLockingContext.md).

### Auto Save

The Auto Save option directs WLT to make frequent and regular state saves while running the application. At any time, the application may be terminated with minimal loss of state.

### Auto Load

The Auto Load option directs WLT to load any previously saved state at startup. This effectively allows the application to resume a new session where it left off (w.r.t. WLT) from the last session.

### Full persistence

With both Auto Save and Auto Load enabled, WLT operates seamlessly across sessions. While the position and orientation of global space is essentially arbitrary on the first run (since there is no previous state saved, it uses the head pose at startup as the origin), subsequent runs will share that same coordinate frame.

This leads to interesting behavior when the application starts a new session in a space disconnected from the previous session's space. See the [persistence by location](#persistence-by-location) section below for details.

> [!NOTE]
> The Auto Save and Auto Load settings also apply to global SpacePins. See [below](#persistence-of-spacepins) for details.

## Application control over persistence

The default full persistence is quite suitable for a broad range of applications. 

Some applications, however, might want finer control over the process.

It may seem odd that enabling WLT automatic persistence is broken into two properties, the Auto Save and the Auto Load. Examining cases where the two are used independently might provide insights into the overall persistence system.

### Auto Save but not Auto Load

![](~/DocGen/Images/Screens/PersistSave.jpg)

With this configuration, WLT is set to periodically save its state. However, it will not automatically load any persisted state at startup.

Rather, the system will start in a fresh state, as if it is the first time being run on this device. Only after an explicit request to [Load()](xref:Microsoft.MixedReality.WorldLocking.Core.WorldLockingManager.Load) will it restore the previous session's state.

This allows the application to decide whether or not restoring previous session state would be appropriate, and even to modify the data being restored if necessary.

The general WLT save state is in the file "_LocalState_/frozenWorldState.hkfw". Once created by WLT, that file can be copied to another location and restored back at the application's discretion.

The save file for alignment (SpacePin) data defaults to "_LocalState_/Persistence/Alignment.fwb". However, that can be overridden by the application via the alignment manager's [SaveFileName](xref:Microsoft.MixedReality.WorldLocking.Core.AlignmentManager.SaveFileName).

### Manual save but Auto Load

![](~/DocGen/Images/Screens/PersistLoad.jpg)

### Manual save and load

![](~/DocGen/Images/Screens/PersistNone.jpg)


## Disabling persistence

## A caution during development

## Persistence by location

## Persistence of SpacePins
