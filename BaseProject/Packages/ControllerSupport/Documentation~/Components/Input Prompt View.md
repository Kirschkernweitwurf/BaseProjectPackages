---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - artist
  - gamepad
  - input
  - ui
summary: Shows the button icon of one action for the device in use, and follows the player when they switch.
---
# Input Prompt View

🔖 **Component** · for everyone

**Tags:** #designer #artist #gamepad #input #ui

Shows the button icon of one action for the device in use: the cross on a PlayStation pad, B on a Switch (the bottom button there), A on an Xbox pad, Enter on a keyboard. It follows the player when they switch devices or pick up another kind of gamepad. A prompt for an input only one device has, like a shoulder button or the Tab key, shows only while that device is in use, so the player always sees every input that works right now and nothing else.

**Add it:** Inspector › **Add Component** › search `Input Prompt View`. Needs an **Image** on the same GameObject. Only one per GameObject.

## How to use it

1. Put it on an **Image** that should show the button, next to the label it explains ("Play", "Back").
2. Pick the **Action**, for example `UI/Submit`.
3. Optional: set **Group** to the object holding icon and label, so both hide together.
4. Set **Shown On** to **Gamepad Only** or **Mouse Keyboard Only** for inputs the other device does not have, like LB or Shift.
5. The icons come from the [Input Glyph Provider](Input%20Glyph%20Provider.md). Tint the Image as you like; the glyphs are white.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| **Action** | Yes | The action whose button is shown. |
| **Icon** | Yes | Shows the button icon. Auto-assigned from the same GameObject. |
| **Group** | No | Hidden together with the icon, like the prompt's label. Leave empty to hide only the icon. |
| **Shown On** | No | On which devices the prompt shows. Pick one device for inputs the other one does not have. |

## Related

- [🗞️ Input Glyph Provider](Input%20Glyph%20Provider.md)
- [🗞️ Input Device Tracker](Input%20Device%20Tracker.md)
- [🗳️ Controller Support components](../Controller%20Support%20Components.md)