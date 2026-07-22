# Base Core Package

Reusable core systems that any Unity project can build on. The package bundles
service location, scene loading, audio, input, tweening, menus, timers, object
pooling and debug tooling under the `Base.CorePackage` namespace.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0`

### Related Base packages

The Core package uses a few sibling packages. Install them alongside it:

- `Base.UtilityPackage` for logging and shared helpers
- `Base.AttributePackage` for inspector attributes such as `[Required]` and `[GetComponent]`
- `Base.ToolPackage` for the menu manager window integration

## Systems

### Services

A lightweight service locator for global access to game systems.

- `ServiceLocator` registers and resolves services by type. Works with
  MonoBehaviour and plain C# services.
- `IGameService` is the marker interface. Its default methods register and
  deregister the service for you.
- `GameServiceBehaviour` is a base MonoBehaviour that registers on `Awake` and
  deregisters on `OnDestroy`.
- `Bootstrapper` instantiates persistent and per-scene manager prefabs, driven
  by a list of gameplay scenes.
- `ShutdownManager` and `IShutdownHandler` give services an ordered cleanup step
  when the application quits, before any objects are destroyed.

```csharp
public class SaveService : GameServiceBehaviour
{
    // Registered automatically. Resolve it anywhere:
    // ServiceLocator.Get<SaveService>();
}
```

### Event Bus

A strongly typed in-process publish and subscribe bus.

- `IEventBus` and `EventBus` dispatch events to every subscriber in
  subscription order.
- `IEvent` marks a payload. Implement it on a `readonly struct` to stay
  allocation-free.
- `Subscription` is a disposable token. Dispose it to unsubscribe.

### Timers

- `Timer` is a reusable countdown with looping, pausing, progress reporting and
  completion callbacks.
- `TimerManager` advances every active timer through the Player Loop, so timers
  run without any GameObject in the scene.

### Tweening

A data driven tween system with runtime factories, ready-made components and
authoring assets.

- `TweenFX` provides high-level factory methods for common components.
- `Tween`, `TweenBase`, `TweenSequence` and `TweenRunner` form the runtime core.
- Component tweens cover transforms, renderers, images and TextMeshPro. Each
  comes in three flavors: `FadeTween` (fixed), `FadeToTween` (captured start)
  and `FadeByTween` (delta relative).
- `TweenGroup` plays several tweens as one sequence or in parallel.
- Profile assets (`FloatTweenProfileSo`, `ColorTweenProfileSo`,
  `Vector3TweenProfileSo` and `TweenSettingsSo`) let many components share one
  authored setup. A custom inspector hides fields that an assigned profile
  already provides.

### Menu Managing

- `Menu` is the base class for all menus. It handles the lifecycle and the open
  and close animations.
- `MenuManager` registers menus and controls opening and closing.
- `MenuModule` components add single concerns on top of a menu: cursor,
  timescale, input map and child reset. Each is scoped by the menu's priority.
- `MenuIdentifier` assets identify menus. A generated accessor class and a
  runtime `MenuIdentifierRegistry` resolve them by name.
- `PauseMenu` is a ready-to-use example.

### Scene Management

- `SceneLoadingManager` loads and unloads scenes with a persistent scene that
  stays loaded. It uses Unity's `Awaitable` for play-mode-safe async work.
- `SceneLoadEvents` broadcasts progress and activity.
- `LoadingScreen` reacts to those events to show a loading UI.

### Audio

- `AudioManager` plays sound effects and music.
- `AudioContainer` is a ScriptableObject holding clips and their settings.
- Pooled audio sources per `EAudioType` keep playback allocation-light.
- `AudioFader` tweens source volume.
- `OnEvent` components play audio on click, hover, select or submit.

### Input

- `InputManager` enables the highest-priority action map and disables the rest.
- `PrioritizedInputMap` bundles a map with its `EPriority`.
- `BaseInputActions` is the generated wrapper for the package's input asset.

### Object Pooling

- `BaseObjectPoolManager` is a base for global pool managers.
- `HashSetObjectPool` is a fast pool for any GameObject or Component.
- `TweenGroupObjectPool` caches animated UI objects and plays enter and exit
  animations on activation and deactivation.

### Tracking and Priority

- `PriorityTracker` tracks items by priority, using insertion order as a
  tiebreaker.
- `Tracker` maps unique keys to values.
- `CursorManager` and `TimeScaleManager` resolve cursor state and timescale from
  competing priority requests.

### Tooltip

- `TooltipService` shows the highest-priority tooltip, backed by a
  `PriorityTracker`.
- `TooltipTrigger` requests a tooltip while its GameObject is hovered.
- `TooltipView` positions the tooltip so it never leaves the screen.

### Camera Utility

- `MainCameraProvider` caches `Camera.main` and handles Unity's fake-null case.

### Debug Menu

- `DebugMenuController` hosts a cheat console and a log console, toggled by
  input and remembers which one was open last.
- The cheat console discovers `[CheatCommand]` methods through
  `CheatCommandRegistry`. `BuiltinCheatCommands` ships a default set.
- `LogConsole` mirrors Unity's log stream, including `CustomLogger` output.
  Capturing starts before the first scene loads so no logs are missed.

### Activation

- `ActivateAfterFrames` and `ActivateAfterTime` enable a target GameObject after
  a frame count or a delay.

## Editor tools

- Tween inspectors that hide fields covered by an assigned profile or settings
  asset.
- `FindUnusedAudioClips` lists AudioClips not referenced by any scene, prefab or
  container.
- Menu identifier generation that keeps the accessor class and the registry in
  sync as identifier assets are added, moved or deleted.