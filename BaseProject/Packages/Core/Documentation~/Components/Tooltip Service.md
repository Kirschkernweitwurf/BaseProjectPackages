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
- `Dock(owner, area)` opens every tooltip in one fixed area instead of beside its target, with its top right corner in the area's top right, like an inspector panel. Use it where tooltips beside their targets would cover what the player compares, like the cards in a hand. `ReleaseDock(owner)` ends it.
- `Pin()` keeps the shown tooltip while other requests come and go, for example while a key is held; `Unpin()` lets it go and the view catches up. Its own trigger may rebuild it while pinned (`IsPinnedBy(caller)`, `PinnedChanged`), for example to add more detail.

## Related

- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🔖 Tooltip View](Tooltip%20View.md)