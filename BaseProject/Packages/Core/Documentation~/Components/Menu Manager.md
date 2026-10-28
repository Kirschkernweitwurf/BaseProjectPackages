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
- Back goes to the open menu with the highest priority that listens to it; between equal priorities the one opened last wins. It is picked again on every open and close.
- Something that uses Back for a step of its own, like putting down a card held with a gamepad, calls `ClaimBack(owner)`. While any claim is open, Back neither closes a menu nor opens the default back menu. `ReleaseBack(owner)` hands it back.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Pause Menu](Pause%20Menu.md)
- [📖 Menus guide](Menus.md)