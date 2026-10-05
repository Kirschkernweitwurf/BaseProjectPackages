# Core Package

📦 **Package** · reusable, works in any of our projects

Menus, scenes, timers, state machines, input and pooling: the runtime pieces most screens are built from.

> [!NOTE]
> Tweening, Audio and the Bootstrapper used to be documented here. They now live in their own packages: [Tweening](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/index.md), [Audio](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/index.md) and [Services](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/index.md).

## Components

| Page | What is in it | Key components |
| --- | --- | --- |
| [🔖 Menus](Components/Menus.md) | Screens and their behavior | Menu, Menu Modules, Pause Menu, Loading Screen |
| [🔖 Utility](Components/Utility.md) | Small single purpose helpers | Activate After Time, Activate After Frames, Tooltip Trigger |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Menu Identifier](Scriptable%20Objects/Menu%20Identifier.md) | A name tag for a menu, so nothing references menus by string | `MID_` | `Base > Menus > New Menu Identifier` |

All create paths start with `Create > Scriptable Objects >`.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🪛 Regenerate Menu Identifiers](Tools/Regenerate%20Menu%20Identifiers.md) | Rebuilds the menu identifier registry and its generated code | `Tools > Base Packages > Assets > Menu > Regenerate Menu Identifiers` |
| [🔧 Event Bus Window](Tools/Event%20Bus%20Window.md) | Live list of every event and who listens to it | `Tools > Base Packages > Runtime > Event Bus` |
| [🔧 State Machine Monitor](Tools/State%20Machine%20Monitor.md) | Draws the running state machines and their current state | `Tools > Base Packages > Gameplay > State Machine Monitor` |

## Where to start

| You want to | Go to |
| --- | --- |
| Build a new screen | [Menus](Components/Menus.md), then a Tween Group from the [Tweening package](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tweening.md) as its Content Root |
| Show something after a delay | [Activate After Time](Components/Utility.md) |
| Show a tooltip | [Tooltip Trigger](Components/Utility.md) |

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
