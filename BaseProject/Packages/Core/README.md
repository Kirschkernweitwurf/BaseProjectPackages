# Base Core Package

Reusable core systems that any Unity project can build on. The package bundles
scene loading, audio, input, menus, timers, object pooling and debug tooling
under the `Base.CorePackage` namespace. Service location and tweening live one
layer down, in the Base Service and Base Tweening packages.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0`

### Related Base packages

The Core package uses a few sibling packages. Install them alongside it:

- `Base.ServicePackage` for the `ServiceLocator`, `GameServiceBehaviour`, the shutdown
  pipeline and the priority trackers every system here builds on
- `Base.TweeningPackage` for the menu open and close animations, the debug menu and
  `TweenGroupObjectPool`
- `Base.UtilityPackage` for logging and shared helpers
- `Base.AttributePackage` for inspector attributes such as `[Required]` and `[GetComponent]`

## Systems

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

### Priority Trackers

- `CursorManager` and `TimeScaleManager` resolve cursor state and timescale from
  competing priority requests, on top of the `PriorityTracker` in the Service
  package.

### Tooltip

- `TooltipService` shows the highest-priority tooltip, backed by a
  `PriorityTracker`.
- `TooltipTrigger` requests a tooltip while its GameObject is hovered.
- `TooltipView` positions the tooltip so it never leaves the screen.

### Camera Utility

- `CameraProvider` caches `Camera.main` and handles Unity's fake-null case.

### Randomization

- `IRandomSource` is the seam every helper is written against. A source only has
  to supply raw bits; ranges, chances, shuffles and point pickers come from
  `RandomSourceExtensions`.
- `SeededRandom` is a reproducible generator. The same seed always replays the
  same sequence, `Reset` rewinds it and `State` plus `Restore` save and continue
  a run in progress. It is not affected by anything else drawing a number,
  unlike Unity's single global sequence.
- `UnityRandomSource.Shared` runs the same helpers on Unity's global generator
  for cases that do not need a seed.
- `RandomSourceExtensions` covers `Range`, `Chance`, `NextBool`, `NextSign`,
  `NextGaussian`, `Pick`, `Shuffle`, `OnUnitCircle`, `InsideUnitCircle`,
  `OnUnitSphere` and `InsideUnitSphere`. Integer ranges use rejection sampling,
  so no outcome is favored by the range not dividing evenly.
- `WeightedEntry<T>` is a serializable item and weight pair, so a weighted list
  is authored in the inspector as `List<WeightedEntry<AudioClip>>`.
- `WeightedTable<T>` draws from those weights in one random value and a binary
  search. `WeightedTable<T>.TryDrawFrom` draws straight from a list for a one
  off pick. A weight of zero switches a row off without deleting it.

### Noise

- `NoiseSettings` is a serializable pattern: shaping mode, frequency, octaves,
  lacunarity, persistence, amplitude and a seed. `Evaluate` samples it along one
  axis, on a plane or in space.
- Perlin noise has no seed of its own, so the seed is turned into an offset into
  the noise field. Changing the seed at runtime through `SetSeed` takes effect
  on the next sample.
- `ENoiseType` picks the character: `Perlin` for rolling hills, `Ridged` for
  mountain crests, `Turbulence` for smoke and marble. All three stay inside the
  same output range.
- `NoiseUtility.CreateMap` fills a whole grid at once for height maps and spawn
  masks. `NoiseUtility.Perlin3D` builds three dimensional noise out of Unity's
  two dimensional generator, at the cost of some contrast.

### Debug Draw

- `DebugDraw` draws lines, rays, arrows, boxes, wire spheres and world space
  text labels that also show up in a player, unlike gizmos and
  `Debug.DrawLine`.
- Lines render through GL after every game and scene view camera, so the
  built-in pipeline as well as URP and HDRP are covered. Labels are drawn as
  screen space IMGUI text.
- Every call is compiled out of a release build, arguments included. Define
  `BASE_DEBUG_DRAW` to keep them.
- A duration of zero draws for one frame, anything longer counts in unscaled
  seconds. `debugdraw_clear` and `debugdraw_enabled` control it from the cheat
  console.

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