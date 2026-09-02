# Base Core

Reusable core systems that any Unity project can build on: menus, audio, scene loading, input, timers, state machines, object pooling, randomization and debug tooling. Service location and tweening live one layer down, in the Base Service and Base Tweening packages.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0` and `com.unity.ugui` `2.0.0`
- `Base.ServicesPackage` for `ServiceLocator`, `GameServiceBehaviour`, the shutdown pipeline and the priority trackers every system here builds on
- `Base.TweeningPackage` for the menu open and close animations, the debug menu and `TweenGroupObjectPool`
- `Base.UtilityPackage` for logging and shared helpers
- `Base.AttributesPackage` for inspector attributes such as `[Required]` and `[GetComponent]`
- `Base.EditorUIPackage.Editor` for the shared look of its editor windows
- Assemblies: `Base.CorePackage`, `Base.CorePackage.Editor` and `Base.CorePackage.Tests`

The manager prefabs, canvases and menu identifier assets these systems expect ship in `Base.ContentPackage`.

## Systems

### Event Bus

A strongly typed in-process publish and subscribe bus.

- `IEventBus` and `EventBus` dispatch events to every subscriber in subscription order.
- `IEvent` marks a payload. Implement it on a `readonly struct` to stay allocation-free.
- `Subscription<TEvent>` is a disposable token. Dispose it to unsubscribe.

`EventBus` drops every handler on destroy, so no subscription survives a scene change by accident.

### State Machine

A finite state machine over an arbitrary context object. The machine does not tick itself: drive it from wherever the owning object updates, so its rate and its time scale stay under the caller's control.

- `IState<TContext>` is one state. The context is handed to every call rather than captured, so one state instance can be shared between machines running over different contexts.
- `StateBase<TContext>` is the convenience base where every hook is optional, and `DelegateState<TContext>` assembles a state from delegates for the many states that are a few lines long.
- `StateTransition<TContext>` is one edge: a target state plus the condition that has to hold.
- `StateChange<TContext>` is raised by `StateChanged` on every switch.
- `StateMachineRegistry` keeps weak references to the machines currently running, which is the only thing the monitor window reads.

### Menu Managing

- `Menu` is the base class for all menus. It handles the lifecycle and the open and close animations.
- `MenuManager` registers menus and controls opening and closing.
- `MenuModule` components add single concerns on top of a menu: `MenuCursorModule`, `MenuTimeScaleModule`, `MenuInputMapModule` and `MenuResetModule`. Each scoped effect is applied at the menu's priority and removed again on close, so overlapping menus resolve instead of fighting.
- `MenuIdentifier` assets identify menus. A generated accessor class and a runtime `MenuIdentifierRegistry` resolve them by name, so menus are never opened by string.
- `PauseMenu` is a ready-to-use example.

### Scene Management

- `SceneLoadingManager` loads and unloads scenes with a persistent scene that stays loaded, using `Awaitable` for play-mode-safe async work.
- `SceneLoadEvents` broadcasts progress and activity, and `LoadingScreen` reacts to those to show a loading UI.

### Audio

- `AudioManager` owns the play, stop and fade API.
- `AudioContainer` is a ScriptableObject holding clips and their playback settings.
- Pooled audio sources per `EAudioType` keep playback allocation-light, and `AudioFader` tweens source volume.
- `PlayAudioOnClick`, `PlayAudioOnHover`, `PlayAudioOnSelect` and `PlayAudioOnSubmit` play a container from UI events.

### Input

- `InputManager` registers action maps with a priority and enables the highest-priority one while disabling the rest.
- `PrioritizedInputMap` bundles a map with its `EPriority`.
- `InputActionMapReference` serializes a reference to a map inside an asset by GUID, so it survives renames, and draws as an asset field plus a map dropdown.
- `ProjectInputServiceBase` is the base of the project's own input service: reference counted map enabling and resolving an `InputActionMapReference` against the runtime action asset. The generated actions wrapper lives in the project, which is what the Base Package Installer's project setup creates.
- `BaseInputActions` is the generated wrapper for this package's own input asset, covering the Permanent, UI and Cheats maps.

### Object Pooling

- `BaseObjectPoolManager<TAsset, TPool>` is a base for global pool managers.
- `HashSetObjectPool<T>`, from the Utility package, is the constant-time pool both of these build on.
- `TweenGroupObjectPool<T>` caches the `TweenGroup` of every instance and plays its enter and exit animation on activation and deactivation.

### Priority Trackers

`CursorManager` and `TimeScaleManager` resolve cursor state and timescale from competing priority requests, falling back to a serialized default while nothing is requested.

### Tooltip

`TooltipService` shows the highest-priority requested tooltip, `TooltipTrigger` requests one while its GameObject is hovered, and `TooltipView` keeps it next to the cursor without letting it leave the screen.

### Camera and raycasting

`CameraProvider` caches `Camera.main` to avoid repeated tag lookups and handles Unity's fake-null case. `RaycastUtility` provides generic, type-safe 2D ray-casting with editor-only debug ray drawing.

### Noise

- `NoiseSettings` is a serializable pattern: shaping mode, frequency, octaves, lacunarity, persistence, amplitude and a seed. `Evaluate` samples it along one axis, on a plane or in space.
- Perlin noise has no seed of its own, so the seed is turned into an offset into the noise field. Changing it at runtime through `SetSeed` takes effect on the next sample.
- `ENoiseType` picks the character: `Perlin` for rolling hills, `Ridged` for mountain crests, `Turbulence` for smoke and marble. All three stay inside the same output range, so switching changes the character and not the scale.
- `NoiseUtility.CreateMap` fills a whole grid at once for height maps and spawn masks. `Perlin3D` builds three dimensional noise out of Unity's two dimensional generator, at the cost of some contrast.

### Debug Draw

`DebugDraw` draws lines, rays, arrows, boxes, wire spheres and world space text labels that also show up in a player, unlike gizmos and `Debug.DrawLine`. Lines render through GL after every game and scene view camera, so the built-in pipeline as well as URP and HDRP are covered; labels are drawn as screen space IMGUI text.

Every call is compiled out of a release build, arguments included. Define `BASE_DEBUG_DRAW` to keep them. A duration of zero draws for one frame, anything longer counts in unscaled seconds, and `debugdraw_clear` and `debugdraw_enabled` control it from the cheat console.

### Debug Menu

- `DebugMenuController` hosts a cheat console and a log console, toggled by input, remembering which one was open last.
- The cheat console discovers `[CheatCommand]` methods through `CheatCommandRegistry`, from assemblies and from scene objects. `BuiltinCheatCommands` ships a default set.
- `LogConsoleView` mirrors Unity's log stream, `CustomLogger` output included. Capturing starts before the first scene loads, so every log is buffered even while the menu is closed.

### Screenshots and activation

`ScreenshotManager` takes and stores screenshots on input. `ActivateAfterFrames` and `ActivateAfterTime` enable a target GameObject after a frame count or a delay.

## Editor tools

### State Machine Monitor

Watches the `StateMachine<TContext>` instances running in play mode: the live machines on the left, the selected one drawn in the middle as boxes and curves with the current state highlighted, and underneath it what the drawing cannot carry, which is where the machine started, how long it has been where it is, and which transitions are being evaluated right now in the order they are asked.

Machines are arranged into columns by distance from the start state, so a machine reads left to right in the order it can actually run, and states nothing can reach end up in a trailing column where they are visible as exactly that.

### Event Bus window

A live view of an event bus: every event type it currently holds handlers for and who is subscribed to each, in the order the bus would invoke them. It re-reads the bus on a timer, so a subscription appearing or going stale shows up on its own. It resolves who is behind a delegate, including lambdas, which are not compiled into the type that reads as their owner.

### Other

- **Find Unused Audio Clips** lists AudioClips not referenced by any scene, prefab or `AudioContainer`, and reports empty clip slots inside containers.
- **Menu identifier generation** keeps the accessor class and the runtime registry in sync as identifier assets are added, moved or deleted. Deletion is handled separately from the other two, because by the time an `AssetPostprocessor` runs the asset is gone and its type can no longer be resolved.

## Tests

`Base.CorePackage.Tests` covers noise, state machine lifecycle and transitions, and the randomization and weighted table helpers that now live in the Utility package. See `Tests/README.md` for how to make them appear in the Test Runner.