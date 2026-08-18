# Base Utility Package

General-purpose runtime and editor helpers for Unity projects. Everything lives under the
`Base.UtilityPackage.*` namespace and is split into a runtime assembly (`Base.UtilityPackage`) and an
editor-only assembly (`Base.UtilityPackage.Editor`).

- **Unity:** 6000.3
- **Dependencies:** none
- **Author:** Jonathan Alber

## Installation

Add the package through the Package Manager using **Add package from git URL** or by referencing the local
folder in your `manifest.json`.

## Runtime

### Collections (`Base.UtilityPackage.Collections`)

- **`CollectionExtensions`** - `Single<T>` wraps one element as an `IEnumerable<T>` without allocating a list.
  `GetRandomElement<T>` returns a random entry from any `IList<T>`, arrays included, with null and empty guards.
- **`FlattenedArray<T>`** - a 2D grid backed by a single 1D array. Exposes `Width`, `Height`, `Length`, an
  `[x, y]` indexer, `Get`, `Set` and `IEnumerable<T>` iteration.
- **`SerializableDictionary<TKey, TValue>`** - a dictionary that serializes and edits in the Inspector. Keeps a
  serialized entry list and a runtime dictionary in sync, so lookups stay fast after Inspector edits.
  Implements `IDictionary<,>` and `IReadOnlyDictionary<,>`, so it drops into any API expecting a dictionary.
  Its drawer shows key-value rows instead of a nested entry list and flags duplicate keys, which the runtime
  dictionary would otherwise drop without a word.
- **`SerializableDictionaryEntry<TKey, TValue>`** - the serializable key-value struct backing the dictionary.
- **`SerializableHashSet<T>`** - a set that serializes and edits in the Inspector. Implements `ISet<T>`, keeps a
  serialized item list and a runtime set in sync, and flags duplicates in its drawer.

### Contracts (`Base.UtilityPackage.Contracts`)

- **`IMenuResettable`** - implemented by components that should reset to a known baseline when their owning
  menu closes. It lives here rather than next to the menu system because the two sides of the contract sit in
  different packages: the Core package's menus call it, the Tweening package's `TweenGroup` implements it.

### Identification (`Base.UtilityPackage.Identification`)

- **`PersistentKey`** - a validated value type for a stable, human-authored key that survives a round trip to
  disk. Used wherever a persisted entry has to be matched back to the code that owns it, such as a settings
  key or a save entry. Expose one `static readonly PersistentKey` per owner as that owner's single source of
  truth.
- **`IUniquelyIdentifiable`** - the contract for anything carrying a regenerable unique id.
- **`UniqueIdScriptableObject`** - a ScriptableObject that holds a GUID and can regenerate it. The Tool
  package's editor tooling assigns and validates these project wide.

### Logging (`Base.UtilityPackage.Logging`)

- **`CustomLogger`** - `Log`, `LogWarning` and `LogError` prefix each message with the calling class name,
  colored and bolded. The class name comes from `[CallerFilePath]` and prefixes are cached per call site.
- **`CustomLogHandler`** - wraps Unity's own log handler so plain `Debug` calls get the same class tag. Costs a
  stack trace lookup per message, so it only resolves callers in the editor and in development builds.
- **`CustomLogHandlerBootstrap`** - installs the handler on startup. Toggle it under
  `Tools > Base Packages > Unity Editor > Logging > Enable Custom Log Handler`. Off by default in the editor.
- **`LogTextFormatter`** - rich-text helpers: `Bold`, `Italic`, `Underline`, `Colorize`, `Size` and an
  `EditorMarker` tag.
- **`EDebugLogColors`** - the color set used by `Colorize`.
- **`CustomLoggingUtils`** - `GetColor` derives a stable hex color from any name, `BuildClassTag` builds the
  styled `[ClassName]` prefix and `GetEditorMarker` returns the edit mode marker.

### Menus (`Base.UtilityPackage.Menus`)

- **`DynamicMenuItemAttribute`** - marks a static method as a data driven editor menu item. Path and priority
  are managed in the Tool package's Menu Manager window instead of being hardcoded in the attribute.
