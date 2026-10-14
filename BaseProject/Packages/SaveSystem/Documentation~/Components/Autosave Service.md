---
type: component
package: Save System
audience: programmer
tags:
  - programmer
  - saving
summary: Saves on a timer and at checkpoints, never more often than its cooldown allows.
---
# Autosave Service

🗞️ **Component** · programmers only

**Tags:** #programmer #saving

Saves the game on a timer and whenever gameplay reports a checkpoint, but never more often than the cooldown allows. The **interval** decides how often a save is offered, the **cooldown** how often one is allowed: a timer alone cannot stop five checkpoints from writing five saves in five seconds, and a cooldown alone never saves during a quiet stretch.

**Add it:** Inspector › **Add Component** › search `Autosave Service`.

## How to use it

1. Put it in the scene next to the **Save Manager**.
2. Create an **Autosave Config** asset and assign it.
3. From gameplay code, report checkpoints. The service decides whether to save.
4. Optional: let the player change it with the autosave setting components.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Autosave Config |  | **Required.** The Autosave Config asset with the defaults. Share it with the autosave setting components. |

## Related

- [🗞️ Save Manager](Save%20Manager.md)
- [🔖 Autosave Enabled Setting](Autosave%20Enabled%20Setting.md)
- [🔖 Autosave Interval Setting](Autosave%20Interval%20Setting.md)
- [🔖 Autosave Cooldown Setting](Autosave%20Cooldown%20Setting.md)