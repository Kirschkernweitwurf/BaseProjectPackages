---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - ui
  - scenes
summary: Turns a GameObject on after a delay in seconds.
---
# Activate After Time

🔖 **Component** · for everyone

**Tags:** #designer #ui #scenes

Turns a GameObject on after a delay. The target is switched **off** when the scene starts and switched on when the time is up.

**Add it:** Inspector › **Add Component** › search `Activate After Time`.

## How to use it

1. Add it to a GameObject that stays active, not to the target itself.
2. Drag in the **Target** and set the **Delay**.
3. Turn on **Use Unscaled Time** if it sits in a menu that pauses the game.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Delay** | Number | 1 | Seconds to wait. |
| **Use Unscaled Time** | Toggle | Off | Ignore the game speed, so the timer also runs while the game is paused. |
| **Target** | GameObject |  | The GameObject to turn on. |

## Good to know

- Putting it on the object it turns off would stop the component too.
- Typical uses: a tutorial hint after a few seconds, staggered UI.

## Related

- [🔖 Activate After Frames](Activate%20After%20Frames.md)
- [📖 Core utility guide](Utility.md)