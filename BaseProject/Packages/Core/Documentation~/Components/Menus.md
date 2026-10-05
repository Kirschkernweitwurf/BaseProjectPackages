# Menus

🔖 **Component** · for everyone

**Tags:** #designer #menus

Every screen in the game (pause, settings, inventory, loading) is a **Menu**. The Menu handles opening, closing, and the animation between those states. Anything else it needs (pausing time, showing the cursor, swapping the controls) is a separate **module** you bolt on.

## Menu

The base component. Put it on the root of your screen.

### Fields

| Field | What it does |
| --- | --- |
| **Menu Identifier** | The Menu Identifier asset that names this menu. Required. |
| **Content Root** | The Tween Group that plays the open and close animation. Required. |
| **Priority** | `Low`, `Medium`, `High`, or `Critical`. Decides which menu wins when several are open. |
| **Open On Start** | Open this menu automatically when the scene starts |
| **Listen To On Back Action** | Let this menu close itself when the player presses Back or Escape |
| **Blocking Menus** | Menus that prevent this one from opening while they are open |

### How to set one up

1. Create a Menu Identifier asset for the screen, for example `MID_Settings`.
2. On your screen's root object, add a **Canvas Group**, a fade tween, and a **Tween Group**. Put the fade tween in the group's show list.
3. Add the **Menu** component. Drop in the identifier and the Tween Group.
4. Pick a **Priority**.
5. Add whichever modules you need (below).

### Good to know

- Opening and closing are **always animated**. If the Content Root has no tweens, the menu still works, it just snaps.
- A menu can open another menu as a child. Closing the parent closes its children automatically.
- **Priority** matters most for Back input. The open menu with the highest priority is the one that receives the Back press. Use `Critical` sparingly, for things like a confirm-quit dialog.

---

## Menu Modules

Each module is one job. Add only what the menu needs. All of them must sit on the **same GameObject** as the Menu, and they find the Menu themselves.

### Menu Cursor Module

Shows, hides, or locks the mouse cursor while the menu is open, and puts it back on close.

| Field | What it does |
| --- | --- |
| **Cursor Settings > Is Cursor Visible** | Whether the cursor is visible |
| **Cursor Settings > Lock Mode** | `None`, `Locked`, or `Confined` |

Typical use: a pause menu sets visible and `None`, so the player gets their cursor back.

### Menu Time Scale Module

Changes the game speed while the menu is open, and restores it on close.

| Field | What it does |
| --- | --- |
| **Time Scale** | The time scale applied while open. `0` freezes the game, `1` is normal. |

Typical use: a pause menu sets `0`. An inventory that should slow the game rather than stop it might set `0.2`.

### Menu Input Map Module

Switches the active input action map while the menu is open, and switches back on close. This is how the player's controls change when a menu is up.

| Field | What it does |
| --- | --- |
| **Action Map** | The action map activated while the menu is open |

### Menu Reset Module

Resets the menu's contents when it closes, so it opens fresh next time instead of showing whatever state you left it in.

No fields. Add it and it works.

It resets every child that supports resetting, which mainly means nested Tween Groups. The menu's own Content Root is skipped, since the menu drives that animation itself.

Typical use: any menu with sub-panels, tabs, or expanded sections that should not be remembered.

### Picking modules

| Menu | Usual modules |
| --- | --- |
| Pause menu | Cursor, Time Scale, Input Map, Reset |
| Settings screen | Cursor, Input Map, Reset |
| HUD popup | Reset, or none at all |
| Loading screen | none |

---

## Pause Menu

A Menu that also tracks whether the game is paused, so other systems can react to it.

It has the same fields as Menu. Pair it with a **Menu Time Scale Module** set to `0`, since the Pause Menu itself does not stop time.

## Loading Screen

A Menu that shows itself automatically during scene loads. It listens for load events, so nothing needs to open or close it.

### Fields

| Field | What it does |
| --- | --- |
| **Progress Image** | An Image whose fill amount shows load progress. Optional. |
| **Spinner** | A RectTransform that rotates while loading. Optional. |
| **Spinner Rotation Speed** | Spin speed in degrees per second |
| **Fill Smooth Speed** | How smoothly the progress bar catches up. Lower is smoother and laggier. |
| **Has Minimum Show Time** | Keep the screen up for a minimum time even if loading finishes early |
| **Minimum Show Time** | That minimum, in seconds. Shown when the toggle above is on. |
| **Scenes To Show For** | Only show for these scenes. Leave empty to show for all of them. |

### Tips

- **Minimum Show Time** stops the screen from flashing on and off for fast loads. One second is a reasonable default.
- The **Progress Image** needs its Image Type set to **Filled**, otherwise the fill amount does nothing.
- The spinner uses unscaled time, so it keeps moving even if something froze the game speed.

---

## Menu Manager

The service that keeps track of every menu and routes the Back button. It lives on a manager prefab, not in your scene.

| Field | What it does |
| --- | --- |
| **Default Back Menu** | The menu opened when Back is pressed and nothing else is listening. Usually the pause menu. Leave empty to disable this. |
