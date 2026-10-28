# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `CheatConsoleModel.ReplaceDiscoveredCommands` swaps the discovered commands and keeps the built-in commands and
  the history.

### Fixed

- Scene loads no longer stall for up to 1.5 seconds per scene: `CheatConsoleController` discovered its commands on
  every scene load by reading the methods of every live MonoBehaviour. It now discovers when the console opens
  (and on a scene load only while it is open), and only asks the scene for the types that have commands, found
  once through `TypeCache` in the editor. `CheatCommandRegistry.CreateFromTargets` reads each type once.
- The cheat console history now survives scene loads.

## [1.4.1] - 2026-10-26

### Fixed

- `CheatConsoleController` no longer throws a NullReferenceException at every play start: its Ctrl+W and Ctrl+U
  shortcuts are now built in `Awake` instead of in field initializers, where the Input System is not ready.

## [1.4.0] - 2026-10-25

### Removed

- `DebugMenuController` no longer has the `Cheat Console Tween Group` and `Log Console Tween Group` fields; the new
  tab colors mark the open console.

### Changed

- The debug menu is restyled: a see-through dark card with crisp rounded corners, segmented tabs that show the
  open console, a rounded input with a prompt and a hint, suggestions with each command's description that hide
  while empty, and calmer message colors. The "commands reloaded" line on every scene load is gone.
- Tab in the cheat console completes like a shell: up to the next dot-separated word all matching commands
  share, so `date.a` becomes `date.affection` instead of the first whole command. Where the matches part ways,
  like after `date.`, it fills in the first match's next word.
- Ctrl+W treats a dot as a word of its own, so `date.affection.add` goes back one step at a time:
  `date.affection.`, `date.affection`, `date.`.

## [1.3.1] - 2026-10-25

### Fixed

- Ctrl+U in the cheat console still opened Undo History in the editor: the shortcut context of 1.3.0 does not
  catch main menu shortcuts. While the console input has focus in play mode, the Game view's own option to ignore
  main menu shortcuts is now switched on, and the user's choice comes back afterwards.

## [1.3.0] - 2026-10-25

### Added

- Cheat console: Ctrl+W deletes the word before the caret and Ctrl+U everything before it, like in a shell.
  While the console input has focus in play mode, the editor leaves these keys alone, so Ctrl+U no longer opens
  Undo History over the game.

## [1.2.0] - 2026-10-22

### Added

- Cheat macros: a `CheatMacro` asset in any `Resources/CheatMacros` folder becomes a cheat console command with
  any name, for example a team member's own, that runs a list of console lines in order. Set up in the editor
  through `Create > Scriptable Objects > Base > Debug > New Cheat Macro`, no code needed. The lines it runs stay
  out of the console history, and a macro that calls itself is skipped instead of looping.

## [1.1.6] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `Core Debug Package.md`, the other root pages are `Core Debug Components.md`, `Core Debug Pages.base` and `Core Debug
  Tags.md`, so Ctrl+O in Obsidian shows what a page is about. Every renamed page has a matching title, and
  links, the README and the documentation URL point to the new names.

## [1.1.5] - 2026-10-14

### Fixed

- The tests compile again. The draw buffer tests reach `DebugDrawBuffer` and `DebugLineCommand`, which
  are internal, and the DebugDrawing assembly never opened its internals to the test assembly. It
  does now. The cheat registry tests named `Object` with both `System` and `UnityEngine` in scope,
  which is ambiguous, and now alias it to `UnityEngine.Object`.

## [1.1.4] - 2026-10-14

### Changed

- Every component has its own docs page: what it does, how to set it up, every Inspector field with type and
  default, and related pages.
- `Components.md` and the start page list every component, grouped by what it is for.
- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.1.3] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.1.2] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.1.1] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [1.1.0] - 2026-09-07

### Added

- `DebugDrawBufferTests` and `CheatCommandRegistryTests`. Neither thing they cover throws when it
  goes wrong: a segment that outlives its duration is a line that never leaves the screen, and a
  command that stops being discovered is simply absent from the console.

### Changed

- The three assemblies and every namespace name the package they are in. They were
  `Base.CorePackage.*`, left over from being carved out of `Base Core`, which read as though this
  were still part of that package rather than one built on top of it. Nothing outside referenced
  them by name and the assembly ids are unchanged, so nothing else moves.
- Debug drawing no longer depends on the debug menu. Its cheat commands moved into the cheat console
  they register with, which is where a command belongs, and the drawing now reaches only the logger.
  A project that wants runtime line drawing no longer pulls in a console, TextMeshPro, the Input
  System and the tweening package to get it.

## [1.0.3]
### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.

 - 2026-09-06

### Fixed

- A stray blank line run in `CheatConsoleModelTests`.
- Releasing a service on the way out no longer reports it as missing. `DebugMenuController` used the
  reporting lookup when detaching from the input service, so a scene unload that took that service
  down first turned a clean teardown into an error. It uses the optional lookup now, which is what it
  is for. Attaching is unchanged and still reports.

## [1.0.0] - 2026-09-06

### Added

- Split out of `Base Core`, where it shared a package with fifteen systems it has
  nothing to do with. The namespaces and assembly names are unchanged, so nothing
  that already uses it has to be touched beyond installing this package as well.