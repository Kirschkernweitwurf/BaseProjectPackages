# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.3.10 were not recorded.

## [Unreleased]

## [1.4.0] - 2026-09-05

### Fixed

- `SaveCodec` now throws `ArgumentNullException` for a null read encryptor list instead of
  failing later with a `NullReferenceException`, matching how its other two arguments behave.

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- Tests for the autosave setting keys, including that the three settings do not share one.

### Changed

- The test assembly references `Base.SaveSystemPackage.Settings`, whose three files could not
  be named by any test before.