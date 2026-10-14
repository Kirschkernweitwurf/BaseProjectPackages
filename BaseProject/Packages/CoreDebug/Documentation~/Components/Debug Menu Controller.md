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

The debug menu. **F1** opens and closes it. It holds the cheat console and the log console and switches between them with two buttons; the console used last opens again next time.

**Add it:** Inspector › **Add Component** › search `Debug Menu Controller`.

## How to use it

1. Use the debug menu prefab from the Core Debug package, or build one with this component on the root.
2. Assign both console menus, their buttons and their Tween Groups.

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
| **Cheat Console Tween Group** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** Tween Group that highlights the cheat console button. |
| **Log Console Button** | Button |  | **Required.** Button that shows the log console. |
| **Log Console Tween Group** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** Tween Group that highlights the log console button. |
| **Cheat Console** | [Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md) |  | **Required.** The cheat console menu. |
| **Log Console** | [Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md) |  | **Required.** The log console menu. |

## Related

- [🗞️ Cheat Console View](Cheat%20Console%20View.md)
- [🗞️ Log Console View](Log%20Console%20View.md)
- [🔖 Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md)