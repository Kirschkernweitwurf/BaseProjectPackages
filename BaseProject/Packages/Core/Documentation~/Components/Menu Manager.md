---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
  - menus
summary: Keeps the one stack of open menus that decides what is on top, and routes the Back button.
---
# Menu Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #menus

The service that keeps track of every menu, opens and closes them by identifier, decides which menu is on top and routes the Back button to it. Back, the gamepad and keyboard focus and every input shortcut follow this one answer, so nothing behind the top screen reacts to input.

**Add it:** Inspector › **Add Component** › search `Menu Manager`.

## How to use it

1. Put it on the scene manager prefab, once.
2. Set **Default Back Menu**, usually the pause menu.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Default Back Menu** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | Opened when Back is pressed and nothing on the top screen takes it, like the pause menu. It does not open below a menu of higher priority, where it could not be seen. Leave empty to disable this fallback. |

## Good to know

- Every Menu registers with it on start. Without it, menus cannot open.
- **On top** (`TopMenu`): the open menu with the highest priority; between equal priorities the one opened last. A menu stops counting the moment its close starts, so focus and Back move on while it fades out. `TopMenuChanged` fires on every change.
- **Screens:** a tab opened by a [Menu Group Module](Menu%20Group%20Module.md) is *embedded* in the menu that holds it, so the two count as one screen. `IsOnTop(component)` tells whether a component's screen is on top: the nearest menu above it, or the scene while no menu is open. Use it for every input shortcut instead of checking menus yourself. `IsScreenOnTop(menu)` is the same for code that already knows its menu.
- **Back goes to exactly one receiver**, the first of:
  1. the newest claim on the top screen,
  2. the innermost menu of the top screen that listens to Back,
  3. the Default Back Menu.

  A top screen that does not listen keeps Back from every menu behind it.
- **Claims:** something that uses Back for a step of its own, like putting down a held card or leaving a wiki page, calls `ClaimBack(owner, onBack)`. The claim only acts while the owner's screen is on top, so a menu opened over it takes Back first. `ReleaseBack(owner)` hands it back. Don't listen to the cancel action yourself: two listeners on one press is how Back closes two things at once.
- `CanShowOnTop(menu)` tells whether a menu would come on top if it opened now. Shortcuts that open a menu on their own, like Pause, ask it, so the menu never opens hidden behind a full screen.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Pause Menu](Pause%20Menu.md)
- [📖 Menus guide](Menus.md)