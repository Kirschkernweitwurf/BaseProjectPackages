---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
  - ui
summary: Turns a UI root into a screen that opens and closes with an animation.
---
# Menu

🔖 **Component** · for everyone

**Tags:** #designer #menus #ui

Every screen in the game (pause, settings, inventory, a popup) is a **Menu**. The Menu opens and closes the screen and plays its animation. Anything else the screen needs, like pausing the game or showing the cursor, is a separate **module** on the same GameObject.

**Add it:** Inspector › **Add Component** › search `Menu`.

## How to use it

1. Create a **Menu Identifier** asset for the screen, for example `MID_Settings`.
2. On the screen's root, add a **Canvas Group**, a **Fade To Tween** and a **Tween Group** with the fade in its Show list.
3. Add **Menu** and drop in the identifier and the Tween Group.
4. Pick a **Priority**.
5. Add the modules the screen needs: cursor, time scale, input map, reset.
6. Open it with an **Open Menu Button**, or from code through the Menu Manager.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The Menu Identifier asset that names this menu. Buttons and code open the menu through it. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The Tween Group that plays the open and close animation. |
| **Priority** | Priority |  | `Low`, `Medium`, `High` or `Critical`. The open menu with the highest priority gets the Back press. |
| **Open On Start** | Toggle | Off | Opens the menu, with animation, when the scene starts. |
| **Listen To On Back Action** | Toggle | On | Lets the menu close itself when the player presses Back (Escape, gamepad B). |
| **Blocking Menus** | List of [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | While one of these menus is open, this menu cannot open. |

## Good to know

- Opening and closing are always animated. Without tweens in the Content Root the menu simply snaps.
- A menu can open another menu as a child. Closing the parent closes its children.
- Use `Critical` priority sparingly, for example for a quit confirmation.
- The menu needs a **Menu Manager** in the scene to register with.

## Related

- [🔖 Menu Cursor Module](Menu%20Cursor%20Module.md)
- [🔖 Menu Time Scale Module](Menu%20Time%20Scale%20Module.md)
- [🔖 Menu Input Map Module](Menu%20Input%20Map%20Module.md)
- [🔖 Menu Reset Module](Menu%20Reset%20Module.md)
- [🔖 Menu Group Module](Menu%20Group%20Module.md)
- [🔖 Open Menu Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Open%20Menu%20Button.md)
- [🔖 Close Menu Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Close%20Menu%20Button.md)
- [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md)
- [📖 Menus guide](Menus.md)