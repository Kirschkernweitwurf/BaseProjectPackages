# Base Service Package

The runtime kernel the other Base packages build on. Service location, service
lifetime, an ordered shutdown pipeline and priority trackers, and nothing else.

It was split out of the Core package so that a project can use the save system,
the settings framework or the controller support without dragging in tweening,
audio, menus and the debug menu. Everything here has two dependencies: the Base
Utility package for logging and the Base Attribute package for inspector
attributes.

## Requirements

- Unity `6000.3` or newer

### Related Base packages

- `Base.UtilityPackage` for `CustomLogger` and `UnityObjectUtility`
- `Base.AttributePackage` for `[Required]`, `[NotNullOrEmpty]` and `[SceneName]`

## Systems

### Service location

A lightweight service locator for global access to game systems.

- `ServiceLocator` registers and resolves services by type. Works with
  MonoBehaviour and plain C# services, and treats a destroyed Unity object as
  missing rather than as a live duplicate.
- `IGameService` is the marker interface. Its default methods register and
  deregister the service for you.
- `GameServiceBehaviour` is a base MonoBehaviour that registers on `Awake` and
  deregisters on `OnDestroy`.

```csharp
public class SaveService : GameServiceBehaviour
{
    // Registered automatically. Resolve it anywhere:
    // ServiceLocator.Get<SaveService>();
}
```

### Bootstrapping

`Bootstrapper` instantiates the manager prefabs a scene needs: persistent
managers once per session, scene managers for every scene, and gameplay managers
only while one of the configured gameplay scenes is loaded.

### Shutdown

`ShutdownManager` and `IShutdownHandler` give services an ordered cleanup step
when the application quits, before any objects are destroyed. A save service can
flush to disk there and still reach every object it needs.

### Tracking and priority

- `PriorityTracker` tracks items by priority and uses insertion order as a
  tiebreaker, so the most recent request at the highest priority wins. This is
  how competing callers agree on one owner for cursor state, timescale, the
  active input map, the visible tooltip or the running rumble.
- `Tracker` maps unique keys to values.
- `TrackedItem` and `EPriority` are the value types those two work with.

## Static state and domain reload

`ServiceLocator`, `ShutdownManager` and `Bootstrapper` all clear their static
state from a `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`, so a play
session starts clean whether or not domain reload is enabled, in the editor and
in a build alike.

## Install

Install through the Git Package Manager window or add the Git URL directly.
Install `Base.UtilityPackage` and `Base.AttributePackage` first; UPM cannot
resolve Base package dependencies for Git installs, so the order is manual.