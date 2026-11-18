---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - ui
summary: Keeps a HUD panel readable while tooltips open above their targets.
---
# Tooltip Keep Visible

🔖 **Component** · for everyone

**Tags:** #designer #ui

Keeps this UI element readable while tooltips open above their targets: a tooltip that would cover it opens beside its target instead. For HUD panels the player reads while choosing, like a score or a list of rules.

**Add it:** Inspector › **Add Component** › search `Tooltip Keep Visible`. Only one per GameObject.

## How to use it

1. Add it to a HUD panel that must stay readable.
2. Leave **Area** on the panel's own rect, or point it at the part that matters.
3. Set the triggers that should open above their targets to **Above Target** (see [🔖 Tooltip Trigger](Tooltip%20Trigger.md)).

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Area** | Rect Transform |  | The area tooltips above their targets must not cover, usually this panel's own rect. |

## Good to know

- It only changes tooltips that open above their targets. Tooltips beside their targets and in a dock ignore it.
- The area is measured each time a tooltip is placed, so a panel that moves, grows or folds is always measured as it is.
- It registers while enabled. A disabled panel no longer keeps its area visible.
- Code does the same with `TooltipService.KeepVisible(owner, area)` and `ReleaseVisible(owner)`.

## Related

- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🔖 Tooltip View](Tooltip%20View.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)