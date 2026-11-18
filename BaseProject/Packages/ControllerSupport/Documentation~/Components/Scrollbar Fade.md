---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - gamepad
  - ui
summary: Shows a scroll bar only while its list moves.
requires:
  - Canvas Group
---
# Scrollbar Fade

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

Shows a scroll bar only while its list moves, like an overlay scroll bar on a phone: scrolling brings it in at once and a moment after the list rests it fades out again. Opening a list that moves to its top flashes it once, which tells that the list scrolls. Runs on unscaled time, because menus often stop the time scale.

**Add it:** Inspector › **Add Component** › search `Scrollbar Fade`. Needs **Canvas Group** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a scroll bar with a Canvas Group and point **Scroll** at the scroll view the bar belongs to. The bar stays hidden at rest.
2. Set the scroll view's scroll bar visibility to Auto Hide, so a list that fits shows no bar at all.
3. Pair it with [Scroll Soft Edges](Scroll%20Soft%20Edges.md) for the fading list edges.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Scroll** | Scroll Rect |  | **Required.** The scroll view whose movement shows the bar. |
| **Group** | Canvas Group |  | **Required.** Filled in automatically. Fades the bar. |
| **Hold Seconds** | Number | 0.8 | Seconds the bar stays after the list stopped moving. |
| **Fade Seconds** | Number | 0.3 | Seconds the bar takes to fade out. |