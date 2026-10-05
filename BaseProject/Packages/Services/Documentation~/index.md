# Services Package

The foundation the other packages run on: how managers find each other, start up and shut down. Mostly code, but two things matter to everyone: the Bootstrapper in every scene, and the Service Locator window when something "does nothing".

## Components

| Page | What it is for | For whom |
| --- | --- | --- |
| [🔖 Bootstrapper](components/bootstrapper.md) | Spawns the manager prefabs every scene needs | Everyone |

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🔧 Service Locator Window](tools/service-locator-window.md) | Lists every registered service while the game runs | `Tools > Base Packages > Runtime > Service Locator` |

## When something does nothing

1. Check the scene has a [Bootstrapper](components/bootstrapper.md).
2. Enter Play mode and open the [Service Locator Window](tools/service-locator-window.md). If the service you need is missing or marked as destroyed, that is your problem.

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
