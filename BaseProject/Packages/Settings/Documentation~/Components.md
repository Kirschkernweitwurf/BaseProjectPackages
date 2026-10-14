---
type: Overview
package: Settings
tags:
  - settings
  - ui
summary: Everything in this section is a component you drag onto a GameObject.
---
# Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #settings #ui

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

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Context

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Settings Context](Components/Settings%20Context.md) | The brain of the settings system. | Programmers |

### Setting components

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Audio Volume Setting](Components/Audio%20Volume%20Setting.md) | Saves the volume of one audio channel and pushes it into your AudioMixer. | Everyone |
| [🔖 Full Screen Mode Setting](Components/Full%20Screen%20Mode%20Setting.md) | Saves whether the game runs windowed, borderless or exclusive fullscreen. | Everyone |
| [🔖 Resolution Setting](Components/Resolution%20Setting.md) | Saves the screen resolution. | Everyone |
| [🔖 Quality Level Setting](Components/Quality%20Level%20Setting.md) | Saves the graphics quality level (Low, Medium, High, and so on). | Everyone |
| [🔖 V Sync Setting](Components/V-Sync%20Setting.md) | Saves the VSync setting. | Everyone |
| [🔖 Look Sensitivity Setting](Components/Look%20Sensitivity%20Setting.md) | Stores the look sensitivity as a value from 0 to 1 and passes the matching multiplier to Control Settings. | Everyone |
| [🔖 Invert Look Setting](Components/Invert%20Look%20Setting.md) | Stores whether the horizontal or vertical look axis is flipped. | Everyone |
| [🔖 Rebind Setting](Components/Rebind%20Setting.md) | Saves every key and button the player has rebound, for one input actions asset. | Everyone |

### UI elements

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Setting Toggle](Components/Setting%20Toggle.md) | An on / off switch with a label that reads "On" or "Off". | Everyone |
| [🔖 Setting Slider](Components/Setting%20Slider.md) | A slider, with optional minus and plus buttons and a number readout. | Everyone |
| [🔖 Setting Dropdown](Components/Setting%20Dropdown.md) | A classic dropdown list. | Everyone |
| [🔖 Int Multiple Choice Element](Components/Int%20Multiple%20Choice%20Element.md) | A left arrow, a label, a right arrow, and a row of dots showing which option is selected. | Everyone |
| [🔖 String Multiple Choice Element](Components/String%20Multiple%20Choice%20Element.md) | The same left arrow / label / right arrow picker as Int Multiple Choice Element, with one difference: it saves the chosen word itself instead of its position in the list. | Everyone |
| [🔖 Resolution Choice Element](Components/Resolution%20Choice%20Element.md) | The arrow picker for screen resolution. | Everyone |
| [🔖 Selection Indicator Button](Components/Selection%20Indicator%20Button.md) | One dot in the row underneath an arrow picker. | Everyone |
| [🔖 Rebind Button](Components/Rebind%20Button.md) | One row in the controls menu: click it, press a key, and the action is rebound. | Everyone |
| [🔖 Setting Reset Button](Components/Setting%20Reset%20Button.md) | A small button next to a settings row that resets that one row. | Everyone |
| [🔖 Settings Preset Button](Components/Settings%20Preset%20Button.md) | Applies a settings preset like Low, Medium or High and shows whether it is still active. | Everyone |
| [🔖 Setting Flavor Text](Components/Setting%20Flavor%20Text.md) | The panel that shows the name and explanation of whatever setting the player is currently hovering or has selected. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Control Settings](Components/Control%20Settings.md) | Holds the control choices (sensitivity, invert) so gameplay code reads them from one place. | Programmers |
<!-- all-components:end -->