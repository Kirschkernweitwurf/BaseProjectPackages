---
type: Component
package: Tweening
audience: Programmers
tags:
  - programmer
  - animation
summary: The service that moves every running tween forward each frame.
---
# Tween Runner

🗞️ **Component** · programmers only

**Tags:** #programmer #animation

The service that advances every running tween each frame and announces when tweens start and finish. Every tween in the game depends on it.

**Add it:** Inspector › **Add Component** › search `Tween Runner`.

## How to use it

1. Put it on the persistent manager prefab, once.
2. Leave the defaults unless tweens jump after a breakpoint or an alt-tab.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Max Unscaled Delta Time** | Number | 0.05 | The longest time step a tween may take in one frame, in seconds. Stops tweens from jumping to the end after a hitch or a breakpoint. |
| **Skip First Frame After Focus Or Pause** | Toggle | On | Skips one frame after the game regains focus or unpauses, so tweens do not lurch forward. |

## Good to know

- Without a Tween Runner in the scene, no tween moves at all.

## Related

- [🔖 Tween Group](Tween%20Group.md)
- [📖 Tweening guide](Tweening.md)