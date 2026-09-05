# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.6.10 were not recorded.

## [Unreleased]

## [1.6.11] - 2026-09-05

### Fixed

- `StringUtility.NicifyVariableName` no longer returns a leading space for a name that starts
  with an underscore, and collapses a run of underscores into a single word break.

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which references
  the editor assembly now too. The runtime half of this package was well covered while all 28
  editor files were unreachable from any test.
- Tests for `TickProperty`, which every date and duration row resolves through.