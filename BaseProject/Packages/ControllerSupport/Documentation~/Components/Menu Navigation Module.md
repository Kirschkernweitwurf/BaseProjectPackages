# Menu Navigation Module

🔖 **Component** · for everyone

**Namespace:** `Base.ControllerSupport.Controller.Integration`

**Base class:** `MenuModule`

## What it does

Connects a menu's lifecycle to a `NavigableGroup`:

- Menu opens, group activates.
- Menu closes, group deactivates.

This is the single deliberate seam between the menu layer and the controller package. Everything else in the navigation code stays menu agnostic.

## Settings

| Field | Description |
| --- | --- |
| Navigable Group | The group activated while the owning menu is open. Required, found on a child. |

## Setup

1. Add the module to a menu.
2. Assign the `NavigableGroup` (usually on the menu root or a child of it).
3. Turn **Auto Activate** off on that group.
4. Give the group the same **Priority** as the menu.

If Auto Activate is still on, a warning is logged in `Awake`. The Navigation Groups window flags the same problem and can fix it with one click.

## Why not let the group activate itself?

With a menu present, two systems would decide when the group is live. The menu already knows exactly when it is open and closed, so it should be the only one activating.
