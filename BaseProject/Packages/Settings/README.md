# Base Settings Package

A reusable, store-agnostic settings framework: typed persistable settings, a registry that drives load, save, revert and reset across all of them, drop-in components for the common display, audio and control settings, and a set of ready-made UI elements. Backed by `PlayerPrefs` out of the box, with the store behind an interface so it can be swapped for a file, cloud or in-memory backend without touching anything else.

The package carries no game-specific keys. The consuming project decides which settings exist by placing components in a scene or by registering settings directly.

## Requirements

- Unity `6000.3` or newer
- `com.unity.inputsystem` `1.19.0` for the rebind setting and rows
- `com.unity.localization` `1.5.11` for the `LocalizedString` titles and labels
- `com.unity.ugui` `2.0.0` and TextMeshPro
- `Base.ServicePackage` for the service locator and `GameServiceBehaviour`
- `Base.CorePackage` for object pooling
- `Base.TweeningPackage` for the selection indicator animation
- `Base.UtilityPackage` for `PersistentKey`, logging and helpers
- `Base.AttributePackage` for the inspector attributes
- One assembly: `Base.SettingsPackage`

## Layout

| Folder | What is in it |
|---|---|
| `Core` | `ISetting`, `Setting<T>` and the five concrete types, `ISettingsStore` with its `PlayerPrefsSettingsStore` default, `SettingsRegistry` and the `SettingsContext` that owns them in a scene |
| `Components` | `SettingComponent`, `TypedSettingComponent<TValue, TSetting>` and its per-type bases, plus the ready-to-use components |
| `Display` | `DisplaySettings` wrapping Unity's display APIs, and `ResolutionProvider` turning resolutions into stable labels |
| `Controls` | `ControlSettings`, the service gameplay code reads look sensitivity and invert flags from, plus `ELookAxis` and `ControlSettingKeys` |
| `Presets` | `SettingsPreset`, `SettingsPresetEntry` and `ESettingValueType` |
| `UI` | `SettingElement`, `TypedSettingElement<TValue, TSetting>` and the concrete widgets |

## How it fits together

A setting is a single persistable value. `Setting<T>` holds the current value, the default and a snapshot of the last saved value, so it can revert or reset. Assigning `Value` raises `OnValueChanged`; assigning an equal value is a no-op, so appliers never re-run for an unchanged value.

`SettingsRegistry` holds every registered setting in registration order and drives `LoadAll`, `SaveAll`, `RevertAll` and `ResetAllToDefault`. Registration order matters when one setting must be applied before another, for example full screen mode before resolution. It also raises `OnAnyValueChanged`, so a preset button can follow the whole set without subscribing to every setting itself.

Alongside the typed `OnValueChanged`, every setting raises a non-generic `OnChanged` and reports `IsDefault`. That is what lets the reset and preset buttons work without knowing what type of value they are looking at.

`SettingsContext` is a `GameServiceBehaviour` that creates the store and registry and exposes them through the `ServiceLocator`. It saves on destroy and offers `Save`, `Revert`, `ResetToDefaults` and `Reload`. It runs at execution order -98, so it exists before any setting component wakes.

Each `SettingComponent` resolves the context in `Awake`, creates its typed setting, registers it and subscribes its applier. A component that finds no context disables itself instead of failing again on every call. The applier is the only place that touches the thing the setting controls, so the value model stays free of Unity APIs.

`TypedSettingElement<TValue, TSetting>` is the other half: it resolves the setting from the registry, keeps the subscription alive and tears it down on destroy, so concrete widgets only implement `OnBound` and `OnSettingChanged`. Elements broadcast their localized title and description while focused and reset the focused setting when `SettingsEvents.RaiseResetSelected` is called.

## Getting started

1. Add a `SettingsContext` to your scene.
2. Add setting components for what you want to persist. Order them in the scene so dependent settings apply in the right sequence: mode before resolution, VSync before quality level.
3. Add the UI elements you need and set each element's setting key to match the component's key.
4. Wire your input to `SettingsEvents.RaiseResetSelected` and, if you use them, to `RaiseSubMenuChanged`.

### Registering a setting from code

```csharp
BoolSetting subtitles = context.Registry.Register(
    new BoolSetting(context.Store, new PersistentKey("Subtitles"), defaultValue: true));

subtitles.OnValueChanged += enabled => subtitleView.SetActive(enabled);
context.Reload();
```

### Adding a new setting type

Subclass `Setting<T>` and implement `Read` and `Write` against the store. For a component, subclass the matching per-type base, supply the key and default and implement `Apply`.

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

Subclass `TypedSettingElement<TValue, TSetting>` and implement `OnBound` (show the current value and subscribe your control) and `OnSettingChanged` (push a new value into the control). The base does the rest.

### Swapping the store

Implement `ISettingsStore` and return it from `SettingsContext.CreateStore` in a subclass. Writes should buffer until `Flush`, which is what keeps revert behavior correct.

```csharp
public sealed class FileSettingsContext : SettingsContext
{
    protected override ISettingsStore CreateStore() => new FileSettingsStore();
}
```

