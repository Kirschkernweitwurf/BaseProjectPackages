---
type: component
package: Core
audience: programmer
tags:
  - programmer
  - menus
summary: Decides how the mouse cursor looks, by priority.
---
# Cursor Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #menus

Decides whether the cursor is visible and locked. Menus and gameplay request a cursor state with a priority; the highest request wins, and the default below applies when nobody asks.

**Add it:** Inspector › **Add Component** › search `Cursor Manager`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.
2. Set the **Default Cursor Settings** for gameplay, for example hidden and locked for a shooter.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Default Cursor Settings** | Cursor Request |  | The cursor state while no menu or system asks for something else. |
| Default Cursor Settings › **Is Cursor Visible** | Toggle | Off | Whether the cursor is visible. |
| Default Cursor Settings › **Lock Mode** | Cursor Lock Mode |  | `None`, `Locked` or `Confined`. |

## Related

- [🔖 Menu Cursor Module](Menu%20Cursor%20Module.md)