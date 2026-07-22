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

Needs Unity 2021.3 or newer (it leans on `TypeCache` and the standard IMGUI drawer API).

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

All of these take a member name via `nameof`. The reference can be a bool field, a bool property or a parameterless method returning bool.

```csharp
[ShowIf(nameof(_enabled))]      // hide unless true
[HideIf(nameof(_enabled))]      // hide while true
[EnableIf(nameof(_enabled))]    // grey out unless true
[DisableIf(nameof(_enabled))]   // grey out while true

[ShowIfEnum(nameof(_mood), EMood.Electric)]                 // enum equals one of these
[ShowIfEnum(nameof(_mood), EMood.Electric, EMood.Sad)]

[ReadOnly]              // never editable
[ReadOnlyInPlayMode]    // locked while playing
[ReadOnlyInEditMode]    // locked while stopped
```

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
```

Animator and audio, which resolve their options from a sibling field:

```csharp
public Animator animator;
[AnimatorParam(nameof(animator))] public string param;       // stores the name
[AnimatorParam(nameof(animator))] public int paramHash;      // on an int it stores the hash instead

public AudioMixer mixer;
[MixerParameter(nameof(mixer))] public string exposedParam;  // exposed mixer parameters
[AudioMixerGroup(nameof(mixer))] public AudioMixerGroup group;
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

There is also a change callback:

```csharp
[OnValueChanged(nameof(OnHealthChanged))] public int health;
private void OnHealthChanged() { }               // fires when the field is edited in the inspector
```

## How it works, briefly

There are three ways a thing gets drawn:

- **Decorators handled in the inspector** (titles, lines, validation, conditions, auto-assign) run through a small handler pipeline. Each attribute is one tiny handler class.
- **Property drawers** (tag, curve, enum buttons, pickers, progress bar, inline button) replace how a single field renders.
- The **inspector itself** only does grouping (foldouts, tabs) and hands each field to the pipeline.

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
- A couple of drawers do real work every repaint by nature. `[MixerParameter]` reads the mixer's exposed parameters and `[AnimatorParam]` reads the controller's parameters each time. On a field or two it's nothing; don't stack a dozen of them on one object.