---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - ui
summary: Presses its button when the player goes back, so Back leaves a screen the way a click on that button does.
requires:
  - Button
---
# Back Button

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #ui

Presses its button when the player goes back, so Back leaves a screen the way a click on that button does, with its animation. It takes Back through the menu manager: it only acts while its screen is on top.

**Add it:** Inspector › **Add Component** › search `Back Button`. Needs **Button** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to the button Back should press, like a Resume button that plays its own animation.
2. Nothing else: it claims Back on the menu manager while it is enabled and lets go when disabled.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Button** | Button |  | **Required.** Filled in automatically. The button Back presses. Auto-assigned from the same GameObject. |
| **Is Focused First** | Toggle | Off | Back first moves the focus to the button and presses it only while it has the focus. For a button that throws something away, like Skip on a reward pick. |

## Good to know

- Use it only when Back should look like a click on a visible button. A menu that just closes on Back needs no Back Button; turn on **Listen To On Back Action** on its Menu instead.