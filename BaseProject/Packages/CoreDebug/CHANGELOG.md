# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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