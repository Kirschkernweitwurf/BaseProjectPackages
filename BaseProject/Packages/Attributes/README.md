# Attribute Package

A pile of inspector attributes for Unity. Section headers, validation, conditional fields, auto-assignment, pickers, buttons, progress bars, the usual stuff you keep wishing Unity had built in once a project gets big enough.

The whole thing works by taking over the default inspector for every `MonoBehaviour` and `ScriptableObject`. You don't inherit from a base class and you don't write a `[CustomEditor]` per type. You just tag your fields and they draw.

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

## Installing

Needs Unity 6000.3 or newer, the version the package manifest targets (it leans on `TypeCache` and the standard IMGUI drawer API).

Either add it through the Package Manager with *Add package from git URL* or just drop the folder into your project. There are three assemblies: one for the attributes (runtime), one for the drawing (editor only), and one holding the reference samples, which is compiled only in the editor and never enters a build.

## Colors

Anything that takes a color accepts either a hex string or a preset from the `EColor` enum, whichever you find less annoying to type:

```csharp
[Title("Combat", "#E74C3C")]
[Title("Combat", EColor.Red)]
```

## Dynamic values

Any string argument that could sensibly be computed accepts `"$" + nameof(Member)` instead of a literal.
The member can be a field, a property or a parameterless method, and it is read every repaint.

```csharp
[Title("$" + nameof(SectionTitle))]
[InfoBox("$" + nameof(Status))]
[Label("$" + nameof(Caption))]
[Required("$" + nameof(WhyItMatters))]
public Material material;

private string SectionTitle => $"Stats ({_level})";
private string Status => _health > 0 ? "Alive" : "Dead";
```

Written as `"$" + nameof(X)` rather than as a bare string, so a rename still moves the reference with it.
The `$` is what separates a reference from a literal that happens to share a member's name, and it is the
only string-encoded part of the convention. A reference that no longer resolves falls back to showing
itself, and the troubleshoot window reports it.

Numeric bounds work the same way without the prefix, because a bound that could be a literal is passed as
a number instead:

```csharp
[Slider(0f, nameof(MaxSpeed))] public float speed;
[Slider(nameof(range))] public float withinRange;      // one Vector2 supplies both ends
[MinMaxSlider(nameof(min), nameof(max))] public Vector2 band;
```

## Layout and display

```csharp
[Title("Section")]                       // bold header with an underline
[Title("Section", Foldout = true)]       // header that collapses everything under it
[HorizontalLine]                         // just a separator, [HorizontalLine(EColor.Blue, 2f)] for thickness
[InfoBox("Careful.", EInfoBoxType.Warning)]
[Indent] / [Indent(2)]                   // nudge a field to the right
[Foldout("Advanced")]                    // group consecutive fields under a collapsible header
[Tab("Combat")] / [Tab("Left", "Group")] // consecutive fields become a tab bar
[Prefix("$")] / [Suffix("m/s")]          // small label before or after the field
[GUIColor(EColor.Red)]                   // tint the field background, [GUIColor("#E74C3C")] for hex
```

Title, HorizontalLine and InfoBox sit above a field the way `[Header]` does. Unlike `[Header]` they also show up above lists and arrays, which was half the reason to write them. `[InfoBox]` can move below the field with `EInfoBoxPosition.Below`.

Two read-only display helpers for things that aren't serialized:

```csharp
[ShowNonSerialized] private int _tickCount;   // shows a private runtime field, greyed out
[ShowNativeProperty] public int Doubled => _tickCount * 2;   // shows a getter
```

`[PropertyOrder]` moves a field in the inspector without moving it in the file. Serialization order is
declaration order, so reordering a class to make the inspector read better is otherwise a data layout
change for a cosmetic reason. Unmarked fields count as zero and the sort is stable, so one attribute moves
one field.

```csharp
public float drawnSecond;
[PropertyOrder(-1)] public float drawnFirst;
```

`[Horizontal]` lays consecutive fields sharing a group name on one row, at relative widths. The run ends
where the name changes, so the grouping stays visible in the order the fields are written.

```csharp
[Horizontal("size")] public float width;
[Horizontal("size")] public float height;

[Horizontal("split", Weight = 3f)] public string name;
[Horizontal("split", Weight = 1f)] public int count;
```

`[InlineProperty]` draws a nested type on the field's own row instead of behind a foldout. A type holding
anything a row cannot contain falls back to the foldout rather than drawing something misleading.

