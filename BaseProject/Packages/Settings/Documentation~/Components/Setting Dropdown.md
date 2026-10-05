# Setting Dropdown

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

A classic dropdown list.

Needs a **TMP_Dropdown** on the same GameObject.

Works with settings that store a **position in a list** (a whole number), which covers FullScreenMode, Quality, VSync and Language.

## Inspector fields

Shared by every UI element:

| Field | What to put in it |
| --- | --- |
| **Setting Key** | The key of the setting this controls. Must match exactly. Required. |
| **Title** | Localized name shown in the [🔖 Setting Flavor Text](Setting%20Flavor%20Text.md) panel. |
| **Description** | Localized explanation shown in the same panel. |

Nothing else. The options come from the TMP_Dropdown component itself.

## Filling in the options

On the **TMP_Dropdown** component, open the **Options** list and type one entry per choice.

The order has to match the setting component exactly. For example, if [🔖 Full Screen Mode Setting](Full%20Screen%20Mode%20Setting.md) lists Exclusive, Borderless, Windowed, then your dropdown options must be in that same order.

## Good to know

- Dropdown options are plain text and are **not localized**. If you need translated options, use an [🔖 Int Multiple Choice Element](Int%20Multiple%20Choice%20Element.md) or ask a programmer to refill the options at runtime.
- If a saved value is larger than the number of options, it snaps to the last option.
- Dropdowns can be awkward with a gamepad. Arrow pickers usually feel better in a game menu.

## Setup checklist

- Options typed into the TMP_Dropdown, in the same order as the setting component.
- Setting Key matches.
