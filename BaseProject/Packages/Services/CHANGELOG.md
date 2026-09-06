# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.0.11 were not recorded.

## [Unreleased]

## [1.1.3] - 2026-09-06

### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.
- Every inherited style value in the service locator window now names `EditorTableStyles` rather
  than reaching it through `ServiceLocatorStyles`.
- Disposing the styles reads as one line. `EditorStyleSet` is an ordinary C# object.
- `ServiceLocatorWindow` is 731 lines instead of 843. The state badges and their tooltips moved into
  `ServiceLocatorBadges`, the tab separated export into `ServiceLocatorReport`, and the sort column,
  its direction and the comparison into `ServiceLocatorSorting`. The same three pieces came out of
  the Event Bus window, which is the same window over a different table, so the two now read alike.

### Fixed

- The service locator column layout no longer computes a remainder nothing reads.
- A tracker test asserts the item is there before reading it, so a regression fails as an
  assertion rather than as a dereference.
- Stray blank line runs, and the comment explaining why `StateBadges` is declared after the three
  badges it holds is back on that array. It had come loose and was sitting alone in the field block,
  the same way it had in the Event Bus window.

## [1.1.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- A play mode test assembly. `GameServiceBehaviour` registers in a Unity callback and `Destroy`
  only takes effect at the end of a frame, neither of which edit mode can reach.
- Tests for `ServiceLocatorColumns`, covering column order, no column running into the next, and
  a squashed window giving the dragged widths back.

### Changed

- The two column width preference keys of `ServiceLocatorColumns` are internal, so a test can
  write known widths and put the machine's own back afterwards.
- The test assembly references the editor assembly, whose seven files could not be named by any
  test before.