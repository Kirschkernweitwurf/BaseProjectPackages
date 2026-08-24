# Base Attribute Package

100 inspector attributes for Unity. Section headers, validation, conditional fields, auto-assignment, pickers, buttons, widgets, scene handles.

The whole thing works by taking over the default inspector for every `MonoBehaviour` and `ScriptableObject`. You do not inherit from a base class and you do not write a `[CustomEditor]` per type. You tag your fields and they draw.

```csharp
using Base.AttributePackage;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Title("Stats", EColor.Red)]
    [Required] public Rigidbody body;
    [MinMax(0, 100)] public int health = 100;
    [Suffix("m/s")] public float speed = 5f;

    [ProgressBar(nameof(health), EColor.Green)] public float damageTaken;

    public bool canFly;
    [ShowIf(nameof(canFly))] public float flightSpeed;

    [Button("Reset", Confirm = "Reset this enemy?")]
    private void ResetEnemy() => health = 100;
}
```

## Requirements

- Unity `6000.3` or newer, for `TypeCache` and the standard IMGUI drawer API
- `Base.UtilityPackage` for logging and the serializable collections
- `Base.EditorUiPackage` for the shared look of its windows
- Assemblies: `Base.AttributePackage`, `Base.AttributePackage.Editor` and `Base.AttributePackage.Samples`, the last of which is editor only and never enters a build

No third-party packages.

## Two conventions used everywhere

**Colors.** Anything taking a color accepts a hex string or an `EColor` preset, whichever is less annoying to type: `[Title("Combat", "#E74C3C")]` or `[Title("Combat", EColor.Red)]`.

**Dynamic values.** Any string argument that could sensibly be computed accepts `"$" + nameof(Member)` instead of a literal. The member can be a field, a property or a parameterless method, and it is read every repaint.

```csharp
[Title("$" + nameof(SectionTitle))]
[InfoBox("$" + nameof(Status))]
public Material material;

private string SectionTitle => $"Stats ({_level})";
private string Status => _health > 0 ? "Alive" : "Dead";
```

Written as `"$" + nameof(X)` rather than as a bare string, so a rename moves the reference with it. The `$` is what separates a reference from a literal that happens to share a member's name. A reference that no longer resolves falls back to showing itself, and the troubleshoot window reports it.

Numeric bounds work the same way without the prefix, because a bound that could be a literal is passed as a number instead:

```csharp
[Slider(0f, nameof(MaxSpeed))] public float speed;
[MinMaxSlider(nameof(min), nameof(max))] public Vector2 band;
```

## Layout and display

```csharp
[Title("Section")]                       // bold header with an underline
[Title("Section", Foldout = true)]       // header that collapses everything under it
[HorizontalLine]                         // separator, [HorizontalLine(EColor.Blue, 2f)] for thickness
[InfoBox("Careful.", EInfoBoxType.Warning)]
[Indent] / [Indent(2)]                   // nudge a field to the right
[Foldout("Advanced")]                    // group consecutive fields under a collapsible header
[Tab("Combat")] / [Tab("Left", "Group")] // consecutive fields become a tab bar
[Prefix("$")] / [Suffix("m/s")]          // small label before or after the field
[GUIColor(EColor.Red)]                   // tint the field background
[Label("Display name")]                  // replace the label Unity derives from the field name
[HideMonoScript]                         // on a type, hides the Script row at the top
[StartExpanded]                          // open a nested object or array the first time it is seen
```

Title, HorizontalLine and InfoBox sit above a field the way `[Header]` does. Unlike `[Header]` they also show up above lists and arrays, which was half the reason to write them. `[InfoBox]` can move below the field with `EInfoBoxPosition.Below`.

Two read-only display helpers for things that are not serialized:

```csharp
[ShowNonSerialized] private int _tickCount;                  // shows a private runtime field, greyed out
[ShowNativeProperty] public int Doubled => _tickCount * 2;   // shows a getter
```

`[PropertyOrder]` moves a field in the inspector without moving it in the file. Serialization order is declaration order, so reordering a class to make the inspector read better is otherwise a data layout change for a cosmetic reason. Unmarked fields count as zero and the sort is stable, so one attribute moves one field.

`[Horizontal]` lays consecutive fields sharing a group name on one row, at relative widths. The run ends where the name changes, so the grouping stays visible in the order the fields are written.

