# Controller Support Package

📦 **Package** · reusable, works in any of our projects

## Tools

| Tool | Menu path |
| --- | --- |
| [🪛 NavigationGroupsWindow](<Tools/Navigation Groups Window.md>) | `Tools > Base Packages > Unity Editor > Controller Navigation Groups` |

## Components

| Component | Purpose |
| --- | --- |
| [🔖 NavigableElement](<Components/Navigable Element.md>) | Marks a selectable as a gamepad target |
| [🔖 NavigableGroup](<Components/Navigable Group.md>) | Wires navigation and owns a focus context |
| [🗞️ FocusWatchdog](<Components/Focus Watchdog.md>) | Restores focus when the gamepad loses it |
| [🔖 MenuNavigationModule](<Components/Menu Navigation Module.md>) | Bridges a menu's lifecycle to a group |
| [🔖 GamepadScrollRect](<Components/Gamepad Scroll Rect.md>) | Stick scrolling for a ScrollRect |
| [🔖 ScrollIntoView](<Components/Scroll Into View.md>) | Keeps the selection visible in a ScrollRect |

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

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
