# Base Settings Package

A reusable, store-agnostic settings framework for Unity. It gives you typed persistable
settings, a registry that drives load, save, revert and reset across all of them, drop-in
MonoBehaviour components for the common display and audio settings and a set of ready-made
UI elements. It ships backed by `PlayerPrefs` but the backing store is an interface, so you
can swap it for a file, cloud or in-memory store without touching anything else.

The package carries no game-specific keys. The consuming project decides which settings
exist by placing components in a scene or by registering settings directly.

- **Namespace:** `Base.SettingsPackage`
- **Assembly:** `Base.SettingsPackage`
- **Unity:** 6000.3+

## Layout

- **Core** holds the value model and persistence: `ISetting`, the generic `Setting<T>`
  base, the concrete `BoolSetting`, `IntSetting`, `FloatSetting`, `StringSetting` and
  `EnumSetting<TEnum>`, the `ISettingsStore` abstraction with its `PlayerPrefsSettingsStore`
  default, the `SettingsRegistry` and the `SettingsContext` that owns them in a scene.
- **Components** holds `SettingComponent`, the generic `TypedSettingComponent<TValue, TSetting>`
  and its per-type bases, plus ready-to-use components for audio volume, full screen mode,
  resolution, quality level, VSync, language and gamepad rumble.
