---
type: Component
package: Settings
audience: Everyone
tags:
  - designer
  - settings
  - input
summary: Stores the look sensitivity as a value from 0 to 1 and passes the matching multiplier to Control Settings.
---
# Look Sensitivity Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #input

Stores the look sensitivity as a value from 0 to 1 and passes the matching multiplier to Control Settings. The slider always runs 0 to 1; only the range below decides what that means in game.

**Add it:** Inspector › **Add Component** › search `Look Sensitivity Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Slider** to the menu and type `LookSensitivity` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Sensitivity Range** | Vector2 | (0.2, 3) | Multiplier at the slider's left end (X) and right end (Y). |
| **Default Normalized** | Number | 0.5 | Slider position before the player has changed anything. `0.5` is the middle. |

## Good to know

- The key is always `LookSensitivity`. You cannot change it, so the UI cannot get out of sync.
- To make the game faster or slower overall, change **Sensitivity Range**, not the slider.

## Related

- [🗞️ Settings Context](Settings%20Context.md)
- [🗞️ Control Settings](Control%20Settings.md)
- [🔖 Invert Look Setting](Invert%20Look%20Setting.md)
- [🔖 Setting Slider](Setting%20Slider.md)
- [🗳️ Settings components overview](../Components.md)