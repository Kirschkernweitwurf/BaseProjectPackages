# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.0.11 were not recorded.

## [Unreleased]

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