# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-09-06

### Added

- An edit mode test assembly, `Base.AudioPackage.Tests`, covering `ActiveSounds`, `AudioContainer`,
  `AudioFader`, `AudioPool` and `AudioSourceConfigurator`.
- A play mode test assembly, `Base.AudioPackage.PlayTests`, covering `AudioManager` and
  `AudioPoolManager`. Both build their state in `Awake` and both release sources over frames, so
  neither is reachable from edit mode. Between the two assemblies this is the first coverage the
  package has had.
- `InternalsVisibleTo` for both test assemblies, so the tracking table, the pool wrapper and the
  source configurator stay internal instead of being widened to be reachable.
- Constants on `AudioManager` and `AudioPoolManager` naming their serialized fields, so a test can
  wire a manager before it is switched on without spelling the field names out.

## [1.0.0] - 2026-09-06

### Added

- Split out of `Base Core`, where it shared a package with fifteen systems it has nothing to do
  with. It imports nothing from Core, which is why it carries no `Core` in its name and why a
  project can install it without installing Core at all.
- The namespaces are `Base.AudioPackage`, `Base.AudioPackage.OnEvent`, `Base.AudioPackage.Pool`
  and `Base.AudioPackage.Editor`, replacing the `Base.CorePackage.Audio` ones they had while the
  code lived in Core.