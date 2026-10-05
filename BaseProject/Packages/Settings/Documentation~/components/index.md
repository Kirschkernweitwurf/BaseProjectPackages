# Components

Everything in this section is a component you drag onto a GameObject.

There are three kinds. You almost always need all three.

## 1\. The context

Exactly one per settings scene. It owns the saving and loading.

| Component | What it does |
| --- | --- |
| [🪛 Settings Context](settings-context.md) | Holds all settings. Saves, reverts, resets and reloads them. |

## 2\. Setting components

One per value you want to save. These do the actual work, for example changing the resolution. They have no visuals.

| Component | What the player changes | Key |
| --- | --- | --- |
| [🪛 Audio Volume Setting](audio-volume-setting.md) | Volume of one audio channel | Your mixer parameter name |
| [🪛 Full Screen Mode Setting](full-screen-mode-setting.md) | Windowed / borderless / fullscreen | `FullScreen` |
| [🪛 Resolution Setting](resolution-setting.md) | Screen resolution | `Resolution` |
| [🪛 Quality Level Setting](quality-level-setting.md) | Graphics quality level | `Quality` |
| [🪛 V Sync Setting](v-sync-setting.md) | VSync on or off | `VSync` |
| [🪛 Language Setting](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Localization/Documentation~/components/language-setting.md) | Game language | `Language` |

## 3\. UI elements

The visible controls the player clicks. Each one binds to a setting by key.

| Component | Looks like |
| --- | --- |
| [🪛 Setting Toggle](setting-toggle.md) | On / off switch |
| [🪛 Setting Slider](setting-slider.md) | Slider with optional minus and plus buttons |
| [🪛 Setting Dropdown](setting-dropdown.md) | Classic dropdown list |
| [🪛 Int Multiple Choice Element](int-multiple-choice-element.md) | Left / right arrow picker, saves a position |
| [🪛 String Multiple Choice Element](string-multiple-choice-element.md) | Left / right arrow picker, saves a word |
| [🪛 Resolution Choice Element](resolution-choice-element.md) | Arrow picker that fills itself with resolutions |
| [🪛 Selection Indicator Button](selection-indicator-button.md) | One dot in the row under an arrow picker |
| [🪛 Setting Flavor Text](setting-flavor-text.md) | Title and description panel for the focused setting |

## Order matters

Setting components apply in the order Unity wakes them. Two pairs care about this:

- **FullScreenModeSetting before ResolutionSetting.** The mode must be set before the resolution is applied.
- **VSyncSetting before QualityLevelSetting.** Changing quality can overwrite VSync.

The simplest way to control this is to put them on separate GameObjects and order those GameObjects top to bottom in the Hierarchy.

## Common mistakes

- **The UI does nothing.** The key on the UI element does not match the key on the setting component. Check spelling and capitalization. The Console will tell you the key was not found.
- **Nothing saves.** There is no SettingsContext in the scene, or it wakes after the setting components. Check the Console for a warning naming the component.
- **Saved values are ignored on start.** Someone has to call `Reload()` on the SettingsContext once, after all components have registered. See [🪛 Settings Context](settings-context.md).
