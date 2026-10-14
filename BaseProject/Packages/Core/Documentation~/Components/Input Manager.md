---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
  - input
summary: Decides which input action map is active, by priority.
---
# Input Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #input

Scene-level manager of input action maps. Maps are registered with a priority and the manager enables the highest-priority map while disabling all others. The package's own Permanent map from `BaseInputActions` stays enabled at all times, so global actions like pausing or opening a menu always work. The manager also tracks whether the cursor is over a UI element, which can be used to block input while the player interacts with the UI.

**Add it:** Inspector › **Add Component** › search `Input Manager`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.

## Fields

None. Add it and it works.

## Good to know

- The Permanent map stays on all the time: Back (Escape, gamepad B), the cheat console (F1) and screenshots (F6 small, F7 large).

## Related

- [🔖 Menu Input Map Module](Menu%20Input%20Map%20Module.md)