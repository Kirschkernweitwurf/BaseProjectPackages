---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
  - ui
summary: A Menu that also tracks whether the game is paused.
---
# Pause Menu

🔖 **Component** · for everyone

**Tags:** #designer #menus #ui

A **Menu** that also tracks whether the game is paused, so other systems (and the Pause Menu Button icon) can react. It has the same fields as Menu. If it goes away while open, for example because its scene unloads, it still ends the pause.

**Add it:** Inspector › **Add Component** › search `Pause Menu`.

## How to use it

1. Set it up like any [Menu](Menu.md).
2. Add a **Menu Time Scale Module** with Time Scale `0`. The Pause Menu itself does not stop time.
3. Add a **Menu Cursor Module** so the player gets the cursor back.
4. Set it as **Default Back Menu** on the Menu Manager, so Back opens it.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The Menu Identifier asset that names this menu. Buttons and code open the menu through it. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The Tween Group that plays the open and close animation. |
| **Priority** | Priority |  | `Low`, `Medium`, `High` or `Critical`. The open menu with the highest priority gets the Back press. |
| **Open On Start** | Toggle | Off | Opens the menu, with animation, when the scene starts. |
| **Listen To On Back Action** | Toggle | On | Lets the menu close itself when the player presses Back (Escape, gamepad B). |
| **Blocking Menus** | List of [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | While one of these menus is open, this menu cannot open. |

## Related

- [🔖 Menu](Menu.md)
- [🔖 Menu Time Scale Module](Menu%20Time%20Scale%20Module.md)
- [🔖 Pause Menu Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Pause%20Menu%20Button.md)
- [🗞️ Menu Manager](Menu%20Manager.md)
- [📖 Menus guide](Menus.md)