# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Changes made before 2.1.4 were not recorded.

## [Unreleased]

## [4.0.0] - 2026-09-06

### Changed

- `EventBusWindow` is 1010 lines instead of 1157. The state badges and their tooltips moved into
  `EventBusBadges`, the tab separated export into `EventBusReport`, and the sort column, its
  direction and both comparisons into `EventBusSorting`. A comment that had come loose from the
  array it describes is back on it, along with three runs of stray blank lines.
- `CameraProvider` moved into its own assembly, `Base.CorePackage.CameraUtility`. It was the only
  thing outside this package that ever reached into `Base.CorePackage`, and reaching it meant taking
  the activation helpers, the noise generator, the object pools, the raycast helpers and the whole
  tooltip system along with it. `Base.CorePackage` is now a leaf that nothing outside Core references.

### Fixed

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