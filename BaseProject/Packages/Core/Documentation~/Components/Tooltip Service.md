---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
  - ui
summary: Shows the most important requested tooltip.
---
# Tooltip Service

🗞️ **Component** · programmers only

**Tags:** #programmer #ui

Shows the highest priority tooltip that is currently requested. Backed by a `PriorityTracker`, so overlapping requests resolve by priority.

**Add it:** Inspector › **Add Component** › search `Tooltip Service`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.

## Fields

None. Add it and it works.

## Good to know

- Needs a Tooltip View in the UI to draw anything.
- `KeepClear(owner, area)` keeps a tooltip that opens beside its target clear of an area running alongside the target, like a gamepad pin pointing at it from the side: the tooltip opens past the area. `ReleaseClear(owner)` ends it.

## Related

- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🔖 Tooltip View](Tooltip%20View.md)