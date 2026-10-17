---
type: Guide
package: Core
tags:
  - designer
  - menus
summary: How a screen is built from a Menu, a Tween Group and menu modules, and which modules a screen needs.
---
# Menus

📖 **Guide** · for everyone

**Tags:** #designer #menus

Every screen in the game (pause, settings, inventory, loading) is a **Menu**. The Menu opens and closes the screen and plays its animation. Anything else the screen needs is a separate **module** on the same GameObject. Every component has its own page with all fields.

## Build a screen in five steps

1. Create a [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) asset, for example `MID_Settings`.
2. On the screen's root, add a **Canvas Group**, a Fade To Tween and a **Tween Group** with the fade in its Show list.
3. Add [🔖 Menu](Menu.md). Drop in the identifier and the Tween Group (as **Content Root**).
4. Pick a **Priority**.
5. Add the modules the screen needs (below), then open it with an **Open Menu Button**.

## The menus

| Component | Use it for |
| --- | --- |
| [🔖 Menu](Menu.md) | Any screen |
| [🔖 Pause Menu](Pause%20Menu.md) | The pause screen. Also tracks whether the game is paused. |
| [🔖 Loading Screen](Loading%20Screen.md) | The screen shown while scenes load. Opens and closes itself. |

## The modules

Each module does one job. Put them on the **same GameObject** as the Menu; they find it themselves.

| Module | While the menu is open, it |
| --- | --- |
| [🔖 Menu Cursor Module](Menu%20Cursor%20Module.md) | shows, hides or locks the mouse cursor |
| [🔖 Menu Time Scale Module](Menu%20Time%20Scale%20Module.md) | slows down or freezes the game |
| [🔖 Menu Input Map Module](Menu%20Input%20Map%20Module.md) | switches the controls, for example to the UI map |
| [🔖 Menu Reset Module](Menu%20Reset%20Module.md) | (on close) resets the contents, so it opens fresh |
| [🔖 Menu Group Module](Menu%20Group%20Module.md) | shows other menus inside it as tabs, one at a time |

If several open menus want different things, the menu with the highest **Priority** wins.

### Which modules a screen needs

| Screen | Usual modules |
| --- | --- |
| Pause menu | Cursor, Time Scale (`0`), Input Map, Reset |
| Settings screen | Cursor, Input Map, Reset, Group (for tabs) |
| HUD popup | Reset, or none |
| Loading screen | none |

## Opening and closing

| From | Use |
| --- | --- |
| A button | Open Menu Button, Close Menu Button, Pause Menu Button (UI package) |
| The Back key | Turn on **Listen To On Back Action** on the Menu. The [🗞️ Menu Manager](Menu%20Manager.md) sends Back to the open menu with the highest priority. |
| Code | The Menu Manager, by identifier |

## Good to know

- Opening and closing are always animated. Without tweens in the Content Root the menu just snaps.
- A menu can open another menu as a child. Closing the parent closes its children.
- For tabs (phone apps, settings pages) use a [🔖 Menu Group Module](Menu%20Group%20Module.md) instead of opening the menus by hand.
- Use `Critical` priority sparingly, for example for a quit confirmation.
- Gamepad navigation inside a menu comes from the Controller Support package: add a Menu Navigation Module.