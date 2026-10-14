---
type: component
package: Settings
audience: everyone
tags:
  - designer
  - settings
  - input
  - ui
summary: "One row in the controls menu: click it, press a key, and the action is rebound."
requires:
  - Button
---
# Rebind Button

🔖 **Component** · for everyone

**Tags:** #designer #settings #input #ui

One row in a controls menu. The player clicks it, presses a key or button, and the action is rebound. The result is saved through the Rebind Setting.

**Add it:** Inspector › **Add Component** › search `Rebind Button`. Needs **Button** on the same GameObject.

## How to use it

1. Build a button with a text that shows the current binding.
2. Add **Rebind Button**.
3. Drag in the **Rebind Setting** and type `Rebinds` into **Setting Key**.
4. Pick the **Action Reference** and the **Binding Index** (each part of a composite, like W/A/S/D, has its own index).

## Fields

### Setting Element

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Setting Key** | Text |  | **Required.** Always `Rebinds`, the key of the Rebind Setting. |
| **Title** | Localized text |  | Shown in the Setting Flavor Text while this row is focused. |
| **Description** | Localized text |  | Shown in the Setting Flavor Text while this row is focused. |

### Rebind

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Rebind Setting** | [Rebind Setting](Rebind%20Setting.md) |  | **Required.** The component owning the asset and the persisted overrides. Its key goes in Setting Key. |
| **Action Reference** | Input Action |  | **Required.** Names the action to rebind. Resolved by id against the asset above. |
| **Binding Index** | Whole number | 0 | Index of the binding inside the action. Every part of a composite has its own index. |
| **Trigger Button** | Button |  | Filled in automatically. The button that starts listening. |
| **Binding Text** | Text (TMP) |  | **Required.** Shows the current binding. |
| **Listening Label** | Localized text |  | Shown while waiting for the player to press something. |
| **Cancel Path** | Text | `<Keyboard>/escape` | Control that aborts the rebind and keeps the old binding. Leave empty to allow none. |

## Good to know

- Escape cancels by default and keeps the old binding. Change **Cancel Path** to allow another key, or empty it.

## Related

- [🔖 Rebind Setting](Rebind%20Setting.md)
- [🔖 Setting Reset Button](Setting%20Reset%20Button.md)
- [🔖 Setting Flavor Text](Setting%20Flavor%20Text.md)
- [🗳️ Settings components overview](../Components.md)