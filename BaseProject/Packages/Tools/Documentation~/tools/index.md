# Tools

Editor windows and menu commands. Almost everything lives under **Tools > Base Packages**.

## Project Health

Find problems before they reach a build.

| Tool | What it does |
| --- | --- |
| [🔧 Codebase Graph](codebase-graph.md) | Finds code nothing uses any more and shows what depends on what. |
| [🔧 Assembly Graph](assembly-graph.md) | Shows how assemblies reference each other and finds unused references. |
| [🔧 Missing Scripts Overview](missing-scripts-overview.md) | Lists broken script references in scenes, prefabs and assets. |
| [🪛 Unused Assets Overview](unused-assets-overview.md) | Lists assets nothing seems to use. |
| [🔧 Unused Scripts Overview](unused-scripts-overview.md) | Lists scripts nothing seems to use. |
| [🪛 Empty Folders Overview](empty-folders-overview.md) | Lists and deletes empty folders. |
| [🔧 Static Reset Checker](static-reset-checker.md) | Finds static fields that are not reset when you enter Play mode. |
| [🪛 Prefab Overview](prefab-overview.md) | Shows prefab variant trees and flags redundant, overloaded or broken variants. |
| [🔧 Namespace Conventions](namespace-conventions.md) | Checks that namespaces match their folders. |
| [🪛 Folder Conventions](folder-conventions.md) | Checks folder names, nesting depth and required folders against your own rules. |

## Code Overviews

Answer "who else uses this number?" questions.

| Tool | What it does |
| --- | --- |
| [🔧 Menu Item Overview](menu-item-overview.md) | Every menu item in the project, sorted by priority. |
| [🔧 Create Asset Menu Overview](create-asset-menu-overview.md) | Every asset creation entry, sorted by order. |
| [🔧 Execution Order Overview](execution-order-overview.md) | Every script with a custom execution order. |
| [🔧 Todo Overview](todo-overview.md) | Every TODO, FIXME and BUG note in the code, with owner and date. |

## Code Generation

| Tool | What it does |
| --- | --- |
| [🔧 Generate Tags](generate-tags.md) | Turns project tags into const strings. |
| [🔧 Generate Layers](generate-layers.md) | Turns project layers into const ints and masks. |
| [🔧 Order Manager](order-manager.md) | Manages named number constants and generates the file for them. |

## Menu Management

| Tool | What it does |
| --- | --- |
| [🔧 Menu Item Manager](menu-item-manager.md) | Rearrange menu items by drag and drop. |
| [🔧 Create Asset Manager](create-asset-manager.md) | Rearrange the Assets > Create menu by drag and drop. |

## Everyday Editor Helpers

| Tool | What it does |
| --- | --- |
| [🪛 Command Palette](command-palette.md) | Search and run any menu item, Create entry or settings page. `Ctrl + Shift + K`. |
| [🪛 Component Clipboard](component-clipboard.md) | Copy and paste several components at once. |
| [🪛 Play Mode Saver](play-mode-saver.md) | Keep changes you made during Play mode. |
| [🪛 Hierarchy Sorter](hierarchy-sorter.md) | Sort the hierarchy alphabetically. |
| [🪛 Auto Start Scene](auto-start-scene.md) | Always start Play mode from the same scene. |

## Assets

| Tool | What it does |
| --- | --- |
| [🪛 Asset Naming Conventions](asset-naming-conventions.md) | Checks asset file names against your own rules and renames them in place. |
| [🪛 Audio Rules](audio-rules.md) | Sets audio import settings from rules and finds problems in clips. |
| [🪛 Asset Zoo Builder](asset-zoo-builder.md) | Lay out prefabs in a scene so you can look at them all at once. |
| [🔧 Generate Unique Ids](generate-unique-ids.md) | Give scriptable objects a stable ID. |
| [🔧 Unique ID Validation](unique-id-validation.md) | Turn the automatic ID checks on or off. |
| [🔧 Reserialize Assets](reserialize-assets.md) | Rewrites prefabs and scenes that still store an inspector field under its old name after a rename. |
