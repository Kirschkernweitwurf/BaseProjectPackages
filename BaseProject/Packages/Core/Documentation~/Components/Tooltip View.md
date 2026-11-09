---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: Draws the tooltip panels beside the cursor or the hovered object and keeps them on screen.
---
# Tooltip View

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Draws the tooltip beside the cursor or the hovered object, never letting it leave the screen. It prefers the right side with the tops lined up and flips to the left or upward when there is no room. The spot is picked while the tooltip waits to appear and then kept, so it never wanders when the object moves on screen, for example under a camera that follows the pointer. While the Tooltip Service holds a dock, every tooltip opens in the dock area instead, its top right corner in the area's top right. A tooltip can have several panels, for example a description plus one panel per keyword it mentions. Each panel is a copy of the first one. One per UI is enough; every Tooltip Trigger uses it.

**Add it:** Inspector › **Add Component** › search `Tooltip View`. Only one per GameObject.

## How to use it

1. Build the content: an empty RectTransform with a layout group and a Content Size Fitter. A Vertical Layout Group stacks the panels, a Horizontal one lines them up side by side.
2. Build one panel inside it with a [Tooltip Panel](Tooltip%20Panel.md).
3. Add **Tooltip View** and drag in the content, the panel and the content's RectTransform.
4. Put it under the top-most canvas so it draws above everything.

## Fields

### Settings

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Screen Offset** | Vector2 | (15, 15) | Distance in pixels between the anchor and the tooltip. |
| **Edge Margin** | Number | 8 | Distance in pixels the tooltip keeps away from the screen edge. |

### Timing

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Show Delay** | Number | 0.3 | Seconds the pointer rests before a tooltip shows. Unscaled, so it also works while paused. |
| **Fade Duration** | Number | 0.1 | Seconds a tooltip takes to fade in. |
| **Warm Duration** | Number | 0.4 | Seconds after a tooltip hid in which the next one shows at once, without delay or fade. |
| **Slide Distance** | Number | 8 | How far a tooltip in the dock slides in from the right while it fades in, in canvas units. 0 only fades. |
| **Swap Duration** | Number | 0.12 | Seconds a tooltip that moves straight on to the next thing takes to bring its new content in. |

### References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Content** | GameObject |  | **Required.** Content GameObject that holds the tooltip visuals. |
| **Panel Template** | Tooltip Panel |  | **Required.** First panel inside the content. Further entries get copies of it, next to it. |
| **Source Panel** | Tooltip Panel |  | Header of the card, filled from the tooltip's source: what the panels explain. A tooltip with one titled entry and no source shows that title here and only its text below. Optional: without it every entry is a panel of its own. |
| **Tooltip Rect** | Rect Transform |  | **Required.** RectTransform used for positioning, the rect of the content object. |
| **Tail** | Rect Transform |  | Small pointer on the tooltip's edge toward its target while it opens beside it, like on a popover. Authored on the left edge pointing left, pivot on its right; flipped when the tooltip opens left of the target, hidden in the dock and at the cursor. Optional. |
| **Tail Margin** | Number | 22 | Distance the tail keeps from the tooltip's top and bottom edge, in canvas units, so it never sits on a rounded corner. |
| **Canvas** | Canvas |  | Filled in automatically. Canvas the tooltip lives under. Auto-assigned from the parents when empty. |

## Good to know

- **Motion:** a tooltip in the dock slides in while it fades; moving straight on to the next thing only fades the new content in, and the same tooltip again (rebuilt, pinned, or hovered again right away) just changes in place. Set `IsMotionReduced` from a Reduce Motion setting to drop the slide.

- **One card:** put the panel template and a **Source Panel** inside one shared background, and the tooltip reads as a card: the header names what it explains (from an `ITooltipSourceProvider` next to the trigger's providers), every titled panel after it gets its **Separator**.

- The content starts hidden and only shows while a trigger is hovered.
- Copied panels are kept and reused, so a big tooltip only creates them once.
- Sweeping the pointer across the screen stays calm thanks to the show delay, while moving from one tooltip straight to the next shows it at once.
- Code can change the show delay at runtime through `ShowDelay`, for example for a tooltip speed setting.
- The content gets a Canvas Group that never takes the pointer, so a tooltip covering its own object cannot make it flicker.

## Related

- [🔖 Tooltip Panel](Tooltip%20Panel.md)
- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)