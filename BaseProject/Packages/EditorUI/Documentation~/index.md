# Editor UI Package

The shared look of every Base editor window. You never add anything from it to a scene. The one thing worth knowing: the look is adjustable.

## Changing the look

1. Open `Project Settings > Base Tools > Editor UI Theme`.
2. Assign a theme asset, or create one.
3. Change colors, row heights, corner radii and spacing. Every open Base window redraws right away.

Without a theme, the windows use the built-in look.

For programmers building their own windows on top of it, see the package [README](../README.md).

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
