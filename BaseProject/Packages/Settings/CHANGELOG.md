# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 2.3.3 were not recorded.

## [Unreleased]

## [2.3.6] - 2026-09-06

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.

## [2.3.4] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.