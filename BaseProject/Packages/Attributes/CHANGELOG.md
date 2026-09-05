# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 1.7.5 were not recorded.

## [Unreleased]

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