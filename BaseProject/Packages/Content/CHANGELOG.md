# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.0.5 were not recorded.

## [Unreleased]

## [1.1.0] - 2026-10-25

### Added

- `S_RoundedRect`: a white 128 px rounded rectangle sprite with 48 px slice borders, sharp at any corner radius
  through the Image's pixels per unit multiplier.

### Changed

- The debug menu prefabs are restyled to match CoreDebug 1.4: a see-through dark card, segmented tabs, a rounded
  input with a prompt, a suggestion box that hides while empty and a smaller Clear button. Needs CoreDebug 1.4.0.

## [1.0.12] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `Content Package.md`, the other root pages are `Content Pages.base` and `Content Tags.md`, so Ctrl+O in Obsidian
  shows what a page is about. Every renamed page has a matching title, and links, the README and the
  documentation URL point to the new names.

## [1.0.11] - 2026-10-14

### Changed

- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.0.10] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.0.9] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.0.8] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [1.0.7] - 2026-09-06

### Changed

- The README lists `Base Audio` and `Base Core Debug` as requirements. The audio containers
  and the cheat and log console prefabs in this package reference components that moved out of
  Core, so installing without them leaves those prefabs with missing scripts.

## [1.0.6] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.