# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `AudioContainer` has an **Is Placeholder** toggle for sounds that are hooked up before their clips exist. An
  empty placeholder plays nothing and logs nothing, and the Clips field no longer reports it as empty. The
  inspector warns when a placeholder already has clips.

### Fixed

- The PlayAudioOn components no longer throw when their element gets an event in the frame it turns on, like
  the first row of a page that is selected right away. They look the audio manager up on the first sound through
  the new shared base `PlayAudioOnEvent`.
- A new `AudioContainer` starts with an empty clip array instead of none, so an empty container reports
  itself as empty instead of null.

## [1.1.11] - 2026-10-17

### Changed

- Docs pages show the menu paths of the reorganized Base Packages menus.

## [1.1.10] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `Audio Package.md`, the other root pages are `Audio Components.md`, `Audio Pages.base` and `Audio Tags.md`, so Ctrl+O
  in Obsidian shows what a page is about. Every renamed page has a matching title, and links, the README and the
  documentation URL point to the new names.

## [1.1.9] - 2026-10-14

### Changed

- Every component has its own docs page: what it does, how to set it up, every Inspector field with type and
  default, and related pages. The grouped pages were split up; what is left of them is a short guide.
- `Components.md` and the start page list every component, grouped by what it is for.
- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.1.8] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.1.7] - 2026-10-05

### Fixed

- Docs links work on GitHub. Links write spaces as `%20`, and every docs folder and page is stored with the
  same case as its name, so nothing breaks on case-sensitive systems.

## [1.1.6] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.1.5] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [1.1.4] - 2026-09-07

### Removed

- Three serialized field names nothing reads, not even the tests they were written for: the two
  randomized pitch bounds on the manager and the scene load clearing flag on the pool. The rest are
  read by the play tests and stay.

## [1.1.2]
### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.

 - 2026-09-06

### Added

- An edit mode test assembly, `Base.AudioPackage.Tests`, covering `ActiveSounds`, `AudioContainer`,
  `AudioFader`, `AudioPool` and `AudioSourceConfigurator`.
- A play mode test assembly, `Base.AudioPackage.PlayTests`, covering `AudioManager` and
  `AudioPoolManager`. Both build their state in `Awake` and both release sources over frames, so
  neither is reachable from edit mode. Between the two assemblies this is the first coverage the
  package has had.
- `InternalsVisibleTo` for both test assemblies, so the tracking table, the pool wrapper and the
  source configurator stay internal instead of being widened to be reachable.
- Constants on `AudioManager` and `AudioPoolManager` naming their serialized fields, so a test can
  wire a manager before it is switched on without spelling the field names out.

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.

## [1.0.0] - 2026-09-06

### Added

- Split out of `Base Core`, where it shared a package with fifteen systems it has nothing to do
  with. It imports nothing from Core, which is why it carries no `Core` in its name and why a
  project can install it without installing Core at all.
- The namespaces are `Base.AudioPackage`, `Base.AudioPackage.OnEvent`, `Base.AudioPackage.Pool`
  and `Base.AudioPackage.Editor`, replacing the `Base.CorePackage.Audio` ones they had while the
  code lived in Core.