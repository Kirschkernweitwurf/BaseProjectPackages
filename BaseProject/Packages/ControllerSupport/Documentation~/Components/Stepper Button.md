---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - ui
summary: A button that steps a value with left and right instead of moving the focus.
---
# Stepper Button

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #ui

A button that steps a value with left and right instead of moving the focus, like a settings row, so it can sit in a Navigable Group that wires its sides to other buttons. Up and down move the focus as usual and a press still clicks.

**Add it:** Inspector › **Add Component** › search `Stepper Button`.

## How to use it

1. Use it in place of a Button on a row that holds a value, like a difficulty row.
2. Hook `Stepped` in code: -1 is left, 1 is right. A press still raises `onClick`, so it can step forward too.
3. Its group may wire left and right to other buttons; this button never follows them, unlike a [Side Step Input](Side%20Step%20Input.md).

## Fields

None. Add it and it works.