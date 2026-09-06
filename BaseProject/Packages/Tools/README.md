# Base Tools

Editor tooling for everyday project work: static analysis, project health windows, code generators, a data driven menu manager, asset identification and a set of scene and workflow utilities.

This package is a leaf. Nothing in the Base set references it, so it can be installed or removed on its own.

## Requirements

- Unity `6000.3` or newer
- `Base.UtilityPackage` for `CustomLogger`, `PersistentKey` and the dynamic menu attributes
- `Base.AttributesPackage` for the inspector attributes on its config assets
- `Base.EditorUIPackage.Editor` for the shared look of its windows
- Assemblies: one per tool, with `Base.ToolsPackage.Editor` holding the small ones nothing
  depends on, plus `Base.ToolsPackage` for the runtime halves and
  `Base.ToolsPackage.Editor.Tests`. Run the Assembly Graph window for the current shape
  rather than keeping a list here in step by hand.

The dynamic menu attributes the Menu Manager reads live in the Utility package rather than here, so a package can be tagged with `[DynamicMenuItem]` without depending on this one.

Most windows live under **Tools > Base Packages**, but the paths are data driven, so the Menu Manager is where they actually get decided.

## Command Palette

A searchable list of every editor menu item and every asset creation entry, static ones and ones arranged in the menu manager alike. Opens on a shortcut or from the main toolbar, ranks by fuzzy match over the whole path plus tags, pins and recent use, and runs the selection on Enter.

Settings pages are in there too, listed under `Project Settings` and `Preferences` and opened in the right window on Enter. Only the pages the project and its packages declare; Unity's own are left out, because reading them means loading each settings asset, and Unity Search finds them with its `set:` token anyway. A page also carries its own search keywords, so a term its path does not contain can still reach it, ranked below a real path match.

Type `>` to narrow to menu items, `+` to asset creation, `@` to settings pages and `#` to a tag.

For a project with as many tools as this one, it is the fastest way to reach any of them, including Unity's own.

## Codebase Graph

Reads the compiled IL of every project assembly and draws what depends on what, at three levels: namespaces, types and members. It exists to answer one question honestly, which is whether a piece of code is still reachable.

- **Findings**, ranked high to low: dead members and types, serialized fields nothing reads, fields written and never read, members that could be private, internal or readonly, mutable static state, type and namespace cycles with the cheapest edge to cut named, very large types, and types that are load bearing and concrete at once.
- **What a code scan cannot see is looked for anyway.** `Invoke` and `SendMessage` by name, UnityEvent targets wired in the inspector, animation events, types stored by `SerializeReference`, and consts the compiler inlined are all resolved from IL string literals and from asset YAML, so working code is not reported dead.
- **Dismissals** are per finding and stored in `ProjectSettings/CodebaseGraphDismissed.json`. An id embeds the signature it was written for, so a rename brings the finding back and the stale entry is listed for review rather than silently kept.
- **Export findings** writes the whole report as Markdown, dismissal block included. **Export scope** writes one namespace or assembly on its own, with its boundary first, small enough to hand to somebody working on that part alone.
- **New only** shows what this scan found and the last one did not, compared against `ProjectSettings/CodebaseGraphBaseline.json`.
- `[CodebaseGraphIgnore]` from the Utility package marks a type that should never be reported, such as a test fixture or a generator's output.

Its liveness rules are covered by a test suite under `Tests`. Its known gaps are written down in `Editor/CodebaseGraph/README.md`.

## Project health

| Window | What it lists |
|---|---|
| **Assembly Graph** | Project assemblies and their references, with a rolled-up edge view for finding references that are not declared. Unused references can be cleaned straight from the graph |
| **Missing Scripts Overview** | Every missing script in the project, jumping to it on click |
| **Empty Folders Overview** | Empty folders, with jump and delete |
| **Unused Assets Overview** | Assets that look unused, with ping, dismiss and delete |
| **Unused Scripts Overview** | Scripts that look dead, same three actions |
| **Prefab Overview** | Every prefab as a variant tree, how far each variant drifted from its base, and which variants look redundant, overloaded or too deeply chained |
| **Execution Order Overview** | Every script with a custom execution order, sorted by the order that wins at runtime |
| **Static Reset Checker** | Static fields not reset on Enter Play Mode, each linked to its source line |
| **Menu Item Overview** | Every `MenuItem` in the project, its packages and Unity itself, sorted by priority |
| **Create Asset Menu Overview** | The same for every `CreateAssetMenu` attribute, sorted by menu order |

