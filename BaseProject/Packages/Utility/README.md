# Base Utility Package

General-purpose runtime and editor helpers. It sits at the bottom of the dependency graph: no dependencies at all, so every other Base package can build on it.

## Requirements

- Unity `6000.3` or newer
- No dependencies, Base or otherwise
- Assemblies: `Base.UtilityPackage` and `Base.UtilityPackage.Editor`

## Serialization

The things Unity cannot serialize on its own, each with the drawer that makes it usable in the inspector.

- **`SerializableDictionary<TKey, TValue>`** keeps a serialized entry list and a runtime dictionary in sync, so lookups stay O(1) after inspector edits. Implements `IDictionary<,>` and `IReadOnlyDictionary<,>`, so it drops into any API expecting a dictionary. Its drawer shows key-value rows on Unity's own reorderable list rather than the nested entry list Unity would draw, and tints duplicate keys, which the runtime dictionary would otherwise drop without a word.
- **`SerializableHashSet<T>`** does the same for `ISet<T>`.
- **`SceneReference`** references a scene by asset instead of by name, so moving or renaming the scene file keeps the reference intact. The path, name and build index are cached alongside it, because runtime code cannot ask the asset database anything.
- **`TypeReference`** serializes a `Type`. The assembly qualified name is what persists; the resolved type is cached and rebuilt after every deserialization. `TypeReferenceOfBase<TBase>` constrains the picker to types assignable to `TBase`, with the constraint in the type argument rather than in an attribute so it survives renames. `[TypeScope]` and `ETypeScope` widen the picker beyond project code for the rare field that has to point at a Unity or framework type.
- **`InterfaceReference<TInterface, TObject>`** serializes a Unity object while exposing it through an interface. `Value` returns the interface and is null-safe against destroyed objects, `UnderlyingValue` returns the Unity object. `InterfaceReference<TInterface>` is the common form that accepts any Unity object implementing the interface, so only the interface has to be named at the use site.
- **`SerializableDateTime`** and **`SerializableTimeSpan`** keep a `DateTime` or `TimeSpan` as its tick count, because Unity refuses every type declared in the core library. Both convert implicitly, so they drop into any API expecting the real type.

```csharp
[SerializeField] private InterfaceReference<IDamageable> target;
[SerializeField] private SerializableDateTime eventStart;
[SerializeField] private SerializableTimeSpan cooldown;

private void Hit() => target.Value?.TakeDamage(1);
private bool IsOpen() => DateTime.Now >= eventStart.Value;
```

The interface drawer restricts assignment to objects that implement the interface. Dropping a GameObject or a component that does not implement it directly resolves the first component on that object which does, so dragging a whole prefab works without hunting for the right component.

Ticks and not seconds, because a float of seconds stops being able to name a single second once the value gets far enough from zero, which is exactly where calendar dates live. `SerializableDateTime` stores no `DateTimeKind`: a date typed into an inspector has no time zone behind it, and carrying one would make the value look authoritative about something nobody was ever asked. Convert where it actually matters.

The Attributes package's `[Date]` and `[Time]` narrow which fields of the two rows are drawn.

## Async

- **`AwaitableUtility`** composes `Awaitable` operations. `Completed` and `FromResult<T>` hand back an already finished awaitable, for an implementation that has to satisfy an async signature without doing async work. `WhenAll` waits for a whole batch and reports every failure instead of only the first. `WithTimeout` runs an operation under a deadline and cancels it once the deadline passes.
- **`AwaitableExtensions`** covers the two cases a plain await does not. `Forget` awaits an operation nobody is waiting for, so a failure is logged instead of swallowed and the awaitable returns to Unity's pool. `HasCompleted` reports an expected cancellation as `false` rather than throwing.

`WithTimeout` takes the work as a `Func<CancellationToken, Awaitable>` rather than a running awaitable. An `Awaitable` is pooled and can only be awaited once, so there is no safe way to cancel one from the outside after it was created: the instance may already have been recycled. Handing the deadline token into the operation is what makes a timeout actually stop the work. That is also why there is no `WhenAny`: every version of it leaves a loser that is either canceled unsafely or never awaited at all.

## Logging

- **`CustomLogger`** prefixes `Log`, `LogWarning` and `LogError` with the calling class name, colored and bolded. The class name comes from `[CallerFilePath]`, so there is no stack walk, and prefixes are cached per call site.
- **`CustomLogHandler`** wraps Unity's own log handler so plain `Debug` calls get the same class tag. That one does cost a stack trace lookup per message, so it only resolves callers in the editor and in development builds.
- **`CustomLogHandlerBootstrap`** installs the handler at startup in a build. In the editor a toggle under `Tools > Base Packages > Unity Editor > Logging > Enable Custom Log Handler` decides, off by default.
- **`LogTextFormatter`** and **`EDebugLogColors`** are the rich-text helpers: `Bold`, `Italic`, `Underline`, `Colorize`, `Size` and an editor marker tag.
- **`CustomLoggingUtils`** derives a stable color from a name, builds the styled `[ClassName]` prefix and returns the edit mode marker.

