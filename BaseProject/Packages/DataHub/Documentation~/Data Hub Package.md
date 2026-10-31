---
type: Start Page
package: Data Hub
tags:
  - data
  - setup
summary: "One window to see, create, edit and check every data asset of the project, grouped by type."
---
# Data Hub Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #data #setup

One window to see, create, edit and check every data asset of the project, grouped by type. No more hunting through folders, and broken assets show up in the table before they show up in play mode.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🔧 Data Hub Window](Tools/Data%20Hub%20Window.md) | Tables of all data assets with inline editing, checks, bulk changes and spreadsheet sync | `Tools > Base Packages > Data Hub` |

## For programmers

What shows up in the window is declared by the project, one class per entry. The interfaces live in `Base.DataHubPackage.Editor.Extensions`:

| Interface | Adds |
| --- | --- |
| `IDataTable` | A type as a table in the tree |
| `IDataColumn` | A computed column |
| `IDataFieldText` | One readable text column for a field type, like a localized string |
| `IDataValidator` | A check that turns the badge red, or amber through `CollectWarnings` |
| `IDataUsageCheck` | An amber warning for assets nothing uses yet |
| `IDataCreateRule` | Folder, start name and collection for new assets, and which assets may stay out of it |
| `IDataHubPage` | A whole custom page, like a balancing overview |
| `IDataSummary` | A live summary at the top of the Contents tab, like the balance of a deck |
| `IDataPreview` | A card in the Preview tab, like the finished card face or playtest numbers |

Keep every text a designer reads short: one plain sentence per tooltip or message, no repeated titles.

The README of the package has an example and the details of every action.