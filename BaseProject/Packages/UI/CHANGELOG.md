# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.4.2 were not recorded.

## [Unreleased]

## [1.5.0] - 2026-09-06

### Added

- An edit mode test assembly, `Base.UIPackage.Tests`, covering the version file. Together with the
  billboard play tests, that is the logic in this package that can be wrong without anyone noticing.

### Changed

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