## Identification

- **`PersistentKey`** is a validated value type for a stable, human-authored key that survives a round trip to disk. A valid key is non-empty, free of surrounding whitespace, control characters, path separators and quotes, so it stays safe to compose into storage keys. Value equality makes it a safe dictionary key, and the underlying string is what is written, so the persisted format stays independent of code structure. Expose one `static readonly PersistentKey` per owner as that owner's single source of truth.
- **`IUniquelyIdentifiable`** is the contract for anything carrying a regenerable unique id, and **`UniqueIdScriptableObject`** is the ScriptableObject that holds one. The Tools package assigns and validates these project wide.

## Collections

- **`FlattenedArray<T>`** is a 2D grid backed by a single 1D array, with `Width`, `Height`, `Length`, an `[x, y]` indexer, `Get`, `Set` and iteration.
- **`CollectionExtensions`** has `Single<T>`, which wraps one element as an `IEnumerable<T>` without allocating a list, and `GetRandomElement<T>` for any `IList<T>`.

## Contracts and menus

- **`IMenuResettable`** is implemented by components that should reset to a known baseline when their owning menu closes. It lives here rather than next to the menu system because the two sides of the contract sit in different packages: the Core package's menus call it, the Tweening package's `TweenGroup` implements it.
- **`[DynamicMenuItem]`** and **`[DynamicCreateAssetMenu]`** mark a static method or a ScriptableObject type as a data driven menu entry, with the path and priority managed in the Tools package's Menu Manager window rather than hardcoded in the attribute.
- **`[CodebaseGraphIgnore]`** marks a type or member the Codebase Graph must never report on, for findings that are wrong for a reason the scan cannot see and never will.

All three live here, at the bottom of the graph, so any package can be tagged without depending on the Tools package that reads them.

## Runtime helpers

| Type | What it does |
|---|---|
| `Platform` | Runtime flags for platform and build conditions (`IsUnityEditor`, `IsWindows`, `IsMobile`, `IsRelease` and more) plus `IsEditorMode()`, so code branches on platform without preprocessor directives |
| `UnityObjectUtility` | Working with `Object` references from non-Unity static types, where the fake-null overload is not in scope |
| `ReflectionUtility` | Safe reflection over the assemblies loaded in the current domain |
| `CoroutineRunner` | Lets non-MonoBehaviour classes run coroutines, next frame, after N frames or after a delay, tracked so they can be stopped individually or all at once |
| `CustomSingleton<T>` | Generic singleton base with an optional `DontDestroyOnLoad`, warning on and destroying duplicates |
| `InstantiationUtility` | `CleanInstantiate` spawns a prefab, strips the `(Clone)` suffix and optionally parents it or marks it `DontDestroyOnLoad` |
| `ComponentUtility` | `TryGetComponentInParent<T>` extensions for `Object`, `GameObject` and `Component` |
| `AudioMathUtility` | Linear-to-decibel and back |
| `PercentageUtils` | Normalized values to percentages and a `"56%"` formatter |
| `RotationUtility` | `NormalizeAngle` to `[-180, 180]` and `ApproximatelyEqual` for quaternions |
| `StringUtility` | `NicifyVariableName` turns a raw field name into a readable display name |
| `TimeFormattingExtensions` | Formats a second count as `"2 hours, 5 minutes and 30 seconds"` |
| `TypeExtensions` | `bool.ToInt()` and `int.ToBool()` |

## Editor helpers

| Type | What it does |
|---|---|
| `AssetDatabaseUtility` | The repetitive parts of querying the AssetDatabase |
| `CustomEditorUtility` | `FindProp` locates a `SerializedProperty` by nice name or `k__BackingField` name |
| `PropertyDrawerUtility` | `DrawObjectPopup<T>` draws an object-reference popup with a "None" entry, allocation-free |
| `SerializableDefaults` | Base for `[Serializable]` containers that need C# field defaults applied to instances Unity allocates without running the constructor, such as a new list element added with the inspector's "+" button |
| `SearchableDropdown` | A searchable, tree-shaped dropdown built from a flat label list. Labels containing slashes become nested submenus, matching the Add Component menu |
| `LabeledField` | Draws a labeled control while keeping the label's tooltip, which the convenient `EditorGUI` string overloads silently throw away |
| `NoIndentScope` | Draws a block at indent zero and restores the previous level |
| `EditorConstants` | Shared editor constants such as `ScriptPropertyName` |

The drawers for the serializable types above live here too, in `Editor/Collections` and `Editor/Serialization`, along with the shared pieces they are built from: `TickProperty` resolves the tick property behind a bare `long` or a wrapper struct so every date and duration drawer agrees on what it was pointed at, `TimeUnitField` draws one number cell with its unit letter, and `CalendarPopup` is the month grid behind the calendar button.