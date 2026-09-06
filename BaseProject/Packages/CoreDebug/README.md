# Base Core Debug

The in-game debug tooling of the Base packages. Split out of `Base Core` because it is meant
to be absent from a shipping build, which is easier to guarantee by not installing it than by
stripping it.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0` and `com.unity.ugui` `2.0.0`
- `Base Core` for the menu framework and the input service the debug menu is toggled by
- `Base.ServicesPackage` for `ServiceLocator` and the shutdown pipeline
- `Base.TweeningPackage` for the menu open and close animations
- `Base.UtilityPackage` for logging and the pooling helpers
- `Base.AttributesPackage` for inspector attributes such as `[Required]`
- Assemblies: `Base.CorePackage.DebugMenu`, `Base.CorePackage.DebugDrawing` and
  `Base.CorePackage.Debug.Tests`

The debug menu, cheat console and log console prefabs ship in `Base.ContentPackage`.

## Systems

### Debug Menu

- `DebugMenuController` hosts a cheat console and a log console, toggled by input, remembering
  which one was open last.
- The cheat console discovers `[CheatCommand]` methods through `CheatCommandRegistry`, from
  assemblies and from scene objects. `BuiltinCheatCommands` ships a default set.
- `LogConsoleView` mirrors Unity's log stream, `CustomLogger` output included. Capturing starts
  before the first scene loads, so every log is buffered even while the menu is closed.

### Debug Draw

`DebugDraw` draws lines, rays, arrows, boxes, wire spheres and world space text labels that also
show up in a player, unlike gizmos and `Debug.DrawLine`. Lines render through GL after every game
and scene view camera, so the built-in pipeline as well as URP and HDRP are covered; labels are
drawn as screen space IMGUI text.

Every call is compiled out of a release build, arguments included. Define `BASE_DEBUG_DRAW` to
keep them. A duration of zero draws for one frame, anything longer counts in unscaled seconds,
and `debugdraw_clear` and `debugdraw_enabled` control it from the cheat console.

## Namespaces

The types keep the namespaces they had inside `Base Core`, so moving to this package changes
nothing in code that already uses them. `Base.CorePackage.DebugMenu` and
`Base.CorePackage.DebugDrawing`.