```csharp
[Horizontal("size")] public float width;
[Horizontal("size")] public float height;

[Horizontal("split", Weight = 3f)] public string name;
[Horizontal("split", Weight = 1f)] public int count;
```

`[InlineProperty]` draws a nested type on the field's own row instead of behind a foldout. A type holding anything a row cannot contain falls back to the foldout rather than drawing something misleading.

## Conditional visibility

All of these take one or more member names via `nameof`. Each reference can be a bool field, a bool property or a parameterless method returning bool.

```csharp
[ShowIf(nameof(_enabled))]      // hide unless true
[HideIf(nameof(_enabled))]      // hide while true
[EnableIf(nameof(_enabled))]    // grey out unless true
[DisableIf(nameof(_enabled))]   // grey out while true

[ShowIf(nameof(_isRanged), nameof(_hasAmmo))]                     // every member must be true
[ShowIf(EConditionMode.Any, nameof(_isMelee), nameof(_unarmed))]  // at least one must be true

[ShowIfEnum(nameof(_mood), EMood.Electric)]                       // enum equals one of these
[ShowIfEnum(nameof(_mood), EMood.Electric, EMood.Sad)]

[ReadOnly]                      // never editable
```

`EConditionMode.All` is the default, so listing several members without a mode means all of them. There is no negation syntax on purpose: use `[HideIf]` instead of a `!` prefix, so nothing is encoded in a string that a rename could break.

There are play-mode variants of all four: `[ShowInPlayMode]`, `[HideInPlayMode]`, `[EnableInPlayMode]` and `[DisableInPlayMode]`.

## Validation

These flag problems in the inspector or quietly correct the value. They stack, so `[Required] [AssetOnly]` on the same field is fine.

```csharp
[Required] public Transform target;              // red box when null
[NotNullOrEmpty] public string id;               // works on strings and on lists/arrays
[MinMax(0, 100)] public int amount;              // clamps on entry, no slider
[Max(100)] public int cap;                       // upper bound only, clamps on entry
[NotZero] public float divisor;                  // pushes the value off zero
[PowerOfTwo] public int textureSize = 256;       // snaps to nearest power of two
[MaxLength(16)] public string code;              // trims text past the limit
[Unique] public List<Item> loot;                 // every entry of a list must be distinct
[AssetOnly] public GameObject prefab;            // rejects scene objects
[SceneObjectOnly] public Transform anchor;       // rejects project assets
[ValidateInput(nameof(IsEven), "Must be even.")] public int value;

private bool IsEven(int v) => v % 2 == 0;        // also works with no parameter
```

`[RequiredIf]` is `[Required]` that only fires while a condition holds, for fields that are mandatory in one setup and meaningless in another. It takes the same members and modes as `[ShowIf]`.

`[MustImplement]` restricts an object reference by type rather than by location. The picker only lists objects that qualify, dropping a GameObject resolves the first component on it that does, and an assignment that cannot be satisfied is reverted.

```csharp
[MustImplement(typeof(IDamageable))] public GameObject target;
[MustImplement(typeof(IDamageable), typeof(ISelectable))] public Component both;
```

`[Unique]` names the first duplicate pair in an error box. Null and empty entries are ignored, so a list still being filled stays quiet. Object references compare by reference, strings and value types by value.

`[MinMax]` and `[Max]` really do reset the value: type 500 with a max of 100 and it snaps back when you commit. Both also clamp component-wise on `Vector2`, `Vector3`, `Vector2Int` and `Vector3Int`.

`[ValidateInput]` methods may return a `ValidationResult` instead of a bool, which lets one validator name which of its checks failed and choose between an error and a warning:

```csharp
[ValidateInput(nameof(ValidateTexture))] public Texture2D tex;

private ValidationResult ValidateTexture()
{
    if (tex == null) return ValidationResult.Error("No texture assigned.");
    if (!tex.isReadable) return ValidationResult.Warning("The texture is not readable.");
    return ValidationResult.Valid;
}
```

`[Required]` and `[ValidateInput]` can carry a fix button, for the failures whose answer is always the same:

```csharp
[Required(FixAction = nameof(UseSelf), FixActionName = "Use self")]
public Transform spawnPoint;

private void UseSelf() => spawnPoint = transform;
```

`[ArraySize]` fixes or bounds the element count of a list or array, which is what removes the add and remove buttons: `[ArraySize(4)]` for exactly four, `[ArraySize(Min = 2, Max = 6)]` for a range.

## Auto-assignment