## Included components

| Component | What it stores and applies |
|---|---|
| `AudioVolumeSetting` | A normalized 0..1 volume, pushed as decibels into an `AudioMixer` parameter. One per channel; the key matches the mixer parameter name |
| `FullScreenModeSetting` | An index into a curated list of `FullScreenMode` values |
| `ResolutionSetting` | A `"{width}x{height}"` label, applied with the active mode |
| `QualityLevelSetting` | The Unity quality level index, preserving VSync across the change |
| `VSyncSetting` | The VSync count |
| `LookSensitivitySetting` | A normalized 0..1 sensitivity, pushed into `ControlSettings` as the multiplier it maps to |
| `InvertLookSetting` | Whether one look axis is flipped. One per axis; the axis picks the key |
| `RebindSetting` | Every binding override of one `InputActionAsset` |

### Components in other packages

A setting that drives another package's feature ships with that feature, so this package never has to reference it. Those assemblies only compile when this package is installed, through a version define on `com.baseprojectpackages.settings`.

- `LanguageSetting` in the Localization package.
- `RumbleEnabledSetting` and `RumbleIntensitySetting` in the Controller Support package.
- `AutosaveEnabledSetting`, `AutosaveIntervalSetting` and `AutosaveCooldownSetting` in the Save System package.

## Included UI elements

| Element | Bound to |
|---|---|
| `SettingToggle` | A `Toggle` on a `BoolSetting`, with an on/off label |
| `SettingSlider` | A `Slider` on a normalized `FloatSetting`, with optional step buttons and a percentage label |
| `SettingDropdown` | A `TMP_Dropdown` on an `IntSetting` holding the option index |
| `IntMultipleChoiceElement`, `StringMultipleChoiceElement` | A fixed option list cycled with left and right buttons and a row of indicators |
| `ResolutionChoiceElement` | A string picker filled from the available display resolutions at bind time |
| `RebindButton` | One rebindable row, backed by the shared `RebindSetting` |
| `SettingsPresetButton` | Applies a preset and shows whether it is still the active one |
| `SettingResetButton` | Resets one element to its default |
| `SettingFlavorText` | The title and description of the focused element |
| `SelectionIndicatorButton` | A button paired with a `TweenGroup` shown while its option is selected |

## Control settings

Look sensitivity and the invert flags are plain values with nothing in this package to apply them to, so they are pushed into `ControlSettings`, a `GameServiceBehaviour` at execution order -97. Gameplay code reads from there rather than looking settings up by key:

```csharp
if (ServiceLocator.TryGet(out ControlSettings controls))
    _lookDelta = controls.ApplyLook(rawLook);
```

`ApplyLook` multiplies by the sensitivity and flips whichever axes are inverted. The individual values are also exposed, and `OnControlsChanged` fires whenever one of them moves.

`LookSensitivitySetting` stores a normalized value and maps it onto a serialized multiplier range, so retuning the feel of the slider never touches what is on disk.

## Rebinding

`RebindSetting` persists every binding override of one `InputActionAsset` as the JSON the input system writes itself. One setting covers the whole asset instead of one key per binding, so a rebind row added later needs no migration.

Each row is a `RebindButton`. Clicking it listens for the next control the player presses, writes the result back through the `RebindSetting` and shows the new binding. Resetting a row clears only that binding rather than the whole shared setting.

The overrides land on the asset instance the `RebindSetting` resolves, so it has to be the one the game actually plays with. A project whose input comes from a generated wrapper plays with that wrapper's clone, not with the source asset, and has to subclass and return the clone:

```csharp
public sealed class ProjectRebindSetting : RebindSetting
{
    protected override InputActionAsset ResolveAsset()
        => ServiceLocator.TryGet(out ProjectInputService input)
            ? input.Actions.asset
            : null;
}
```

The `InputActionReference` on a `RebindButton` only names which action is meant; the action itself is resolved by id against that asset, so the clone is what gets rebound.

## Presets

A `SettingsPreset` is a ScriptableObject holding a list of key and value pairs, for example Low, Medium and High. Applying one writes every entry into the matching registered setting, which runs that setting's own applier, so a preset carries no code and knows nothing about what it controls. A key that is not registered in the current scene is reported and skipped.

Unity cannot serialize a value whose type is only known at runtime, so each entry names its value type and carries one field per supported type. Only the matching field is shown in the inspector.

A preset is an action rather than a persisted value. What gets saved is the settings it wrote, so a player who applies High and then turns one thing down keeps that change on the next launch. `SettingsPresetButton` shows whether the current values still match its preset, so a row of buttons highlights the one the player is on and none of them once something was tuned by hand.

## Per-setting reset

`SettingElement.ResetToDefault` is public, so a reset can be driven at one element directly instead of going through the focus-based `SettingsEvents.RaiseResetSelected` path. `SettingResetButton` does exactly that: point it at an element and it resets that element on click, greying itself out while the setting already holds its default, which it follows through the registry's `OnAnyValueChanged`.