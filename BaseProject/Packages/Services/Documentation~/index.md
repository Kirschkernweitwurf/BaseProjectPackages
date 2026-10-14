---
type: Start Page
package: Services
tags:
  - setup
summary: "The foundation the other packages run on: how managers find each other, start up and shut down."
---
# Services Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #setup

The foundation the other packages run on: how managers find each other, start up and shut down. Mostly code, but two things matter to everyone: the Bootstrapper in every scene, and the Service Locator window when something "does nothing".

## Components

Every component has its own page. [🗳️ Components](Components.md) lists them all.

### Startup

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Bootstrapper](Components/Bootstrapper.md) | Spawns the manager prefabs the game needs. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Shutdown Manager](Components/Shutdown%20Manager.md) | Lets systems clean up in a fixed order when the game quits. | Programmers |

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🔧 Service Locator Window](Tools/Service%20Locator%20Window.md) | Lists every registered service while the game runs | `Tools > Base Packages > Runtime > Service Locator` |

## When something does nothing

1. Check the scene has a [Bootstrapper](Components/Bootstrapper.md).
2. Enter Play mode and open the [Service Locator Window](Tools/Service%20Locator%20Window.md). If the service you need is missing or marked as destroyed, that is your problem.

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