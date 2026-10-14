---
type: Component
package: Core Debug
audience: Programmers
tags:
  - programmer
  - debugging
summary: Shows the Unity console inside the game, also in builds.
---
# Log Console View

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

Shows Unity's console log inside the game, so you can read errors in a build. Logging starts before the first scene loads, so nothing is missed while the menu is closed. Warnings and errors get a colored timestamp.

**Add it:** Inspector › **Add Component** › search `Log Console View`.

## How to use it

1. Use it inside the debug menu prefab.
2. Assign the log text, its scroll view and a clear button.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The unique identifier asset for this menu. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The root TweenGroup for this menu's open/close animations. |
| **Priority** | Priority |  | The priority of this menu in the stack. |
| **Open On Start** | Toggle | Off | If true, this menu will automatically open on Start (with animation). |
| **Listen To On Back Action** | Toggle | On | If true, this menu will listen to the OnBack action to close itself. |
| **Blocking Menus** | List of [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | Menus that block this menu from opening if they are currently open. |

### Log

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Log Text** | Text (TMP) |  | **Required.** Text that shows the log. |
| **Scroll Rect** | Scroll Rect |  | **Required.** Scroll view around the log. |
| **Clear Button** | Button |  | **Required.** Button that empties the log. |

## Related

- [🗞️ Debug Menu Controller](Debug%20Menu%20Controller.md)