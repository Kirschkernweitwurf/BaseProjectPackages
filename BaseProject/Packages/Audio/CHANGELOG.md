# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-09-06

### Added

- Split out of `Base Core`, where it shared a package with fifteen systems it has nothing to do
  with. It imports nothing from Core, which is why it carries no `Core` in its name and why a
  project can install it without installing Core at all.
- The namespaces are `Base.AudioPackage`, `Base.AudioPackage.OnEvent`, `Base.AudioPackage.Pool`
  and `Base.AudioPackage.Editor`, replacing the `Base.CorePackage.Audio` ones they had while the
  code lived in Core.