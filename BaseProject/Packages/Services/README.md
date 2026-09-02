# Base Services

The runtime kernel the other Base packages build on. Service location, service lifetime, an ordered shutdown pipeline and priority trackers, and nothing else.

It was split out of the Core package so that a project can use the save system, the settings framework or the controller support without dragging in tweening, audio, menus and the debug menu.

## Requirements

- Unity `6000.3` or newer
- `Base.UtilityPackage` for `CustomLogger` and `UnityObjectUtility`
- `Base.AttributesPackage` for `[Required]`, `[NotNullOrEmpty]` and `[SceneName]`
- `Base.EditorUIPackage.Editor` for the Service Locator window
- Assemblies: `Base.ServicesPackage` and `Base.ServicesPackage.Editor`

## Systems

### Service location

- `ServiceLocator` registers and resolves services by type. Works with MonoBehaviour and plain C# services, and treats a destroyed Unity object as missing rather than as a live duplicate.
- `IGameService` is the marker interface. Its default methods register and deregister the service for you.
- `GameServiceBehaviour` is a base MonoBehaviour that registers on `Awake` and deregisters on `OnDestroy`.

```csharp
public class SaveService : GameServiceBehaviour
{
    // Registered automatically. Resolve it anywhere:
    // ServiceLocator.TryGet(out SaveService service);
}
```

### Bootstrapping

`Bootstrapper` instantiates the manager prefabs a scene needs: persistent managers once per session, scene managers for every scene, and gameplay managers only while one of the configured gameplay scenes is loaded. The prefabs it expects ship in `Base.ContentPackage`.

### Shutdown

`ShutdownManager` and `IShutdownHandler` give services an ordered cleanup step when the application quits, before any objects are destroyed. A save service can flush to disk there and still reach every object it needs.

### Tracking and priority

- `PriorityTracker<T>` tracks items by priority and uses insertion order as a tiebreaker, so the most recent request at the highest priority wins. This is how competing callers agree on one owner for cursor state, timescale, the active input map, the visible tooltip or the running rumble.
- `Tracker<TKey, TValue>` maps unique keys to values.
- `TrackedItem<T>` and `EPriority` are the value types those two work with.

## Service Locator window

A live view of the locator while play mode runs: every registered type, the instance behind it, where that instance lives and whether it is still usable. The table re-reads the locator on a timer, so a registration appearing or going stale shows up on its own rather than on a manual refresh. Columns are sortable and resizable.

An entry whose instance was destroyed is what the window exists for: from inside the game that looks like a service that is present and then is not, and here it is one colored badge.

## Static state and domain reload

`ServiceLocator`, `ShutdownManager` and `Bootstrapper` all clear their static state from a `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`, so a play session starts clean whether or not domain reload is enabled, in the editor and in a build alike.

## Install

Install through the Git Package Manager window. Installing this package selects `Base.UtilityPackage`, `Base.AttributesPackage` and `Base.EditorUIPackage.Editor` automatically. UPM cannot resolve Base package dependencies for Git installs, so installing by hand means installing those first, in that order.