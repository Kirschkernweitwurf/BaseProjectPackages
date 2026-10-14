---
type: component
package: Core
audience: everyone
tags:
  - designer
  - menus
summary: Shows, hides or locks the mouse cursor while the menu is open.
---
# Menu Cursor Module

🔖 **Component** · for everyone

**Tags:** #designer #menus

Shows, hides or locks the mouse cursor while its menu is open, and gives the cursor back on close. If several menus want different cursors, the one with the highest priority wins.

**Add it:** Inspector › **Add Component** › search `Menu Cursor Module`.

## How to use it

1. Put it on the **same GameObject** as the Menu. It finds the Menu itself.
2. Set **Is Cursor Visible** and **Lock Mode**. A pause menu usually wants visible and `None`.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Owner Menu** | [Menu](Menu.md) |  | Filled in automatically. The menu this module belongs to. Filled in from the same GameObject when empty. |
| **Cursor Settings** | Cursor Request |  | The cursor state while the menu is open. |
| Cursor Settings › **Is Cursor Visible** | Toggle | Off | Whether the cursor is visible. |
| Cursor Settings › **Lock Mode** | Cursor Lock Mode |  | `None`, `Locked` (centered and hidden) or `Confined` (kept inside the window). |

## Related

- [🔖 Menu](Menu.md)
- [🗞️ Cursor Manager](Cursor%20Manager.md)
- [📖 Menus guide](Menus.md)