The Unused Assets, Unused Scripts and Asset Naming windows all remember what you chose to keep, by GUID, in a per-project file under `ProjectSettings`, so dismissals survive rescans and restarts and can be committed for the team.

## Todo Overview

Lists every TODO, BUG, FIXME and whatever else the project marks its open work with. Items are searched, filtered by keyword, owner and date, grouped, and opened at the exact line with a double click. A project says whether its dates are deadlines or a note of when something was written, and an individual item can say so for itself, so overdue and stale never get mistaken for each other. Only the rows the scroll view shows are drawn, so a project with thousands of open items stays as responsive as one with ten.

The keywords, the notation for owner and date, and the paths that are out of scope live in `ProjectSettings`, so they are version controlled and the whole project searches for the same things in the same notation.

## Conventions and import rules

**Asset Naming** lists every asset that breaks the project naming conventions and renames it on the spot. The rules live in an `AssetNamingRuleSet` asset, so they are versioned with the project, and they can be read from the assets that already exist with a single button rather than typed out. Rules, Dismissed, Scan Results and History are collapsible sections in one scroll view, and every rename, dismiss and restore lands in the clearable History.

**Folder Convention Validator** checks the project folders against a `FolderConventionConfig` and lists every violation, with a one click fix for missing folders.

**Audio Rules** enforces import settings across the project's audio. Three panes that can all be dragged to any size: the rule list on the left, the results table on the right, the details of whatever is selected underneath. A scan never writes anything: it resolves what the rules want and shows it as a diff, and only Apply touches an importer. Reading sample data is the slow half of a scan, so it runs in the background a few clips at a time and streams findings into the table instead of blocking behind a modal progress bar. The thresholds the deeper analysis judges by live in the rule set, so a project decides for itself what counts as too quiet or as too much silence at the head of a clip.

## Code generation

- **Generate Layers** writes a `Layers` class with all layer indices (0-31) plus a nested `Masks` class of bit shifted mask values.
- **Generate Tags** writes a `Tags` class with all project tags as const strings.
- **Order Manager** manages named order constants and regenerates the generated file.

## Menu management

A data driven replacement for hardcoded `MenuItem` and `CreateAssetMenu` paths. Mark a static method with `[DynamicMenuItem]` or a ScriptableObject type with `[DynamicCreateAssetMenu]`, then arrange the paths and priorities in a window: **Menu Item Manager** for the first, **Create Asset Manager** for the second.

## Assets and identification

**Generate Unique IDs** assigns a globally unique, stable id to every ScriptableObject implementing `IUniquelyIdentifiable`. A postprocessor, a project validator and a pre-build check catch duplicates and gaps early. `UniqueIdSettings` is the central on/off switch: with it off none of the automatic validation runs, on editor load, on import or before a build.

**Asset Reserializer** rewrites assets with the current serializer so that a `[FormerlySerializedAs]` rename actually lands on disk. Scope the run to a few folders, check the count first, then commit the diff it produces. The diff is larger than the field rename alone, so commit your work first.

**Asset Zoo** builds a showcase scene laying out prefabs in a grid, line or circle with optional labels. Configure it through a Zoo Config asset and build or clear it from the Zoo Builder window.

## Scene and workflow helpers

- **Component Clipboard** lists the components of the active GameObject with checkbox multi selection and offers copy, paste, delete and reorder. It fills the gap left by Unity's single entry component clipboard.
- **Play Mode Saver** carries changes made during play mode back out of it. Mark what matters while playing; in edit mode the window lists what was captured and applies or discards each entry. Nothing runs automatically, every apply is triggered by hand, and the captured list is cleared when the next play session starts.
- **Lighting Profile** stores the render settings of a scene so they can be applied without making that scene active. A `LightingProfileApplier` component applies a profile as soon as its scene loads.
- **Hierarchy Sorter** sorts the children of a GameObject or a whole scene alphabetically and recursively.
- **Auto Start Scene** forces a chosen scene to load when entering Play mode and restores the previous scene on exit. It defaults to the first enabled scene in Build Settings.

## Project settings

**Base Tools overview** fills the `Base Tools` node in the Project Settings, which Unity otherwise leaves blank. It lists every page registered under `Project/Base Tools` with a description and a button that jumps there.

The list is built by walking the settings providers of the project, so a new page appears on its own. To give one a description, put `[BaseToolsPage("...")]` next to its `[SettingsProvider]` method; without it the page falls back to its own search keywords. Pages from other packages are picked up the same way, including the `Git Packages` page of the Base Package Installer.

## Tests

`Base.ToolsPackage.Editor.Tests` covers the Codebase Graph's liveness rules. See `Tests/README.md` for how to make them appear in the Test Runner.