```csharp
[InlineProperty] public Range range;              // Range  [min] [max]
[InlineProperty(LabelWidth = 50f)] public Range wide;
```

`[Label]` replaces the label Unity derives from the field name, and `[HideMonoScript]` on a type hides the
Script row at the top of its inspector.

## Conditional visibility

All of these take one or more member names via `nameof`. Each reference can be a bool field, a bool property or a parameterless method returning bool.

```csharp
[ShowIf(nameof(_enabled))]      // hide unless true
[HideIf(nameof(_enabled))]      // hide while true
[EnableIf(nameof(_enabled))]    // grey out unless true
[DisableIf(nameof(_enabled))]   // grey out while true

[ShowIf(nameof(_isRanged), nameof(_hasAmmo))]                  // every member must be true
[ShowIf(EConditionMode.Any, nameof(_isMelee), nameof(_unarmed))]  // at least one must be true

[ShowIfEnum(nameof(_mood), EMood.Electric)]                 // enum equals one of these
[ShowIfEnum(nameof(_mood), EMood.Electric, EMood.Sad)]

[ReadOnly]              // never editable
[ReadOnlyInPlayMode]    // locked while playing
[ReadOnlyInEditMode]    // locked while stopped
```

`EConditionMode.All` is the default, so listing several members without a mode means all of them. There is no negation syntax on purpose: use `[HideIf]` instead of a `!` prefix, so nothing is encoded in a string that a rename could break.

There are play-mode variants of show, hide, enable and disable too: `[ShowInPlayMode]`, `[HideInPlayMode]`, `[EnableInPlayMode]` and `[DisableInPlayMode]`.

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
[AssetOnly] public GameObject prefab;            // rejects scene objects
[SceneObjectOnly] public Transform anchor;       // rejects project assets
[ValidateInput(nameof(IsEven), "Must be even.")] public int value;

private bool IsEven(int v) => v % 2 == 0;        // also works with no parameter
```

`[RequiredIf]` is `[Required]` that only fires while a condition holds, for fields that are mandatory in one setup and meaningless in another. It takes the same members and modes as `[ShowIf]`.

```csharp
public bool usesCustomIcon;
[RequiredIf(nameof(usesCustomIcon))] public Sprite icon;
```

`[MustImplement]` restricts an object reference by type rather than by location. The picker only lists objects that qualify, dropping a GameObject resolves the first component on it that does, and an assignment that cannot be satisfied is reverted.

```csharp
[MustImplement(typeof(IDamageable))] public GameObject target;
[MustImplement(typeof(IDamageable), typeof(ISelectable))] public Component both;
```

`[MinMax]` and `[Max]` really do reset the value: type 500 with a max of 100 and it snaps back to 100 when you commit. Both also clamp component-wise on `Vector2`, `Vector3`, `Vector2Int` and `Vector3Int`.

`[ValidateInput]` methods may return a `ValidationResult` instead of a bool, which lets one validator name
which of its checks failed and choose between an error and a warning:

```csharp
[ValidateInput(nameof(ValidateTexture))] public Texture2D tex;

private ValidationResult ValidateTexture()
{
    if (tex == null) return ValidationResult.Error("No texture assigned.");
    if (!tex.isReadable) return ValidationResult.Warning("The texture is not readable.");
    return ValidationResult.Valid;
}
```

`[Required]` and `[ValidateInput]` can also carry a fix button, for the failures whose answer is always
the same:

```csharp
[Required(FixAction = nameof(UseSelf), FixActionName = "Use self")]
public Transform spawnPoint;

private void UseSelf() => spawnPoint = transform;
```

## Auto-assignment

Fill a reference from the hierarchy so you stop dragging things by hand. They only fill when the field is empty, so you can still override manually.

```csharp
[GetComponent] public Rigidbody body;            // GetComponent on the same object
[GetComponentInParent] public Canvas canvas;     // searches strictly upward
[GetComponentInParent("Root")] public Transform root;  // named ancestor
[Child] public Renderer renderer;                // GetComponentInChildren
[Child("Muzzle")] public Transform muzzle;       // named descendant
```

`[RequiredGet]` is the auto-assign and the requirement in one attribute, since on a mandatory sibling
reference they are always written together anyway. It fills the field and reports it when nothing is
found.

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
[FolderPath(true)] public string absolute;
[FilePath] public string anyFile;
[FilePath("png")] public string texture;         // filtered by extension
[ResourcesAsset] public string res;              // picks an asset under a Resources folder
[ResourcesAsset(typeof(GameObject))] public string prefabPath;
[ShowAssetPreview] public Texture2D icon;        // thumbnail under the field, [ShowAssetPreview(96)] for size
[Tag] public string tag;                         // tag dropdown, [Tag(true)] to forbid new tags
[ComponentPicker] public Collider hit;           // drop a GameObject, it picks the matching component
[OpenAsset] public TextAsset config;             // button that opens the asset in its editor
[Layer] public int spawnLayer;                   // single layer, use LayerMask when you need several
[Layer] public string layerName;                 // on a string it stores the name instead
[SortingLayer] public string sortingLayer;       // sorting layer name, or the id on an int
```

