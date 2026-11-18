---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
  - ui
summary: Turns left and right on the focused element into steps, like browsing a carousel.
---
# Side Step Input

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad #ui

Turns left and right on the focused element into steps, like browsing a carousel while the focus stays on its button. Up and down keep moving the focus as usual. The element needs no left or right neighbors, or the focus would move instead.

**Add it:** Inspector › **Add Component** › search `Side Step Input`. Only one per GameObject.

## How to use it

1. Put it on the element that keeps the focus, like the current item of a carousel.
2. Give that button no left or right neighbors.
3. Hook `Stepped` in code: -1 is left, 1 is right.

## Fields

None. Add it and it works.