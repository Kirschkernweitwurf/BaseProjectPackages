---
type: Component
package: Core Debug
audience: Programmers
tags:
  - programmer
  - debugging
summary: The debug menu that holds the cheat console and the log console. Toggle with F1.
---
# Debug Menu Controller

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

The debug menu. **F1** opens and closes it. It holds the cheat console and the log console and switches between them with two tabs that work like a segmented control: the open console's tab is filled and bold. The console used last opens again next time. The consoles open embedded in the debug menu, so the menu manager treats them as one screen: Back closes the whole debug menu, and nothing behind it reacts to input while it is open. Give the debug menu and its consoles **Critical** priority, so they sit on top of every menu.

**Add it:** Inspector › **Add Component** › search `Debug Menu Controller`.

## How to use it

1. Use the debug menu prefab from the Core Debug package, or build one with this component on the root.
2. Assign both console menus and their tab buttons. Give each tab a Button with transition **None** and an Image as its target graphic; the tab colors below fill it.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The unique identifier asset for this menu. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The root TweenGroup for this menu's open/close animations. |
| **Priority** | Priority |  | The priority of this menu in the stack. |
| **Open On Start** | Toggle | Off | If true, this menu will automatically open on Start (with animation). |
| **Listen To On Back Action** | Toggle | On | If true, this menu will listen to the OnBack action to close itself. |
| **Blocking Menus** | List of [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | Menus that block this menu from opening if they are currently open. |

### Consoles

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Cheat Console Button** | Button |  | **Required.** Button that shows the cheat console. |
| **Log Console Button** | Button |  | **Required.** Button that shows the log console. |
| **Cheat Console** | [Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md) |  | **Required.** The cheat console menu. |
| **Log Console** | [Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md) |  | **Required.** The log console menu. |

### Tabs

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Open Tab Color** | Color | White, 18 % | Fill of the open console's tab. |
| **Closed Tab Color** | Color | Clear | Fill of the other tab. |
| **Open Label Color** | Color | White | Label of the open console's tab. |
| **Closed Label Color** | Color | Grey | Label of the other tab. |

## Related

- [🗞️ Cheat Console View](Cheat%20Console%20View.md)
- [🗞️ Log Console View](Log%20Console%20View.md)
- [🔖 Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md)