Animator and audio, which resolve their options from a sibling field:

```csharp
public Animator animator;
[AnimatorParam(nameof(animator))] public string param;       // stores the name
[AnimatorParam(nameof(animator))] public int paramHash;      // on an int it stores the hash instead

public AudioMixer mixer;
[AudioMixerParameter(nameof(mixer))] public string exposedParam;  // exposed mixer parameters
[AudioMixerGroup(nameof(mixer))] public AudioMixerGroup group;

[AnimatorState(nameof(animator))] public string state;       // "LayerName.StateName", or the hash on an int

public Material material;
[ShaderParam(nameof(material))] public string property;      // any shader property, or the id on an int
[ShaderParam(nameof(material), EShaderParamType.Color)] public string tint;
```

`[ShaderParam]` also accepts a `Renderer` or a `Shader` field as its source.

`[AssetDropdown]` replaces an object field with a searchable dropdown of matching project assets, so a
reference can be picked by name instead of found in the Project window and dragged. The filter is the
string the Project window search itself takes, and it is derived from the field type when omitted.

```csharp
[AssetDropdown] public Material material;
[AssetDropdown("t:Prefab", "Assets/Enemies")] public GameObject enemy;
```

## Editing referenced assets

`[Expandable]` adds a toggle next to an asset reference that draws the asset's own inspector inline, so a ScriptableObject can be edited without changing the Project window selection.

```csharp
[Expandable] public WeaponConfig config;
[Expandable(DefaultExpanded = true)] public AudioConfig audio;
```

`[ReferencePicker]` gives a `[SerializeReference]` field the type picker Unity does not provide, so the concrete implementation can be chosen and swapped in the inspector. The picker is searchable and grouped by namespace.

```csharp
[SerializeReference] [ReferencePicker] public IAbility ability;
[SerializeReference] [ReferencePicker] public List<ICondition> conditions;
```

## Buttons and widgets

```csharp
[Button] private void Rebuild() { }
[Button("Danger", Mode = EButtonMode.PlayMode, Confirm = "Sure?")]
private void Nuke() { }

[InlineButton(nameof(Randomize), "Roll")] public int rolled;     // button next to the field
[ClearButton] public string note;                                // inline button that empties the field
[CopyButton] public string id;                                   // inline button that copies the value

[Dropdown(nameof(Options))] public string choice;                // options from a member
[ProgressBar(100f, EColor.Green)] public float health;           // drag to set the value
[ProgressBar(nameof(maxMana), EColor.Blue)] public float mana;   // dynamic max from a member
[ProgressBar(100f, EColor.Orange, readOnly: true)] public float shown;

[MinMaxSlider(0, 100)] public Vector2 range;                     // one slider with two handles
[Percentage] public float ratio;                                 // shows 0..1 as a percent, [Percentage(true)] for a slider
[CurveRange(0, 0, 1, 1, EColor.Cyan)] public AnimationCurve curve;
[EnumFlags] public MyFlags flags;                                // mask field for [Flags] enums
[EnumToggleButtons] public MyEnum mode;                          // enum as a row of buttons

private string[] Options => new[] { "a", "b", "c" };
```

Buttons and the read-only native members render at the bottom of the inspector, after your fields.

`[HeaderButton]` puts a button in the component header instead, where it costs no vertical space and stays reachable while the component is collapsed. Buttons lay out right to left in declaration order.

```csharp
[HeaderButton("Open")] private void OpenWindow() { }
[HeaderButton("Reset", Width = 50f, Confirm = "Reset everything?")] private void ResetAll() { }
```

