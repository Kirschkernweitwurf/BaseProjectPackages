# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 2.1.4 were not recorded.

## [Unreleased]

### Added

- `MenuGroupModule` shows several menus inside its menu as tabs, one at a time. Opening a tab closes the
  others, the old tab finishes closing before the new one opens, and Back can return to a default tab first.
  Groups can be nested, and only the innermost open group reacts to the optional next and previous tab actions.
- `Menu.IsTransitioning` is public, and the new `Menu.IsClosing` tells a closing menu from an opening one.
- Tooltips show several panels at once, each with an optional icon, title and text, through `TooltipEntry`
  and the new `TooltipPanel`. Texts grow up to a maximum width and wrap after that.
- `ITooltipProvider` lets components add tooltip panels built at hover time, and `TooltipTrigger.Refresh`
  rebuilds a visible tooltip. `TooltipTrigger` has an optional title.
- `TooltipTrigger.Placement` puts the tooltip beside the hovered object instead of the cursor, for UI elements
  and 3D objects. Follow Cursor falls back to it without a mouse.
- `TooltipView` waits a short delay and fades in, but shows the next tooltip at once when one just hid.

### Changed

- BREAKING: `TooltipView` draws `TooltipPanel`s instead of one text. Replace its Text Element with a Panel
  Template, and read `TooltipData.Entries` instead of the removed `TooltipData.Message`.
- BREAKING: `TooltipData.GetScreenPosition` is now `TooltipData.GetScreenAnchor`, a screen rect. The
  constructors that take a position still work.

### Fixed

- A tooltip never takes the pointer, so it can no longer flicker by covering the object it explains.
- Dragging something across a `TooltipTrigger` no longer shows its tooltip.

## [4.0.12] - 2026-10-17

### Changed

- Docs pages show the menu paths of the reorganized Base Packages menus.

## [4.0.11] - 2026-10-15

### Changed

- Regenerate Menu Identifiers always logs its result, also when nothing had to change, so a manual run
  gives visible feedback.

## [4.0.10] - 2026-10-15

### Changed

- Docs pages have descriptive file names that are unique across a vault with every package. The start page is
  `Core Package.md`, the other root pages are `Core Components.md`, `Core Pages.base`, `Core Scriptable Objects.md`,
  `Core Tags.md` and `Core Tools.md`, so Ctrl+O in Obsidian shows what a page is about. Every renamed page has a
  matching title, and links, the README and the documentation URL point to the new names.

## [4.0.9] - 2026-10-14

### Changed

- Every component has its own docs page: what it does, how to set it up, every Inspector field with type and
  default, and related pages. The grouped pages were split up; what is left of them is a short guide.
- `Components.md` and the start page list every component, grouped by what it is for.
- Every docs page has properties (type, package, audience, tags, summary) for Obsidian, and `Pages.base` shows
  them as a sortable table. `Tags.md` is regenerated from them.

## [4.0.8] - 2026-10-05

### Added

- Tags in the docs. Every page has a **Tags** line saying who needs it and what it is about, and
  `Tags.md` lists every page of the package by tag.

## [4.0.7] - 2026-10-05

### Fixed

- Docs links work on GitHub. Links write spaces as `%20`, and every docs folder and page is stored with the
  same case as its name, so nothing breaks on case-sensitive systems.

## [4.0.6] - 2026-10-05

### Changed

- Team docs follow the new layout. Page files are named after their title, each section has an overview
  page (`Components.md`, `Tools.md`, ...) next to its folder, and every page starts with a badge line
  saying what it is and who it is for.

## [4.0.5] - 2026-10-05

### Added

- Team documentation in `Documentation~`, moved over from the old wiki and checked against the code.
  `documentationUrl` now points at it.

## [4.0.4] - 2026-09-07

### Fixed

- Two locals and a parameter in the event bus window no longer hide members of the window itself.

## [4.0.2] - 2026-09-06

### Changed

- The three state machine style classes that only their own type used are private.
- Assembly references are GUIDs rather than names, matching the ones already written that way.
  A GUID reference survives an assembly being renamed; a name reference silently stops
  resolving. Unity's own assemblies stay named, since they have no GUID to point at.
- The event bus inspector and the menu manager editor no longer declare a reference to the service
  locator. Neither has used a single type from it since they were split out.
