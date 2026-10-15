# Changelog

All notable changes to this package are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2026-10-15

### Added

- Contents tab: the asset lists of the selected asset as counts per entry, with minus, plus, a typed count,
  remove, an add row and drop from the table.
- Used In tab: every list in the project that can hold the selected rows, with their count and minus, plus
  and a typed count that apply to every selected row.
- Actions in the toolbar and on right click: run a `[Button]` method on every selected asset.
- Drag rows by their name to drop the selected assets on lists, fields or the Project window.
- Shortcuts in the table: Ctrl+D duplicates, Delete deletes, F2 or a double click on a name renames,
  Ctrl+A selects every row, Ctrl+F jumps to the search, Shift with the arrow keys, Home or End extends
  the selection.
- An empty table offers a button for the first asset, an empty search result one that clears the filters.
- A status line under the table with the shown, selected and problem counts.
- Tooltips on every column title say what the column holds and how to edit it, led by the field's own
  `[Tooltip]` when it has one. Every button, tab, filter and tree entry explains itself on hover too.

### Fixed

- The window stayed empty after a script reload. It now sets itself up again whenever its helpers are gone.
- Column titles are no longer cut off. Columns the user never resized grow to fit their title.

### Changed

- The asset counts in the type tree are plain dimmed numbers instead of badges.
- Reworked look: every pane has a header with its title, the table a breadcrumb title with labeled
  buttons, a wide search and an All or Problems switch, the side panel the selected asset and segmented
  tabs.
- The type tree has icons, foldable groups and a rounded selection.
- The name and check columns stay in place while the other columns scroll sideways.
- Cells show plain values and turn into fields under the mouse. Asset references become pickers on
  selected rows only, and still take a drop on any row.
- Numbers are right-aligned, toggles centered, and floats no longer show rounding noise like
  0.300000011920929.
- The check column shows a quiet check mark for a fine row and a red count for a broken one, which also
  gets a red edge.
- Struct titles no longer leave an empty band above the other column titles.
- Contents and Used In draw each list as a card with one stepper per entry, light up the list a drag is
  over, and split Used In into lists that hold the selection and lists that could.
- Rename opens on the row's name and only mentions the saved name when the naming conventions change it.

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