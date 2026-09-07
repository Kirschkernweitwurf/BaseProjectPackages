# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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