---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - menus
summary: Ignores clicks and confirms for a moment after the menu opens.
---
# Menu Open Click Guard

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #menus

Ignores clicks and confirms for a moment after the menu opens. Without it the second press of a fast double click, or of confirm pressed twice, lands on the screen the first press opened. The content keeps blocking raycasts, so an ignored click never falls through to the screen behind.

**Add it:** Inspector › **Add Component** › search `Menu Open Click Guard`. Only one per GameObject.

## How to use it

1. Add it to the root of a screen, next to its **Menu**. The menu's content root needs a **Canvas Group**.
2. Leave **Guard Duration** at about 0.35 seconds: long enough for a fast double click, too short to notice.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Guard Duration** | Number | 0.35 | Seconds after the menu opens in which presses are ignored. A bit longer than a fast double click. Runs on unscaled time, since many menus stop the game time. |

## Good to know

- Add it to every screen a click or confirm can open.