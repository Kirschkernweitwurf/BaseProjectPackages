# Settings Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #settings #ui

Everything you need to build a settings menu: volume, resolution, full screen mode, quality, VSync and language, all saved between sessions.

You do not need to write code. You drag components onto GameObjects, fill in a few fields, and the package handles saving, loading and applying.

## Sections

| Section | What is in it |
| --- | --- |
| [🗳️ Components](Components.md) | Everything you add to a GameObject in a scene. This is the section you want. |

## The 30 second version

1. Put a **SettingsContext** in your settings scene. Nothing works without it.
2. Add one **setting component** per thing you want to save (volume, resolution, and so on).
3. Add **UI elements** (toggle, slider, dropdown, picker) and type the matching setting key into each one.

That is it. The value the player picks is saved automatically when the scene is destroyed.

## Words you will see

- **Setting** - one saved value, for example "music volume" or "resolution".
- **Key** - the name a setting is saved under, for example `MusicVolume`. UI elements find their setting by key, so the key on the UI element must match the key on the component.
- **Default** - the value used the very first time the game runs, and the value you get back when a setting is reset.
- **Apply** - the moment the value actually does something, for example the screen changing resolution.

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
