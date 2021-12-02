---
title: FrozenWorldEngine
description: Low level information about the Frozen World Engine.
author: fast-slow-still
ms.author: mafinc
ms.date: 10/06/2021
ms.localizationpriority: high
keywords: Unity, HoloLens, HoloLens 2, Augmented Reality, Mixed Reality, ARCore, ARKit, development, MRTK
---

# Inside the Frozen World Engine

## `Typedefs`, `structs`, and constants used throughout this documentation

### `Typedefs`

```cpp
typedef uint64_t FrozenWorld_AnchorId;
typedef uint64_t FrozenWorld_FragmentId;
```

### `Structs`

```cpp
struct FrozenWorld_Vector
{
    float x;
    float y;
    float z;
};

struct FrozenWorld_Quaternion
{
    float x;
    float y;
    float z;
    float w;
};

struct FrozenWorld_Transform
{
    FrozenWorld_Vector position;
    FrozenWorld_Quaternion rotation;
};

struct FrozenWorld_AttachmentPoint
{
    FrozenWorld_AnchorId anchorId;
    FrozenWorld_Vector locationFromAnchor;
};
```

### Constants

```cpp
// Special values for FrozenWorld_AnchorId
static const FrozenWorld_AnchorId FrozenWorld_AnchorId_INVALID = 0;
static const FrozenWorld_AnchorId FrozenWorld_AnchorId_UNKNOWN = 0xFFFFFFFFFFFFFFFF;

// Special values for FrozenWorld_FragmentId
static const FrozenWorld_FragmentId FrozenWorld_FragmentId_INVALID = 0;
static const FrozenWorld_FragmentId FrozenWorld_FragmentId_UNKNOWN = 0xFFFFFFFFFFFFFFFF;
```

The most significant practical distinction between the INVALID and UNKNOWN anchor or fragment identifiers is that INVALID identifiers can never be stored in a snapshot (and attempting to do so anyway will lead to an error being reported) but UNKNOWN identifiers can. Semantically, use INVALID to express 'the anchor or fragment does not exist' and UNKNOWN to express 'the anchor or fragment exists, but it is ambiguous, not known, or not relevant at this point'.

You can use `FrozenWorld_FragmentId_UNKNOWN` as the fragment association of all anchors you add to a SPONGY snapshot, for example, as Frozen World ignores them anyway and automatically assigns unique fragment identifiers to all anchors when they are added to the FROZEN snapshot during alignment.


## General considerations and conventions

### Parameter naming conventions

