# Components

🗳️ **Components** · things you add to a GameObject

Everything in this section is a component you drag onto a GameObject.

There are three kinds. You almost always need all three.

## 1\. The context

Exactly one per settings scene. It owns the saving and loading.

| Component | What it does |
| --- | --- |
| [🔖 Settings Context](Components/Settings%20Context.md) | Holds all settings. Saves, reverts, resets and reloads them. |

## 2\. Setting components

One per value you want to save. These do the actual work, for example changing the resolution. They have no visuals.

| Component | What the player changes | Key |
| --- | --- | --- |
| [🔖 Audio Volume Setting](Components/Audio%20Volume%20Setting.md) | Volume of one audio channel | Your mixer parameter name |
| [🔖 Full Screen Mode Setting](Components/Full%20Screen%20Mode%20Setting.md) | Windowed / borderless / fullscreen | `FullScreen` |
| [🔖 Resolution Setting](Components/Resolution%20Setting.md) | Screen resolution | `Resolution` |
| [🔖 Quality Level Setting](Components/Quality%20Level%20Setting.md) | Graphics quality level | `Quality` |
| [🔖 V Sync Setting](Components/V-Sync%20Setting.md) | VSync on or off | `VSync` |
| [🔖 Language Setting](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Localization/Documentation~/Components/Language%20Setting.md) | Game language | `Language` |

## 3\. UI elements

The visible controls the player clicks. Each one binds to a setting by key.

| Component | Looks like |
| --- | --- |
| [🔖 Setting Toggle](Components/Setting%20Toggle.md) | On / off switch |
| [🔖 Setting Slider](Components/Setting%20Slider.md) | Slider with optional minus and plus buttons |
| [🔖 Setting Dropdown](Components/Setting%20Dropdown.md) | Classic dropdown list |
| [🔖 Int Multiple Choice Element](Components/Int%20Multiple%20Choice%20Element.md) | Left / right arrow picker, saves a position |
| [🔖 String Multiple Choice Element](Components/String%20Multiple%20Choice%20Element.md) | Left / right arrow picker, saves a word |
| [🔖 Resolution Choice Element](Components/Resolution%20Choice%20Element.md) | Arrow picker that fills itself with resolutions |
| [🔖 Selection Indicator Button](Components/Selection%20Indicator%20Button.md) | One dot in the row under an arrow picker |
| [🔖 Setting Flavor Text](Components/Setting%20Flavor%20Text.md) | Title and description panel for the focused setting |

## Order matters

Setting components apply in the order Unity wakes them. Two pairs care about this:

- **FullScreenModeSetting before ResolutionSetting.** The mode must be set before the resolution is applied.
- **VSyncSetting before QualityLevelSetting.** Changing quality can overwrite VSync.

The simplest way to control this is to put them on separate GameObjects and order those GameObjects top to bottom in the Hierarchy.

## Common mistakes

- **The UI does nothing.** The key on the UI element does not match the key on the setting component. Check spelling and capitalization. The Console will tell you the key was not found.
- **Nothing saves.** There is no SettingsContext in the scene, or it wakes after the setting components. Check the Console for a warning naming the component.
- **Saved values are ignored on start.** Someone has to call `Reload()` on the SettingsContext once, after all components have registered. See [🔖 Settings Context](Components/Settings%20Context.md).
