# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.1.3 were not recorded.

## [Unreleased]

## [1.2.0] - 2026-09-07

### Added

- Three presets, for eight in total, drawn as two rows of four. They are named after the pigment or
  stone each is built on, so a name says what it looks like: Slate, Onyx, Graphite, Verdigris, Cobalt,
  Quartz, Amber and Malachite.
- `EditorRowsSortArrowTests`, `TheStatusColorsDifferInLightness` and
  `SecondaryTextIsAStepBelowBodyText`, pinning the three guarantees below.

### Changed

- A preset varies on four axes rather than one. How much hue is in the walls, from none in Graphite
  to a quarter in Cobalt and Amber. How dark those walls are, with Amber the darkest by a clear step.
  How warm the text is, since a warm theme with grey text does not read as warm. And how it is built,
  with Onyx squaring its corners and thickening its hairlines while Amber softens both. An accent
  swapped over a shared grey is not a theme.
- The three status colors are staggered in lightness rather than all fitted to one contrast target.
  Three colors at the same level are the same brightness by construction, so the moment hue is lost
  they collapse into one, which is what a greyscale screenshot or a reader who sees no color gets.
- The accent is pushed away from the window until a label can sit on it. A mid orange is the case
  that failed: neither black nor white cleared the floor on it, so a primary button had no readable
  text.
- The preset grid wraps into rows rather than assuming everything fits on one line.

### Fixed

- The theme preview insets its rows on all four sides. The card behind them was padded at the
  top and bottom only, so it showed above and below the list and nowhere else, which reads as a
  stray rounded rectangle rather than as the card the rows sit on.
- The sort arrow sits where the row centres it. A glyph is placed by its font box, and the box
  reserves room under the baseline for descenders a triangle does not have, so centring the box
  left the ink high.
- The marker on the sorted column is a shape again. It was stacked one pixel rectangles, which only
  stay one pixel while the editor draws on whole pixels: at 125 or 150 percent display scaling each
  row lands between two of them and comes out at half brightness, so the arrow read as a smudge. It
  is a glyph now, because text is the one thing here already resolved against the real pixel grid.
- Verdigris keeps its three states apart under simulated deuteranopia. As Harbor it measured a
  smaller difference than the eye can rely on, which is the one thing that preset exists to promise.
- Onyx and Amber give body and secondary text different colors. They were identical, so two of the
  presets had no text hierarchy.

## [1.1.6] - 2026-09-06

### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.
- `EditorList` and the window chrome buttons read as one expression rather than three lines. Both
  operands are ordinary C# objects, a delegate and a style set, so the null-conditional operator is
  the real one and not the overload a `UnityEngine.Object` would have brought.

## [1.1.4] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- Tests for `EditorThemeColors.Matches`, including a sweep that fails naming any color the
  comparison forgot, so a color added later cannot quietly go unchecked.