Fill a reference so you stop dragging things by hand. All of them only fill when the field is empty, so a manual override sticks.

```csharp
[GetComponent] public Rigidbody body;                  // same object
[GetComponentInParent] public Canvas canvas;           // strictly upward
[GetComponentInParent("Root")] public Transform root;  // named ancestor
[Child] public Renderer meshRenderer;                  // GetComponentInChildren
[Child("Muzzle")] public Transform muzzle;             // named descendant
```

Three more reach outside the object's own hierarchy, for the things a component needs a handle on but does not own:

```csharp
[GetInScene] public AudioManager audioManager;         // anywhere in the open scenes
[GetPrefabWithComponent] public Projectile defaultShot; // first prefab asset carrying the type
[GetScriptableObject] public GameConfig config;        // first asset of that type in the project
```

`[RequiredGet]` is the auto-assign and the requirement in one attribute, since on a mandatory sibling reference they are always written together anyway. It fills the field and reports it when nothing is found.

```csharp
[RequiredGet] public Collider ownCollider;
[RequiredGet(InParents = true)] public Rigidbody body;
[RequiredGet(InChildren = true, IncludeSelf = false)] public Renderer[] childRenderers;
```

## Pickers and references

```csharp
[SceneName] public string scene;                 // dropdown of build scenes (string = name)
[SceneName] public int sceneIndex;               // on an int it stores the build index instead
[FolderPath] public string folder;               // "..." button, stores "Assets/..."
[FilePath("png")] public string texture;         // filtered by extension
[ResourcesAsset] public string res;              // picks an asset under a Resources folder
[ShowAssetPreview] public Texture2D icon;        // thumbnail under the field
[Tag] public string tagName;                     // tag dropdown, [Tag(true)] to forbid new tags
[Layer] public int spawnLayer;                   // single layer, or the name on a string
[SortingLayer] public string sortingLayer;       // sorting layer name, or the id on an int
[ComponentPicker] public Collider hit;           // drop a GameObject, it picks the matching component
[OpenAsset] public TextAsset config;             // button that opens the asset in its editor
```

Animator, audio and shader pickers resolve their options from a sibling field:

```csharp
public Animator animator;
[AnimatorParam(nameof(animator))] public string param;       // stores the name, or the hash on an int
[AnimatorState(nameof(animator))] public string state;       // "LayerName.StateName"

public AudioMixer mixer;
[AudioMixerParameter(nameof(mixer))] public string exposedParam;
[AudioMixerGroup(nameof(mixer))] public AudioMixerGroup group;

public Material material;
[ShaderParam(nameof(material))] public string property;      // any shader property, or the id on an int
[ShaderParam(nameof(material), EShaderParamType.Color)] public string tint;
[ShaderKeyword(nameof(material))] public string keyword;     // the keyword sibling of ShaderParam
```

`[ShaderParam]` and `[ShaderKeyword]` also accept a `Renderer` or a `Shader` field as their source.

`[AssetDropdown]` replaces an object field with a searchable dropdown of matching project assets, so a reference can be picked by name instead of found in the Project window and dragged. The filter is the string the Project window search itself takes, derived from the field type when omitted.

```csharp
[AssetDropdown] public Material mat;
[AssetDropdown("t:Prefab", "Assets/Enemies")] public GameObject enemy;
```

## Editing referenced assets

`[Expandable]` adds a toggle next to an asset reference that draws the asset's own inspector inline, so a ScriptableObject can be edited without changing the Project window selection.

`[ReferencePicker]` gives a `[SerializeReference]` field the type picker Unity does not provide, so the concrete implementation can be chosen and swapped in the inspector. The picker is searchable and grouped by namespace.

```csharp
[Expandable] public WeaponConfig config;
[SerializeReference] [ReferencePicker] public IAbility ability;
[SerializeReference] [ReferencePicker] public List<ICondition> conditions;
```

## Buttons and widgets

