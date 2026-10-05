# Save System Package

📦 **Package** · reusable, works in any of our projects

Save and load for Unity, built for designers to wire up in the inspector.

Everything is driven by one component in the scene. You never have to touch files, JSON or encryption yourself.

## Sections

| Section | What is inside |
| --- | --- |
| [🗳️ Components](Components.md) | Everything you drag onto a GameObject: the manager, the UI buttons, the screenshot and play time helpers. |

## The 30 second version

1. Put a **SaveManager** on a GameObject in your first scene.
2. Pick a **Slot Model** in its inspector (Fixed, Appending or Named).
3. Add a **SelectSlotButton**, **SaveGameButton**, **LoadGameButton** and **DeleteGameButton** to your save menu UI.
4. Optional: add a **ScreenCapturer** and a **PlaytimeTracker** for thumbnails and play time in the menu.

That is a working save menu without a single line of code.

## Where saves live

Each slot is a folder inside `Application.persistentDataPath/Saves/`. A slot folder holds up to three files: the save data, the screenshot and the metadata. The metadata file is written last, so a crash mid save can never look like a finished save.

In the editor saves are plain readable JSON by default. In a build they are encrypted.

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