- **`DynamicCreateAssetMenuAttribute`** - the same idea for `CreateAssetMenu` entries on a ScriptableObject.

Both attributes live here, at the bottom of the dependency graph, so every package can be tagged without
depending on the Tool package that reads them.

### Root (`Base.UtilityPackage`)

- **`AudioMathUtility`** - linear-to-decibel and decibel-to-linear conversion.
- **`ComponentUtility`** - `TryGetComponentInParent<T>` extensions for `Object`, `GameObject` and `Component`.
- **`CoroutineRunner`** - a singleton `MonoBehaviour` that lets non-`MonoBehaviour` classes run coroutines. Runs
  coroutines with an optional completion callback, next frame, after N frames or after a delay. Tracks and stops
  them individually or all at once.
- **`CustomSingleton<T>`** - a generic singleton base with an optional `DontDestroyOnLoad` toggle. Warns on and
  destroys duplicate instances.
- **`InstantiationUtility`** - `CleanInstantiate` spawns a prefab, strips the `(Clone)` suffix and optionally
  parents it or marks it `DontDestroyOnLoad`.
- **`PercentageUtils`** - convert between normalized values and percentages plus a `"56%"` style formatter.
- **`Platform`** - runtime flags for platform and build conditions (`IsUnityEditor`, `IsWindows`, `IsMobile`,
  `IsRelease` and more) plus `IsEditorMode()`. Lets you branch on platform without preprocessor directives.
- **`RotationUtility`** - `NormalizeAngle` to `[-180, 180]` and `ApproximatelyEqual` for quaternions.
- **`StringUtility`** - `NicifyVariableName` turns a raw field name into a readable display name.
- **`TimeFormattingExtensions`** - `ToMinutesSecondsText` formats a second count as
  `"2 hours, 5 minutes and 30 seconds"`.
- **`TypeExtensions`** - `bool.ToInt()` and `int.ToBool()`.

### Serialization (`Base.UtilityPackage.Serialization`)

- **`InterfaceReference<TInterface, TObject>`** - serializes a Unity object while exposing it through an
  interface, which Unity cannot serialize on its own. `Value` returns the interface and is null-safe against
  destroyed objects; `UnderlyingValue` returns the Unity object itself.
- **`InterfaceReference<TInterface>`** - the common form that accepts any Unity object implementing the
  interface, so only the interface has to be named at the use site.

The drawer restricts assignment to objects that implement the interface. Dropping a GameObject or a component
that does not implement it directly resolves the first component on that object which does, so dragging a whole
prefab works without hunting for the right component.

```csharp
[SerializeField] private InterfaceReference<IDamageable> target;

private void Hit() => target.Value?.TakeDamage(1);
```

### Generated (`Base.UtilityPackage.Generated`)

- **`MenuOrders`** - auto-generated menu priority constants. Do not edit by hand.

## Editor (`Base.UtilityPackage.Editor`)

- **`CustomEditorUtility`** - `FindProp` locates a `SerializedProperty` by its nice name or its
  `k__BackingField` name, on a `SerializedObject` or relative to a parent property.
- **`PropertyDrawerUtility`** - `DrawObjectPopup<T>` draws an object-reference popup from a list of options with
  a "None" entry, allocation-free and writing back only on change.
- **`SerializableDefaults`** - a base class for `[Serializable]` containers that applies field defaults exactly
  once, covering the case where Unity adds a list element without running your constructor.
- **`EditorConstants`** - shared editor constants such as `ScriptPropertyName` (`m_Script`).
- **`Collections.SerializableCollectionGui`** - shared row metrics and duplicate detection for the serialized
  collection drawers, so the dictionary and the set look and behave the same.
- **`Collections.SerializableDictionaryDrawer`** - draws key-value rows with add and remove buttons.
- **`Collections.SerializableHashSetDrawer`** - draws a flat item list with add and remove buttons.
- **`Serialization.InterfaceReferenceDrawer`** - draws an interface reference as a filtered object field.