---
type: Guide
package: Core
tags:
  - designer
  - ui
  - scenes
summary: Small single-purpose helpers in Core and the services behind them.
---
# Utility

📖 **Guide** · for everyone

**Tags:** #designer #ui #scenes

Small helpers that solve one problem each, plus the services in Core that most of the game relies on. Every component has its own page with all fields.

## Helpers you add to scenes

| You want | Add |
| --- | --- |
| Show something after a few seconds | [🔖 Activate After Time](Activate%20After%20Time.md) |
| Hide a one-frame layout glitch | [🔖 Activate After Frames](Activate%20After%20Frames.md) |
| A tooltip on hover | [🔖 Tooltip Trigger](Tooltip%20Trigger.md), plus one [🔖 Tooltip View](Tooltip%20View.md) in the UI |

## Services on the manager prefabs

These sit on the manager prefabs the Bootstrapper spawns. You rarely add them yourself, but it helps to know they exist.

| Service | Job |
| --- | --- |
| [🗞️ Menu Manager](Menu%20Manager.md) | Opens and closes menus, routes the Back key |
| [🗞️ Input Manager](Input%20Manager.md) | Decides which input action map is active |
| [🗞️ Cursor Manager](Cursor%20Manager.md) | Decides how the mouse cursor looks |
| [🗞️ Time Scale Manager](Time%20Scale%20Manager.md) | Decides the game speed |
| [🗞️ Scene Loading Manager](Scene%20Loading%20Manager.md) | Loads and unloads scenes |
| [🗞️ Tooltip Service](Tooltip%20Service.md) | Shows the most important tooltip |
| [🗞️ Event Bus](Event%20Bus.md) | Lets systems send each other events |
| [🗞️ Camera Provider](Camera%20Provider.md) | Fast access to the main camera |
| [🗞️ Screenshot Manager](Screenshot%20Manager.md) | Saves screenshots on F6 and F7 |

## Handy keys

| Key | Does |
| --- | --- |
| **Escape** / gamepad **B** | Back |
| **F1** | Debug menu with the cheat console (Core Debug package) |
| **F6** | Screenshot at screen size |
| **F7** | Screenshot at four times screen size |

These keys are in the Permanent input map, so they work in every menu.

The Bootstrapper that spawns the manager prefabs lives in the Services package: [🔖 Bootstrapper](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/Components/Bootstrapper.md).