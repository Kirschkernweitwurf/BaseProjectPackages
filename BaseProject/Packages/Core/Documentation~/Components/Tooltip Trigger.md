---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - ui
summary: Shows a tooltip while the pointer hovers this object.
---
# Tooltip Trigger

🔖 **Component** · for everyone

**Tags:** #designer #ui

Shows a tooltip while the pointer hovers this object and hides it when the pointer leaves.

**Add it:** Inspector › **Add Component** › search `Tooltip Trigger`. Only one per GameObject.

## How to use it

1. Add it to the UI element or object that should explain itself.
2. Write the **Tooltip Text**.
3. Make sure the UI has one **Tooltip View**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Tooltip Text** | Text |  | **Required.** The text to show. Several lines are fine. |
| **Priority** | Priority |  | When several tooltips overlap, the highest priority wins. |

## Good to know

- Needs a Graphic with **Raycast Target** on and an Event System in the scene.
- An empty text logs a warning and shows nothing.
- If the object is disabled while hovered, the tooltip hides itself.

## Related

- [🔖 Tooltip View](Tooltip%20View.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)
- [📖 Core utility guide](Utility.md)