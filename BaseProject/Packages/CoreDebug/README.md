# Base Core Debug

The in-game debug tooling of the Base packages. Split out of `Base Core` because it is meant
to be absent from a shipping build, which is easier to guarantee by not installing it than by
stripping it.

**Team docs:** how to use everything in the editor is in [`Documentation~/Core Debug Package.md`](Documentation~/Core%20Debug%20Package.md). In Unity, open `Help > Base Packages Documentation`.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0` and `com.unity.ugui` `2.0.0`
- `Base Core` for the menu framework and the input service the debug menu is toggled by
- `Base.ServicesPackage` for `ServiceLocator` and the shutdown pipeline
- `Base.TweeningPackage` for the menu open and close animations
- `Base.UtilityPackage` for logging and the pooling helpers
- `Base.AttributesPackage` for inspector attributes such as `[Required]`
- Assemblies: `Base.CoreDebugPackage.DebugMenu`, `Base.CoreDebugPackage.DebugDrawing` and
  `Base.CoreDebugPackage.Tests`

The debug menu, cheat console and log console prefabs ship in `Base.ContentPackage`.

## Systems

### Debug Menu

- `DebugMenuController` hosts a cheat console and a log console, toggled by input, remembering
  which one was open last.
- The cheat console discovers `[CheatCommand]` methods through `CheatCommandRegistry`, from
  assemblies and from scene objects. `BuiltinCheatCommands` ships a default set.
- `CheatMacro` assets in any `Resources/CheatMacros` folder become console commands with a name
  picked in the editor. Each runs its console lines in order through the model, without adding
  them to the history, and a macro that calls itself is skipped.
- `autorun.add <command>` makes a command run on its own as soon as the console finds it, for
  this machine only (PlayerPrefs, not version control). A macro waits until every command its
  lines call is found, and runs again whenever a scene load brings new objects for them.
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

`Base.CoreDebugPackage.DebugMenu` and `Base.CoreDebugPackage.DebugDrawing`, matching the package
they are in. They kept the `Base.CorePackage` names for a while after being carved out of
`Base Core`, which read as though this were still part of that package rather than one built on
top of it.