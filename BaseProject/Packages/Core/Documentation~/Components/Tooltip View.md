---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: Draws the tooltip next to the cursor and keeps it on screen.
---
# Tooltip View

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Draws the tooltip text and keeps it next to the cursor, never letting it leave the screen. One per UI is enough; every Tooltip Trigger uses it.

**Add it:** Inspector › **Add Component** › search `Tooltip View`. Only one per GameObject.

## How to use it

1. Build the tooltip look: a panel (the content) with a TextMeshPro text inside.
2. Add **Tooltip View** to it and drag in the content, the text and the panel's RectTransform.
3. Put it under the top-most canvas so it draws above everything.

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
| **Text Element** | Text (TMP) |  | **Required.** Text element that shows the message. |
| **Tooltip Rect** | Rect Transform |  | **Required.** RectTransform used for positioning, the rect of the content object. |
| **Canvas** | Canvas |  | Filled in automatically. Canvas the tooltip lives under. Auto-assigned from the parents when empty. |

## Good to know

- The content starts hidden and only shows while a trigger is hovered.

## Related

- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)