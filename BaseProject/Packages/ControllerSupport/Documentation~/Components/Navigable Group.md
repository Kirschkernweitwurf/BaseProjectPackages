---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - gamepad
  - ui
summary: A self-contained navigation context.
---
# Navigable Group

🔖 **Component** · for everyone

**Tags:** #designer #gamepad #ui

**Namespace:** `Base.ControllerSupport.Controller.Navigation`

## What it does

A self-contained navigation context. Put it on a menu root. It collects every `NavigableElement` below it, wires explicit four-way navigation between them, and provides a default focus target.

While active, it registers with the `FocusWatchdog`, so focus can be restored to it when the gamepad loses its selection.

The group knows nothing about menus or the game. It is pure UI navigation.

## Settings

### Focus

| Field | Description |
| --- | --- |
| Default Element | Selected when the group gains focus and nothing is remembered. Optional: when empty or not navigable right now (for example behind a lock screen), the first navigable element below the group gets focus. |
| Priority | Higher priority wins focus restoration when several groups are active. |
| Auto Activate | Group activates itself in `OnEnable`. Turn this **off** when a menu drives it. |
| Remember Last Selected | Focus returns to the element used last instead of the default. |

### Wiring

| Field | Description |
| --- | --- |
| Wrap | Navigation loops around the edges of the group. So navigating down on the lowest selectable moves the user to the highest selectable. |
| Reach Off Axis | A direction with nothing lined up (within 45 degrees) takes the nearest element on that side instead of staying unwired. For loose layouts like a row of cards above a single button, where Down on an outer card would otherwise lead nowhere. |
| Rebuild On Activate | Rewires the navigation each time the group activates, after the layout ran, and whenever one of its elements starts or stops taking the focus, like a card that turns clickable once its intro landed. For groups whose elements are spawned or unlocked at runtime, like a row of cards. |

## How wiring works

`Rebuild()` writes explicit navigation onto each selectable, based on their on-screen positions (elements on a world space canvas or on a 3D object are projected through their camera first):

- For each direction, the nearest neighbor inside a 45-degree cone is picked.
- Elements outside that cone are ignored, so off-axis buttons never get wired by accident.
- With **Wrap** on, an element at the edge links to the element furthest in the opposite direction.

Rebuilds only run when you ask for them or turn on **Rebuild On Activate**, so authored wiring never changes silently. The wiring only links elements that take the focus at that moment, which is why a runtime group also rewires when one of them turns navigable later. In the editor, a rebuild also adds a missing `NavigableElement` to any selectable underneath, then wires it in the same pass.

## Focus behavior

- `Activate()` registers the group with the `FocusWatchdog`.
- `Deactivate()` removes it. Called automatically in `OnDisable`.
- `RestoreFocus()` selects the remembered element if it is still valid and interactable, otherwise the default element. If neither works, a warning is logged once per activation.
- `IsFocusHeld` keeps the focus empty while the group stays the focus owner, for a screen that waits for the player's first input. A selection outside the group is cleared meanwhile.

## API

| Member | Description |
| --- | --- |
| `Priority` | Focus priority the watchdog compares. |
| `AutoActivate` | Whether the group activates itself. |
| `Activate()` | Register with the watchdog. |
| `Deactivate()` | Remove from the watchdog. |
| `RestoreFocus()` | Select the remembered or default element. Only clears a selection outside the group while the focus is held. |
| `IsFocusHeld` | Keeps the focus empty without giving it to a group behind. |
| `Contains(GameObject)` | True when the object lives inside this group. |
| `Rebuild()` | Recollect elements and rewire navigation. Also on the context menu as "Rebuild Navigation". |

## Rebuilding

- **Inspector:** Rebuild and Rebuild Scene buttons.
- **Context menu:** Rebuild Navigation.
- **Window:** `Tools > Base Packages > Editor > Controller Navigation Groups`.
- **Code:** `group.Rebuild()`.

## Common mistakes

- Leaving **Auto Activate** on while a `MenuNavigationModule` drives the group. Both would fight over activation. The Navigation Groups window flags this and can fix it.
- A priority that differs from the owning menu's priority. Also flagged in the window.
- Forgetting to rebuild after changing the layout. Navigation stays wired to the old positions.
- Calling `Deactivate()` then `Activate()` to rewire. It makes the group the newest one, so it can take the focus from a screen opened after it. A group with **Rebuild On Activate** rewires on its own; otherwise call `Rebuild()`.