# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.1.3 were not recorded.

## [Unreleased]

## [1.1.6] - 2026-09-06

### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.
- `EditorList` and the window chrome buttons read as one expression rather than three lines. Both
  operands are ordinary C# objects, a delegate and a style set, so the null-conditional operator is
  the real one and not the overload a `UnityEngine.Object` would have brought.

## [1.1.4] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- Tests for `EditorThemeColors.Matches`, including a sweep that fails naming any color the
  comparison forgot, so a color added later cannot quietly go unchecked.