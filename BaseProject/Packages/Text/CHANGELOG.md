# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/). This package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- The package, moved from a game project: localized text styles for TextMeshPro, line balancing, hyphenation,
  readable fonts, text size scaling and the Text Fit editor window.
- `ELineFit.Fill` for chat bubbles: the lines stay as they break, each full before the next starts.
- `WidenWithTextSize` **Is Fit To Longest Line**: a wrapped short text ends its box at its longest line, so a chat
  bubble is only as wide as its words.

### Fixed

- `KeepRowInPlaceOnTextChange` no longer throws when the scroll content itself is selected. It counted the
  content as one of its own rows and climbed past the root.
- A balanced text box that gets a shorter text, like a one line title after a two line one, no longer keeps
  the height of the old lines. The layout measured it at the old, narrowed width and left an empty line under it.