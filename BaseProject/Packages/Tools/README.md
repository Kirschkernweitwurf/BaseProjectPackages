# Base Tools Package

A collection of Unity editor tools and small runtime helpers that speed up everyday project work. It groups together project health windows, code generators, a data driven menu manager, asset identification and a few scene and workflow utilities.

Target Unity version: **6000.3**

Namespaces: `Base.ToolPackage` (runtime) and `Base.ToolPackage.Editor` (editor).

## Installation

Add the package through the Package Manager using the Git URL or a local path. You can also drop the `Tools` folder into your project's `Packages` or `Assets` directory.

## Tools

Most windows live under **Tools > Base Packages** in the Unity menu bar.

### Project health

- **Assembly Graph** (`Unity Editor/Project Health`) visualizes project assemblies and their references. It can also clean unused references straight from the graph.
- **Menu Item Overview** (`Code/Health`) lists every `MenuItem` in the project, its packages and Unity itself, sorted by priority. Click a row to open its script.
- **Create Asset Menu Overview** (`Code/Health`) does the same for every `CreateAssetMenu` attribute, sorted by menu order.
- **Execution Order Overview** (`Code/Health`) lists every script with a custom execution order, sorted by the order that wins at runtime.
- **Static Reset Checker** (`Code/Health`) scans for static fields that are not reset on Enter Play Mode and links each finding to its source line.
- **Missing Scripts Overview** lists every missing script in the project and jumps to it on click.
- **Empty Folders Overview** lists empty folders and lets you jump to or delete them.

### Codebase Graph

**Codebase Graph** (`Unity Editor/Project Health`) reads the compiled IL of every project assembly and
draws what depends on what, at three levels: namespaces, types and members. It exists to answer one
question honestly, which is whether a piece of code is still reachable.

- **Findings**, ranked high to low: dead members and types, serialized fields nothing reads, fields
  written and never read, members that could be private, internal or readonly, mutable static state,
  type and namespace cycles with the cheapest edge to cut named, very large types and types that are
  load bearing and concrete at once.
- **What a code scan cannot see is looked for anyway.** Invoke and SendMessage by name, UnityEvent
  targets wired in the inspector, animation events, types stored by `SerializeReference`, and consts
  the compiler inlined are all resolved from IL string literals and from asset YAML, so working code is
  not reported dead.
- **Dismissals** are per finding and stored in `ProjectSettings/CodebaseGraphDismissed.json`. An id
  embeds the signature it was written for, so a rename brings the finding back and the stale entry is
  listed for review rather than silently kept.
- **Export findings** writes the whole report as Markdown, dismissal block included. **Export scope**
  writes one namespace or assembly on its own, with its boundary first, small enough to hand to
  somebody working on that part alone.
- **New only** shows what this scan found and the last one did not, compared against
  `ProjectSettings/CodebaseGraphBaseline.json`.
- Put `[CodebaseGraphIgnore]` on a type that should never be reported, such as a test fixture or a
  generator's output.

Its own liveness rules are covered by a test suite under `Tests`. See `Tests/README.md` for how to make
them appear in the Test Runner.

### Code generation

- **Generate Layers** (`Code/Generation`) writes a `Layers` class with all layer indices (0-31) plus a nested `Masks` class of bit shifted mask values.
- **Generate Tags** (`Code/Generation`) writes a `Tags` class with all project tags as const strings.
- **Order Manager** (`Code/Generation`) manages named order constants and regenerates the generated file.

### Menu management

A data driven replacement for hardcoded `MenuItem` and `CreateAssetMenu` paths. Mark a static method with `[DynamicMenuItem]` or a ScriptableObject type with `[DynamicCreateAssetMenu]`, then arrange the paths and priorities in a window.

- **Menu Item Manager** (`Menu Management`) arranges dynamic menu item entries.
- **Create Asset Manager** (`Menu Management`) arranges dynamic asset creation entries.

### Assets and identification

- **Generate Unique IDs** (`Assets/Identifier`) assigns a globally unique, stable ID to every ScriptableObject that implements `IUniquelyIdentifiable`. Includes a postprocessor, project validator and pre build check so duplicates are caught early.
- **Asset Zoo** builds a showcase scene that lays out your prefabs in a grid, line or circle with optional labels. Configure it through an `Asset Zoo > Zoo Config` asset and build or clear it from the Zoo Builder window.

### Scene and workflow helpers

- **Component Clipboard** lists the components of the active GameObject with checkbox multi selection and offers copy, paste, delete and reorder. It fills the gap left by Unity's single entry component clipboard.
- **Lighting Profile** stores the render settings of a scene so they can be applied without making that scene active. A `LightingProfileApplier` component applies a profile as soon as its scene loads.
- **Hierarchy Sorter** sorts the children of a GameObject or a whole scene alphabetically and recursively.
- **Auto Start Scene** forces a chosen scene to load when entering Play mode and restores the previous scene on exit. It defaults to the first enabled scene in Build Settings.

## Assembly definitions

- `Base.ToolPackage` for runtime code.
- `Base.ToolPackage.Editor` for editor only code, scoped to the Editor platform.

## Dependencies

This package is a leaf: nothing in the Base set references it, so it can be installed or
removed on its own.

- `Base.UtilityPackage` for `CustomLogger`, `PersistentKey` and the dynamic menu attributes
- `Base.AttributePackage` for the inspector attributes on its config assets
- `Base.EditorUiPackage` for the shared look of its windows

The dynamic menu attributes the Menu Manager reads live in the Utility package rather than
here, so a package can be tagged with `[DynamicMenuItem]` without depending on this one.

## Author

Jonathan Alber