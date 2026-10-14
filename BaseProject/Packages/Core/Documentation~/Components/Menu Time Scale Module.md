---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
summary: Changes the game speed while the menu is open.
---
# Menu Time Scale Module

🔖 **Component** · for everyone

**Tags:** #designer #menus

Changes the game speed while its menu is open and restores it on close. With several menus open, the one with the highest priority decides.

**Add it:** Inspector › **Add Component** › search `Menu Time Scale Module`.

## How to use it

1. Put it on the **same GameObject** as the Menu. It finds the Menu itself.
2. Set **Time Scale**: `0` freezes the game, `1` is normal, `0.2` is slow motion.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Owner Menu** | [Menu](Menu.md) |  | Filled in automatically. The menu this module belongs to. Filled in from the same GameObject when empty. |
| **Time Scale** | Number | 0 | The game speed while the menu is open. `0` freezes the game. |

## Good to know

- UI tweens and the loading spinner use unscaled time, so they keep playing at Time Scale 0.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Pause Menu](Pause%20Menu.md)
- [🗞️ Time Scale Manager](Time%20Scale%20Manager.md)
- [📖 Menus guide](Menus.md)