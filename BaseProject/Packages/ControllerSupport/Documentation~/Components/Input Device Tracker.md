---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - input
summary: Knows whether the player is using mouse and keyboard or a gamepad right now.
---
# Input Device Tracker

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #input

Knows which device the player is using right now: mouse and keyboard, or a gamepad. It switches on real button presses and stick movement, not on noise like a slightly drifting stick. For a gamepad it also knows the kind (Xbox, PlayStation, Switch, Steam Deck, Steam Controller or generic), taken from the names the gamepad that was actually pressed reports. So the first button press, like on a "Press any key" screen, already picks the right icons, and picking up another kind of gamepad switches them. It remembers the last device across sessions: a gamepad player who starts the game again sees gamepad icons from the first frame, as long as a gamepad is plugged in.

**Add it:** Inspector › **Add Component** › search `Input Device Tracker`.

## How to use it

1. Put it on the persistent manager prefab, once.

## Fields

None. Add it and it works.

## Related

- [🗞️ Input Glyph Provider](Input%20Glyph%20Provider.md)
- [🗞️ Focus Watchdog](Focus%20Watchdog.md)
- [🗳️ Controller Support components](../Controller%20Support%20Components.md)