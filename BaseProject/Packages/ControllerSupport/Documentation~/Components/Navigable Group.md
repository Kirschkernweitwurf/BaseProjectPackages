# Navigable Group

🔖 **Component** · for everyone

**Namespace:** `Base.ControllerSupport.Controller.Navigation`

## What it does

A self-contained navigation context. Put it on a menu root. It collects every `NavigableElement` below it, wires explicit four-way navigation between them, and provides a default focus target.

While active, it registers with the `FocusWatchdog`, so focus can be restored to it when the gamepad loses its selection.

The group knows nothing about menus or the game. It is pure UI navigation.

## Settings

### Focus

| Field | Description |
| --- | --- |
| Default Element | Selected when the group gains focus and nothing is remembered. Required. |
| Priority | Higher priority wins focus restoration when several groups are active. |
| Auto Activate | Group activates itself in `OnEnable`. Turn this **off** when a menu drives it. |
| Remember Last Selected | Focus returns to the element used last instead of the default. |

### Wiring

| Field | Description |
| --- | --- |
| Wrap | Navigation loops around the edges of the group. So navigating down on the lowest selectable moves the user to the highest selectable. |

## How wiring works

`Rebuild()` writes explicit navigation onto each selectable, based on their on-screen positions:

- For each direction, the nearest neighbor inside a 45-degree cone is picked.
- Elements outside that cone are ignored, so off-axis buttons never get wired by accident.
- With **Wrap** on, an element at the edge links to the element furthest in the opposite direction.

Rebuilds only run when you ask for them, so wiring never changes silently. In the editor, a rebuild also adds a missing `NavigableElement` to any selectable underneath, then wires it in the same pass.

## Focus behavior

- `Activate()` registers the group with the `FocusWatchdog`.
- `Deactivate()` removes it. Called automatically in `OnDisable`.
- `RestoreFocus()` selects the remembered element if it is still valid and interactable, otherwise the default element. If neither works, a warning is logged once per activation.

## API

| Member | Description |
| --- | --- |
| `Priority` | Focus priority the watchdog compares. |
| `AutoActivate` | Whether the group activates itself. |
| `Activate()` | Register with the watchdog. |
| `Deactivate()` | Remove from the watchdog. |
| `RestoreFocus()` | Select the remembered or default element. |
| `Contains(GameObject)` | True when the object lives inside this group. |
| `Rebuild()` | Recollect elements and rewire navigation. Also on the context menu as "Rebuild Navigation". |

## Rebuilding

- **Inspector:** Rebuild and Rebuild Scene buttons.
- **Context menu:** Rebuild Navigation.
- **Window:** `Tools > Base Packages > Unity Editor > Controller Navigation Groups`.
- **Code:** `group.Rebuild()`.

## Common mistakes

- Leaving **Auto Activate** on while a `MenuNavigationModule` drives the group. Both would fight over activation. The Navigation Groups window flags this and can fix it.
- A priority that differs from the owning menu's priority. Also flagged in the window.
- Forgetting to rebuild after changing the layout. Navigation stays wired to the old positions.
