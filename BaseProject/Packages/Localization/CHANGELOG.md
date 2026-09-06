# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.2.1 were not recorded.

## [Unreleased]

## [1.3.2] - 2026-09-06

### Added

- `LanguageSettingLocaleTests`, covering which language the component answers with when the stored
  index no longer points at anything. The list lives in the scene and the value on disk does not, so
  the two drift apart the moment a language is added or removed.

### Changed

- `LanguageSetting` names its two serialized fields, so a test can fill in a locale list without
  spelling the field names out.

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.

## [1.3.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- A test assembly, the first this package has had, with an `AssemblyInfo` opening the editor
  assembly's internals to it.
- Tests for the sync guards and `SyncResult`.
- Tests for the language setting key, which nothing else in the code pins.

### Changed

- `GoogleSheetsSync.MissingCollectionMessage` is internal, so a test can name the reason a sync
  was refused instead of repeating the text.
- The test assembly references `Base.LocalizationPackage.Settings`, whose one file could not be
  named by any test before.