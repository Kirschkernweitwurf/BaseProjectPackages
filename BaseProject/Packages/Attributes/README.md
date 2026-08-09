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

Either add it through the Package Manager with *Add package from git URL* or just drop the folder into your project. There are two assemblies: one for the attributes (runtime) and one for the drawing (editor only). Nothing editor gets pulled into a build.

## Colors

Anything that takes a color accepts either a hex string or a preset from the `EColor` enum, whichever you find less annoying to type:

```csharp
[Title("Combat", "#E74C3C")]
[Title("Combat", EColor.Red)]
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

## Auto-assignment

Fill a reference from the hierarchy so you stop dragging things by hand. They only fill when the field is empty, so you can still override manually.

```csharp
[GetComponent] public Rigidbody body;            // GetComponent on the same object
[GetComponentInParent] public Canvas canvas;     // searches strictly upward
[GetComponentInParent("Root")] public Transform root;  // named ancestor
[Child] public Renderer renderer;                // GetComponentInChildren
[Child("Muzzle")] public Transform muzzle;       // named descendant
```

## Pickers and references

```csharp
[SceneName] public string scene;                 // dropdown of build scenes (string = name)
[SceneName] public int sceneIndex;               // on an int it stores the build index instead
[FolderPath] public string folder;               // "..." button, stores "Assets/..."
[FolderPath(true)] public string absolute;
[FilePath] public string anyFile;
[FilePath("png")] public string texture;         // filtered by extension
[ResourcesPath] public string res;               // picker that stores a Resources.Load path
[ResourcesPath(typeof(GameObject))] public string prefabPath;
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
[MixerParameter(nameof(mixer))] public string exposedParam;  // exposed mixer parameters
[AudioMixerGroup(nameof(mixer))] public AudioMixerGroup group;

[AnimatorState(nameof(animator))] public string state;       // "LayerName.StateName", or the hash on an int

public Material material;
[ShaderParam(nameof(material))] public string property;      // any shader property, or the id on an int
[ShaderParam(nameof(material), EShaderParamType.Color)] public string tint;
```

`[ShaderParam]` also accepts a `Renderer` or a `Shader` field as its source.

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

## Troubleshoot window

`Tools > Base Packages > Unity Editor > Project Health > Attribute Troubleshoot`

Every drawer and handler in this package fails quietly on purpose: an attribute that cannot resolve what it
points at falls back to the plain field so a typo never breaks the whole inspector. That is the right runtime
behavior and a terrible way to find mistakes, because the fallback is only visible while the affected object
happens to be selected.

Press **Scan Project** and the window walks every component, ScriptableObject and serializable type and lists
what cannot work:

- a condition member that does not exist or is not a bool, which makes the condition evaluate to true forever
- `[Dropdown]` pointing at something that is not enumerable
- `[AnimatorParam]`, `[AnimatorState]`, `[MixerParameter]` or `[ShaderParam]` whose sibling field is missing or
  of the wrong type, including the exact-type rule the sibling resolver applies
- an attribute on a field type its drawer cannot handle, for example `[Required]` on an int
- `[GetComponent]`, `[Child]` or `[GetComponentInParent]` on a non-component type, on a `GameObject` field, or
  on a ScriptableObject that has no hierarchy to search
- `[Button]` or `[HeaderButton]` on a method with parameters, which the renderer skips
- `[OnValueChanged]`, `[OnArraySizeChanged]`, `[InlineButton]` or `[ValidateInput]` whose target method is gone
  or no longer matches the expected signature
- `[ReferencePicker]` without `[SerializeReference]`, or on a type with no instantiable implementation

Click a row's header to open the script. Errors mean the attribute does nothing; warnings mean it works but not
the way it reads.

### Samples tab

A healthy project produces an empty report, which is the right outcome and a useless way to learn what the
window looks like. The **Samples** tab scans a set of types that are broken on purpose, one per family of
mistake, and shows the report they produce. These types are excluded from the project scan, so they never
appear as real findings.

They are also the test fixture: if a check stops working, its sample stops being reported.

### Showcase tab

A throwaway asset carrying one of every attribute, drawn through the real inspector. Edit anything, nothing is
saved. Useful for seeing what an attribute actually looks like before reaching for it, and for checking a new
drawer against the existing ones.

Header buttons are the one thing that does not show up there. They live in the component header, which an
embedded inspector does not draw.

Adding a check is one file. Implement `IAttributeCheck` anywhere and `TypeCache` picks it up, the same way
handlers and validation rules work.

The scan is manual rather than continuous, because walking every type in the project is not cheap enough to run
on a timer. A domain reload clears the result instead of showing a stale one.

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
- A couple of drawers do real work every repaint by nature. `[MixerParameter]` reads the mixer's exposed parameters and `[AnimatorParam]` reads the controller's parameters each time. On a field or two it's nothing; don't stack a dozen of them on one object.
