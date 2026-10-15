---
type: Component
package: Controller Support
audience: Programmers
tags:
  - programmer
  - gamepad
summary: Plays gamepad rumble patterns, with priorities so important rumble wins.
---
# Rumble Service

🗞️ **Component** · programmers only

**Tags:** #programmer #gamepad

Plays gamepad rumble from curve-based patterns. Requests have a priority: a scripted hit outranks an engine hum. Only the strongest request reaches the motors, but every request keeps its own timer, so a short burst that was outranked still ends on time.

**Add it:** Inspector › **Add Component** › search `Rumble Service`.

## How to use it

1. Put it on the persistent manager prefab, once.
2. Create a **Rumble Config** asset and assign it.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Rumble Config |  | **Required.** The Rumble Config asset with the defaults. Share it with the rumble setting components. |

## Related

- [🔖 Rumble Enabled Setting](Rumble%20Enabled%20Setting.md)
- [🔖 Rumble Intensity Setting](Rumble%20Intensity%20Setting.md)
- [🗳️ Controller Support components](../Controller%20Support%20Components.md)