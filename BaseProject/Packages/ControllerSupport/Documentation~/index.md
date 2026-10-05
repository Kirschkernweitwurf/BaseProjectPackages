# Controller Support Package

## Tools

| Tool | Menu path |
| --- | --- |
| [🪛 NavigationGroupsWindow](tools/navigation-groups-window.md) | `Tools > Base Packages > Unity Editor > Controller Navigation Groups` |

## Components

| Component | Purpose |
| --- | --- |
| [🔖 NavigableElement](components/navigable-element.md) | Marks a selectable as a gamepad target |
| [🔖 NavigableGroup](components/navigable-group.md) | Wires navigation and owns a focus context |
| [🔖 FocusWatchdog](components/focus-watchdog.md) | Restores focus when the gamepad loses it |
| [🔖 MenuNavigationModule](components/menu-navigation-module.md) | Bridges a menu's lifecycle to a group |
| [🔖 GamepadScrollRect](components/gamepad-scroll-rect.md) | Stick scrolling for a ScrollRect |
| [🔖 ScrollIntoView](components/scroll-into-view.md) | Keeps the selection visible in a ScrollRect |

Input prompt components (`InputDeviceTracker`, `InputGlyphSet`, `InputGlyphProvider`) are not covered here yet.

## Typical setup

1. Add `FocusWatchdog` to the service scene.
2. Put a `NavigableGroup` on each menu root and assign its Default Element.
3. Add `MenuNavigationModule` where a menu drives the group, and turn Auto Activate off.
4. Hit Rebuild. Missing `NavigableElement`s are added for you.
5. For long lists, add `ScrollIntoView`, and `GamepadScrollRect` if you want stick scrolling.

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
