# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-10-15

### Added

- Data Hub window under `Tools > Base Packages > Assets > Data Hub`: a type tree, a table with one column per
  serialized field and inline editing through `SerializedObject`, and the inspector of the selected rows.
- Nested serializable structs flatten into one column per field, grouped under the struct name. Backing field
  names of `[field: SerializeField]` properties are shown cleanly.
- Sorting, resizable columns that remember their width, search over names and values, a problems-only filter.
- Multi-edit: changing a cell of a selected row writes the value to every selected row in one undo step.
- New, Duplicate, Rename and Delete. New assets follow the naming conventions and join their collection.
  Delete removes every reference from the project's data assets before moving the files to the trash.
- Check column with the problems of the validation attributes, the project's validators and missing
  collection membership.
- Extension interfaces `IDataTable`, `IDataColumn`, `IDataValidator`, `IDataCreateRule` and `IDataHubPage`,
  found through `TypeCache`.