# BaseProjectPackages

My personal collection of base Unity packages, the stack I use myself for every project. Built up over years of past projects (especially [Laskards](https://github.com/Kirschkernweitwurf/Laskards), source also on my GitHub) and continuously updated.

Fourteen packages, Unity 6. Everything below is in here and in use.

## What's inside

### Inspector

98 custom attributes that replace Unity's default MonoBehaviour and ScriptableObject inspectors outright, running every member through one pipeline instead of handing subtrees back to Unity.

- **Validation** - `[Required]`, `[NotZero]`, `[NotNullOrEmpty]`, `[MinMax]`, `[PowerOfTwo]`, `[Unique]`, `[ValidateInput]`, `[MustImplement]`, `[ArraySize]`, with fixable help boxes that repair the field for you
- **Auto assignment** - `[GetComponent]`, `[GetComponentInParent]`, `[GetInScene]`, `[GetPrefabWithComponent]`, `[GetScriptableObject]`, so a reference you would have dragged in fills itself
- **Conditionals** - `[ShowIf]`, `[HideIf]`, `[EnableIf]`, `[DisableIf]`, `[ShowIfEnum]`, `[ShowInPlayMode]`, evaluated against any field, property or method
- **Layout** - `[Title]`, `[TabGroup]`, `[HorizontalRow]`, `[Indent]`, `[Suffix]`, `[InfoBox]`, collapsible title sections
- **Widgets** - `[ProgressBar]`, `[Table]`, `[ListDrawerSettings]`, `[Expandable]` inline editors, `[PreviewObject]`, inline buttons, copy and clear buttons
- **Scene handles** - draw and drag a serialized position, direction or radius straight in the scene view
- **Attribute Explorer** window with a live reference of every attribute, runnable showcases and a troubleshooter that finds misapplied attributes

### Runtime foundation

- **ServiceLocator** with typed registration and destroyed-object aware lookups, plus a `GameServiceBehaviour` base that registers and deregisters itself
- **EventBus** for decoupled communication
- **Scene bootstrapper** for persistent, per-scene and gameplay manager prefabs, with an ordered shutdown pipeline
- **Priority trackers** that resolve which of several competing callers currently owns a piece of global state, used for cursor visibility and timescale
- **Object pooling**, **timers**, **scene loading**, **raycast helpers**, **camera provider**, **tooltips**, **screenshots**
- **Audio** with pooled sources and container assets
- **Menu framework** with identifier assets and open, close and back handling
- **In-game debug menu** with a cheat console and a log console

### Tweening

Component-driven and code-driven. Drop a tween on a transform, renderer, graphic or text and drive position, rotation, scale, color, alpha, fill or text from reusable profile assets. Groups sequence and reverse whole hierarchies for menu enter and exit animations. Per-tween delay, easing, looping and ping-pong, all editable from a custom inspector.

### Save system

Async and slot-based. Objects own their own data through `ISavable` and register at runtime, so gameplay code never touches files. Fixed, appending and named slot models, optional AES encryption with plain JSON in the editor, versioned migrations, crash-safe writes, metadata with screenshot thumbnails and play-time tracking, and ready-made save, load, delete and select UI buttons. Storage is a swappable layer, so a console save API drops in behind it.

### Settings

Typed persistable settings (bool, int, float, string, enum) behind a registry that handles load, save, revert and reset. Drop-in components for volume, full screen mode, resolution, quality level and VSync, and ready-made toggle, slider, dropdown and multiple-choice UI. PlayerPrefs out of the box, with a swappable store for file, cloud or in-memory.

### Controller support

Gamepad navigation that wires explicit navigation between elements by proximity, a focus watchdog that keeps the UI from going dead when a selection is lost, input glyph prompts that follow the active device, and priority-based haptics.

### Editor tooling

- **Codebase Graph** - static analysis over compiled metadata that finds unreachable code, serialized fields nothing reads, write-only fields, namespace and type cycles and load-bearing types, with a dismissal system that goes stale on purpose when the code changes
- **Assembly graph** with a rolled-up edge view for finding references that are not declared
- **Command palette** that indexes every menu item, create-asset entry and managed menu entry, with fuzzy matching, tags and usage ranking
- **Menu manager** - data-driven menu entries via `[DynamicMenuItem]`, with overview windows for menu items and create-asset menus
- **Project health** - missing scripts, empty folders, execution order, static reset checker, unused assets and scripts, prefab overview, folder and asset naming conventions
- **Generators** for layer, tag and sorting-order constants, so no magic strings
- **Component clipboard**, **hierarchy sorter**, **lighting profiles**, **asset zoo scene builder**, **auto start scene**, **unique IDs for ScriptableObjects**
- **Localization** - one-click Pull and Push between Unity String Table Collections and Google Sheets, with validation and per-collection reporting
- **Memory Profiler** - automated `.snap` capture on a timer or on scene load, in the editor and in development builds, so leaks show up as a timeline instead of a guess
- **Editor UI** - the shared skin every one of these windows is built from: skin-aware colors, rounded nine-sliced cards, pills and buttons, badges and draggable column dividers that remember their widths

### Utility

Serializable dictionaries and a flattened 2D array, `SceneReference`, `TypeReference`, `InterfaceReference` and a validated `PersistentKey`, a tracked coroutine runner, class-tagged rich-text logging with an optional global log handler, safe assembly reflection, platform and build flags, and helpers for audio math, percentages, strings, components, rotation and time formatting.

## Installation

**Use the [BasePackageInstaller](https://github.com/Kirschkernweitwurf/BasePackageInstaller).** It is the only thing that knows how these packages fit together.

1. Open your project in Unity
2. Open the **Package Manager**
3. Click **+**, select **Install package from git URL**, and paste:
   ```
   https://github.com/Kirschkernweitwurf/BasePackageInstaller.git
   ```
4. Open `Tools > Git Package Manager`
5. Tick the packages you want and hit the action button

Tick one package and everything it needs comes along, resolved and installed for you.

### Why not just paste the Git URLs

These packages depend on each other, but none of them says so in its `package.json`, and that is deliberate. A `package.json` dependency is resolved through a registry, and these packages are not on one. Declaring `Base.CorePackage` as a dependency of `Base.UIPackage` would make UPM go looking for it in a registry that does not have it, and the install fails.

So the dependency graph lives in the installer instead, where it can be resolved against Git URLs. That is the piece you are doing by hand if you install by hand: adding a single Git URL gives you exactly that one package, and Unity then compiles it against a project missing everything it needs.

### Installing manually anyway

The Git URL for a single package is:

```
https://github.com/Kirschkernweitwurf/BaseProjectPackages.git?path=BaseProject/Packages/<PackageName>
```

`<PackageName>` is the folder name from the table below, not the display name. You also have to add everything in the **Needs** column, and whatever those need, until nothing is missing.

| Package | Folder | Needs |
|---|---|---|
| Attributes | `Attributes` | Editor UI, Utility |
| Content | `Content` | Controller Support, Save System, Settings System, UI |
| Controller Support | `ControllerSupport` | Core |
| Core | `Core` | Tweening |
| Editor UI | `EditorUi` | nothing |
| Localization | `Localization` | Utility |
| Memory Profiler | `MemoryProfiler` | Core |
| Save System | `SaveSystem` | Services |
| Services | `Services` | Attributes |
| Settings System | `Settings` | Core |
| Tools | `Tools` | Attributes |
| Tweening | `Tweening` | Services |
| UI | `UI` | Core |
| Utility | `Utility` | nothing |

The chains get long quickly. `UI` on its own pulls in six more packages, `Content` ten. Only `Editor UI` and `Utility` stand alone.

This table is also the thing most likely to be out of date after I change something, which is the other reason to let the installer do it.

## Why?

I made this to have a solid, consistent base for all my Unity projects. It saves me from rewriting the same systems over and over. If it's useful to you too, great, go ahead and use it.

## Updates

This repo gets updated whenever I improve something in one of my projects. Expect changes over time.
