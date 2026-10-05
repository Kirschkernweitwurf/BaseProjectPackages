# Components

The runtime pieces of the Controller Support package, grouped by the job they do.

## Navigation

Decides what a stick can reach and how it moves between elements.

| Component | What it does |
| --- | --- |
| [🔖 NavigableElement](navigable-element.md) | Marks one selectable as a real gamepad target |
| [🔖 NavigableGroup](navigable-group.md) | Collects the elements below it, wires four-way navigation, owns a focus context |

## Focus

Makes sure the gamepad always has something selected.

| Component | What it does |
| --- | --- |
| 🔖[FocusWatchdog](focus-watchdog.md) | Global service that restores focus to the highest priority active group |

## Integration

The one deliberate seam between menus and navigation.

| Component | What it does |
| --- | --- |
| [🔖 MenuNavigationModule](menu-navigation-module.md) | Activates a group while its menu is open |

## Scrolling

Fixes what uGUI does not handle for gamepads.

| Component | What it does |
| --- | --- |
| [🔖 ScrollIntoView](scroll-into-view.md) | Keeps the selected element visible inside a ScrollRect |
| [🔖 GamepadScrollRect](gamepad-scroll-rect.md) | Scrolls a ScrollRect directly with a stick |

## Not covered here

Input prompt components (`InputDeviceTracker`, `InputGlyphSet`, `InputGlyphProvider`) will be documented in the future, when developed further.

## Helpers behind the scenes

These are static classes, not components, so they have no page of their own:

- `NavigationBuilder` computes the explicit up/down/left/right wiring. See [NavigableGroup](navigable-group.md).
- `NavigationValidator` adds missing `NavigableElement`s during an editor rebuild.
- `NavigationRebuildService` shared rebuild entry points for the inspector and the window.
