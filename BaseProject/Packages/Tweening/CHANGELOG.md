# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.0.11 were not recorded.

## [Unreleased]

## [1.0.14] - 2026-09-06

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.
- Stopping a tween no longer reports a missing runner. `Tween.Stop` runs on shutdown as well, where
  the runner is already gone, and it used the reporting lookup to unregister from it, so every tween
  reported the same thing at once. It uses the optional lookup now. `Play` is unchanged and still
  reports, since a tween without a runner never ticks.

## [1.0.12] - 2026-09-05

### Fixed

- `TweenRunner` resets its static events on entering play mode. With domain reload disabled
  their subscribers survived into the next session and were invoked twice.
- `Tween<T>` reads a `fromGetter` once the delay has elapsed instead of at `Start`, which is what
  the class always documented. With a delay in front of it the old capture read a value the delay
  then made stale, so anything that moved the target while the tween waited was undone by a jump
  back. A literal `from` value still applies at `Start` and holds through the delay.

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- A play mode test assembly, driving tweens through `TweenRunner` over real frames.