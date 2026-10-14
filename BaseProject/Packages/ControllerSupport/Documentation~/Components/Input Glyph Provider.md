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

Picks the right button icon for an action, based on the device in use: a keyboard key, an Xbox button, a PlayStation button. Prompts can show it as an image or inline in a TextMeshPro text, and they refresh when the device changes.

**Add it:** Inspector › **Add Component** › search `Input Glyph Provider`.

## How to use it

1. Put it on the persistent manager prefab, once.
2. Assign one **Input Glyph Set** asset per device family.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Glyph Sets** | List of Input Glyph Set |  | **Required.** One glyph set per device family. |

## Related

- [🗞️ Input Device Tracker](Input%20Device%20Tracker.md)
- [🗳️ Controller Support components](../Components.md)