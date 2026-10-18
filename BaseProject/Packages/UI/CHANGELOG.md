# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.4.2 were not recorded.

## [Unreleased]

### Changed

- Docs pages describe the FPS counter's Is Shown toggle and that closing a confirmation with Back cancels it.

### Added

- `MenuTabButton` selects a tab of a `MenuGroupModule` and turns on an optional indicator while its tab is
  selected.

### Fixed

- Closing a confirmation with Back counts as cancel. Before, the open request never finished and every
  later confirmation was denied as already shown.

## [1.6.0] - 2026-10-17

### Added

- `FpsCounter` has an `Is Shown` checkbox that hides the counter, also while playing, without removing it from
  the scene.

## [1.5.8] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `UI Package.md`, the other root pages are `UI Components.md`, `UI Pages.base` and `UI Tags.md`, so Ctrl+O in Obsidian
  shows what a page is about. Every renamed page has a matching title, and links, the README and the
  documentation URL point to the new names.

## [1.5.7] - 2026-10-14

### Changed

- Every component has its own docs page: what it does, how to set it up, every Inspector field with type and
  default, and related pages.
- `Components.md` and the start page list every component, grouped by what it is for.
- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.5.6] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.5.5] - 2026-10-05

### Fixed

- Docs links work on GitHub. Links write spaces as `%20`, and every docs folder and page is stored with the
  same case as its name, so nothing breaks on case-sensitive systems.

## [1.5.4] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.5.3] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [1.5.2] - 2026-09-06

### Added

- `FpsSamplerTests` and `SceneLoaderTests`. Between them and the version file, that is every
  piece of this package that can be wrong without anyone noticing on screen.
- `ConfirmationRequestTests`, covering what an omitted button label falls back to.
- An edit mode test assembly, `Base.UIPackage.Tests`, covering the version file. Together with the
  billboard play tests, that is the logic in this package that can be wrong without anyone noticing.

### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.
- The frame rate smoothing and the update throttle moved out of `FpsCounter` into
  `FpsSampler`. Both sat inside `Update` reading `Time.unscaledDeltaTime`, so neither could be
  driven by a test. `FpsCounter` keeps the component and the label.
- The button label fallback moved out of `ConfirmationMenu` onto `ConfirmationRequest`. Reading an
  empty label as "use the default" is a decision the request makes, and behind the menu it could
  only be reached by standing up a live menu with buttons and text to draw it with.
- The version file reading, counting and formatting moved out of `BuildVersion` into
  `BuildVersionFile`. All three sat behind a MonoBehaviour pointed at one fixed path, which is why
  none of them could be covered. `BuildVersion` keeps the component and the path.
- Depends on `Base.CorePackage.CameraUtility` instead of `Base.CorePackage`. The only thing this
  package took from that assembly was the `CameraProvider`, and the reference dragged five unrelated
  Core systems along with it. Nothing here changes; a change to any of those five stops recompiling
  this package.

## [1.4.4] - 2026-09-06

### Changed

- References `Base.CorePackage.MenuManaging` and `Base.CorePackage.SceneManagement` alongside
  `Base.CorePackage`, which it still needs for `CameraProvider`. Those two systems moved into
  assemblies of their own in the Core split.

## [1.4.3] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- The package's first test assembly, in play mode, covering `Billboard`.