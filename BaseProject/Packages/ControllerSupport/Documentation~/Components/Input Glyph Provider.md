---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - input
  - ui
summary: Picks the right button icon (keyboard, Xbox, PlayStation, ...) for an action.
---
# Input Glyph Provider

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #input #ui

Picks the right button icon for an action, based on the device in use: a keyboard key, an Xbox button, a PlayStation button. Prompts can show it as an image or inline in a TextMeshPro text, and they refresh when the device changes. For a gamepad it takes the set of the gamepad's kind first, then the **Generic** gamepad set, then any gamepad set.

**Add it:** Inspector › **Add Component** › search `Input Glyph Provider`.

## How to use it

1. Put it on the persistent manager prefab, once.
2. Assign one **Input Glyph Set** asset per device family, and one per kind of gamepad that should show its own buttons (set **Gamepad Family** on the set). Make one gamepad set **Generic** as the fallback.
3. Show the icons with an [Input Prompt View](Input%20Prompt%20View.md).

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Glyph Sets** | List of Input Glyph Set |  | **Required.** One glyph set per device family. |

## Related

- [🗞️ Input Device Tracker](Input%20Device%20Tracker.md)
- [🗳️ Controller Support components](../Controller%20Support%20Components.md)