# Getting started in World Locking Tools

Getting started in World Locking Tools can be as simple as dragging a prefab into a Unity scene. 

However, understanding the work the Unity layer of World Locking Tools performs, as well as the underlying FrozenWorld engine, can be helpful as well as educational. 

### Goals of this documentation

The first goal is to establish an understanding of what World Locking Tools is trying to do. This can help in setting expectations for what problems World Locking Tools can solve, and what problems are out of its scope.

The number-crunching optimization at the core of World Locking Tools is performed in an engine implemented as an efficient C-style DLL. While a C# shim is provided for directly interfacing to the World Locking Tools engine from Unity scripting, it is hoped that low level interaction with the engine will be rarely if ever needed.

It is important to understand that World Locking Tools' Unity layer acts as a proxy application, performing the tasks that the vast majority of applications built on Unity would need to perform in managing the FrozenWorld engine. Sharing this engine harness makes sense, avoiding each application developer being responsible for implementing essentially the same control structure. 

But for an application which is in the minority having special requirements, World Locking Tools' Unity layer acts as a sample scaffolding for building a custom harness for the engine. Understanding what the provided scaffolding is doing is necessary to modify it, or to implement a variation of it.

It will be shown that a good deal of customization is available, even without modifying code. Understanding what World Locking Tools is doing, as well as the customizable properties, is helpful for fine tuning World Locking Tools for a specific application.

Finally, an understanding of World Locking Tools can be helpful when things go wrong, in narrowing which system is faulty, providing useful bug reports, and establishing workarounds.

### Guide structure

These guides are arranged to both build an understanding of World Locking Tools at a conceptual level, and provide practical step-by-step instructions on putting World Locking Tools to use in real world MR applications.

* [Before getting started](HowTos/UsingWLT/BeforeGettingStarted.md)
* [Quick start](HowTos/QuickStart.md)
* [Initial setup](HowTos/InitialSetup.md)
* [Concepts](Concepts.md)
* [The basic system](Concepts/BasicConcepts.md)
* [Advanced topics](Concepts/AdvancedConcepts.md)
* [How-to articles](HowTos.md)
* [Samples](HowTos/SampleApplications.md)

Additionally, the [API documenation](../api_doc/Architecture.md) provides a reference on programmatic interfaces into World Locking Tools. It should be stressed that in most cases, there will be no need to code directly to the World Locking Tools interfaces, and scene setup and property settings in the inspector are all that are required. The exception is with **attachment points**, which are covered later both [conceptually](Concepts/Advanced/AttachmentPoints.md) and in the [programming reference](xref:Microsoft.MixedReality.WorldLocking.Core.AttachmentPoint).