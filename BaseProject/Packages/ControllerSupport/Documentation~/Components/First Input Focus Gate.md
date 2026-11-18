---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - ui
summary: Keeps the focus off a screen until the player steers it.
requires:
  - Navigable Group
---
# First Input Focus Gate

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #ui

Keeps the focus off a screen until the player steers it. Without it the Focus Watchdog selects the default element as soon as the screen takes input, so a player who mashes confirm through a lock screen presses whatever comes first behind it. The first press only lights the focus.

**Add it:** Inspector › **Add Component** › search `First Input Focus Gate`. Needs **Navigable Group** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it next to the **Navigable Group** of a screen that opens behind a press, like a main menu behind a lock screen.
2. Drag the screen's **Canvas Group** into **Screen**, so the press that revealed it does not count.
3. Set **Navigate Action** and **Submit Action** to the UI actions of the input module.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Group** | Navigable Group |  | **Required.** Filled in automatically. The group whose focus stays empty until the first input. |
| **Screen** | Canvas Group |  | **Required.** The screen that takes the input, like the homescreen. Presses before it is interactable, like the one that unlocks the lock screen, do not count. |
| **Navigate Action** | Input Action Reference |  | **Required.** The navigate action of the UI, like the stick and the arrow keys. |
| **Submit Action** | Input Action Reference |  | **Required.** The submit action of the UI, like A or Enter. |