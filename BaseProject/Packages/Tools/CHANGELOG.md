# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 2.0.10 were not recorded.

## [Unreleased]

## [2.1.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- Tests for `CommentReader`, the character walk the whole TODO overview is built on.
- Tests for `TodoDateParser`, covering format order on an ambiguous date and the overdue,
  today and future states.
- Tests for `CommandFilter` and `CommandMatcher`, covering the search box markers and the ranking
  order the palette depends on.
- Tests for `TodoPatterns` and `TodoMetadataParser`, covering whole word keyword matching,
  invalid patterns being dropped, and two notations completing one item.

- An XML summary on `MenuManagerWindowBase.MenuPriority`, the one non-override member in
  either repository that carried none.

- `IAssetIndex`, the three questions a scanning tool asks about what is in the project, with
  `AssetDatabaseIndex` answering them from the live one. A tool reading `AssetDatabase` directly
  can only run against the project it runs in, which put its rules out of reach of any test.
- Tests for `FolderConventionScanner`, covering naming style, allowed exceptions, forbidden
  names, ignored folders, required folders, loose assets and the depth limit.
- Tests for `AssetNamingScanner.CollectAssetPaths`, covering folders, paths outside the project,
  packages and scripts going in and out of scope.
- Tests for `OrderCodeGenerator`, covering ordering, XML escaping, identifier cleanup and
  duplicate names.
- Tests for `StaticResetScanner`, covering fields, events, reset methods, the readonly and event
  switches, the ignore marker and the word appearing inside a string.
- Tests for `GuidDismissStore`, covering persistence across instances, the stable write order,
  an unreadable file and the guards on empty input.
- Tests for `LightingProfile`, round tripping every render setting it stores so a line missing
  from `Capture` or `Apply` is named rather than silently dropping one setting.

### Changed

- The test assembly references the runtime assembly, whose four files could not be named by
  any test before.
- `StaticResetScanner.ScanFile` is internal, so the analysis can be pointed at a source string.
  `Scan` still walks the disk and is unchanged.
- `OrderCodeGenerator.BuildCode` takes the namespace, class name and constants as plain values
  instead of an `OrderRegistry`, which is a `ScriptableSingleton` backed by a file in
  `ProjectSettings`. `OrderConstant` gained a constructor so one can be built without Unity
  deserializing it.
- `AssetNamingScanner.CollectAssetPaths` takes an `IAssetIndex`, and `IAssetIndex` gained
  `GetAllAssetPaths` for it. `Scan` and `AssetConventionDetector` pass `AssetDatabaseIndex.Default`,
  so nothing changes in normal use.
- `FolderConventionScanner.Scan` takes an `IAssetIndex` instead of reading `AssetDatabase` itself.
  The window passes `AssetDatabaseIndex.Default`, so nothing changes in normal use.
- `TodoPatterns` compiles from a plain `TodoPatternInput` instead of reading `TodoSettings`
  itself. The settings are a `ScriptableSingleton` backed by a file in `ProjectSettings`, which
  tied pattern compilation to the whole project's state and put it out of reach of any test.