```csharp
[Button] private void Rebuild() { }
[Button("Danger", Mode = EButtonMode.PlayMode, Confirm = "Sure?")] private void Nuke() { }

[InlineButton(nameof(Randomize), "Roll")] public int rolled;     // button next to the field
[ClearButton] public string note;                                // inline button that empties the field
[CopyButton] public string id;                                   // inline button that copies the value

[Dropdown(nameof(Options))] public string choice;                // options from a member
[ProgressBar(100f, EColor.Green)] public float health;           // drag to set the value
[ProgressBar(nameof(maxMana), EColor.Blue)] public float mana;   // dynamic max from a member

[Slider(0f, 10f)] public float speed;
[MinMaxSlider(0, 100)] public Vector2 range;                     // one slider with two handles
[Percentage] public float ratio;                                 // shows 0..1 as a percent
[Rate(1, 5)] public int difficulty;                              // a row of clickable stars
[CurveRange(0, 0, 1, 1, EColor.Cyan)] public AnimationCurve curve;
[ColorPalette(nameof(Swatches))] public Color tint;              // restricted to a project palette
[EnumToggleButtons] public MyEnum mode;                          // enum as a row of buttons

[Date] public long eventStart;                                   // year, month, day plus a calendar picker
[Date(EDateDisplay.DateAndTime)] public long lastBuilt;
[Time(ShowDays = true)] public long cooldown;                    // signed d : h : m : s duration row
```

Buttons and the read-only native members render at the bottom of the inspector, after your fields.

Three attributes put things in the component header instead, where they cost no vertical space and stay reachable while the component is collapsed. They lay out right to left in declaration order.

```csharp
[HeaderButton("Open")] private void OpenWindow() { }
[HeaderLabel] private string Version => _version;      // one fact worth seeing while collapsed
[HeaderDraw] private void DrawBadge(Rect rect) { }     // the escape hatch, draw whatever you like
```

`[Date]` and `[Time]` both sit on a `long` of ticks. `[Date]` is a point in time and `[Time]` is a duration, and on a bare `long` the attribute is the only thing that says which, which is why they name the meaning rather than the layout. Both also work on the Utility package's `SerializableDateTime` and `SerializableTimeSpan`, where the type already says it and the attribute only narrows which fields are drawn. A unit that is switched off keeps what it held rather than being dropped, so a two day cooldown drawn without the day field reads as forty eight hours.

`[Dropdown]` switches from a plain popup to a searchable tree once there are more than a handful of options. Options containing a slash become submenus.

`[ColorPalette]` reads its swatches from another member, so a project stays on its palette instead of picking from the full wheel.

There are three change callbacks:

```csharp
[OnValueChanged(nameof(OnHealthChanged))] public int health;
private void OnHealthChanged() { }               // fires when the field is edited in the inspector

[OnArraySizeChanged(nameof(OnSlotsResized))] public List<Item> slots;
private void OnSlotsResized(int size) { }        // fires only when the element count changes

[OnCollectionChanged(nameof(Before), nameof(After))] public List<Item> inventory;
```

`[OnArraySizeChanged]` accepts a parameterless method or one taking a single int. `[OnCollectionChanged]` runs a method before and after the count changes, for the cases that need to see both states. Edits to element values that keep the count fire neither; use `[OnValueChanged]` for those.

`[PreviewObject]` draws a preview large enough to judge, interactive where the asset supports it:

```csharp
[PreviewObject] public GameObject prefab;                          // drag inside it to rotate
[PreviewObject(64f, Width = 64f)] public Texture2D icon;
[PreviewObject(160f, Foldout = true, DefaultExpanded = false)] public Mesh mesh;
```

## Scene handles

Five attributes turn a serialized value into something you drag in the scene view instead of typing.

```csharp
[PositionHandle] public Vector3 spawnOffset;     // movable gizmo, local space by default
[RotationHandle] public Quaternion facing;
[ScaleHandle] public Vector3 boxSize;
[RadiusHandle] public float attackRange;         // draggable circle, sized by eye
[SceneViewPicker] public Transform target;       // pick button, next scene click assigns what was hit
```

Three more draw without being draggable, for orientation while authoring: `[DrawLine]`, `[DrawLabel]` and `[DrawWireDisc]`. `ESpace` and `ENormalAxis` control the space and the plane a handle works in.

Handles draw for the selected object, so they only apply to components, not to ScriptableObject assets.

## Collections

Lists stay Unity's own list. `[ListDrawerSettings]` only tells that list what to do through its callbacks, so a list with the attribute reorders, selects and resizes exactly like a list without one.

```csharp
[ListDrawerSettings(Searchable = true)]                  // search box that hides rows whose label misses
[ListDrawerSettings(ConfirmDelete = true)]               // removing a row asks first, naming the row
[ListDrawerSettings(ShowAlternatingBackground = false)]  // tinting off, on by default
```

