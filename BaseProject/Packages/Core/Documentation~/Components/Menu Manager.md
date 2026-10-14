---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
  - menus
summary: Keeps track of every menu and routes the Back button.
---
# Menu Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #menus

The service that keeps track of every menu, opens and closes them by identifier and routes the Back button to the right menu.

**Add it:** Inspector › **Add Component** › search `Menu Manager`.

## How to use it

1. Put it on the scene manager prefab, once.
2. Set **Default Back Menu**, usually the pause menu.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Default Back Menu** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | Opened when Back is pressed and no open menu takes it. Usually the pause menu. Empty turns this off. |

## Good to know

- Every Menu registers with it on start. Without it, menus cannot open.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Pause Menu](Pause%20Menu.md)
- [📖 Menus guide](Menus.md)