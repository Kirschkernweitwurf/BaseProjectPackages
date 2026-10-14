---
type: Component
package: Settings
audience: Everyone
tags:
  - designer
  - settings
  - ui
summary: A small button next to a settings row that resets that one row.
requires:
  - Button
---
# Setting Reset Button

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

A small button next to a settings row that resets just that row to its default. Without it, the player has to focus the row and press the reset key (F on the keyboard).

**Add it:** Inspector › **Add Component** › search `Setting Reset Button`. Needs **Button** on the same GameObject.

## How to use it

1. Add a button next to the row.
2. Add **Setting Reset Button** and drag the row's element into **Target**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Target** | Setting Element |  | **Required.** The element this button resets. |
| **Button** | Button |  | Filled in automatically. The button on this GameObject. |
| **Disable When Default** | Toggle | On | Greys the button out while the setting already holds its default value. |

## Related

- [🔖 Setting Slider](Setting%20Slider.md)
- [🔖 Setting Toggle](Setting%20Toggle.md)
- [🔖 Setting Dropdown](Setting%20Dropdown.md)
- [🗳️ Settings components overview](../Components.md)