Parameters may have an implied contract based on their name if it matches any of the following patterns (with foo in the following description being a generic placeholder that's substituted by any valid symbol name):

| Naming pattern	| Implied contract
|---|---|
| fooBufferSize, fooOut	| <ul><li>The fooOut pointer points to a writable memory buffer that has room for (at least) fooBufferSize elements of fooOut's pointed-to data type.</li><li>fooBufferSize must be zero or positive.</li><li>fooBufferSize is counted in elements of fooOut's pointed-to data type (not e.g. bytes).</li><li>fooOut must not be null. It must always point to a valid address, even if fooBufferSize is zero.</li><li>The called function will write no more than fooBufferSize elements to the memory pointed to by fooOut even if more data would be available.</li></ul> |
| fooOut	| <ul><li>The memory pointed to by fooOut must be safe to be written to.</li><li>Existing data in the pointed-to memory location is ignored.</li><li>The function will not change the pointed-to memory except by writing a valid update to it; if fooOut points to a buffer intended to receive multiple elements of the same type, only some of the elements may have been written if an error occurs, but each of the written elements will have been written completely.</li></ul> |
| fooInOut	| <ul><li>The memory pointed to by fooInOut may be read and must be safe to be written to.</li><li>The information at the memory location pointed to by fooInOut must be valid (per the function's description).</li><li>The function will not change the pointed-to memory except by writing a valid update to it; if fooInOut points to a buffer containing multiple elements of the same type, only some of the elements may have been updated if an error occurs, but each of the actually updated elements will have been updated completely.</li></ul> |

### Thread safety

This library is thread-aware, but functions in this library that change its state are not generally re-entrant or safe for being called concurrently unless explicitly noted otherwise. Read or query operations can be called safely from different threads in parallel as long as there are no concurrent calls to any functions that change the internal state of the library.

  * __Version information__ can be queried at any time in any thread.
  * __Error information__ can be queried at any time in any thread and always return error information for the last function called in the same thread.
  * __Startup and teardown__ are internally synchronized. It is acceptable to call `FrozenWorld_Destroy()` from a different thread than the one `FrozenWorld_Init()` was called on.
  * __Snapshot access__ of different snapshots is thread-safe – but reads and modifications of the same snapshot are not. However, it is safe to query the same snapshot's data from multiple threads in parallel as long as there are no concurrent modifications to it.
  * __Refit operations__ can be safely prepared in a background thread – though `Init()` and `Apply()` must be externally synchronized with all other accesses to the snapshots they read and modify.
  * __Persistence__ allows reading or writing to streams in a background thread – though `Gather()` and `Apply()` must be externally synchronized with all other accesses to the snapshots they read and modify.


## Diagnostics and errors

### Version information

```cpp
// -> number of chars (excluding trailing null) copied to the buffer
int FrozenWorld_GetVersion(  
    bool detail,
    int versionBufferSize,
    char* versionOut);
```

Returns a representation of the DLL's version. This is useful to know when investigating suspected bugs and weird phenomena because it allows us to relate what you're seeing with a specific version of the Frozen World source code.

If the detail flag is false, the returned version information is a short, single-line string contains a number – this representation is suitable for being displayed on screen or in an info dialog box. If the detail flag is true, the version information is a multi-line string that specifies exactly which source files were compiled to build the DLL.

Under extraordinary circumstances, e.g. if you received a bleeding-edge test build of the DLL directly from a Frozen World developer, both the compact and the detailed version information may describe several distinct version numbers or contain a more detailed listing of different source files and their respective revisions.

This function can be called safely regardless of the library's state (i.e. even before startup and after teardown) and the thread they're called from.


### Error flag and diagnostic error messages

```cpp
bool FrozenWorld_GetError();

// -> number of chars (excluding trailing null) copied to the buffer
int FrozenWorld_GetErrorMessage(  
    int messageBufferSize,
    char* messageOut);
```

Every function validates parameters and preconditions. If anything is amiss, the function returns immediately (with 0, false, or a similar non-result) and sets the error flag that can be queried with `FrozenWorld_GetError()`. If the function completes without errors, the error flag is reset.

Call `FrozenWorld_GetErrorMessage()` to get further detailed diagnostic information about the cause of the error to help you debug the problem. If the error flag isn't set, the returned error message is empty.

These functions can be called safely regardless of the library's state (i.e. even before startup and after teardown). The error information returned by these functions always relates to the most recent (other) function call executed on the same thread.


### Diagnostic data recordings

Frozen World's serialization facility can be used to create a continuous recording of all state necessary to investigate Frozen World's runtime behavior after the fact. Diagnostic recordings can be invaluable assets for offline debugging and testing and are designed to be sufficiently compact and unobtrusive to allow them to be created by default.

See [Persistence](#persistence) below.


## Startup and teardown

```cpp
void FrozenWorld_Init();
void FrozenWorld_Destroy();
```

The `FrozenWorld_Init()` function initializes memory management and allocates some internal data structures in the Frozen World library. It must be called at least once at the start of the session before any of the other Frozen World functions are called.

At the end of the session, `FrozenWorld_Destroy()` must be called once for every prior call to `FrozenWorld_Init()` to clean up.

Both functions can be called multiple times, but must be called in pairs: The first invocation of `FrozenWorld_Init()` performs the actual initialization, and the corresponding (last) invocation of `FrozenWorld_Destroy()` performs the actual cleanup. All other invocations do nothing. This is useful if there are several user libraries that want to access the Frozen World library without requiring them to coordinate startup and teardown among themselves.

These functions are internally synchronized. It is acceptable to call `FrozenWorld_Destroy()` from a different thread than `FrozenWorld_Init()`.


## Alignment (frame-to-frame)

### Initializing the spongy snapshot and aligning the frozen frame of reference

#### 1. Initialize the spongy snapshot

```cpp
// Step 1 of 3:
void FrozenWorld_Step_Init();
```

This clears the SPONGY snapshot. After you've called `FrozenWorld_Step_Init()`, just must fill the SPONGY snapshot manually with…

  * The head's current position and orientation.

  * All anchors you know the current transform (in relation to the head) of. The fragment association of anchors in the SPONGY snapshot is ignored (fragments are created during the alignment step automatically based on whether Frozen World can automatically deduce a spatial relationship between them), so you can use `FrozenWorld_FragmentId_UNKNOWN` for them.
    
  * Edges between those anchors to signify which pairs of anchors are directly spatially related to each other. For example, two anchors next to each other in the same room should be connected by an edge; but two anchors separated by a wall shouldn't be.
    
  * The current most significant anchor among those you've added to the SPONGY snapshot. This is the anchor whose relation to the head is presumably (or likely) most accurately represented in the SPONGY snapshot. This information is used in various ways, e.g. as a starting point when walking though the anchor graph to gather supports (see next step) or when placing scene objects (see [Creating and tracking scene object attachment points](#creating-and-tracking-scene-object-attachment-points) below).

See [Accessing snapshots](#accessing-snapshots) below (also for an introduction on the different kinds of snapshots).


#### 2. Gather alignment supports from the spongy snapshot

```cpp
// Step 2 of 3:
int FrozenWorld_Step_GatherSupports();  // -> number of gathered supports
```

After this function has run, alignment supports can be inspected or tweaked (e.g. to extend or filter the set gathered by default, change the specific location used for alignment, change the relevance and tightness metrics used to weigh supports against each other, or just to visualize the gathered supports).

This function uses the Frozen World alignment configuration to select which anchors from the SPONGY snapshot to gather for supports and how their relevance and tightness metrics are calculated.

Calling this function is optional: You can just as well implement this step manually by creating a set of alignment supports yourself.

See [Configuring Frozen World alignment](#configuring-frozen-world-alignment) and [Accessing alignment supports](#accessing-alignment-supports) below.


#### 3. Align the Frozen World to the alignment supports

```cpp
// Step 3 of 3:
void FrozenWorld_Step_AlignSupports();
```

Alignment is based on the previously initialized SPONGY snapshot and the previously gathered supports.

After this function has run, the FROZEN snapshot can be inspected to find the updated head (i.e. camera) transform (or the alignment transform of the most recently used spongy coordinate frame relative to the frozen coordinate frame) or to visualize frozen anchors and edges.

In addition, after running this function all alignment metrics are also updated and can be queried to find out if a fragment merge or refreeze is indicated (based on configurable thresholds).

See [Accessing snapshots](#accessing-snapshots) and [Querying metrics](#querying-metrics) below.


### Configuring Frozen World alignment

Modify the Frozen World alignment configuration to tweak the results of `FrozenWorld_Step_GatherSupports()`, which affect alignment quality, to the requirements of the implemented scenario. There is a default Frozen World alignment configuration, so doing this is optional.

```cpp
struct FrozenWorld_AlignConfig
{
    // Max edge deviation (0.0..1.0, default 0.05) to cut off 
    // significantly deviating anchors from alignment
    float edgeDeviationThreshold;

    // Relevance gradient away from head
    float relevanceSaturationRadius;  // 1.0 at this distance from head
    float relevanceDropoffRadius;     // 0.0 at this distance (must be 
                                      // greater than saturation radius)

    // Tightness gradient away from head
    float tightnessSaturationRadius;  // 1.0 at this distance from head
    float tightnessDropoffRadius;     // 0.0 at this distance (must be greater 
                                      // than saturation radius)
};

void FrozenWorld_GetAlignConfig(
    FrozenWorld_AlignConfig* configOut);

void FrozenWorld_SetAlignConfig(
    FrozenWorld_AlignConfig* config);
```

### Accessing alignment supports

Access alignment supports after the `FrozenWorld_Step_Gather()` function has run to extend, filter, change, or just inspect the alignment supports gathered from the SPONGY snapshot. Doing this is optional.

```cpp
struct FrozenWorld_Support
{
    FrozenWorld_AttachmentPoint attachmentPoint;

    float relevance;   // 1.0 (max) .. 0.0 (min, excluded)
    float tightness;   // 1.0 (max) .. 0.0 (min, only lateral alignment)
};

int FrozenWorld_GetNumSupports();

int FrozenWorld_GetSupports(
    int supportsBufferSize,
    FrozenWorld_Support* supportsOut);  // -> number of elements copied to the buffer

void FrozenWorld_SetSupports(
    int numSupports,
    FrozenWorld_Support* supports);
```

### Accessing snapshots

Anchor and edge data is organized in different snapshots. Each snapshot contains (at least) any number of anchors along with their poses, fragment associations, and connecting edges. In addition, the SPONGY and FROZEN snapshots contain information about the current head pose and most significant anchor.

  * The SPONGY snapshot must be populated (by you) frame-to-frame with input data to be used for [alignment](#alignment-frame-to-frame).
  * The FROZEN snapshot is maintained and kept up to date as a matter of course during alignment and will also be updated when the results of a [refit operation](#understanding-refit-operations-fragment-merge-and-refreeze) are applied.

Use these enum constants to indicate which snapshot's information you want to access:

```cpp
enum FrozenWorld_Snapshot
{
    FrozenWorld_Snapshot_SPONGY = 0,
    FrozenWorld_Snapshot_FROZEN = 1,
};
```

It is safe to read and modify different snapshots concurrently from different threads. It is unsafe to access the same snapshot (read or modify) concurrently from different threads, including through the use of functions that are documented to require access to these snapshots (e.g. all alignment functions, which require access to the SPONGY and FROZEN snapshots).


### Accessing the head pose and alignment

Get or set the head (i.e. camera) location and directions (only SPONGY and FROZEN snapshots):

```cpp
void FrozenWorld_GetHead(
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_Vector* headPositionOut,
    FrozenWorld_Vector* headDirectionForwardOut,
    FrozenWorld_Vector* headDirectionUpOut);

void FrozenWorld_SetHead(
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_Vector* headPosition,
    FrozenWorld_Vector* headDirectionForward,
    FrozenWorld_Vector* headDirectionUp);
```

Get or set the alignment transform, which maps coordinates in the Frozen World frame of reference into the most recently used spongy frame of reference:

```cpp
void FrozenWorld_GetAlignment(
    FrozenWorld_Transform* spongyFromFrozenTransformOut);

void FrozenWorld_SetAlignment(
    FrozenWorld_Transform* spongyFromFrozenTransform);
```

The alignment transform together with the most recent spongy head transform is wholly redundant with the frozen head transform. Use whichever is more convenient for you.


### Accessing the most significant anchor

Get or set the most significant anchor, i.e. the anchor whose pose relative to the head is currently known best (only SPONGY and FROZEN snapshots):

```cpp
void FrozenWorld_GetMostSignificantAnchorId(
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId* anchorIdOut);

void FrozenWorld_SetMostSignificantAnchorId(
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId anchorId);
```

Get the fragment identifier (as defined by the FROZEN snapshot) of the current most significant anchor:

```cpp
void FrozenWorld_GetMostSignificantFragmentId(
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_FragmentId* fragmentIdOut);
```

If the queried snapshot's most significant anchor is `FrozenWorld_AnchorId_INVALID`, this function returns `FrozenWorld_FragmentId_INVALID`.

If you query the fragment identifier of the most significant anchor in the SPONGY snapshot, this will still look up this anchor's fragment stored in the FROZEN snapshot (because fragment associations in the SPONGY snapshot are ignored). If the spongy most significant anchor doesn't exist in the FROZEN snapshot yet, querying its fragment identifier returns `FrozenWorld_FragmentId_UNKNOWN`.


### Accessing anchors

```cpp
struct FrozenWorld_Anchor
{
    FrozenWorld_AnchorId anchorId;
    FrozenWorld_FragmentId fragmentId;
    FrozenWorld_Transform transform;
};
```

Read all anchors in the snapshot:

```cpp
int FrozenWorld_GetNumAnchors(
    FrozenWorld_Snapshot snapshot);

int FrozenWorld_GetAnchors(  // -> number of elements copied to the buffer
    FrozenWorld_Snapshot snapshot,
    int anchorsBufferSize,
    FrozenWorld_Anchor* anchorsOut);
```

Add anchors to the snapshot or update an individual anchor's transform or fragment association (use with care!):

```cpp
void FrozenWorld_AddAnchors(
    FrozenWorld_Snapshot snapshot,
    int numAnchors,
    FrozenWorld_Anchor* anchors);

bool FrozenWorld_SetAnchorTransform(  // -> true if the anchor exists
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId anchorId,
    FrozenWorld_Transform* transform);

bool FrozenWorld_SetAnchorFragment(  // -> true if the anchor exists
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId anchorId,
    FrozenWorld_FragmentId fragmentId);
```

Remove an individual anchor (and all edges attached to it), or all anchors (along with all edges) at once:

```cpp
bool FrozenWorld_RemoveAnchor(  // -> true if the anchor existed before being removed
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId anchorId);

void FrozenWorld_ClearAnchors(
    FrozenWorld_Snapshot snapshot);
```

### Accessing graph edges

```cpp
struct FrozenWorld_Edge
{
    FrozenWorld_AnchorId anchorId1;
    FrozenWorld_AnchorId anchorId2;
};
```

Read all edges between anchors in the snapshot:

```cpp
int FrozenWorld_GetNumEdges(
    FrozenWorld_Snapshot snapshot);

int FrozenWorld_GetEdges(  // -> number of elements copied to the buffer
    FrozenWorld_Snapshot snapshot,
    int edgesBufferSize,
    FrozenWorld_Edge* edgesOut);
```

Note that querying the number of edges is not a constant-time operation because edges are stored in a sparse array, so all edges must be enumerated in order to find out how many edges there are. If this is a performance concern, consider saving the number of edges from frame to frame and change your edge buffer size based on the number of edges stored indicated by the return value of `FrozenWorld_GetEdges()`.

Add edges between anchors to the snapshot:

```cpp
void FrozenWorld_AddEdges(
    FrozenWorld_Snapshot snapshot,
    int numEdges,
    FrozenWorld_Edge* edges);
```

Remove an individual edge, or all edges at once:

```cpp
bool FrozenWorld_RemoveEdge(  // -> true if the edge existed before being removed
    FrozenWorld_Snapshot snapshot,
    FrozenWorld_AnchorId anchorId1,
    FrozenWorld_AnchorId anchorId2);

void FrozenWorld_ClearEdges(
    FrozenWorld_Snapshot snapshot);
```

## Utility functions

### Merging anchors and edges

```cpp
int FrozenWorld_MergeAnchorsAndEdges(  // -> number of anchors added to the target snapshot
    FrozenWorld_Snapshot sourceSnapshot,
    FrozenWorld_Snapshot targetSnapshot);
```

Copies all anchors and edges that exist in sourceSnapshot but don't exist in targetSnapshot into targetSnapshot, effectively merging all anchors and edges from both snapshots into targetSnapshot.

While doing that, this function adapts fragment associations and anchor poses of the source anchors that are copied over:

  * If there is an overlap of anchors between a source and a target fragment, all non-overlapping anchors in that source fragment are added to the corresponding target fragment (i.e. have their fragmentId reassigned to match the target fragment) and have their poses adapted to become consistent with the poses of previously existing target anchors in the target fragment.

  * If one source fragment __overlaps several target fragments__, the source fragment is split and all non-overlapping source anchors are added to the single target fragment that has the greatest overlap with the source fragment (in number of overlapping anchors).

  * If a source fragment __overlaps no target fragment__ at all, its anchors are copied into targetSnapshot into a new target fragment with a uniquely chosen fragmentId that doesn't exist yet in targetSnapshot. The poses of these anchors remain the same as in sourceSnapshot.

You can use this function to bulk-integrate an entire SPONGY snapshot into the FROZEN snapshot instead of relying on auto-discovery of not-yet-seen support anchors during alignment. This is not usually needed, but it can be useful if you require a guarantee that all anchors you have in your SPONGY snapshot have meaningful corresponding FROZEN poses.


### Identifying missing edges that would guarantee full graph connectivity

```cpp
int FrozenWorld_GuessMissingEdges(  // -> number of elements copied to the buffer
    FrozenWorld_Snapshot snapshot,
    int guessedEdgesBufferSize,
    FrozenWorld_Edge* guessedEdgesOut);
```

Identifies edges that are missing in the given snapshot to guarantee that all anchors in every fragment are fully connected through edges.

The 'guessing' aspect of this function that's suggested by the function name is that while an edge between two anchors should signify that there's traversable free space between these two anchors, this function (obviously) can't know that and therefore makes a guess based on the geometric proximity of anchors. The result of this is a graph that more or less represents the path a user might have taken while creating these anchors.

This function attempts (on a best-effort basis) to avoid very short edges between anchors that are very close to each other. Since the 'edge deviation' metric used internally to identify fractures in the graph (which are caused by SPONGY anchor relations deviating too much from FROZEN anchor relations) is relative to edge length, a very short edge will exhibit a huge 'edge deviation' metric (and cause undesired fracturing or refreeze) if its two anchors change their relation even just a little bit. For this reason, this function will suggest a detour over a slightly more distant anchor to avoid a very short edge as long as full graph connectivity can still be guaranteed.

If the return value of this function indicates that the entire `guessedEdgesOut` buffer was filled with data, there may be more missing edges than can be returned given the buffer size specified by `guessedEdgesBufferSize`. In this case, you can add the guessed edges to the snapshot using the `FrozenWorld_AddEdges()` function and call `FrozenWorld_GuessMissingEdges()` again to identify more missing edges.


### Inspecting metrics and indicators

Query Frozen World alignment metrics to get a standardized high-level view on the current alignment quality. Metrics include indicator flags you can use to determine if a fragment merge is currently possible or if a refreeze is indicated (based on thresholds you configured in your Frozen World alignment and metric configuration settings).

Visual deviation, which is caused by trade-offs made while aligning the Frozen World to the SPONGY snapshot, is measured by these metrics:

  * __Linear deviation__ is simply the distance between a support point in the most recent SPONGY snapshot and its aligned counterpart in the Frozen World. If alignment is perfect, linear deviation is zero.

  * __Lateral deviation__ shoots two imaginary rays, a 'spongy ray' and a 'frozen ray', from the head (i.e. camera) position to a support point in the most recent SPONGY snapshot and to its aligned counterpart in the Frozen World. The spongy ray is then intersected with the frozen support point's view plane (i.e. a plane that is orthogonal to the frozen ray and goes through the frozen support point). The lateral deviation metric is the distance between this intersection point of the spongy ray and the frozen support point. If alignment is perfect, lateral deviation is zero.

  * __Angular deviation__ is the angle (expressed in radians) between the spongy ray and the frozen ray described for lateral deviation above. If alignment is perfect, angular deviation is zero; the maximum possible angular deviation is pi (i.e. 180°). Angular deviation is capped to a configurable minimum distance between the head and the spongy and frozen support point, i.e. limited to the angle as if the head was moved to the center of a circle whose radius is the configured 'near distance' and that goes through the spongy and frozen support point.

Other than being convenient, using these functions is optional: All metrics calculated by the built-in function could just as well be calculated in user code.

### Querying metrics

```cpp
struct FrozenWorld_Metrics
{
    // Merge and refreeze indicators
    bool refitMergeIndicated;
    bool refitRefreezeIndicated;          // configurable

    // Currently trackable fragments
    int numTrackableFragments;

    // Alignment supports
    int numVisualSupports;
    int numVisualSupportAnchors;
    int numIgnoredSupports;
    int numIgnoredSupportAnchors;

    // Visual deviation metrics
    float maxLinearDeviation;
    float maxLateralDeviation;
    float maxAngularDeviation;            // configurable
    float maxLinearDeviationInFrustum;    // configurable
    float maxLateralDeviationInFrustum;   // configurable
    float maxAngularDeviationInFrustum;   // configurable
};

void FrozenWorld_GetMetrics(
    FrozenWorld_Metrics* metricsOut);
```

Metrics are calculated for the support points in the SPONGY snapshot, so if you use `FrozenWorld_Step_GatherSupports()` instead of your own code to gather supports, metrics are affected by these alignment configuration settings (see [Configuring Frozen World alignment](#configuring-frozen-world-alignment) above):

  * The relevanceDropoffRadius setting controls the maximum distance of a support point from the head.
  * The edgeDeviationThreshold setting may cause some supports to be ignored for visual alignment, which is a refreeze indicator in and of itself and also excludes the ignored supports from all visual deviation metrics.

Some metrics are also affected by metrics configuration settings (see [Configuring metrics](#configuring-metrics) below):

  * The refitRefreezeIndicated flag is controlled by the refreeze… thresholds.
  * The angular deviation metrics (maxAngularDeviation and …InFrustum) are limited by the angularDeviationNearDistance setting.
  * The max…DeviationInFrustum metrics are controlled by the frustum… settings.

Metrics apply to the most recent result of calling `FrozenWorld_Step_Align()` and are calculated lazily when    `FrozenWorld_GetMetrics()` is called either for the first time during a step or after `FrozenWorld_SetMetricsConfig()` was called.


### Configuring metrics

Modify the metrics configuration to tweak indicators and frustum-dependent metrics to the requirements of the implemented scenario. There is a default metrics configuration, so doing this is optional.

```cpp
struct FrozenWorld_MetricsConfig
{
    // Angular deviation capped to this distance
    float angularDeviationNearDistance;

    // View frustum
    float frustumHorzAngle;
    float frustumVertAngle;

    // Thresholds for refreeze indicator
    float refreezeLinearDeviationThreshold;
    float refreezeLateralDeviationThreshold;
    float refreezeAngularDeviationThreshold;
};

void FrozenWorld_GetMetricsConfig(
    FrozenWorld_MetricsConfig* configOut);

void FrozenWorld_SetMetricsConfig(
    FrozenWorld_MetricsConfig* config);
```

## Understanding refit operations (fragment merge and refreeze)

In general, you can simply work with locations, distances, and scene object transforms in your scene graph as in any big, rigid coordinate system. Allowing you to do this is at the core of what Frozen World wants to provide to you.

However, as the device's tracking doesn't supply an absolute position in the real world, sometimes two parts of the coordinate system that were originally thought to be separate are discovered to be actually connected to each other, necessitating a fragment merge; or tracking errors accumulate to a degree that makes it necessary to rearrange things in the scene graph to improve alignment quality from there on out, necessitating a refreeze.

These refit operations (i.e. fragment merge and refreeze) occur relatively rarely. Usually, they happen somewhat more frequently as long as the device is still exploring unknown spaces, and they become more infrequent (or even stop happening at all) as the device continues learning about its environment. (In fact, if your device has sufficiently learned about the environment you are using it in, you may never have to deal with refit operations at all.)

Refit operations do not happen automatically: You must actively initiate them when they are indicated. This gives you the opportunity to postpone them until your scene is in a state that makes it easier for you to deal with the refit. You can even rate-limit refit operations yourself.

A refit operation becomes indicated (see [Inspecting metrics and indicators](#inspecting-metrics-and-indicators) above) as deviations between the generally immutable Frozen World and the always-changing, always-evolving SPONGY snapshots supplied during alignment become too great. Whatever counts as 'too great' is a subjective and application-dependent quality trade-off you must make based on your particular scenario. (You can use the default configuration and indicators as a starting point.)

As the last step of doing a refit operation, some or all of your scene objects must change their actual transform in the scene coordinate system so they stay visually aligned with the real world. Since you are yourself in control of initiating refit operations, there is no need for this 'scene refit' to be done in realtime on a per-frame budget: Your code can take however much time it needs to get it right.


### Creating and tracking scene object attachment points

Unfortunately, as device tracking errors accumulate, the result can be that things are close to each other (or even overlap) in Frozen World coordinate space that are nowhere near each other in the real world. For that reason, Frozen World coordinates alone aren't sufficient to fullly describe which ways two nearby scene objects should move, respectively, as the result of a refit operation.

__Attachment points__ are small data structures (see [Typedefs, structs, and constants used throughout this page](#typedefs-structs-and-constants-used-throughout-this-documentation) above) that describe the logical attachment of something to a certain part of the Frozen World.

In essence, an attachment point captures which anchor is that scene object's own 'most significant' one (like the most significant anchor supplied for the device itself in a SPONGY snapshot). In addition to the anchor identifier, an attachment point also contains a location in that anchor's own frame of reference. This location normally coincides with the scene object's location if the scene object is sufficiently close to its anchor, but this is not a general rule you should rely on.

You should create and maintain an attachment point for every top-level scene object that can move independently.

__Attachment points aren't anchors__. Even though they technically refer to _one_ anchor in the frozen graph and encode a point in that anchor's coordinate system, they are not interpreted by Frozen World in a way that relates them to just _that one_ anchor. Instead, an attachment point logically encodes 'a point in the graph between its anchors' for future reference when applying a refit operation, and to a significant degree any given attachment point could be referencing any of the anchors close to it without affecting its behavior at all.

This seems not so different from attaching a scene object directly to an anchor (e.g. an individual SpatialAnchor or Unity WorldAnchor) without Frozen World. However, there are two significant differences: Firstly, Frozen World attachment points never change position on their own; and secondly, unlike anchors, attachment points can (and should) be purposely transitioned through the scene to tag along with the scene object they're attaching to the Frozen World.

__Attachment points are lightweight__. Creating or using an attachment point leaves no footprint inside the Frozen World library. None of the library functions that accept FrozenWorld_AttachmentPoint parameters alter the state of the Frozen World in any way (though they _do_ inspect it). There is no library-side overhead involved in creating or maintaining a great number of attachment points (aside from, obviously, the compute involved in calling library functions with them).


#### Create an attachment point for a newly placed scene object

```cpp
void FrozenWorld_Tracking_CreateFromHead(
    FrozenWorld_Vector* frozenLocation,
    FrozenWorld_AttachmentPoint* attachmentPointOut);

void FrozenWorld_Tracking_CreateFromSpawner(
    FrozenWorld_AttachmentPoint* spawnerAttachmentPoint,
    FrozenWorld_Vector* frozenLocation,
    FrozenWorld_AttachmentPoint* attachmentPointOut);
```

Which one of these functions you should call to create an attachment point for a newly placed scene object depends on whether the newly placed scene object was, from the logic and intent of your particular scenario, spawned off some already-existing scene object (like a rocket launched from a rocket launcher, or an egg laid by a duck, or a child window slate detached from its parent window slate) or not.

If you are placing a scene object without an initial relation to any existing scene objects, use `FrozenWorld_Tracking_CreateFromHead()`, which creates the initial attachment point for the scene object as if the device had spawned it. Otherwise, use `FrozenWorld_Tracking_CreateFromSpawner()` and pass the existing scene object's attachment point as the spawnerAttachmentPoint.


#### Track an attachment point when its scene object moves

When your scene object continuously moves through the scene (because it is animated or simulated; this does not apply to scene objects being relocated because of a refit operation!), you should move its attachment point along with it.

```cpp
void FrozenWorld_Tracking_Move(
    FrozenWorld_Vector* targetFrozenLocation,
    FrozenWorld_AttachmentPoint* attachmentPointInOut);
```

It is not necessary to move a scene object's attachment point every frame during continuous movement: You can wait until it has moved into a distance of at least a half-unit away from where you updated its attachment point before until you need to move the attachment point along with it. However, if you do so, you should, after preparing a refreeze, make sure to do one final update of the attachment point just prior to invoking `FrozenWorld_RefitRefreeze_CalcAdjustment()` to ensure that the calculated adjustment is based on the scene object's latest position.

Note that if you teleport a scene object through the scene (instead of continuously moving it through the scene), you should forget its prior attachment point data and initialize a new one from scratch based on the same considerations as for a newly placed scene object.


### Initiating and executing a fragment merge

Fragment merge is due when there are multiple simultaneously trackable fragments represented in the SPONGY snapshot. (Built-in Frozen World metrics take only the support anchors used for alignment into consideration for the `refitMergeIndicated` flag.)


#### 1. Initialize the fragment merge

```cpp
// Step 1 of 4:
bool FrozenWorld_RefitMerge_Init();
```

The fragment merge operation is initialized with the current version of the SPONGY snapshot set up after `FrozenWorld_Step_Init()`. After the fragment merge has been initialized, it is safe to change the SPONGY and FROZEN snapshots without affecting the results of the fragment merge.

`FrozenWorld_RefitMerge_Init()` returns true if the necessary preconditions for performing a fragment merge are given (i.e. there's more than one simultaneously trackable fragment represented in the SPONGY snapshot). If this is not the case, the function returns false, and the fragment merge operation is not initialized.

Successfully initializing a fragment merge operation while any other refit operation is running silently cancels the previous refit operation and discards its results. If initializing the fragment merge operation is not successful, the other refit operation (if any) remains unaffected.


#### 2. Prepare the fragment merge

```cpp
// Step 2 of 4:
// Can be executed in a background thread.
void FrozenWorld_RefitMerge_Prepare();
```

Preparing the fragment merge is done based on information gathered by `FrozenWorld_RefitMerge_Init()` and is independent from ongoing changes to the state of the SPONGY snapshot or the overall Frozen World (including the FROZEN snapshot and Frozen World configuration).

Normally, this step executes quickly, but if guaranteed realtime performance is a concern, it is safe to execute `FrozenWorld_RefitMerge_Prepare()` asynchronously in a background worker thread even across several frames while the SPONGY snapshot continues to evolve and the Frozen World alignment continues to be done. However, you must take care to not initialize another refit operation while this one is being prepared in the background.


#### 3. Inspect fragment merge results and refit the scene

When `FrozenWorld_RefitMerge_Prepare()` has finished executing, you must change the transforms of some or all of your scene objects to accommodate the pending fragment merge. The scene objects affected by this are identified by the `anchorId` (or more precisely: the `fragmentId` of that anchor) stored in the attachment point you created and maintained for that scene object (see [Creating and tracking scene object attachment points](#creating-and-tracking-scene-object-attachment-points) above).

```cpp
struct FrozenWorld_RefitMerge_AdjustedFragment
{
    FrozenWorld_FragmentId fragmentId;
    int numAdjustedAnchors;
    FrozenWorld_Transform adjustment;   // post-merged from pre-merged
};

// Step 3.1 of 4:
int FrozenWorld_RefitMerge_GetNumAdjustedFragments();

// Step 3.2 of 4:
// -> number of elements copied to the buffer
int FrozenWorld_RefitMerge_GetAdjustedFragments(  
    int adjustedFragmentsBufferSize,
    FrozenWorld_RefitMerge_Adjustment* adjustedFragmentsOut);

// Step 3.3 of 4, for each adjusted fragment:
// -> number of elements copied to the buffer
int FrozenWorld_RefitMerge_GetAdjustedAnchorIds(  
    FrozenWorld_FragmentId fragmentId,
    int adjustedAnchorIdsBufferSize,
    FrozenWorld_AnchorId* adjustedAnchorIdsOut);

// Step 3.4 of 4:
void FrozenWorld_RefitMerge_GetMergedFragmentId(
    FrozenWorld_FragmentId* mergedFragmentIdOut);
```

All scene objects that are in the same Frozen World fragment (i.e. attached to anchors that have the same fragmentId) must have their transforms adjusted by a single common adjustment transform, so you can rely on scene objects in the same fragment keeping relative position and orientation to each other. Keep in mind that orientations may change, too, so don't forget to adjust any directional vectors (e.g. velocities and accelerations) as well.

Note that the fragment itself that everything else is merged into is kept stationary. (Among all fragments that need to be merged, the one whose axis-aligned bounding box has the greatest volume in the Frozen World is chosen to remain stationary and be merged into.) Scene objects in the stationary fragment don't require adjustment, so this fragment isn't reported as an adjusted fragment.


#### 4. Apply the fragment merge results to the Frozen World itself

Finally, after you have taken care of adjusting your own scene objects, corresponding adjustments must be applied to the Frozen World itself to finalize the fragment merge operation.

```cpp
// Step 4 of 4:
void FrozenWorld_RefitMerge_Apply();
```

You can only call `FrozenWorld_RefitMerge_Apply()` only once for a fragment merge operation. After `FrozenWorld_RefitMerge_Apply()` has been called, the function calls required to refit your scene's objects (see [3. Inspect fragment merge results and refit the scene](#3-inspect-fragment-merge-results-and-refit-the-scene) above) cannot be called any longer until the next fragment merge has been prepared.


### Initiating and executing a refreeze

Refreeze is due when anchor relations in the SPONGY snapshot have become so different from their Frozen World counterparts that the visual trade-offs made to align the Frozen World to the SPONGY snapshot are too significant to simply ignore. There is no clear-cut, objective threshold for this: Whether a refreeze is advisable depends on the quality trade-offs you are willing to make in your particular scenario. (Built-in Frozen World metrics use configurable thresholds and take only support anchors into consideration for the refitRefreezeIndicated flag.)

If a refreeze is executed when there are multiple simultaneously trackable fragments in the SPONGY snapshot, it will implicitly merge all anchors in those fragments into a single fragment during the refreeze.

#### 1. Initialize the refreeze

```cpp
// Step 1 of 4:
bool FrozenWorld_RefitRefreeze_Init();
```

The refreeze operation is initialized with the current version of the SPONGY snapshot set up after `FrozenWorld_Step_Init()`. After the refreeze has been initialized, it is safe to change the SPONGY and FROZEN snapshot without affecting the results of the refreeze.

`FrozenWorld_RefitRefreeze_Init()` returns true if the necessary preconditions for performing a refreeze are given (i.e. there's more than one trackable anchor represented in the SPONGY snapshot within relevance distance from the head that's graph-connected to the current most significant anchor). If this is not the case, the function returns false, and the refreeze operation is not initialized.

Initializing a refreeze operation while any other refit operation is running silently cancels the previous refit operation and discards its results. If initializing the refreeze operation is not successful, the other refit operation (if any) remains unaffected.


#### 2. Prepare the refreeze

```cpp
// Step 2 of 4:
// Can be executed in a background thread.
void FrozenWorld_RefitRefreeze_Prepare();
```

Preparing the refreeze is done based on information gathered by `FrozenWorld_RefitRefreeze_Init()` and is independent from changes to the ongoing state of the SPONGY snapshot or the overall Frozen World (including the FROZEN snapshot and Frozen World configuration).

Normally, this step executes quickly, but if guaranteed realtime performance is a concern, it is safe to execute `FrozenWorld_RefitRefreeze_Prepare()` asynchronously in a background worker thread even across several frames while the SPONGY snapshot continues to evolve and the Frozen World alignment continues to be done. However, you must take care to not initialize another refit operation while this one is being prepared in the background.


#### 3. Inspect refreeze results and refit the scene

When `FrozenWorld_RefitRefreeze_Prepare()` has finished executing, you must change the transforms of some or all of your scene objects to accommodate the pending refreeze. The scene objects affected by this are identified by the anchorId stored in the attachment point you created and maintained for that scene object (see Creating and tracking scene object attachment points above).

```cpp
// Step 3.1 of 4:
int FrozenWorld_RefitRefreeze_GetNumAdjustedAnchors();
int FrozenWorld_RefitRefreeze_GetNumAdjustedFragments();

// Step 3.2 of 4:
// -> number of elements copied to the buffer
int FrozenWorld_RefitRefreeze_GetAdjustedFragmentIds(  
    int adjustedFragmentIdsBufferSize,
    FrozenWorld_FragmentId* adjustedFragmentIdsOut);
// -> number of elements copied to the buffer
int FrozenWorld_RefitRefreeze_GetAdjustedAnchorIds(  
    int adjustedAnchorIdsBufferSize,
    FrozenWorld_AnchorId* adjustedAnchorIdsOut);

// Step 3.3 of 4, for each attached scene object:
// -> true if actually adjusted
bool FrozenWorld_RefitRefreeze_CalcAdjustment(  
    FrozenWorld_AttachmentPoint* attachmentPointInOut,
    FrozenWorld_Transform* objectAdjustmentOut);   // post-refrozen from pre-refrozen

// Step 3.4 of 4:
void FrozenWorld_RefitRefreeze_GetMergedFragmentId(
    FrozenWorld_FragmentId* mergedFragmentIdOut);
```

All scene objects that are attached to one of the anchors reported by `FrozenWorld_RefitRefreeze_GetAdjustedAnchorIds()` must have their transforms adjusted by the attachment-point-specific adjustment transform supplied by `FrozenWorld_RefitRefreeze_CalcAdjustment()`. The scene object's attachment point itself must also adjusted, which happens automatically to the attachment point passed to this function.

It's possible for an anchor (or fragment) to be reported by `FrozenWorld_RefitRefreeze_GetAdjustedAnchorIds()` or `…_GetAdjustedFragmentIds()` but for `FrozenWorld_RefitRefreeze_CalcAdjustment()` still to return false when it is called with an attachment point attached to that anchor. This can happen when the more in-depth calculations performed by `FrozenWorld_RefitRefreeze_CalcAdjustment()` come to the conclusion that, despite this anchor being within the refrozen area, it doesn't actually require any adjustment. In this case you're free to simply skip any follow-on processing you might otherwise want to do on your side after an adjustment.


#### 4. Apply the refreeze results to the Frozen World itself

Finally, after you have taken care of adjusting your own scene objects, corresponding adjustments must be applied to the Frozen World itself to finalize the refreeze operation.

```cpp
// Step 4 of 4:
void FrozenWorld_RefitRefreeze_Apply();
```

You can only call `FrozenWorld_RefitRefreeze_Apply()` only once for a refreeze operation. After `FrozenWorld_RefitRefreeze_Apply()` has been called, the function calls required to refit your scene's objects (see [3. Inspect refreeze results and refit the scene](#3-inspect-refreeze-results-and-refit-the-scene) above) cannot be called any longer until the next refreeze has been prepared.


## Persistence

The Frozen World library's persistence support is mainly there for your convenience – there's no inaccessible essential internal state in the library and the binary recording/persistence format is simple and well-documented (see [Frozen World binary recording format](#frozen-world-binary-recording-format) for details).

Instead of using the functions described in this section, you can also implement your own writing and reading facilities for Frozen World without any loss of fidelity. The functions described here just give you a simple, portable way to do the same with less effort, and they give you the no-effort guarantee that they will always be up to date with the latest version of both Frozen World itself (which might perhaps change or extend its data representation in a future update) and the Frozen World binary recording format (which might perhaps be extended to represent some data more efficiently in a future update).

The canonical Frozen World binary format is organized as a series (or: stream) of records and can be used for…

  * __Persistence__ – i.e. saving essential Frozen World data with the intention of restoring its state later to continue a session.

  * __Diagnostics__ – i.e. saving all Frozen World data, including transient data like the SPONGY snapshot, with the intention of using it later to investigate why Frozen World behaved in a certain way in a certain situation (e.g. to debug your scene, or Frozen World's integration into your scene, or Frozen World itself) or to implement automated offline testing based on real recordings and interactions.

Each stream of records is self-contained, i.e. can be usefully stored, transmitted, and read on its own.

However, the records in a given stream are not necessarily all self-contained: The recording format includes the possibility of encoding some data as updates relative to the previous record in the same stream in order to save space, so you need all records from the very start of that stream to guarantee that you can fully restore the recorded Frozen World state up to that point.

For any given stream you create using these functions, you can select what data is going to be saved to or restored from it:

  * The `includePersistent` flag controls inclusion of the most essential Frozen World data required to fully restore a session. This includes all data that can be accessed through the following functions:
    * `FrozenWorld_GetAnchors(FROZEN, …)`
    * `FrozenWorld_GetEdges(FROZEN, …)`


  * The includeTransient flag controls inclusion of all other Frozen World state required to diagnose or replay a session. This includes all data that can be accessed through the following functions:
    * `FrozenWorld_GetAlignConfig()`
    * `FrozenWorld_GetHead(SPONGY, …)` and `(FROZEN, …)`
    * `FrozenWorld_GetAlignment()`
    * `FrozenWorld_GetMostSignificantAnchor(SPONGY, …)` and `(FROZEN, …)`
    * `FrozenWorld_GetAnchors(SPONGY, …)`
    * `FrozenWorld_GetEdges(SPONGY, …)`
    * `FrozenWorld_GetSupports()`

Note that you should enable both flags to get useful diagnostic recordings. Enabling just the includeTransient flag by itself only really makes sense if your only intention is to replay SPONGY snapshot data for offline scene testing.


### Serializing (saving) Frozen World state

```cpp
struct FrozenWorld_Serialize_Stream
{
    // Internal handle to this serialization stream
    int handle;

    // Number of bytes that at least remain to be serialized for a complete record
    int numBytesBuffered;

    // Real time in seconds serialized into this stream so far
    // (can be modified to control relative timestamps serialized into the stream)
    float time;

    // Selection of data to include in the stream
    // (can be modified to control what is serialized into the stream)
    bool includePersistent;     // frozen anchors and edges
    bool includeTransient;      // alignment config, all other snapshot data, supports
};
```

Frozen World recordings are an unbounded sequence of records. Each record encodes a single update's worth of Frozen World data including a relative time stamp that indicates how much real time passed since the last record in the stream was created.

The granularity of updates is entirely up to you:

  * For recordings intended for persistence, you might write just a single record and then close the stream again – or keep a stream open and infrequently (e.g. once every few seconds) append regular updates as a good trade-off between storage space consumption and the timeliness and completeness of the saved data in case your scene quits unexpectedly (e.g. because it crashed).

  * For recordings intended for diagnostics and replay, keep the stream open and frequently (i.e. once every step, just after doing Frozen World alignment) append updates to your recording file. The recording format is designed to be space-efficient and doesn't write a lot of data if nothing much changed since the last record.


#### 1. Opening the stream

```cpp
// Step 1 of 3:
void FrozenWorld_Serialize_Open(
    FrozenWorld_Serialize_Stream* streamInOut);
```

Allocate one instance of the `FrozenWorld_Serialize_Stream` data structure per stream you want to keep open at the same time.

The stream's time property passed to `FrozenWorld_Serialize_Open()` defines the absolute starting time of the stream, which is used later to calculate the relative time encoded in the first record when `FrozenWorld_Serialize_Gather()` is called for the first time for this stream.

It's best to set the stream's initial time property to your scene's current absolute runtime when you call  `FrozenWorld_Serialize_Open()`.


#### 2. Preparing a data record and getting its binary data to save

After opening the stream (which allocates and prepares some resources in the library) you can then repeat the following steps as often as you like, even across an entire session:

```cpp
// Step 2.1 of 3:
void FrozenWorld_Serialize_Gather(
    FrozenWorld_Serialize_Stream* streamInOut);

// Step 2.2 of 3, repeated until no more data is available:
// Can be executed in a background thread.
int FrozenWorld_Serialize_Read(  // -> number of bytes copied to the buffer
    FrozenWorld_Serialize_Stream* streamInOut,
    int bytesBufferSize,
    char* bytesOut);
```

Calling `FrozenWorld_Serialize_Gather()` quickly gathers all information needed for a full record.

The stream's time property when `FrozenWorld_Serialize_Gather()` is called directly controls what relative time (since last record) is encoded in the new record – it's best to keep the stream's time property always set to your scene's absolute runtime.

After gathering data, repeatedly call `FrozenWorld_Serialize_Read()` to copy the serialized binary data of the record into a buffer provided by you, which you can then in turn output/write to wherever you want the recording stream to be physically stored (e.g. a file on disk or a network stream). While you're reading data you can check the stream's numBytesBuffered property to get an indication of how much more data there is at least left to be serialized for this record, which may be useful if you want to e.g. implement rotating size-limited recording files on your side.

Reading serialized binary data by calling `FrozenWorld_Serialize_Read()` can be safely done in a background thread. Doing this can be very useful because it decouples your main thread that runs your scene from unpredictable I/O latencies that are hard to completely avoid when writing data to disk or over network.

You mustn't call `FrozenWorld_Serialize_Gather()` again before all data from the previous record has been read (it will signal an error if you do), but it is safe to simply skip a call to `FrozenWorld_Serialize_Gather()` if writing the previous record's data is still in progress in your background thread. This won't cause your saved recording to become inconsistent or lose data – it will only reduce the granularity of the recording in that instance.


#### 3. Closing the stream to release internal resources

```cpp
// Step 3 of 3:
void FrozenWorld_Serialize_Close(
    FrozenWorld_Serialize_Stream* streamInOut);
```

Close the stream after you're finished using it to release some memory used internally in the library to keep track of it. All open streams are implicitly closed when `FrozenWorld_Destroy()` is called.

`FrozenWorld_Serialize_Close()` will signal an error if the previous record wasn't fully read because that means you have received (and may have output/written) incomplete data that can't be fully deserialized later. You can set the stream's numBytesBuffered property to zero prior to calling `FrozenWorld_Serialize_Close()` to suppress this error.

After calling `FrozenWorld_Serialize_Close()`, the stream's internal handle is deallocated (and set to zero) and cannot be used anymore. You can, however, reuse the `FrozenWorld_Serialize_Stream` data structure to open a new stream later.


### Deserializing (loading and restoring) Frozen World state

```cpp
struct FrozenWorld_Deserialize_Stream
{
    // Internal handle to this deserialization stream
    int handle;

    // Number of bytes that at least remain to be deserialized for a complete record
    int numBytesRequired;

    // Real time in seconds deserialized from this stream so far
    // (can be modified to change its base value for subsequent deserialized records)
    float time;

    // Selection of data applied from the stream
    // (can be modified to control what is deserialized from the stream)
    bool includePersistent;     // frozen anchors and edges
    bool includeTransient;      // alignment config, all other snapshot data, supports
};
```

When reading a Frozen World recording, you must read and apply all updates beginning at the start of the stream.

However, there is no requirement to do this in real time, and you can read and apply even large streams (in the order of hundreds of megabytes and tens of thousands of records) fairly quickly from start to end in order to arrive at a certain recorded point in time.

You can choose to read just a subset of the data contained in the stream by setting the stream's `includePersistent` and `includeTransient` flags. Of course, enabling `includePersistent` won't do anything if the stream doesn't contain such data (i.e. wasn't created with the `includePersistent` flag set during serialization), and the same goes for `includeTransient`.


#### 1. Opening the stream

```cpp
// Step 1 of 3:
void FrozenWorld_Deserialize_Open(
    FrozenWorld_Deserialize_Stream* streamInOut);
```

Allocate one instance of the `FrozenWorld_Deserialize_Stream` structure per stream you want to keep open at the same time.

The stream's time property is ignored by `FrozenWorld_Deserialize_Open()`, but it's later updated by `FrozenWorld_Deserialize_Apply()` by successively aggregating the relative time stamps included in the records that are read. You can change the time property at any time.

It's best to set the stream's initial time property either to zero (in order to track this stream's progress in time) or to your scene's current absolute runtime (in order to track the stream's progress in terms of your scene's absolute runtime).


#### 2. Loading binary data into the stream and applying the results

After opening the stream (which allocates and prepares some resources in the library) you can then repeat the following steps as often as you like and as long as you have data to feed into the stream:

```cpp
// Step 2a of 3, repeatedly until no more data is consumed:
// Can be executed in a background thread.
int FrozenWorld_Deserialize_Write(  // -> number of bytes consumed from the buffer
    FrozenWorld_Deserialize_Stream* streamInOut,
    int numBytes,
    char* bytes);

// Step 2b of 3:
void FrozenWorld_Deserialize_Apply(
    FrozenWorld_Deserialize_Stream* streamInOut);
```

Repeatedly call `FrozenWorld_Deserialize_Write()` with more data read from your data source (e.g. a file on disk or a network stream you've opened) until it returns zero, indicating that the record is complete and no more data needs to be consumed for this record. While you're doing this, you can check the stream's `numBytesRequired` to get an indication of how much more data must be read at least for this record, which is useful if you want to make sure you read no more than the exact required amount of data from your data source.

Feeding serialized binary data by calling `FrozenWorld_Deserialize_Write()` can be safely done in a background thread. Doing this can be very useful because it decouples your main thread that runs your scene from unpredictable I/O latencies (or blocking I/O calls while a network stream is waiting for more data to arrive) that are hard to avoid when reading data from disk or from a network.

Even if input data is invalid, `FrozenWorld_Deserialize_Write()` will never report this as an error (as it's near-impossible to recognize binary recording data as malformed until it is parsed in detail and applied to Frozen World state). However, sufficiently malformed input may mislead `FrozenWorld_Deserialize_Write()` into requesting and consuming (if not necessarily buffering or meaningfully processing) any amount of input data while waiting for a valid record footer to appear in the data stream. If you're out of data to feed into `FrozenWorld_Deserialize_Write()` and it's still requesting more data, that in and of itself indicates that something may be wrong with the data you've fed it so far.

After feeding a record's worth of data to `FrozenWorld_Deserialize_Write()`, call `FrozenWorld_Deserialize_Apply()` to apply the information contained in the record to Frozen World's state. Any invalid data previously fed to `FrozenWorld_Deserialize_Write()` will be reported as an error by `Frozenworld_Deserialize_Apply()`.

You mustn't call `FrozenWorld_Deserialize_Apply()` until and unless a full record's worth of data has been fed into the library (it will signal an error if you do). This includes calling `FrozenWorld_Deserialize_Apply()` more than once without feeding more data in between the two calls. This behavior cannot be suppressed, so it is impossible to apply truncated records.

You can change the `includePersistent` and `includeTransient` flags in between calls to `FrozenWorld_Deserialize_Apply()`. However, after the first call to `FrozenWorld_Deserialize_Apply()` for a given stream, you can only switch those flags off – you cannot start out with one or both of the flags disabled and switch them on mid-stream. The reason for this is that individual records might only encode an update instead of being fully self-contained, so skipping some records in the middle of the stream may leave the affected Frozen World data structures in an incomplete state.


#### 3. Closing the stream to release internal resources

```cpp
// Step 3 of 3:
void FrozenWorld_Deserialize_Close(
    FrozenWorld_Deserialize_Stream* streamInOut);
```

Close the stream after you're finished using it to release some memory used internally in the library to keep track of it. All open streams are implicitly closed when `FrozenWorld_Destroy()` is called.

After calling `FrozenWorld_Deserialize_Close()`, the stream's internal handle is deallocated (and set to zero) and cannot be used anymore. You can, however, reuse the `FrozenWorld_Deserialize_Stream` data structure to open a new stream later.

Frozen World configuration and snapshots (both spongy and frozen) can be recorded in a platform-independent, compact, binary, streaming format to help with debugging and diagnostics and to create (or record) test scenarios.

## Frozen World binary recording format

### General structure

A recording stream is an unbounded sequence of records. Each record is a sequence of tagged chunks. Each chunk is a sequence of fields.


#### Field types

| Symbol | Storage | Description |
|---|---|---|
| uint16 | 2 bytes, little-endian | Unsigned 16-bit integer |
| uint32 | 4 bytes, little-endian | Unsigned 32-bit integer |
| uint64 | 8 bytes, little-endian | Unsigned 64-bit integer |
| float | 4 bytes, little-endian | IEEE single-precision floating point |
| *type*[N] | N times the size of *type* | Sequence of N instances of *type* |


#### Field padding and alignment

No padding is inserted between fields. Fields therefore have no particular guaranteed alignment (with respect to the start of the stream).


#### General record structure

Each record starts with a record header chunk, followed by any number of data chunks (including potentially no data chunks at all), and completed with a record footer chunk. The required presence of a record footer is designed to allow readers to read a recording data stream without having to look ahead into the next record's data.

While records may contain any number of chunks, each chunk tag (see [General chunk structure](#general-chunk-structure) below) can appear at most once per record. Readers are not required to support records that contain several instances (or several versions) of the same kind of chunk.


#### General chunk structure

Each chunk has the following structure:

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | Defines the kind of the chunk. |
| uint16 | version | Defines the specific format of the chunk payload (together with the tag). <br/>Version numbers start with 1. Version number 0 is reserved and not used. |
| uint32 | payload size | Number of chunk payload bytes (not including the general chunk header bytes). |
| (…) | (…) | Chunk payload. |

The presence of the payload size in the chunk header is designed to allow readers to load entire chunks without having to parse them to find the end of the chunk or to skip chunks they cannot read.


### Record header and footer chunks

#### Record header chunk

| Type | Content | Additional information
|---|---|---|
| uint16 | tag | 0x0000
| uint16 | version | 1
| uint32 | payload size | 4
| float | relative time since last record | Number of (usually fractional) seconds that have passed since the last record in the stream. The value in the first record of the stream is ignored by readers.


#### Record footer chunk

| Type | Content | Additional information
|---|---|---|
| uint16 | tag | 0xFFFF
| uint16 | version | 1
| uint32 | payload size | 0


### Data chunks

#### Alignment configuration chunk

| Type | Content | Additional information
|---|---|---|
| uint16 | tag | 0x0101
| uint16 | version | 1
| uint32 | payload size | 20
| float | edge deviation threshold | > 0.0
| float | relevance saturation radius | > 0.0
| float | relevance drop-off radius | Greater than relevance saturation radius.
| float | tightness saturation radius | > 0.0
| float | tightness drop-off radius | Greater than tightness saturation radius.


#### Alignment supports chunk

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | 0x0401 |
| uint16 | version | 1 |
| uint32 | payload size | 4 + 28 * number of supports |
| uint32 | number of supports | |	
| […] | [Support definitions](#support-definition) | One definition per support, in no particular order. |

#### Support definition

| Type | Content | Additional information
|---|---|---|
| uint64 | support anchor identifier | |
| float[3] | support position from anchor | X, Y, Z (meters) |
| float | support relevance | 0.0 … 1.0 |
| float | support tightness | 0.0 … 1.0 |

### Spongy snapshot chunks

Spongy snapshots are stored as multiple chunks: the spongy snapshot header and, if required, the spongy graph.

The spongy snapshot header chunk can appear before or after the spongy graph chunk in the record, and there may be other chunks in between the header and the graph chunks.

In order to improve the stream's compactness (and potentially reader performance), the spongy graph can be stored in several alternative ways, individually chosen for each record:

  * as a complete graph definition (that replaces the last known spongy graph),
  * as a graph update (applied to the last known state of the spongy graph read from this stream),
  * or not at all (to indicate that the spongy graph is unchanged from its last known state read from this stream).

Readers assume that the spongy graph is initially empty when a recording stream starts. The first spongy snapshot stored in a recording stream usually includes a complete graph definition (but is not required to).


#### Spongy snapshot header chunk

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | 0x0201 |
| uint16 | version | 1 |
| uint32 | payload size | 36 |
| float[3] | head position | X,Y,Z (meters) |
| float[4] | head orientation | X,Y,Z,W (quaternion) |
| uint64 | most significant anchor identifier | |	


#### Spongy graph chunk

See [Graph chunk alternatives](#graph-chunk-alternatives) below for version, payload size, and payload.

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | 0x0202 – Complete graph definition<br/>0x0203 – Graph update |
| uint16 | version | |	
| uint32 | payload size | |	
| (…) | (…) | |


### Frozen snapshot chunks

Frozen snapshots are stored as multiple chunks: the frozen snapshot header and, if required, the frozen graph.

The frozen snapshot header chunk can appear before or after the frozen graph chunk in the record, and there may be other chunks in between the header and the graph chunks.

In order to improve the stream's compactness (and potentially reader performance), the frozen graph can be stored in several alternative ways, individually chosen for each record:

  * as a complete graph definition (that replaces the last known frozen graph),
  * as a graph update (applied to the last known state of the frozen graph read from this stream),
  * or not at all (to indicate that the frozen graph is unchanged from its last known state read from this stream).

Readers assume that the spongy graph is initially empty when a recording stream starts. The first spongy snapshot stored in a recording stream usually includes a complete graph definition (but is not required to).


#### Frozen snapshot header chunk

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | 0x0301 |
| uint16 | version | 1 |
| uint32 | payload size | 64 |
| float[3] | alignment translation | X,Y,Z (meters) |
| float[4] | alignment rotation | X,Y,Z,W (quaternion) |
| float[3] | head position | X,Y,Z (meters) |
| float[4] | head orientation | X,Y,Z,W (quaternion) |
| uint64 | most significant anchor identifier | |


#### Frozen graph chunk

See [Graph chunk alternatives](#graph-chunk-alternatives) below for version, payload size, and payload.

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | 0x0302 – [Complete graph definition](#complete-graph-definition)<br/>0x0303 – [Graph update](#graph-update) |
| uint16 | version	|
| uint32 | payload size |	
| (…) | (…)	|


### Graph chunk alternatives

Writers should choose the most compact representation for a given graph. If the graph is unchanged compared to its last version written to this stream, no graph chunk should be written at all.

Readers are required to support any graph chunk representation at any time.


#### Complete graph definition

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | See specific uses above |
| uint16 | version | 1 |
| uint32 | payload size | 8<br/>+ 44 * number of anchors<br/>+ 20 * number of edges |
| uint32 | number of anchors | |	
| uint32 | number of edges | |
| […] | [Anchor definitions](#anchor-definition) | One definition per anchor, in no particular order.<br/>Each anchor identifier can appear only once in this chunk. |
| […] | [Edge definitions](#edge-definition) | One definition per edge, in no particular order.<br/>Each pair of edge anchor identifiers (1/2 or 2/1) can appear only once in this chunk. |

#### Graph update

| Type | Content | Additional information |
|---|---|---|
| uint16 | tag | See specific uses above. |
| uint16 | version | 1 |
| uint32 | payload size | 8<br/>+ 44 * number of added or changed anchors<br/>+ 20 * number of added or changed edges<br/>+ 8 * number of removed anchors<br/>+ 16 * number of removed edges |
| uint32 | number of added or changed anchors |
| uint32 | number of added or changed edges |
| uint32 | number of removed anchors |
| uint32 | number of removed edges |
| […] | Added or changed [anchor definitions](#anchor-definition) | One definition per added or changed anchor, in no particular order. |
| […] | Added or changed [edge definitions](#edge-definition) | One definition per added or changed edge, in no particular order. |
| […] | Removed [anchor identifiers](#anchor-identifier-definition) | One identifier per removed anchor, in no particular order.<br/>Each anchor identifier can appear only once in this chunk. |	
| […] | Removed [edge anchor identifiers](#edge-anchor-identifier-definition) | One definition per removed edge, in no particular order.<br/>Each pair of edge anchor identifiers (1/2 or 2/1) can appear only once in this chunk. |

#### Anchor definition

| Type | Content | Additional information |
|---|---|---|
| uint64 | anchor identifier | |
| uint64 | anchor fragment identifier | |
| float[3]| anchor position | X,Y,Z (meters) |
| float[4] | anchor orientation | X,Y,Z,W (quaternion) |

#### Edge definition

| Type | Content | Additional information |
|---|---|---|
| uint64 | edge anchor identifier 1 | Edge anchor identifiers are stored in slots 1/2 in no particular order. |
| uint64 | edge anchor identifier 2 | |
| float | edge confidence | 0.0 … 1.0 |

#### Anchor identifier definition

| Type | Content | Additional information |
|---|---|---|
| uint64 | anchor identifier | |

#### Edge anchor identifier definition

| Type | Content | Additional information |
|---|---|---|
| uint64 | edge anchor identifier 1 | Edge anchor identifiers are stored in slots 1/2 in no particular order. |
| uint64 | edge anchor identifier 2 | |
