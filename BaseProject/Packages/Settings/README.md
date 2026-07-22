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
- **Components** holds `SettingComponent` and its typed bases, plus ready-to-use components
  for audio volume, full screen mode, resolution, quality level, VSync and language.
- **Display** holds `DisplaySettings` (thin wrappers over Unity's display APIs) and
  `ResolutionProvider` (turns available resolutions into stable labels).
- **GUI** holds `SettingElement` and the concrete widgets: toggle, slider, dropdown and the
  multiple-choice pickers, along with the flavor-text display and the shared event hub.

## How it fits together

A setting is a single persistable value. `Setting<T>` holds the current value, the default
and a snapshot of the last saved value, so it can revert or reset. Assigning `Value` raises
`OnValueChanged` and assigning an equal value is a no-op, so appliers never re-run for an
unchanged value.

The `SettingsRegistry` holds every registered setting in registration order and drives
`LoadAll`, `SaveAll`, `RevertAll` and `ResetAllToDefault`. Registration order matters when
one setting must be applied before another, for example full screen mode before resolution.

The `SettingsContext` is a `GameServiceBehaviour` that creates the store and registry and
exposes them through the `ServiceLocator`. It saves on destroy and offers `Save`, `Revert`,
`ResetToDefaults` and `Reload`. Give it a low execution order so it exists before any
setting component runs.

Each `SettingComponent` resolves the context in `Awake`, creates its typed setting, registers
it and subscribes its applier. The applier is the only place that touches the thing the
setting controls, so the value model stays free of Unity APIs. Concrete components inherit
from the typed base that matches their value type, never from `SettingComponent` directly.

The `SettingElement` widgets are the other half. Each one binds to a setting by key through
the registry, pushes user input into the setting and updates itself when the setting changes
elsewhere. They broadcast their localized title and description while focused and reset the
focused setting when `SettingsEvents.RaiseResetSelected` is called.

## Getting started

1. Add a `SettingsContext` to your scene. Give it a low execution order so it wakes before
   the setting components.
2. Add setting components for what you want to persist, for example `AudioVolumeSetting`,
   `FullScreenModeSetting`, `ResolutionSetting`, `QualityLevelSetting`, `VSyncSetting` and
   `LanguageSetting`. Order components in the scene so dependent settings apply in the right
   sequence (mode before resolution, VSync before quality).
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
subclass the matching typed base (`FloatSettingComponent`, `IntSettingComponent` and so on),
supply the key and default and implement `Apply`.

```csharp
public sealed class MouseSensitivitySetting : FloatSettingComponent
{
    [SerializeField] [Range(0f, 1f)] private float defaultSensitivity = 0.5f;

    public override PersistentKey Key => new("MouseSensitivity");
    protected override float DefaultValue => defaultSensitivity;

    protected override void Apply(float value) => InputConfig.SetSensitivity(value);
}
```

### Swapping the store

Implement `ISettingsStore` and hand it to the registry instead of `PlayerPrefsSettingsStore`.
Writes should buffer until `Flush`, which is what keeps revert behavior correct.

## Included components

- `AudioVolumeSetting` stores a normalized 0..1 volume and pushes it as decibels into an
  `AudioMixer` parameter. Use one per channel. The setting key matches the mixer parameter
  name.
- `FullScreenModeSetting` stores an index into a curated list of `FullScreenMode` values.
- `ResolutionSetting` stores a "{width}x{height}" label and applies it with the active mode.
- `QualityLevelSetting` stores the Unity quality level index and preserves VSync across the
  change.
- `VSyncSetting` stores the VSync count.
- `LanguageSetting` stores an index into a curated list of locales and applies it through the
  Localization package.

## Included UI elements

- `SettingToggle` binds a `Toggle` to a `BoolSetting` and updates an on/off label.
- `SettingSlider` binds a `Slider` to a normalized `FloatSetting`, with optional step
  buttons and a percentage label.
- `SettingDropdown` binds a `TMP_Dropdown` to an `IntSetting` holding the option index.
- `IntMultipleChoiceElement` and `StringMultipleChoiceElement` cycle through a fixed list of
  options with left and right buttons and a row of selection indicators.
- `ResolutionChoiceElement` is a string picker that fills its options from the available
  display resolutions at bind time.

## Dependencies

- `Base.CorePackage` (service locator, object pooling, tweening)
- `Base.ToolPackage` (`PersistentKey`)
- `Base.UtilityPackage` (logging, math and coroutine helpers)
- `Base.AttributePackage` (inspector attributes such as `[Required]`)
- Unity Localization, TextMeshPro and Unity UI