Rows are named after the element's first string field, with nothing to configure. Searching hides a row by giving it a height of zero rather than by drawing a different list, so a filtered list is the same control with fewer rows. Dragging switches off while a filter is on, because the row above is then not the element above.

`[Table]` draws an array of a serializable type as a grid instead: one row per element, one column per field, on the same list. Columns come from the first element, so an empty table shows only its header. `[TableColumn]` on the element's fields changes a column's relative width, its header text, or hides it.

```csharp
[Table] public List<LootEntry> loot;

[Serializable]
public sealed class LootEntry
{
    [TableColumn(2f)] public string id;
    [TableColumn(Header = "Qty")] public int amount;
    [TableColumn(Hidden = true)] public string note;
}
```

There is deliberately no paging, no drag toggle and no per-list add or remove switch. Anything that would need a second implementation of a list is left out: two renderers that have to look identical never quite do, and the difference shows up as a layout bug rather than as a missing feature.

## Attribute Explorer

`Tools > Base Packages > Unity Editor > Project Health > Attributes`

One window, three tabs. It is both the documentation and the thing that tells you when an attribute is not doing what it reads like.

**Reference** is one page per attribute, all 100 of them, searchable and grouped. Each page carries a live sample drawn through the real inspector, what the sample needs before it does anything, the other ways the attribute can be written, and the source of the whole sample class underneath.

The samples are one class per attribute, which is what lets the page draw the whole object and print the whole class: everything in a sample is part of the answer, including the bool a condition watches and the property a dropdown reads its options from. The snippet is therefore paste-and-compile by construction.

The attributes needing a GameObject rather than an asset have samples that are components. Their pages carry a **Create in scene** button that drops a temporary copy into the open scene and selects it, which is the only way to see a scene handle or a component header control: handles draw for the selected object in the Scene view, and header controls are drawn by the real Inspector rather than by an embedded one. That copy is never saved with the scene.

**Showcase** is a throwaway asset carrying one of every attribute, drawn through the real inspector. Edit anything, nothing is saved. Useful for seeing several attributes against each other rather than one at a time.

**Troubleshoot** exists because every drawer and handler in this package fails quietly on purpose: an attribute that cannot resolve what it points at falls back to the plain field, so a typo never breaks the whole inspector. That is the right runtime behavior and a terrible way to find mistakes, because the fallback is only visible while the affected object happens to be selected.

Press **Scan** and the window walks every component, ScriptableObject and serializable type and lists what cannot work:

- a condition member that does not exist or is not a bool, which makes the condition evaluate to true forever
- `[Dropdown]` pointing at something that is not enumerable
- `[AnimatorParam]`, `[AnimatorState]`, `[AudioMixerParameter]`, `[ShaderParam]` or `[ShaderKeyword]` whose sibling field is missing or of the wrong type
- an attribute on a field type its drawer cannot handle, for example `[Required]` on an int
- `[GetComponent]`, `[Child]` or `[GetComponentInParent]` on a non-component type, on a `GameObject` field, or on a ScriptableObject that has no hierarchy to search
- `[Button]` or `[HeaderButton]` on a method with parameters, which the renderer skips
- `[OnValueChanged]`, `[OnArraySizeChanged]`, `[InlineButton]` or `[ValidateInput]` whose target method is gone or no longer matches the expected signature
- `[ReferencePicker]` without `[SerializeReference]`, or on a type with no instantiable implementation

Click a row's header to open the script. Errors mean the attribute does nothing; warnings mean it works but not the way it reads.

A healthy project produces an empty report, which is the right outcome and a useless way to learn what the window looks like. The **Demo types** toggle scans a set of types that are broken on purpose, one per family of mistake. Those types are never part of a project scan, and the toggle clears itself on every tab switch so a demo report can never be mistaken for the project's own state. They are also the test fixture: if a check stops working, its sample stops being reported.

Adding a check is one file. Implement `IAttributeCheck` anywhere and `TypeCache` picks it up.

The scan is manual rather than continuous, because walking every type in the project is not cheap enough to run on a timer. A domain reload clears the result instead of showing a stale one.

## The other three windows

- **Required Reference Overview** lists every validation issue in the open scenes and on ScriptableObject assets, refreshing live. Scene issues rescan often; asset issues are cached and refreshed on demand.
- **Assign GetComponents** (`Tools > Base Packages > Unity Editor > References`) fills every empty `[GetComponent]` and `[GetComponentInParent]` field on prefab assets and in the open scenes in one pass, so references resolve without opening each object.
- **GetComponent Require Audit** (same menu) lists `[GetComponent]` fields whose class is missing a matching `[RequireComponent]`. Each row opens the offending script. `[GetComponentInParent]` is ignored, since the component is not expected on the same object.

