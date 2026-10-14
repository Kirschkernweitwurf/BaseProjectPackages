---
type: Component
package: Core Debug
audience: Programmers
tags:
  - programmer
  - debugging
summary: "The cheat console window: input field, log and suggestions."
---
# Cheat Console View

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

The visible part of the cheat console: the input field, the output log and the list of command suggestions. It is a Menu, opened by the debug menu.

**Add it:** Inspector › **Add Component** › search `Cheat Console View`.

## How to use it

1. Put it on the cheat console panel together with a **Cheat Console Controller**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The unique identifier asset for this menu. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The root TweenGroup for this menu's open/close animations. |
| **Priority** | Priority |  | The priority of this menu in the stack. |
| **Open On Start** | Toggle | Off | If true, this menu will automatically open on Start (with animation). |
| **Listen To On Back Action** | Toggle | On | If true, this menu will listen to the OnBack action to close itself. |
| **Blocking Menus** | List of [Menu Identifier](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Scriptable%20Objects/Menu%20Identifier.md) |  | Menus that block this menu from opening if they are currently open. |
| **Input Field** | TMP_Input Field |  | **Required.** Where commands are typed. |

### Log

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Log Text** | Text (TMP) |  | **Required.** Text that shows command output. |
| **Scroll Rect** | Scroll Rect |  | **Required.** Scroll view around the log. |

### Suggestions

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Suggestion Prefab** | Text (TMP) |  | **Required.** Text prefab for one suggestion line. |
| **Suggestion Parent** | Transform |  | **Required.** Where suggestion lines are spawned. |
| **Max Suggestions** | Whole number | 5 | Most suggestions shown at once. |

### Colors

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Info Color** | Color | White | Color of normal output. |
| **Warning Color** | Color | (1, 0.6, 0.2) | Color of warnings. |
| **Error Color** | Color | Red | Color of errors. |
| **Command Color** | Color | (0.5, 1, 0.5) | Color of the commands you typed. |

## Related

- [🗞️ Cheat Console Controller](Cheat%20Console%20Controller.md)
- [🗞️ Debug Menu Controller](Debug%20Menu%20Controller.md)