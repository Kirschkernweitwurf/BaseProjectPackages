# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.0.5 were not recorded.

## [Unreleased]

## [1.0.7] - 2026-09-06

### Changed

- The README lists `Base Audio` and `Base Core Debug` as requirements. The audio containers
  and the cheat and log console prefabs in this package reference components that moved out of
  Core, so installing without them leaves those prefabs with missing scripts.

## [1.0.6] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.