---
type: Component
package: Settings
audience: Everyone
tags:
  - designer
  - settings
  - ui
summary: Applies a settings preset like Low, Medium or High and shows whether it is still active.
requires:
  - Button
---
# Settings Preset Button

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

Applies a Settings Preset (for example Low, Medium, High graphics) when clicked, and lights up while the current settings still match it. As soon as the player changes something by hand, every preset button goes dark, which is the honest state.

**Add it:** Inspector › **Add Component** › search `Settings Preset Button`. Needs **Button** on the same GameObject.

## How to use it

1. Create a **Settings Preset** asset per preset.
2. Add one button per preset in a row.
3. Add **Settings Preset Button** to each and assign its preset.
4. Optional: a label for the preset name and an object shown while the preset is active.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Preset** | Settings Preset |  | **Required.** The preset this button applies. |
| **Button** | Button |  | Filled in automatically. The button on this GameObject. |
| **Label** | Text (TMP) |  | Optional label filled with the preset's own display name. |
| **Active Indicator** | GameObject |  | Optional object shown while every value in the preset matches the current settings. |

## Related

- [🗞️ Settings Context](Settings%20Context.md)
- [🔖 Quality Level Setting](Quality%20Level%20Setting.md)
- [🗳️ Settings components overview](../Components.md)