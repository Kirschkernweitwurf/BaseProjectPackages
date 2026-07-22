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

## Author

Jonathan Alber
