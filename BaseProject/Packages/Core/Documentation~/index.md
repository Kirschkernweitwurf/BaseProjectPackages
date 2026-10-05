# Core Package

Menus, scenes, timers, state machines, input and pooling: the runtime pieces most screens are built from.

> [!NOTE]
> Tweening, Audio and the Bootstrapper used to be documented here. They now live in their own packages: [Tweening](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/index.md), [Audio](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/index.md) and [Services](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/index.md).

## Components

| Page | What is in it | Key components |
| --- | --- | --- |
| [🔖 Menus](components/menus.md) | Screens and their behavior | Menu, Menu Modules, Pause Menu, Loading Screen |
| [🔖 Utility](components/utility.md) | Small single purpose helpers | Activate After Time, Activate After Frames, Tooltip Trigger |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Menu Identifier](scriptable-objects/menu-identifier.md) | A name tag for a menu, so nothing references menus by string | `MID_` | `Base > Menus > New Menu Identifier` |

All create paths start with `Create > Scriptable Objects >`.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🪛 Regenerate Menu Identifiers](tools/regenerate-menu-identifiers.md) | Rebuilds the menu identifier registry and its generated code | `Tools > Base Packages > Assets > Menu > Regenerate Menu Identifiers` |
| [🔧 Event Bus Window](tools/event-bus-window.md) | Live list of every event and who listens to it | `Tools > Base Packages > Runtime > Event Bus` |
| [🔧 State Machine Monitor](tools/state-machine-monitor.md) | Draws the running state machines and their current state | `Tools > Base Packages > Gameplay > State Machine Monitor` |

## Where to start

| You want to | Go to |
| --- | --- |
| Build a new screen | [Menus](components/menus.md), then a Tween Group from the [Tweening package](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/components/tweening.md) as its Content Root |
| Show something after a delay | [Activate After Time](components/utility.md) |
| Show a tooltip | [Tooltip Trigger](components/utility.md) |

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
