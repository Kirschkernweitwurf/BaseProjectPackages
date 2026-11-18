# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). This package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- The package, moved from a game project: localized text styles for TextMeshPro, line balancing, hyphenation,
  readable fonts, text size scaling and the Text Fit editor window.

### Fixed

- `KeepRowInPlaceOnTextChange` no longer throws when the scroll content itself is selected. It counted the
  content as one of its own rows and climbed past the root.