# Tools

🧰 **Tools** · menu items and editor windows

Editor windows and menu commands. Almost everything lives under **Tools > Base Packages**.

## Project Health

Find problems before they reach a build.

| Tool | What it does |
| --- | --- |
| [🔧 Codebase Graph](Tools/Codebase%20Graph.md) | Finds code nothing uses any more and shows what depends on what. |
| [🔧 Assembly Graph](Tools/Assembly%20Graph.md) | Shows how assemblies reference each other and finds unused references. |
| [🔧 Missing Scripts Overview](Tools/Missing%20Scripts%20Overview.md) | Lists broken script references in scenes, prefabs and assets. |
| [🪛 Unused Assets Overview](Tools/Unused%20Assets%20Overview.md) | Lists assets nothing seems to use. |
| [🔧 Unused Scripts Overview](Tools/Unused%20Scripts%20Overview.md) | Lists scripts nothing seems to use. |
| [🪛 Empty Folders Overview](Tools/Empty%20Folders%20Overview.md) | Lists and deletes empty folders. |
| [🔧 Static Reset Checker](Tools/Static%20Reset%20Checker.md) | Finds static fields that are not reset when you enter Play mode. |
| [🪛 Prefab Overview](Tools/Prefab%20Overview.md) | Shows prefab variant trees and flags redundant, overloaded or broken variants. |
| [🔧 Namespace Conventions](Tools/Namespace%20Conventions.md) | Checks that namespaces match their folders. |
| [🪛 Folder Conventions](Tools/Folder%20Conventions.md) | Checks folder names, nesting depth and required folders against your own rules. |

## Code Overviews

Answer "who else uses this number?" questions.

| Tool | What it does |
| --- | --- |
| [🔧 Menu Item Overview](Tools/Menu%20Item%20Overview.md) | Every menu item in the project, sorted by priority. |
| [🔧 Create Asset Menu Overview](Tools/Create%20Asset%20Menu%20Overview.md) | Every asset creation entry, sorted by order. |
| [🔧 Execution Order Overview](Tools/Execution%20Order%20Overview.md) | Every script with a custom execution order. |
| [🔧 Todo Overview](Tools/Todo%20Overview.md) | Every TODO, FIXME and BUG note in the code, with owner and date. |

## Code Generation

| Tool | What it does |
| --- | --- |
| [🔧 Generate Tags](Tools/Generate%20Tags.md) | Turns project tags into const strings. |
| [🔧 Generate Layers](Tools/Generate%20Layers.md) | Turns project layers into const ints and masks. |
| [🔧 Order Manager](Tools/Order%20Manager.md) | Manages named number constants and generates the file for them. |

## Menu Management

| Tool | What it does |
| --- | --- |
| [🔧 Menu Item Manager](Tools/Menu%20Item%20Manager.md) | Rearrange menu items by drag and drop. |
| [🔧 Create Asset Manager](Tools/Create%20Asset%20Manager.md) | Rearrange the Assets > Create menu by drag and drop. |

## Everyday Editor Helpers

| Tool | What it does |
| --- | --- |
| [🪛 Command Palette](Tools/Command%20Palette.md) | Search and run any menu item, Create entry or settings page. `Ctrl + Shift + K`. |
| [🪛 Component Clipboard](Tools/Component%20Clipboard.md) | Copy and paste several components at once. |
| [🪛 Play Mode Saver](Tools/Play%20Mode%20Saver.md) | Keep changes you made during Play mode. |
| [🪛 Hierarchy Sorter](Tools/Hierarchy%20Sorter.md) | Sort the hierarchy alphabetically. |
| [🪛 Auto Start Scene](Tools/Auto%20Start%20Scene.md) | Always start Play mode from the same scene. |

## Assets

| Tool | What it does |
| --- | --- |
| [🪛 Asset Naming Conventions](Tools/Asset%20Naming%20Conventions.md) | Checks asset file names against your own rules and renames them in place. |
| [🪛 Audio Rules](Tools/Audio%20Rules.md) | Sets audio import settings from rules and finds problems in clips. |
| [🪛 Asset Zoo Builder](Tools/Asset%20Zoo%20Builder.md) | Lay out prefabs in a scene so you can look at them all at once. |
| [🔧 Generate Unique Ids](Tools/Generate%20Unique%20Ids.md) | Give scriptable objects a stable ID. |
| [🔧 Unique ID Validation](Tools/Unique%20ID%20Validation.md) | Turn the automatic ID checks on or off. |
| [🔧 Reserialize Assets](Tools/Reserialize%20Assets.md) | Rewrites prefabs and scenes that still store an inspector field under its old name after a rename. |
