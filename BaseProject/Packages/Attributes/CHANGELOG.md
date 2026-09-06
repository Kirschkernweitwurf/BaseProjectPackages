# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.7.5 were not recorded.

## [Unreleased]

## [1.8.2] - 2026-09-06

### Added

- `HeaderItemCollectorTests`, covering which header controls a type declares and which are passed
  over. Every rule there fails silently: a method carrying the attribute but the wrong signature is
  skipped rather than reported, so a button that never appears looks like one nobody wrote.

### Changed

- Reading which header controls a type declares moved out of `HeaderItemRenderer` into
  `HeaderItemCollector`, together with the tooltip text and the play mode check. None of the three
  draws anything, and behind the renderer none of them could be reached without a live header.
- The live sample behind an attribute page moved out of `AttributeReferencePane` into
  `AttributeSamplePreview`. The object carrying the attribute, the inspector drawing it, the script
  it came from and the snippet read out of that script are one lifetime: built together, reused
  together when the same page is opened twice, and destroyed together or a temporary object and an
  editor are left behind on every page turn. Clearing the keyboard focus now happens before that
  teardown rather than after it, which is the safer order of the two.

### Fixed

- Edit mode tests build their objects outside any scene. They used to be created in whatever scene
  happened to be open, so every run put them in it and a run that never reached its teardown left
  them there to be saved with it. They go through `EditorUtility.CreateGameObjectWithHideFlags` now,
  which never puts them in a scene at all, so they cannot show up in the hierarchy or be saved
  however the run ends.
- Three stray blank line runs and a comment that had come loose from the string it describes.
  `RequireComponentAuditWindow` carried an explanation of why `Description` is declared after the
  two labels it reads, sitting below that field with seven blank lines under it.

## [1.8.0] - 2026-09-05

### Added

- `documentationUrl` and `changelogUrl` in `package.json`, so the Package Manager window
  links straight to the README and to this file.
- An `AssemblyInfo` opening the editor assembly's internals to the test assembly, which reference
  the editor assembly now too. The editor half was unreachable from any test until now.
- Tests for `ElementLabel`, the text a list row is titled and filtered by.
- Tests for `ListDrawerState`, covering the per-instance, per-field key its filter is stored under.
- Tests for `FirstDraw`, the single first draw tracker the merge below leaves behind.
- Tests for `AttributeNames`, `StateKey` and `PathUtility`: the display names shown in messages,
  the keys per field editor state is filed under, and the paths the path drawers hand back.
- Tests for `ConditionEvaluator`, the editor half of every conditional attribute, including that
  an edit reaches a condition before it is applied to the object.
- Tests for `PropertySorter`, covering pinning, stability, a run moving as one and a field being
  unable to leave its section.
- Tests for `ArraySizeLimits`, covering the bounds read off a field and whether the add and
  remove controls draw, including that a string is not treated as an array.
- Tests for `ValueResolver`, covering literals, field, property and method references, a member
  holding nothing, and the fallback when a reference cannot be resolved.
- Tests for `NumericPropertyClamp` and `ColorAttributeUtility`: component wise clamping across
  every numeric type, and resolving a color from a hex or a preset.
- Tests for `EnumButtonLayout`, covering the plain and flags paths, the zero member getting no
  button, and every label lining up with the bit it writes.

### Changed

- `EnumButtonLayout` measures its button width the first time it is asked for instead of while
  building. Measuring reads the editor styles, which only exist while something is drawn, so
  building a layout no longer has to happen inside a repaint.

### Removed

- `ListDrawerState.IsFirstDraw` and `ListDrawerState.Forget`, which duplicated `FirstDraw` down to
  the same key and the same body. `TableRenderer` uses `FirstDraw` like `StartExpandedHandler`
  already did, `SamplePreviewDefaults` forgets one tracker instead of two, and `ListDrawerState` is
  left owning only the filter. The two copies had drifted: only `FirstDraw` cleared on entering
  play mode, so a table kept a fold the same field on a plain list did not.