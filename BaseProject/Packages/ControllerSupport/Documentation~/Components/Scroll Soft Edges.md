---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - gamepad
  - ui
summary: Fades the top and bottom edge of a scroll view while more rows lie beyond it.
requires:
  - Scroll Rect
---
# Scroll Soft Edges

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

Fades the top and bottom edge of a scroll view once it has been scrolled, each only while more rows lie beyond it. At rest the first rows show crisp and whole, at the end the last ones do.

**Add it:** Inspector › **Add Component** › search `Scroll Soft Edges`. Needs **Scroll Rect** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a scroll view that has a Rect Mask 2D.
2. Set **Softness** to the fade you want once scrolled; the mask's own softness is overwritten at runtime.
3. Turn on **Is Fading Toward More** for a list that must show it goes on: its bottom edge fades already at rest. The mask's padding is moved at runtime so the last row shows whole at the end.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Scroll** | Scroll Rect |  | **Required.** Filled in automatically. The scroll view. Auto-assigned from the same GameObject. |
| **Mask** | Rect Mask2 D |  | **Required.** Filled in automatically. The mask whose edges fade. Auto-assigned from the same GameObject. |
| **Softness** | Whole number | 24 | Pixels of the fade at the top and bottom edge once scrolled. |
| **Is Fading Toward More** | Toggle | Off | Fades the bottom edge already at rest, so the list says it scrolls before anyone moves it. Off: the edges fade only once scrolled. Either way an edge fades only while more rows lie beyond it. |