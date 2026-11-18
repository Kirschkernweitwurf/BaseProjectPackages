---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - ui
summary: Makes tooltips beside the things in a panel open past the panel.
---
# Tooltip Keep Clear

🔖 **Component** · for everyone

**Tags:** #designer #ui

Keeps this UI element clear of the tooltips of the things inside it or alongside it: a tooltip beside one of them opens past this element instead of covering it, like a list whose rows explain themselves. The tooltip and its tail then sit next to the list, not on it.

**Add it:** Inspector › **Add Component** › search `Tooltip Keep Clear`. Only one per GameObject.

## How to use it

1. Add it to the panel whose rows or items have tooltips.
2. Leave **Area** on the panel's own rect, or point it at the part that is visible, like a list's content.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Area** | Rect Transform |  | The area a tooltip beside a target inside it or alongside it opens past, usually this panel's own visible rect. |

## Good to know

- It changes tooltips beside their targets whose height overlaps the area: their target counts as wide as the area, so the tooltip opens past it. The tail still points at the target's middle.
- Tooltips above their targets and in a dock ignore it. To keep a panel from being covered by those, use [🔖 Tooltip Keep Visible](Tooltip%20Keep%20Visible.md).
- The area is measured each time a tooltip is placed, so a panel that moves, grows or folds is always measured as it is.
- Code does the same with `TooltipService.KeepClear(owner, area)` and `ReleaseClear(owner)`.

## Related

- [🔖 Tooltip View](Tooltip%20View.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)
- [🔖 Tooltip Keep Visible](Tooltip%20Keep%20Visible.md)