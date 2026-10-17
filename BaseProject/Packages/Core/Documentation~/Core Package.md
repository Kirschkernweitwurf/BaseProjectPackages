---
type: Start Page
package: Core
tags:
  - menus
summary: "Menus, scenes, timers, state machines, input and pooling: the runtime pieces most screens are built from."
---
# Core Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #menus

Menus, scenes, timers, state machines, input and pooling: the runtime pieces most screens are built from.

> [!NOTE]
> Tweening, Audio and the Bootstrapper used to be documented here. They now live in their own packages: [Tweening](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Tweening%20Package.md), [Audio](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/Audio%20Package.md) and [Services](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/Services%20Package.md).

## Components

Every component has its own page. [🗳️ Components](Core%20Components.md) lists them all, the [📖 guide](Components/Menus.md) explains how they fit together.

### Menus

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Menu](Components/Menu.md) | Turns a UI root into a screen that opens and closes with an animation. | Everyone |
| [🔖 Pause Menu](Components/Pause%20Menu.md) | A Menu that also tracks whether the game is paused. | Everyone |
| [🔖 Loading Screen](Components/Loading%20Screen.md) | A Menu that shows itself while scenes load, with progress bar and spinner. | Everyone |

### Menu modules

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Menu Cursor Module](Components/Menu%20Cursor%20Module.md) | Shows, hides or locks the mouse cursor while the menu is open. | Everyone |
| [🔖 Menu Time Scale Module](Components/Menu%20Time%20Scale%20Module.md) | Changes the game speed while the menu is open. | Everyone |
| [🔖 Menu Input Map Module](Components/Menu%20Input%20Map%20Module.md) | Switches the active controls while the menu is open. | Everyone |
| [🔖 Menu Reset Module](Components/Menu%20Reset%20Module.md) | Resets the menu's contents when it closes, so it opens fresh. | Everyone |

### Helpers

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Activate After Time](Components/Activate%20After%20Time.md) | Turns a GameObject on after a delay in seconds. | Everyone |
| [🔖 Activate After Frames](Components/Activate%20After%20Frames.md) | Turns a GameObject on after a number of frames. | Everyone |
| [🔖 Tooltip Trigger](Components/Tooltip%20Trigger.md) | Shows a tooltip while the pointer hovers this object. | Everyone |
| [🔖 Tooltip View](Components/Tooltip%20View.md) | Draws the tooltip next to the cursor and keeps it on screen. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Camera Provider](Components/Camera%20Provider.md) | Gives code fast access to the main and the current camera. | Programmers |
| [🗞️ Cursor Manager](Components/Cursor%20Manager.md) | Decides how the mouse cursor looks, by priority. | Programmers |
| [🗞️ Event Bus](Components/Event%20Bus.md) | Lets systems send each other events without knowing each other. | Programmers |
| [🗞️ Input Manager](Components/Input%20Manager.md) | Decides which input action map is active, by priority. | Programmers |
| [🗞️ Menu Manager](Components/Menu%20Manager.md) | Keeps track of every menu and routes the Back button. | Programmers |
| [🗞️ Scene Loading Manager](Components/Scene%20Loading%20Manager.md) | Loads and unloads scenes and keeps the persistent scene loaded. | Programmers |
| [🗞️ Screenshot Manager](Components/Screenshot%20Manager.md) | Saves screenshots on F6 (normal size) and F7 (four times larger). | Programmers |
| [🗞️ Time Scale Manager](Components/Time%20Scale%20Manager.md) | Decides the game speed, by priority. | Programmers |
| [🗞️ Tooltip Service](Components/Tooltip%20Service.md) | Shows the most important requested tooltip. | Programmers |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Menu Identifier](Scriptable%20Objects/Menu%20Identifier.md) | A name tag for a menu, so nothing references menus by string | `MID_` | `Base > Menus > New Menu Identifier` |

All create paths start with `Create > Scriptable Objects >`.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🪛 Regenerate Menu Identifiers](Tools/Regenerate%20Menu%20Identifiers.md) | Rebuilds the menu identifier registry and its generated code | `Tools > Base Packages > Code Generation > Regenerate Menu Identifiers` |
| [🔧 Event Bus Window](Tools/Event%20Bus%20Window.md) | Live list of every event and who listens to it | `Tools > Base Packages > Debug > Event Bus` |
| [🔧 State Machine Monitor](Tools/State%20Machine%20Monitor.md) | Draws the running state machines and their current state | `Tools > Base Packages > Debug > State Machine Monitor` |

## Where to start

| You want to | Go to |
| --- | --- |
| Build a new screen | The [📖 Menus guide](Components/Menus.md), then a Tween Group from the [Tweening package](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) as its Content Root |
| Show something after a delay | [Activate After Time](Components/Activate%20After%20Time.md) |
| Show a tooltip | [Tooltip Trigger](Components/Tooltip%20Trigger.md) |

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Core%20Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.