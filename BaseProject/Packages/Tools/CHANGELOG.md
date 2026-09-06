# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 2.0.10 were not recorded.

## [Unreleased]

## [3.1.1] - 2026-09-06

### Added

- `ComponentPastePlanTests`, covering what a component paste is going to do before it does it. The
  plan is what the window previews and what the paste then carries out, so a wrong one either
  overwrites something that should have been left alone or adds a duplicate beside the component it
  should have replaced, neither of which says anything.
- `MenuItemConventionTests`, which pins the three places in this package that use Unity's
  `[MenuItem]` instead of the Menu Manager, and checks that both manager windows keep an entry
  that does not depend on the registration they exist to repair.
- The Namespace Convention Validator, a window that checks the scripts below a configurable root
  against the folders they sit in. It replaces the test that did the same thing only for the base
  packages: with the root and the root namespace on a config asset, the same tool runs on a game's
  own scripts, where no assembly definition names anything.
- `NamespaceConventionScannerTests`, covering the scanner against layouts written for each case
  rather than against whatever is in the Assets folder that day.
- The Todo Overview asks what a date on an item means. A date can be a deadline or a note of
  when something was written, and nothing about the date itself says which. Until now only the
  first reading existed, so a project that writes down when it wrote a note had every item in
  it red the day after it was written, which is the same as having no date column at all.
  `What A Date Means` on the settings page picks the reading. `Due` behaves exactly as before.
  `Written` leaves recent notes calm and flags them once they have been sitting there for the
  configured number of days.
- An item can say what its own date means, by putting it in a `due` or a `written` group
  instead of the plain `date` group. Two default patterns come with it, so `TODO (due 01.10.26)`
  and `TODO (Jonny, written 20.08.26)` are read without anyone touching the settings. That is
  what lets one codebase carry both readings rather than having to choose.

### Changed

- `Base.ToolsPackage.Editor.ComponentClipboard` is reachable from the test assembly, through an
  `AssemblyInfo` rather than by widening anything. It had no coverage before, because nothing
  referenced it.
- `CodebaseGraphWindow` is 692 lines instead of 807. Where the reader is looking moved into
  `CodebaseGraphNavigation`: the level being browsed, the namespace or type open at it, and the entry
  the graph is pointed at. The heading, the breadcrumb and the focus notice went with that state
  rather than staying behind it, because all three are built from nothing else. The window keeps its
  serialized copy for surviving a reload, which is read into and written out of the new type.
- The temporary highlight the menu manager windows show when an overview links straight to one entry
  moved into `MenuRowFocus`. Its id, its expiry and the pending scroll were three loose fields on the
  window with four methods reaching into them, and nothing said they belonged together.
- `TodoOverviewWindow` is 634 lines instead of 842. The search field, the owner, sort and group
  dropdowns and the three buttons across the top moved into `TodoToolbar`, together with the twenty
  labels and tooltips only they used. The window is told whether a control changed the filter or
  asked for a rescan, since a dropdown reports its choice when the menu closes rather than while the
  toolbar is drawn.
- `StaticResetScanner` is 446 lines instead of 1169. The text handling underneath it moved into
  `SourceCleaner`, `SourceNavigator`, `DeclarationReader` and `SourceLines`, which between them knew
  nothing about static fields, resets or findings and were only sitting in that file because that is
  where they were first needed. No behavior changed and no method was dropped; `CleanSource` is the
  one rename, to `SourceCleaner.Clean`.
- `IAssetIndex` can read the text of a file. A scanner that reads source rather than only the
  layout was not expressible behind it before, which is what kept the namespace check in a test.
- The three `[MenuItem]` uses now record why they cannot move. The Play Mode Applier entries need
  the `MenuCommand` that the Menu Manager's registration cannot pass, and the two manager
  windows are how a broken registration gets repaired, so neither can be registered by it.
- `Base.ToolsPackage.Editor.MenuManagerWindows` and `Base.ToolsPackage.Editor.PlayModeApplier`
  are reachable from the test assembly. Neither had any coverage before, because nothing
  referenced them.
- The Codebase Graph liveness fixtures moved from `Base.ToolsPackage.Editor.Tests.Fixtures` to
  `Base.ToolsPackage.Editor.Tests.CodebaseGraph.Fixtures`, so their namespace matches the folder
  they have always been in. They were the only eight files in the ecosystem that did not.
- `LivenessTests` names the fixtures through one shared prefix constant rather than repeating the
  namespace eight times. They stay plain string literals, because a `typeof` here would mark the
  dead fixtures as used and defeat the tests that assert they are dead.
- The date column, the filter pill and the pill tooltips follow what the dates mean. The column
  reads `Due` or `Written`, the pill reads `Overdue` or `Stale`, and hovering a date says how
  far past it is in words next to the text the comment was written with.
- Existing projects are upgraded once on load: the two thresholds are filled in and the two
  patterns for a marked date are added, unless the project already reads one. Nothing that was
  configured by hand is written over, and the step never runs twice.

### Fixed

- A stray blank line run in `AssetNamingWindow`.

## [3.0.0] - 2026-09-06

### Changed

- The one editor assembly everything referenced became twenty, so an assembly definition
  pointing at `Base.ToolsPackage.Editor` now reaches only the eight smallest tools. Reference
  the tool you actually use instead. Two namespaces moved with their code, the assembly edge
  analysis to `Base.ToolsPackage.Editor.CodebaseGraph.Architecture` and the menu model to
  `Base.ToolsPackage.Editor.MenuManagerModel`, which is the only source change an upgrade
  needs.
- One assembly per tool instead of one for all twenty-five. Editing a one-file tool recompiled
  the other four hundred and thirty-two files; the root assembly is twenty-three now. The cut
  is at six files, below which an assembly costs more than the recompiles it saves, so the eight
  smallest tools stay together in `Base.ToolsPackage.Editor`.
- `Shared` and `BaseToolsOverview` are their own assemblies, since everything else sits on top
  of them. Both keep their folder and namespace, so no consumer had to change.
- The menu model splits out of `MenuManagerWindows` into `MenuManagerModel`. The Command Palette
  and the naming convention scanner read twelve types out of it, which meant either a window
  tool carrying a public API the size of a package or those two tools staying welded to it. The
  model stays internal and names its three consumers instead.
- `MenuNode`, `MenuEntryNode` and `MenuGroupNode` carry `[MovedFrom]`. The shipped registry
  stores its nodes as `[SerializeReference]` records naming the namespace and the assembly, so
  without it the split would have loaded every one as null and written the emptied asset back
  out on the next save.
- The assembly edge analysis moved from `AssemblyGraph/Architecture` to `CodebaseGraph`, where
  its own documentation already said it belonged. That was the package's only cycle.
- `OverviewGui`, `EOverviewAccent` and the `Shared` types are public now, because ten assemblies
  read them and that list grows with every tool added.
- The README no longer lists the assemblies by name. That was maintainable at three and is not
  at twenty, so it points at the Assembly Graph window instead.

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