- Every inherited style value in the event bus window now names `EditorTableStyles`, the type that
  declares it, rather than reaching it through `EventBusStyles`. Reading a base member through a
  derived name hides which type owns it and breaks silently the day the base changes.
- Disposing the styles reads as one line. `EditorStyleSet` is an ordinary C# object.
- `EventBusWindow` is 919 lines instead of 1010. What it lists moved into `EventBusModel`: the buses
  in the loaded scenes, the events the chosen one holds, the filtered set and the rows. The window
  keeps what was asked for, which is the search text, the leak filter, the expansion set and the
  sort, and hands those in. The model is rebuilt four times a second while play mode runs and the
  drawing is not, which is the line the two were split along.
- The tooltip system, the raycast helpers and the object pools each moved into their own assembly.
  They shared one with the activation helpers and the noise generator while having nothing to do with
  any of them, so `Base.CorePackage` needed TextMeshPro, the Input System, the tweening package and
  the service locator to compile two components that need none of it. It needs the attributes and the
  utility helpers now, and nothing else. No file moved, so nothing that already points at these
  scripts has to be repointed.
- `EventBusWindow` is 1010 lines instead of 1157. The state badges and their tooltips moved into
  `EventBusBadges`, the tab separated export into `EventBusReport`, and the sort column, its
  direction and both comparisons into `EventBusSorting`. A comment that had come loose from the
  array it describes is back on it, along with three runs of stray blank lines.
- `CameraProvider` moved into its own assembly, `Base.CorePackage.CameraUtility`. It was the only
  thing outside this package that ever reached into `Base.CorePackage`, and reaching it meant taking
  the activation helpers, the noise generator, the object pools, the raycast helpers and the whole
  tooltip system along with it. `Base.CorePackage` is now a leaf that nothing outside Core references.

### Fixed

- The event bus column layout no longer computes a remainder nothing reads, and the neutral badge
  color is read from the type that declares it rather than through the one that inherits it.
- `StateMachinePane` builds its title as a local. Only the constructor ever touched it.
- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.
- Releasing a service on the way out no longer reports it as missing. `MenuManager.Shutdown`,
  `MenuCursorModule`, `MenuTimeScaleModule` and `MenuInputMapModule` all used the reporting lookup, so
  a scene unload that took the service down first turned a clean teardown into one error per object.
  They use the optional lookup now, which is what it is for. Acquiring is unchanged and still reports.

## [3.0.0] - 2026-09-06

### Changed

- Two systems left this package and the one assembly everything referenced became fifteen, so
  upgrading needs work in every project using it. Install `Base Audio` if the project
  plays sound and `Base Core Debug` if it uses the debug menu, then add
  `Base.CorePackage.MenuManaging` and `Base.CorePackage.SceneManagement` to any assembly
  definition that referenced `Base.CorePackage` for either of them. Nothing was renamed, so
  the code itself does not change.
- One assembly per system instead of one for all sixteen. A project that only wants the event
  bus no longer compiles the menu framework, the pooling helpers and the scene loader with it.
  The six small leaves nothing depends on stay together in `Base.CorePackage`, which keeps its
  name and its GUID so existing references still resolve.
- `Screenshot` is its own assembly despite being a single file. It is the only thing in that
  group reaching into `Input`, and the assembly every consumer references should not drag
  `Input` along behind it.
- The editor half splits the same way, with the event bus inspector, the state machine monitor
  and the menu manager windows each on their own and the input map drawer left in
  `Base.CorePackage.Editor`.
- The blanket `InternalsVisibleTo` that opened the whole runtime to the whole editor assembly
  is replaced by one on the event bus alone, aimed at the inspector window that reads its
  handler table. Nothing else crosses a boundary, so nothing else had to be widened.

### Removed

- The audio system, which moved to `Base Audio`. It reads nothing in this package, which is
  why its name carries no `Core` and why it installs without Core.
- The debug menu, the cheat console, the log console and debug drawing, which moved to
  `Base Core Debug` along with their tests. They are meant to be absent from a shipping build,
  which is easier to guarantee by not installing them than by stripping them.
- Both `AssemblyInfo` files. The assemblies they sat on no longer have internals anyone reads.

## [2.1.5] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which references
  the editor assembly now too. Its 28 files could not be named by any test.
- Tests for `StateMachineLayout`, covering columns by distance from the entry state, unreachable
  states being parked behind, any state targets, and columns being centred.