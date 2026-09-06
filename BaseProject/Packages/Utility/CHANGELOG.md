# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.6.10 were not recorded.

## [Unreleased]

## [1.6.13]
### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.

 - 2026-09-06

### Fixed

- A missing `param` tag on `SerializableListCache`.
- A coroutine that never yields no longer leaks its handle. Unity runs a coroutine up to its first
  yield inside `StartCoroutine`, so one that finishes straight away ran its tracker before the handle
  existed. The tracker removed a handle that was still null, the real one was added afterwards and
  nothing ever took it out, so the set grew for the lifetime of the runner.

## [1.6.11] - 2026-09-05

### Fixed

- `StringUtility.NicifyVariableName` no longer returns a leading space for a name that starts
  with an underscore, and collapses a run of underscores into a single word break.

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which references
  the editor assembly now too. The runtime half of this package was well covered while all 28
  editor files were unreachable from any test.
- Tests for `TickProperty`, which every date and duration row resolves through.