## How it works

There are three ways a thing gets drawn:

- **Decorators handled in the inspector** (titles, lines, validation, conditions, auto-assign) run through a small handler pipeline. Each attribute is one tiny handler class.
- **Property drawers** (tag, curve, enum buttons, pickers, progress bar, inline button) replace how a single field renders.
- The **inspector itself** only does grouping (foldouts, tabs, horizontal rows) and hands each field to the pipeline.

There is a fourth thing that is not drawing at all: **checks**, which the troubleshoot window runs over the whole project.

Handlers are discovered with `TypeCache`, so adding one is dropping in a file. Pick the interface that matches when it should run (`IBeforeFieldHandler`, `IVisibilityHandler`, `IEnableHandler`, `IAfterFieldHandler`, `IFieldReplacementHandler` or `IInlineFieldWidget`) or write a normal `PropertyDrawer` if you are replacing the field. No registration step.

## Custom editors

The package draws every `MonoBehaviour` and `ScriptableObject` through a `[CustomEditor]` on the base types. The moment you write your own `[CustomEditor]` for a specific type, Unity picks the more specific one and this package's inspector drops out, taking all the attribute drawing with it.

Derive from `AttributePackageEditor` instead of `UnityEditor.Editor` and call `base.OnInspectorGUI()`. This is required for every custom editor you write if you want the attributes to keep working.

```csharp
[CustomEditor(typeof(Enemy))]
public sealed class EnemyEditor : AttributePackageEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();   // draws the tagged fields, buttons and native members
        // your extra inspector GUI here
    }
}
```

## Turning the inspector off

`Tools > Base Packages > Attributes > Disable Attribute Inspector` stops the package drawing inspectors without uninstalling it.

It exists because of the reach the `[CustomEditor]` registration has. Every `MonoBehaviour` and every `ScriptableObject` in the project goes through this package's inspector, including third-party components that never asked for it, so a fault anywhere in the pipeline shows up everywhere at once. This is the way out that is not a git revert.

It turns off the inspector pipeline: titles, foldouts, tabs, horizontal rows, conditional visibility and enabling, validation messages, auto-assignment, buttons, native members. Unity's own inspector draws instead.

What keeps working, by design:

- **Property drawers.** Roughly thirty attributes are `PropertyDrawer`s. Unity calls those straight from `PropertyField`, so they still render. There is no way to route around that and no reason to want one: a drawer is scoped to one field of one type, so it cannot take the project down the way the global registration can.
- **Header controls.** `[HeaderButton]`, `[HeaderLabel]` and `[HeaderDraw]` are registered with Unity's component header, which cannot be unregistered from cleanly. The switch is read once at load, so they disappear on the next domain reload rather than immediately.
- **Scene handles.** They draw from `OnSceneGUI`, which Unity calls separately from the inspector.

The setting lives in `EditorPrefs` under `Base.AttributePackage.InspectorDisabled`, so it is per user and per machine, not committed with the project.

## A type with no attributes skips the pipeline

Even with the inspector on, a type declaring nothing from this package is handed straight back to Unity. The check walks the type and its base classes once, asks whether any attribute on the type or on any of its members comes from the `Base.AttributePackage` assembly, and caches the answer until the next domain reload. Ownership is decided by assembly rather than by a list of names, so a new attribute added to the package is recognized with nothing to update.

This is why a plain `BoxCollider` or a third-party script looks exactly like it does in a project without this package installed.

## Things worth knowing

- Unity only lets one package own the default inspector. If you also pull in Odin, NaughtyAttributes or similar, they will fight over it and one loses silently.
- The pipeline reaches into nested `[Serializable]` structs and classes at any depth, so validation, conditional and layout attributes work on their fields too. It stops descending in three cases, handing those to Unity's default drawing: arrays and lists (attributes on fields of list elements are skipped), types that have their own `PropertyDrawer`, and Unity or framework types like `Vector3`.
- The serialized collections live in `Base.UtilityPackage`, not here. `SerializableDictionary<,>`, `SerializableHashSet<>` and `InterfaceReference<>` each ship with their own drawer, and the pipeline hands any type with a `PropertyDrawer` straight to that drawer, so their attributes are not evaluated on the inner rows.