- **Display** holds `DisplaySettings` (thin wrappers over Unity's display APIs) and
  `ResolutionProvider` (turns available resolutions into stable labels).
- **Controls** holds `ControlSettings` (the service gameplay code reads look sensitivity and the
  invert flags from), `ELookAxis` and `ControlSettingKeys`.
- **Presets** holds `SettingsPreset`, the `SettingsPresetEntry` it is built from and the
  `ESettingValueType` that decides which value an entry carries.
- **UI** holds `SettingElement`, the generic `TypedSettingElement<TValue, TSetting>` and the
  concrete widgets: toggle, slider, dropdown, the multiple-choice pickers and the rebind row,
  along with the preset button, the per-setting reset button, the flavor-text display and the
  shared event hub.

## How it fits together

A setting is a single persistable value. `Setting<T>` holds the current value, the default
and a snapshot of the last saved value, so it can revert or reset. Assigning `Value` raises
`OnValueChanged` and assigning an equal value is a no-op, so appliers never re-run for an
unchanged value.

The `SettingsRegistry` holds every registered setting in registration order and drives
`LoadAll`, `SaveAll`, `RevertAll` and `ResetAllToDefault`. Registration order matters when
one setting must be applied before another, for example full screen mode before resolution.
It also raises `OnAnyValueChanged` for anything that follows the whole set at once, so a preset
button does not have to subscribe to every setting itself.

Alongside the typed `OnValueChanged`, every setting raises a non-generic `OnChanged` and reports
`IsDefault`. That is what lets the reset buttons and the preset buttons work without knowing what
type of value they are looking at.

The `SettingsContext` is a `GameServiceBehaviour` that creates the store and registry and
exposes them through the `ServiceLocator`. It saves on destroy and offers `Save`, `Revert`,
`ResetToDefaults` and `Reload`. It runs at execution order -98, so it exists before any
setting component wakes.

Each `SettingComponent` resolves the context in `Awake`, creates its typed setting, registers
it and subscribes its applier. A component that finds no context disables itself instead of
failing again on every call. The applier is the only place that touches the thing the setting
controls, so the value model stays free of Unity APIs. Concrete components inherit from the
per-type base that matches their value type, never from `SettingComponent` directly.

The `SettingElement` widgets are the other half. `TypedSettingElement<TValue, TSetting>`
resolves the setting from the registry, keeps the subscription alive and tears it down on
destroy; concrete widgets only implement `OnBound` and `OnSettingChanged`. They broadcast
their localized title and description while focused and reset the focused setting when
`SettingsEvents.RaiseResetSelected` is called.

## Getting started

1. Add a `SettingsContext` to your scene.
2. Add setting components for what you want to persist, for example `AudioVolumeSetting`,
   `FullScreenModeSetting`, `ResolutionSetting`, `QualityLevelSetting` and `VSyncSetting`.
   Order components in the scene so dependent settings apply in the right sequence
   (mode before resolution, VSync before quality).
3. Add the UI elements you need and set each element's setting key to match the component's
   key.
4. Wire your input to `SettingsEvents.RaiseResetSelected` and, if you use them, to
   `RaiseSubMenuChanged`.

### Registering a setting from code

If you would rather not use a component, register a setting directly on the context:

```csharp
BoolSetting subtitles = context.Registry.Register(
    new BoolSetting(context.Store, new PersistentKey("Subtitles"), defaultValue: true));

subtitles.OnValueChanged += enabled => subtitleView.SetActive(enabled);
context.Reload();
```

### Adding a new setting type

Subclass `Setting<T>` and implement `Read` and `Write` against the store. For a component,
subclass the matching per-type base (`FloatSettingComponent`, `IntSettingComponent` and so
on), supply the key and default and implement `Apply`.

```csharp
public sealed class MouseSensitivitySetting : FloatSettingComponent
{
    [SerializeField] [Range(0f, 1f)] private float defaultSensitivity = 0.5f;

    public override PersistentKey Key => new("MouseSensitivity");
    protected override float DefaultValue => defaultSensitivity;

    protected override void Apply(float value) => InputConfig.SetSensitivity(value);
}
```

### Adding a new UI element

Subclass `TypedSettingElement<TValue, TSetting>` and implement `OnBound` (show the current
value and subscribe your control) and `OnSettingChanged` (push a new value into the control).
The base resolves the setting, unsubscribes on destroy and implements the reset request.

### Swapping the store

Implement `ISettingsStore` and return it from `SettingsContext.CreateStore` in a subclass.
Writes should buffer until `Flush`, which is what keeps revert behavior correct.

```csharp
public sealed class FileSettingsContext : SettingsContext
{
    protected override ISettingsStore CreateStore() => new FileSettingsStore();
}
```

## Control settings

Look sensitivity and the invert flags are plain values with nothing in this package to apply them
to, so they are pushed into `ControlSettings`, a `GameServiceBehaviour` at execution order -97.
Gameplay code reads from there rather than looking settings up by key:

```csharp
if (ServiceLocator.TryGet(out ControlSettings controls))
    _lookDelta = controls.ApplyLook(rawLook);
```

`ApplyLook` multiplies by the sensitivity and flips whichever axes are inverted. The individual
values are also exposed, and `OnControlsChanged` fires whenever one of them moves.

`LookSensitivitySetting` stores a normalized 0..1 value and maps it onto a serialized multiplier
range, so retuning the feel of the slider never touches what is on disk. `InvertLookSetting`
handles one axis; add one per axis, and the axis picks the key.

### Rebinding

`RebindSetting` persists every binding override of one `InputActionAsset` as the JSON the input
system writes itself. One setting covers the whole asset instead of one key per binding, so a
rebind row added later needs no migration.

Each row is a `RebindButton`. Clicking it listens for the next control the player presses,
writes the result back through the `RebindSetting` and shows the new binding. Resetting a row
clears only that binding rather than the whole shared setting.

The overrides land on the asset instance the `RebindSetting` resolves, so it has to be the one the
game actually plays with. A project whose input comes from a generated wrapper plays with that
wrapper's clone, not with the source asset, and has to subclass and return the clone:

```csharp
public sealed class ProjectRebindSetting : RebindSetting
{
    protected override InputActionAsset ResolveAsset()
        => ServiceLocator.TryGet(out ProjectInputService input)
            ? input.Actions.asset
            : null;
}
```

The `InputActionReference` on a `RebindButton` only names which action is meant; the action itself
is resolved by id against that asset, so the clone is what gets rebound.

## Presets

A `SettingsPreset` is a ScriptableObject holding a list of key and value pairs, for example Low,
Medium and High. Applying one writes every entry into the matching registered setting, which runs
that setting's own applier, so a preset carries no code and knows nothing about what it controls.
A key that is not registered in the current scene is reported and skipped.

Unity cannot serialize a value whose type is only known at runtime, so each entry names its value
type and carries one field per supported type. Only the matching field is shown in the inspector.

`SettingsPresetButton` applies a preset on click and shows whether the current values still match
it, so a row of buttons can highlight the one the player is on and highlight none of them once
something was tuned by hand.

A preset is an action rather than a persisted value. What gets saved is the settings it wrote, so
a player who applies High and then turns one thing down keeps that change on the next launch.

## Per-setting reset

`SettingElement.ResetToDefault` is public, so a reset can be driven at one element directly instead
of going through the focus-based `SettingsEvents.RaiseResetSelected` path. `SettingResetButton`
does exactly that: point it at an element, and it resets that element on click. It greys itself out
while the setting already holds its default, which it follows through the registry's
`OnAnyValueChanged`.

## Included components

- `AudioVolumeSetting` stores a normalized 0..1 volume and pushes it as decibels into an
  `AudioMixer` parameter. Use one per channel. The setting key matches the mixer parameter
  name.
- `FullScreenModeSetting` stores an index into a curated list of `FullScreenMode` values.
- `ResolutionSetting` stores a "{width}x{height}" label and applies it with the active mode.
- `QualityLevelSetting` stores the Unity quality level index and preserves VSync across the
  change.
- `VSyncSetting` stores the VSync count.
- `LookSensitivitySetting` stores a normalized 0..1 sensitivity and pushes the multiplier it maps
  to into `ControlSettings`.
- `InvertLookSetting` stores whether one look axis is flipped. Use one per axis.
- `RebindSetting` stores the binding overrides of one `InputActionAsset`.

### Components in other packages

A setting that drives another package's feature ships with that feature, not here, so this
package never has to reference it. Those assemblies only compile when this package is
installed, through a version define on `com.baseprojectpackages.settings`.

- `LanguageSetting` lives in the Localization package and applies a locale through Unity
  Localization.
- `RumbleEnabledSetting` and `RumbleIntensitySetting` live in the Controller Support package
  and drive the `RumbleService`.

## Included UI elements

- `SettingToggle` binds a `Toggle` to a `BoolSetting` and updates an on/off label.
- `SettingSlider` binds a `Slider` to a normalized `FloatSetting`, with optional step
  buttons and a percentage label.
- `SettingDropdown` binds a `TMP_Dropdown` to an `IntSetting` holding the option index.
- `IntMultipleChoiceElement` and `StringMultipleChoiceElement` cycle through a fixed list of
  options with left and right buttons and a row of selection indicators.
- `ResolutionChoiceElement` is a string picker that fills its options from the available
  display resolutions at bind time.
- `RebindButton` is a single rebindable row, backed by the shared `RebindSetting`.
- `SettingsPresetButton` applies a `SettingsPreset` and shows whether it is still the active one.
- `SettingResetButton` resets one element to its default.
- `SettingFlavorText` shows the title and description of the focused element.

## Migrating from 1.x

Version 2.0.0 renames a few public types and one namespace. A project-wide find and replace
covers all of it:

| 1.x | 2.0 |
| --- | --- |
| `Base.SettingsPackage.GUI` | `Base.SettingsPackage.UI` |
| `SettingComponent<TValue, TSetting>` | `TypedSettingComponent<TValue, TSetting>` |
| `MultipleChoiceElement` | `MultipleChoiceElement<TValue, TSetting>` |
| `SettingFlavourText` | `SettingFlavorText` |
| `SettingElement.OnHoverFlavourChanged` | `SettingElement.OnHoverFlavorChanged` |

Custom UI elements now derive from `TypedSettingElement<TValue, TSetting>` and replace their
`Bind` and `ResetSetting` overrides with `OnBound` and `OnSettingChanged`. Scene and prefab
references survive the rename because the script GUIDs are unchanged.

## Dependencies

- `Base.ServicePackage` (service locator, `GameServiceBehaviour`)
- `Base.CorePackage` (object pooling)
- `Base.TweeningPackage` (the selection indicator animation)
- `Base.UtilityPackage` (`PersistentKey`, logging, math and coroutine helpers)
- `Base.AttributePackage` (inspector attributes such as `[Required]`)
- Unity Input System, for the rebind setting and the rebind rows
- Unity Localization, for the `LocalizedString` titles and labels on the UI elements
- TextMeshPro and Unity UI