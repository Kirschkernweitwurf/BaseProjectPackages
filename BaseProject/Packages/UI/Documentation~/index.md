# UI Package

Reusable UI building blocks for Unity. Menu buttons, an "Are you sure?" popup and a handful of small helpers, ready to drop into any scene.

The goal is simple: common UI jobs should be a component you add and fill in, not something that gets rebuilt in every project.

## What's inside

| Section | What it covers | Components |
| --- | --- | --- |
| Buttons | Open and close menus, load scenes, open a website. They wire themselves up, so the `OnClick ()` list stays empty. | Open Menu, Close Menu, Pause Menu, Load Scene, Open Link |
| Confirmation | One shared popup that asks the player before something drastic happens, like quitting or leaving a scene. | Confirmation Menu, Confirmation Service, Confirmed Load Scene, Confirmed Quit |
| Utility | Small helpers for world-space UI, debugging and builds. | Billboard, Editor Billboard, World Canvas Wrapper, FPS Counter, Build Version |

## Included assets

| Asset | Path |
| --- | --- |
| Text button prefab | `Prefabs/Buttons/BasicTextButton` |
| Image button prefab | `Prefabs/Buttons/BasicImageButton` |
| Background image | `Images/BG_60` |

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
