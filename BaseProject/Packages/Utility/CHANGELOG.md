# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.6.10 were not recorded.

## [Unreleased]

## [1.8.2] - 2026-10-14

### Fixed

- Documentation Sync writes `/Docs/Packages` to `ignore.conf`. Without the leading slash Plastic listed the
  folder as private instead of ignoring it, so it could get checked in and cause merge conflicts. An existing
  line without the slash is fixed on the next sync.

## [1.8.1] - 2026-10-14

### Changed

- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.8.0] - 2026-10-13

### Added

- Documentation Sync. In a project with a `Docs/index.md`, the docs of every installed Base package are
  copied to `Docs/Packages`, so one Markdown vault on `Docs` (for example in Obsidian) shows the project
  docs and the package docs together. Runs on editor start, whenever packages change and from the new
  **Sync** button in `Help > Base Packages Documentation`. GitHub links between installed packages become
  local links, removed packages disappear, `Docs/Packages/index.md` lists every package, and
  `Docs/Packages` is added to the project's Plastic `ignore.conf`.

## [1.7.3] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.7.2] - 2026-10-05

### Fixed

- Docs links work on GitHub. Links write spaces as `%20`, and every docs folder and page is stored with the
  same case as its name, so nothing breaks on case-sensitive systems.

## [1.7.1] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.7.0] - 2026-10-05

### Added

- `Help > Base Packages Documentation`: a window listing every installed package that ships a
  `Documentation~/index.md`, plus the project's own `Docs/index.md`, and opening them in the default
  Markdown app. Plain `MenuItem`, so it works without the Tools package.
- Team documentation in `Documentation~`, moved over from the old wiki.
- `documentationUrl` now points at the team docs.

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