# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.6.5 were not recorded.

## [Unreleased]

### Added

- `ISteeringSource`: a game can tell the `FocusWatchdog` that the player steers with the keyboard, so keyboard
  players keep a focus like gamepad players do.
- `NavigableGroup` Reach Off Axis: a direction with nothing lined up takes the nearest element on that side, for a
  row of cards above a single button.
- `NavigableGroup.IsFocusHeld` keeps a group's focus empty while it stays the focus owner, for a screen that waits for
  the first input.

### Changed

- `FocusWatchdog` keeps the selection off covered screens: a selection inside a group behind the top group moves to
  the top group while steering and is cleared while pointing. A group that deactivates with the selection inside
  hands it on at once.
- `MenuNavigationModule` deactivates its group when the menu starts to close, so the focus moves on while the menu
  fades out.

### Fixed

- A group with Rebuild On Activate rewires itself whenever one of its elements starts or stops taking the focus.
  The wiring only links elements that take the focus when it runs, so a card that turns clickable once its intro
  landed, or a menu that ignores presses while it fades in, stayed unreachable for the gamepad and the keyboard.
- A menu opened with the mouse no longer leaves the keyboard selection on the button behind it, where Enter or the
  arrows acted on the covered screen.

## [1.9.0] - 2026-10-24

### Added

- `InputPromptView.IsAvailable` hides a prompt while its action does nothing, so a prompt bar can follow the
  game's state, like showing Put back instead of Pause while a card is held.
- `InputDeviceTracker` remembers the last device across sessions. A gamepad player who starts the game again sees
  gamepad prompts from the first frame, as long as a gamepad is plugged in.

### Fixed

- `InputGlyphProvider` no longer warns about a missing glyph set for device `Unknown` before the first input.

## [1.8.0] - 2026-10-23

### Added

- `InputPromptView` shows the button icon of one action on an Image and follows device switches. Shown On picks the
  devices it shows on: always, gamepad only, or mouse and keyboard only.
- `InputDeviceTracker.CurrentGamepadFamily` tells the kind of gamepad pressed last (Xbox, PlayStation, Switch,
  Steam Deck, Steam Controller or generic), detected from its names by the new `GamepadFamilyDetector`. Picking up
  another gamepad raises `OnDeviceChanged` too.
- `InputGlyphSet.GamepadFamily`, so each kind of gamepad can have its own glyphs. `InputGlyphProvider` takes the
  gamepad's own set first, then the `Generic` set.
- `NavigableGroup` Rebuild On Activate rewires a group each time it activates, for elements spawned at runtime.
- `NavigationBuilder.Wire` takes `reachOffAxis`: a direction with nothing lined up takes the nearest element on that
  side, for loose layouts like a hand of cards below a few items.

### Changed

- `NavigableGroup` Default Element is optional. Without it, or while it cannot take focus, the first navigable
  element below the group gets focus.
- `NavigableGroup` only warns about having nothing to focus once the gap lasts longer than a second, so menus that
  fade in or sit behind a lock screen stay quiet.

### Fixed

- `NavigationBuilder` measures elements on a world space canvas or on a 3D object on screen, through their camera,
  so up on the stick is up on the screen.

## [1.7.14] - 2026-10-17

### Changed

- Docs pages show the menu paths of the reorganized Base Packages menus.

## [1.7.13] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `Controller Support Package.md`, the other root pages are `Controller Support Components.md`, `Controller Support
  Pages.base`, `Controller Support Tags.md` and `Controller Support Tools.md`, so Ctrl+O in Obsidian shows what
  a page is about. Every renamed page has a matching title, and links, the README and the documentation URL
  point to the new names.

## [1.7.12] - 2026-10-14

### Changed

- Every component has its own docs page: what it does, how to set it up, every Inspector field with type and
  default, and related pages.
- `Components.md` and the start page list every component, grouped by what it is for.
- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [1.7.11] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [1.7.10] - 2026-10-05

### Fixed

- Docs links work on GitHub. Links write spaces as `%20`, and every docs folder and page is stored with the
  same case as its name, so nothing breaks on case-sensitive systems.

## [1.7.9] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [1.7.8] - 2026-10-05

### Fixed

- Persistent menus (pause, confirmation, debug) now get focus recovery. A navigable group looks up the
  focus watchdog when it activates instead of once in Awake, so it finds the watchdog of the current scene
  even when it woke up before that scene existed.
- No more "FocusWatchdog is not registered" error at startup. The focus watchdog and the input device
  tracker are optional, so a missing one is no longer logged as an error.

## [1.7.7] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [1.7.6] - 2026-09-07

### Changed

- The minimum badge width is private. Only the style class it sits in ever read it.

## [1.7.4]
### Changed

- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.

 - 2026-09-06

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.
- A stray blank line run in `RumbleServiceTests`.

## [1.7.1] - 2026-09-06

### Changed

- References `Base.CorePackage.MenuManaging` rather than `Base.CorePackage`, which is where the
  menu framework lives after the Core split. The package turned out never to have used anything
  else in Core, so the old reference is gone rather than kept alongside.

## [1.7.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which references
  the editor assembly now too. Its six files could not be named by any test.
- Tests for `NavigationValidator`, covering nested and switched off selectables, the wiring of
  the added element, and the step being safe to run at the head of every rebuild.
- Tests for the rumble setting keys.

### Changed

- The test assembly references `Base.ControllerSupportPackage.Settings`, whose two files could
  not be named by any test before.