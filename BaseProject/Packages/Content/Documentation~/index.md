# Content Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #setup #ui #audio

The ready-made prefabs and assets the other Base packages are wired together with. No code. Start here when you need a working manager setup, a button or a canvas.

## What is inside

| Folder | What you find |
| --- | --- |
| `Prefabs/Bootstrap/Resources` | The Bootstrapper, ready for any scene |
| `Prefabs/Managers` | The persistent, scene and gameplay manager prefabs the Bootstrapper spawns |
| `Prefabs/UI/Buttons` | Basic text and image buttons, click and hover sounds already assigned |
| `Prefabs/UI/Canvases` | Persistent, gameplay overlay and gameplay world canvases |
| `Prefabs/UI/Menus` | Confirmation menu and loading screen |
| `Prefabs/UI/DebugMenu` | Debug menu with cheat console and log console |
| `Prefabs/UI/Tooltip` | The tooltip view |
| `Prefabs/UI/Widgets` | Small readouts such as the FPS text |
| `Prefabs/Audio` | One pooled audio source per audio type |
| `Audio/Mixers` | The audio mixer with Master, SFX, Ambience, UI and Music groups |
| `ScriptableObjects/AudioContainers` | Click and hover sound containers |
| `ScriptableObjects/MenuIdentifiers` | The menu identifiers the prefabs use |
| `Sprites` | Shared UI sprites |

## Good to know

- Install it through the Git Package Manager, not by pasting the Git URL. The prefabs use components from the other Base packages, and those must be installed too.
- The click and hover clips are placeholders. Swap the clips inside **AUC_Click** and **AUC_Hover** and every button keeps working.
- Package prefabs are read-only. To change one, create a prefab variant in your project.

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
