# BaseProjectPackages

My personal collection of base Unity packages, the stack I use myself for every project. Built up over years of past projects (especially [Laskards](https://github.com/Kirschkernweitwurf/Laskards), source also on my GitHub) and continuously updated.

Fourteen packages, Unity 6. Everything below is in here and in use.

## What's inside

| Package | Folder | What it is | Needs |
|---|---|---|---|
| Attributes | `Attributes` | 100 inspector attributes that enhance the default inspector | Editor UI, Utility |
| Content | `Content` | The prefabs and assets the other packages are wired together with | Controller Support, Save System, Settings System, UI |
| Controller Support | `ControllerSupport` | Gamepad navigation, input glyphs, haptics | Core |
| Core | `Core` | Menus, audio, scenes, timers, state machines, debug tooling | Tweening |
| Editor UI | `EditorUI` | The shared theme every Base editor window is built from | - |
| Localization | `Localization` | Google Sheets sync for Unity String Tables | Utility |
| Memory Profiler | `MemoryProfiler` | Automated `.snap` capture on a timer or on scene load | Core |
| Save System | `SaveSystem` | Async, slot-based saving with backups and migrations | Services |
| Services | `Services` | Service locator, bootstrapper, shutdown pipeline | Attributes |
| Settings System | `Settings` | Typed persistable settings with ready-made UI | Core |
| Tools | `Tools` | Editor tooling: static analysis, project health, generators | Attributes |
| Tweening | `Tweening` | Component-driven and code-driven tweening | Services |
| UI | `UI` | Buttons, confirmation dialogs, small UI utilities | Core |
| Utility | `Utility` | Serializable collections, logging, platform flags, helpers | - |

**Needs** lists direct dependencies only; the installer walks the rest of the chain.

### Inspector

100 custom attributes that replace Unity's default MonoBehaviour and ScriptableObject inspectors outright, running every member through one pipeline instead of handing subtrees back to Unity.

- **Validation** - `[Required]`, `[NotZero]`, `[NotNullOrEmpty]`, `[MinMax]`, `[PowerOfTwo]`, `[Unique]`, `[ValidateInput]`, `[MustImplement]`, `[ArraySize]`, with fixable help boxes that repair the field for you
- **Auto assignment** - `[GetComponent]`, `[GetComponentInParent]`, `[Child]`, `[GetInScene]`, `[GetPrefabWithComponent]`, `[GetScriptableObject]`, `[RequiredGet]`, so a reference you would have dragged in fills itself
- **Conditionals** - `[ShowIf]`, `[HideIf]`, `[EnableIf]`, `[DisableIf]`, `[ShowIfEnum]`, `[ShowInPlayMode]`, evaluated against any field, property or method
- **Layout** - `[Title]`, `[Tab]`, `[Horizontal]`, `[Foldout]`, `[Indent]`, `[Suffix]`, `[InfoBox]`, `[PropertyOrder]`
- **Widgets** - `[ProgressBar]`, `[Table]`, `[ListDrawerSettings]`, `[Expandable]` inline editors, `[PreviewObject]`, `[Date]` and `[Time]` pickers, inline buttons, header buttons
- **Scene handles** - `[PositionHandle]`, `[RotationHandle]`, `[ScaleHandle]`, `[RadiusHandle]` and the drawing attributes draw and drag a serialized value straight in the scene view
- **Attribute Explorer** window with a live reference page per attribute, runnable showcases and a troubleshooter that finds misapplied attributes

### Runtime foundation

- **ServiceLocator** with typed registration and destroyed-object aware lookups, plus a `GameServiceBehaviour` base that registers and deregisters itself
- **EventBus** for decoupled communication, with an editor window that lists every live subscriber
- **Scene bootstrapper** for persistent, per-scene and gameplay manager prefabs, with an ordered shutdown pipeline
- **State machines** over any context type, plus a monitor window that draws the running machines and the transitions being evaluated
- **Priority trackers** that resolve which of several competing callers currently owns a piece of global state, used for cursor visibility, timescale and tooltips
- **Object pooling**, **timers**, **scene loading**, **raycast helpers**, **camera provider**, **tooltips**, **screenshots**
- **Seeded randomization** and **layered noise**, both reproducible from a seed alone
- **Audio** with pooled sources and container assets
- **Menu framework** with identifier assets and open, close and back handling
- **In-game debug menu** with a cheat console and a log console, plus `DebugDraw` shapes that also render in a player

### Tweening

Component-driven and code-driven. Drop a tween on a transform, renderer, graphic or text and drive position, rotation, scale, color, alpha, fill or text from reusable profile assets. Groups sequence and reverse whole hierarchies for menu enter and exit animations. Per-tween delay, easing, looping and ping-pong, all editable from a custom inspector.

### Save system

Async and slot-based. Objects own their own data through `ISavable` and register at runtime, so gameplay code never touches files. Fixed, appending and named slot models, optional AES encryption with plain JSON in the editor, versioned migrations, crash-safe writes, checksums with automatic fallback to a backup generation, autosave behind a cooldown and metadata with screenshot thumbnails and play-time tracking. Storage is a swappable layer, so a console save API drops in behind it.

### Settings

Typed persistable settings (bool, int, float, string, enum) behind a registry that handles load, save, revert and reset. Drop-in components for volume, full screen mode, resolution, quality level, VSync, look sensitivity and key rebinding, plus ready-made toggle, slider, dropdown and multiple-choice UI and quality presets. PlayerPrefs out of the box, with a swappable store for file, cloud or in-memory.

### Controller support

Gamepad navigation that wires explicit navigation between elements by proximity, a focus watchdog that keeps the UI from going dead when a selection is lost, stick scrolling, input glyph prompts that follow the active device and priority-based haptics.

### Editor tooling

- **Codebase Graph** - static analysis over compiled metadata that finds unreachable code, serialized fields nothing reads, write-only fields, namespace and type cycles and load-bearing types, with a dismissal system that goes stale on purpose when the code changes
- **Command palette** that indexes every menu item, create-asset entry and managed menu entry, with fuzzy matching, tags and usage ranking
- **Assembly graph** with a rolled-up edge view for finding references that are not declared
- **Menu manager** - data-driven menu entries via `[DynamicMenuItem]`, with overview windows for menu items and create-asset menus
- **Project health** - missing scripts, empty folders, execution order, static reset checker, unused assets and scripts, prefab variant overview, todo overview, folder and asset naming conventions, audio import rules
- **Generators** for layer, tag and sorting-order constants, so no magic strings
- **Component clipboard**, **hierarchy sorter**, **lighting profiles**, **play mode saver**, **asset zoo scene builder**, **auto start scene**, **unique IDs for ScriptableObjects**
- **Localization** - one-click Pull and Push between Unity String Table Collections and Google Sheets, with validation and per-collection reporting
- **Memory Profiler** - automated `.snap` capture on a timer or on scene load, in the editor and in development builds, so leaks show up as a timeline instead of a guess
- **Editor UI** - the shared skin every one of these windows is built from: skin-aware colors, rounded nine-sliced cards, pills and buttons, badges and draggable column dividers that remember their widths

### Utility

Serializable dictionaries, sets and a flattened 2D array, `SceneReference`, `TypeReference`, `InterfaceReference`, `SerializableDateTime`, `SerializableTimeSpan` and a validated `PersistentKey`, `Awaitable` composition helpers, a tracked coroutine runner, class-tagged rich-text logging with an optional global log handler, safe assembly reflection, platform and build flags and helpers for audio math, percentages, strings, components, rotation and time formatting.

## Installation

**Use the [BasePackageInstaller](https://github.com/Kirschkernweitwurf/BasePackageInstaller).** It is the only thing that knows how these packages fit together.

1. Open your project in Unity
2. Open the **Package Manager**
3. Click **+**, select **Install package from git URL** and paste:
   ```
   https://github.com/Kirschkernweitwurf/BasePackageInstaller.git
   ```
4. Open `Tools > Git Package Manager`
5. Tick the packages you want and hit the action button

Tick one package and everything it needs comes along, resolved and installed for you.

### Why not just paste the Git URLs

These packages depend on each other, but none of them says so in its `package.json` and that is deliberate. A `package.json` dependency is resolved through a registry and these packages are not on one. Declaring `Base.CorePackage` as a dependency of `Base.UIPackage` would make UPM go looking for it in a registry that does not have it and the install fails.

So the dependency graph lives in the installer instead, where it can be resolved against Git URLs. That is the piece you are doing by hand if you install by hand: adding a single Git URL gives you exactly that one package and Unity then compiles it against a project missing everything it needs.

### Installing manually anyway

The Git URL for a single package is:

```
https://github.com/Kirschkernweitwurf/BaseProjectPackages.git?path=BaseProject/Packages/<PackageName>
```

`<PackageName>` is the folder name from the table above, not the display name. You also have to add everything in the **Needs** column and whatever those need, until nothing is missing.

The chains get long quickly. `UI` on its own pulls in six more packages, `Content` ten. Only `Editor UI` and `Utility` stand alone.

## Optional dependencies

A setting that drives another package's feature ships with that feature rather than with the Settings package, in an assembly gated behind a version define on `com.baseprojectpackages.settings`. Without the Settings package installed those assemblies simply do not compile in and the owning package keeps working:

- `LanguageSetting` in the Localization package
- `RumbleEnabledSetting` and `RumbleIntensitySetting` in the Controller Support package
- `AutosaveEnabledSetting`, `AutosaveIntervalSetting` and `AutosaveCooldownSetting` in the Save System package

## Why?

I made this to have a solid, consistent base for all my Unity projects. It saves me from rewriting the same systems over and over. If it's useful to you too, great, go ahead and use it.

## Updates

This repo gets updated whenever I improve something in one of my projects. Expect changes over time.

## License

[PolyForm Shield 1.0.0](https://polyformproject.org/licenses/shield/1.0.0). Use it in whatever you
build, including commercial work. The one thing it does not allow is building something that competes
with these packages, so you cannot repackage them and sell them as your own library.
