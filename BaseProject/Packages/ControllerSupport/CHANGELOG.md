# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.6.5 were not recorded.

## [Unreleased]

## [1.7.3] - 2026-09-06

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.
- A stray blank line run in `RumbleServiceTests`.

## [1.7.1] - 2026-09-06

### Changed

- References `Base.CorePackage.MenuManaging` rather than `Base.CorePackage`, which is where the
  menu framework lives after the Core split. The package turned out never to have used anything
  else in Core, so the old reference is gone rather than kept alongside.

## [1.7.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which references
  the editor assembly now too. Its six files could not be named by any test.
- Tests for `NavigationValidator`, covering nested and switched off selectables, the wiring of
  the added element, and the step being safe to run at the head of every rebuild.
- Tests for the rumble setting keys.

### Changed

- The test assembly references `Base.ControllerSupportPackage.Settings`, whose two files could
  not be named by any test before.