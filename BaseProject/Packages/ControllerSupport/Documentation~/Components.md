---
type: overview
package: Controller Support
tags:
  - gamepad
  - ui
summary: The runtime pieces of the Controller Support package, grouped by the job they do.
---
# Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #gamepad #ui

The runtime pieces of the Controller Support package, grouped by the job they do.

## Navigation

Decides what a stick can reach and how it moves between elements.

| Component | What it does |
| --- | --- |
| [🔖 NavigableElement](Components/Navigable%20Element.md) | Marks one selectable as a real gamepad target |
| [🔖 NavigableGroup](Components/Navigable%20Group.md) | Collects the elements below it, wires four-way navigation, owns a focus context |

## Focus

Makes sure the gamepad always has something selected.

| Component | What it does |
| --- | --- |
| 🔖[FocusWatchdog](Components/Focus%20Watchdog.md) | Global service that restores focus to the highest priority active group |

## Integration

The one deliberate seam between menus and navigation.

| Component | What it does |
| --- | --- |
| [🔖 MenuNavigationModule](Components/Menu%20Navigation%20Module.md) | Activates a group while its menu is open |

## Scrolling

Fixes what uGUI does not handle for gamepads.

| Component | What it does |
| --- | --- |
| [🔖 ScrollIntoView](Components/Scroll%20Into%20View.md) | Keeps the selected element visible inside a ScrollRect |
| [🔖 GamepadScrollRect](Components/Gamepad%20Scroll%20Rect.md) | Scrolls a ScrollRect directly with a stick |

## Not covered here

Input prompt components (`InputDeviceTracker`, `InputGlyphSet`, `InputGlyphProvider`) will be documented in the future, when developed further.

## Helpers behind the scenes

These are static classes, not components, so they have no page of their own:

- `NavigationBuilder` computes the explicit up/down/left/right wiring. See [NavigableGroup](Components/Navigable%20Group.md).
- `NavigationValidator` adds missing `NavigableElement`s during an editor rebuild.
- `NavigationRebuildService` shared rebuild entry points for the inspector and the window.

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Navigation

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Navigable Element](Components/Navigable%20Element.md) | Marks a button, slider, toggle or any other Selectable as a real gamepad target. | Everyone |
| [🔖 Navigable Group](Components/Navigable%20Group.md) | A self-contained navigation context. | Everyone |
| [🔖 Menu Navigation Module](Components/Menu%20Navigation%20Module.md) | Connects a menu's lifecycle to a NavigableGroup: | Everyone |

### Scrolling

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Gamepad Scroll Rect](Components/Gamepad%20Scroll%20Rect.md) | Lets a stick scroll a ScrollRect directly, usually the right stick. | Everyone |
| [🔖 Scroll Into View](Components/Scroll%20Into%20View.md) | Keeps the selected element inside the visible part of a ScrollRect. | Everyone |

### Rumble settings

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Rumble Enabled Setting](Components/Rumble%20Enabled%20Setting.md) | Stores whether gamepad rumble is on and passes it to the Rumble Service.. | Everyone |
| [🔖 Rumble Intensity Setting](Components/Rumble%20Intensity%20Setting.md) | Stores how strong gamepad rumble is and passes it to the Rumble Service, where it scales every rumble.. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Focus Watchdog](Components/Focus%20Watchdog.md) | Global safety net that keeps a valid selection while a gamepad is in use. | Programmers |
| [🗞️ Input Device Tracker](Components/Input%20Device%20Tracker.md) | Knows whether the player is using mouse and keyboard or a gamepad right now. | Programmers |
| [🗞️ Input Glyph Provider](Components/Input%20Glyph%20Provider.md) | Picks the right button icon (keyboard, Xbox, PlayStation, ...) for an action. | Programmers |
| [🗞️ Rumble Service](Components/Rumble%20Service.md) | Plays gamepad rumble patterns, with priorities so important rumble wins. | Programmers |
<!-- all-components:end -->