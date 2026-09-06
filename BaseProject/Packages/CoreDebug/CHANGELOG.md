# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2026-09-06

### Fixed

- A stray blank line run in `CheatConsoleModelTests`.
- Releasing a service on the way out no longer reports it as missing. `DebugMenuController` used the
  reporting lookup when detaching from the input service, so a scene unload that took that service
  down first turned a clean teardown into an error. It uses the optional lookup now, which is what it
  is for. Attaching is unchanged and still reports.

## [1.0.0] - 2026-09-06

### Added

- Split out of `Base Core`, where it shared a package with fifteen systems it has
  nothing to do with. The namespaces and assembly names are unchanged, so nothing
  that already uses it has to be touched beyond installing this package as well.