`[Dropdown]` switches from a plain popup to a searchable tree once there are more than a handful of options, so long option lists stay usable. Options containing a slash become submenus.

There are two change callbacks:

```csharp
[OnValueChanged(nameof(OnHealthChanged))] public int health;
private void OnHealthChanged() { }               // fires when the field is edited in the inspector

[OnArraySizeChanged(nameof(OnSlotsResized))] public List<Item> slots;
private void OnSlotsResized(int size) { }        // fires only when the element count changes
```

`[OnArraySizeChanged]` accepts a parameterless method or one taking a single int. Edits to element values that keep the count do not fire it; use `[OnValueChanged]` for those.

`[Unit]` shows a unit after a numeric field from a fixed vocabulary rather than a free string, so the same
unit is spelled the same way everywhere:

```csharp
[Unit(UnitAttribute.MetersPerSecond)] public float speed;
[Unit(UnitAttribute.Degree)] public float angle;
[Unit("bananas")] public float custom;            // the exception, not the default
```

`[DisplayAsString]` draws a value as read-only text on one line, collapsing a whole collection rather than
expanding it into rows. Use it for values that are computed and `[ReadOnly]` for values authored elsewhere.

`[PreviewObject]` draws a preview large enough to judge, interactive where the asset supports it:

```csharp
[PreviewObject] public GameObject prefab;                          // drag inside it to rotate
[PreviewObject(64f, Width = 64f)] public Texture2D icon;
[PreviewObject(160f, Foldout = true, DefaultExpanded = false)] public Mesh mesh;
```

## Collections

Lists stay Unity's own list. `[ListDrawerSettings]` only tells that list what to do through its
callbacks, so a list with the attribute reorders, selects and resizes exactly like a list without one.

```csharp
[ListDrawerSettings(Searchable = true)]              // search box that hides rows whose label misses
[ListDrawerSettings(ConfirmDelete = true)]           // removing a row asks first, naming the row
[ListDrawerSettings(ShowAlternatingBackground = false)]  // tinting off, on by default
```

Rows are named after the element's first string field, with nothing to configure. Unity's own list
does the same, so a setting that named that member was work to arrive at the default.

Searching hides a row by giving it a height of zero rather than by drawing a different list, so a
filtered list is the same control with fewer rows. Dragging switches off while a filter is on, because
the row above is then not the element above.

`[Table]` draws an array of a serializable type as a grid instead: one row per element, one column per
field, on the same list. Columns come from the first element, so an empty table shows only its header.
`[TableColumn]` on the element's fields changes a column's relative width, its header text, or hides it.

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

`[ArraySize]` fixes or bounds the element count on either of them, which is what removes the add and
remove buttons.

```csharp
[ArraySize(4)] public List<string> corners;           // exactly four
[ArraySize(Min = 2, Max = 6)] public List<string> tiers;
```

There is deliberately no paging, no drag toggle and no per-list add or remove switch. Anything that
would need a second implementation of a list is left out: two renderers that have to look identical
never quite do, and the difference shows up as a layout bug rather than as a missing feature.

## Attributes window

`Tools > Base Packages > Unity Editor > Project Health > Attributes`

One window, three tabs. It is both the documentation and the thing that tells you when an attribute is
not doing what it reads like.

### Reference

One page per attribute, 98 of them, searchable and grouped. Each page carries a live sample drawn through
the real inspector, what the sample needs before it does anything, the other ways the attribute can be
written, and the source of the whole sample class underneath.

The samples are one class per attribute, which is what lets the page draw the whole object and print the
whole class: everything in a sample is part of the answer, including the bool a condition watches and the
property a dropdown reads its options from. The snippet is therefore paste-and-compile by construction.

Seventeen attributes need a GameObject rather than an asset, so their samples are components. Their pages
carry a **Create in scene** button that drops a temporary copy into the open scene and selects it, which is
the only way to see a scene handle or a component header control: handles draw for the selected object in
the Scene view, and header controls are drawn by the real Inspector rather than by an embedded one. That
copy is never saved with the scene.

### Showcase

A throwaway asset carrying one of every attribute, drawn through the real inspector. Edit anything, nothing
is saved. Useful for seeing several attributes against each other rather than one at a time.

### Troubleshoot

Every drawer and handler in this package fails quietly on purpose: an attribute that cannot resolve what it
points at falls back to the plain field so a typo never breaks the whole inspector. That is the right runtime
behavior and a terrible way to find mistakes, because the fallback is only visible while the affected object
happens to be selected.

