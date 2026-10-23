---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
summary: "Resets the menu's contents when it closes, so it opens fresh."
---
# Menu Reset Module

🔖 **Component** · for everyone

**Tags:** #designer #menus

Resets the menu's contents when it closes, so it opens fresh next time instead of showing the state it was left in. Every child that supports resetting is reset, mainly nested Tween Groups.

**Add it:** Inspector › **Add Component** › search `Menu Reset Module`.

## How to use it

1. Put it on the **same GameObject** as the Menu. It finds the Menu itself.
2. That is all. It has no settings.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Owner Menu** | [Menu](Menu.md) |  | Filled in automatically. The menu this module belongs to. Filled in from the same GameObject when empty. |

## Good to know

- The Content Roots of this menu and of every menu nested in it, like tabs, are skipped, because each menu plays that animation itself. Resetting a tab's Content Root while the tab closes would stop its close, and the tab would stay open.
- Good for menus with tabs, sub-panels or expanded sections.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md)
- [📖 Menus guide](Menus.md)