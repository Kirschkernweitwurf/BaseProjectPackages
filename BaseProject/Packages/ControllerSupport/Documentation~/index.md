# Controller Support Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #gamepad #ui

## Tools

| Tool | Menu path |
| --- | --- |
| [🪛 NavigationGroupsWindow](Tools/Navigation%20Groups%20Window.md) | `Tools > Base Packages > Unity Editor > Controller Navigation Groups` |

## Components

| Component | Purpose |
| --- | --- |
| [🔖 NavigableElement](Components/Navigable%20Element.md) | Marks a selectable as a gamepad target |
| [🔖 NavigableGroup](Components/Navigable%20Group.md) | Wires navigation and owns a focus context |
| [🗞️ FocusWatchdog](Components/Focus%20Watchdog.md) | Restores focus when the gamepad loses it |
| [🔖 MenuNavigationModule](Components/Menu%20Navigation%20Module.md) | Bridges a menu's lifecycle to a group |
| [🔖 GamepadScrollRect](Components/Gamepad%20Scroll%20Rect.md) | Stick scrolling for a ScrollRect |
| [🔖 ScrollIntoView](Components/Scroll%20Into%20View.md) | Keeps the selection visible in a ScrollRect |

Input prompt components (`InputDeviceTracker`, `InputGlyphSet`, `InputGlyphProvider`) are not covered here yet.

## Typical setup

1. Add `FocusWatchdog` to the service scene.
2. Put a `NavigableGroup` on each menu root and assign its Default Element.
3. Add `MenuNavigationModule` where a menu drives the group, and turn Auto Activate off.
4. Hit Rebuild. Missing `NavigableElement`s are added for you.
5. For long lists, add `ScrollIntoView`, and `GamepadScrollRect` if you want stick scrolling.

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