Press **Scan** and the window walks every component, ScriptableObject and serializable type and lists what
cannot work:

- a condition member that does not exist or is not a bool, which makes the condition evaluate to true forever
- `[Dropdown]` pointing at something that is not enumerable
- `[AnimatorParam]`, `[AnimatorState]`, `[AudioMixerParameter]` or `[ShaderParam]` whose sibling field is
  missing or of the wrong type, including the exact-type rule the sibling resolver applies
- an attribute on a field type its drawer cannot handle, for example `[Required]` on an int
- `[GetComponent]`, `[Child]` or `[GetComponentInParent]` on a non-component type, on a `GameObject` field, or
  on a ScriptableObject that has no hierarchy to search
- `[Button]` or `[HeaderButton]` on a method with parameters, which the renderer skips
- `[OnValueChanged]`, `[OnArraySizeChanged]`, `[InlineButton]` or `[ValidateInput]` whose target method is gone
  or no longer matches the expected signature
- `[ReferencePicker]` without `[SerializeReference]`, or on a type with no instantiable implementation

Click a row's header to open the script. Errors mean the attribute does nothing; warnings mean it works but
not the way it reads.

A healthy project produces an empty report, which is the right outcome and a useless way to learn what the
window looks like. The **Demo types** toggle scans a set of types that are broken on purpose, one per family
of mistake, and shows the report they produce. Those types are never part of a project scan, and the toggle
clears itself on every tab switch so a demo report can never be mistaken for the project's own state.

They are also the test fixture: if a check stops working, its sample stops being reported.

Adding a check is one file. Implement `IAttributeCheck` anywhere and `TypeCache` picks it up, the same way
handlers and validation rules work.

The scan is manual rather than continuous, because walking every type in the project is not cheap enough to
run on a timer. A domain reload clears the result instead of showing a stale one.

## How it works, briefly

There are three ways a thing gets drawn:

- **Decorators handled in the inspector** (titles, lines, validation, conditions, auto-assign) run through a small handler pipeline. Each attribute is one tiny handler class.
- **Property drawers** (tag, curve, enum buttons, pickers, progress bar, inline button) replace how a single field renders.
- The **inspector itself** only does grouping (foldouts, tabs) and hands each field to the pipeline.

There is a fourth thing that is not drawing at all: **checks**, which the troubleshoot window runs over the whole
project to find attributes that point at something they cannot use.

Handlers are discovered with `TypeCache`, so adding a new one is genuinely just dropping in a file. Pick the interface that matches when it should run (`IBeforeFieldHandler`, `IVisibilityHandler`, `IEnableHandler`, `IAfterFieldHandler` or `IInlineFieldWidget`) or write a normal `PropertyDrawer` if you're replacing the field. No registration step.

## Custom editors

The package draws every `MonoBehaviour` and `ScriptableObject` through a `[CustomEditor]` on the base types. The moment you write your own `[CustomEditor]` for a specific type, Unity picks the more specific one and the package's inspector drops out, so all the attribute drawing goes with it.

Whenever you need a custom editor, derive it from `AttributePackageEditor` instead of `UnityEditor.Editor` and call `base.OnInspectorGUI()` for the attribute-driven part. This is required for every custom editor you write if you want the attributes to keep working.

```csharp
using Base.AttributePackage.Editor;
using UnityEditor;

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

## Things worth knowing

- Unity only lets one package own the default inspector. If you also pull in Odin, NaughtyAttributes or similar, they'll fight over it and one loses silently. Fine as long as this is your only inspector package.
- The pipeline reaches into nested `[Serializable]` structs and classes at any depth, so validation, conditional and layout attributes work on their fields too. It stops descending in three cases, handing those to Unity's default drawing: arrays and lists (attributes on fields of list elements are skipped), types that have their own `PropertyDrawer` and Unity or framework types like `Vector3`.
- Serialized collections live in `Base.UtilityPackage`, not here: `SerializableDictionary<,>`, `SerializableHashSet<>` and `InterfaceReference<>` each ship with their own drawer. The pipeline hands any type with a `PropertyDrawer` straight to that drawer, so their attributes are not evaluated on the inner rows.
- A couple of drawers do real work every repaint by nature. `[AudioMixerParameter]` reads the mixer's exposed parameters and `[AnimatorParam]` reads the controller's parameters each time. On a field or two it's nothing; don't stack a dozen of them on one object.