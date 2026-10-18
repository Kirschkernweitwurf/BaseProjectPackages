---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: Draws the tooltip panels next to the cursor and keeps them on screen.
---
# Tooltip View

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Draws the tooltip next to the cursor, never letting it leave the screen. A tooltip can have several panels, for example a description plus one panel per keyword it mentions. Each panel is a copy of the first one. One per UI is enough; every Tooltip Trigger uses it.

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
| **Screen Offset** | Vector2 | (15, 15) | Distance in pixels between the cursor and the tooltip. |
| **Edge Margin** | Number | 8 | Distance in pixels the tooltip keeps away from the screen edge. |

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

## Related

- [🔖 Tooltip Panel](Tooltip%20Panel.md)
- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)