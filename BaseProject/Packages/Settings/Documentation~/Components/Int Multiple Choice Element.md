# Int Multiple Choice Element

🔖 **Component** · for everyone

A left arrow, a label, a right arrow, and a row of dots showing which option is selected. The console-style settings control.

Saves the **position** of the chosen option, so use it for [🔖 Full Screen Mode Setting](<Full Screen Mode Setting.md>), [🔖 Quality Level Setting](<Quality Level Setting.md>), [🔖 V Sync Setting](<V-Sync Setting.md>) and 🪛[Language Setting](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Localization/Documentation~/Components/Language%20Setting.md).

For settings that save a word instead, use [🔖 String Multiple Choice Element](<String Multiple Choice Element.md>).

## Inspector fields

Shared by every UI element:

| Field | What to put in it |
| --- | --- |
| **Setting Key** | The key of the setting this controls. Must match exactly. Required. |
| **Title** | Localized name shown in the [🔖 Setting Flavor Text](<Setting Flavor Text.md>) panel. |
| **Description** | Localized explanation shown in the same panel. |

Shared by every arrow picker:

| Field | What to put in it |
| --- | --- |
| **Left Button** | The button that goes back one option. Required. |
| **Right Button** | The button that goes forward one option. Required. |
| **Value Text** | The text object showing the current option. Required. |
| **Selection Indicator Prefab** | A prefab with a [🔖 Selection Indicator Button](<Selection Indicator Button.md>) on it. This is one dot. Required. |
| **Selection Indicator Parent** | The container the dots are spawned into. Usually put a Horizontal Layout Group on it. Required. |
| **Options** | The list of labels the player cycles through. |

## Good to know

- The list **wraps around**. Pressing right on the last option goes back to the first.
- Dots are clickable. Clicking the third dot jumps straight to the third option.
- One dot is created per option, automatically. Do not place them by hand.
- The Options list is plain text and is **not localized**. Language names, which stay the same in every language, are a good fit. Quality names like "Low" and "High" are not, unless your game is English only.

## Setup checklist

- Options are in the same order as the list on the setting component.
- Both buttons, Value Text, indicator prefab and indicator parent are assigned.
- Indicator parent has a layout group so the dots line up.
- Setting Key matches.
