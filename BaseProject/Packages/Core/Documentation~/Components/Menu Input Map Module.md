---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
  - input
summary: Switches the active controls while the menu is open.
---
# Menu Input Map Module

🔖 **Component** · for everyone

**Tags:** #designer #menus #input

Switches the active input action map while its menu is open and switches back on close. This is how the player stops walking around while a menu is up.

**Add it:** Inspector › **Add Component** › search `Menu Input Map Module`.

## How to use it

1. Put it on the **same GameObject** as the Menu. It finds the Menu itself.
2. Pick the **Action Map** the menu needs, usually your UI map.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Owner Menu** | [Menu](Menu.md) |  | Filled in automatically. The menu this module belongs to. Filled in from the same GameObject when empty. |
| **Action Map** | Action map |  | The input action map that is active while the menu is open. |

## Good to know

- The Permanent map (Back, screenshots, cheat console) stays on whatever map is active.

## Related

- [🔖 Menu](Menu.md)
- [🗞️ Input Manager](Input%20Manager.md)
- [📖 Menus guide](Menus.md)