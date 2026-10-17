---
type: Start Page
package: Controller Support
tags:
  - gamepad
  - ui
summary: Input prompt components (InputDeviceTracker, InputGlyphSet, InputGlyphProvider) are not covered here yet.
---
# Controller Support Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #gamepad #ui

## Tools

| Tool | Menu path |
| --- | --- |
| [🪛 NavigationGroupsWindow](Tools/Navigation%20Groups%20Window.md) | `Tools > Base Packages > Editor > Controller Navigation Groups` |

## Components

Every component has its own page. [🗳️ Components](Controller%20Support%20Components.md) lists them all.

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

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Controller%20Support%20Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.