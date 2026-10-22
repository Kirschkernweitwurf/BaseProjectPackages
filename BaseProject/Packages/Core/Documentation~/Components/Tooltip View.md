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

Draws the tooltip beside the cursor or the hovered object, never letting it leave the screen. It prefers the right side with the tops lined up and flips to the left or upward when there is no room. The spot is picked while the tooltip waits to appear and then kept, so it never wanders when the object moves on screen, for example under a camera that follows the pointer. A tooltip can have several panels, for example a description plus one panel per keyword it mentions. Each panel is a copy of the first one. One per UI is enough; every Tooltip Trigger uses it.

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

### References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Content** | GameObject |  | **Required.** Content GameObject that holds the tooltip visuals. |
| **Panel Template** | Tooltip Panel |  | **Required.** First panel inside the content. Further entries get copies of it, next to it. |
| **Tooltip Rect** | Rect Transform |  | **Required.** RectTransform used for positioning, the rect of the content object. |
| **Canvas** | Canvas |  | Filled in automatically. Canvas the tooltip lives under. Auto-assigned from the parents when empty. |

## Good to know

- The content starts hidden and only shows while a trigger is hovered.
- Copied panels are kept and reused, so a big tooltip only creates them once.
- Sweeping the pointer across the screen stays calm thanks to the show delay, while moving from one tooltip straight to the next shows it at once.
- Code can change the show delay at runtime through `ShowDelay`, for example for a tooltip speed setting.
- The content gets a Canvas Group that never takes the pointer, so a tooltip covering its own object cannot make it flicker.

## Related

- [🔖 Tooltip Panel](Tooltip%20Panel.md)
- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)