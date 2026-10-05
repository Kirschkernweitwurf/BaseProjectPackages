# Audio Package

📦 **Package** · reusable, works in any of our projects

Sounds as assets. An Audio Container describes one sound, the Play Audio components play it from UI events, and the Audio Manager does the rest.

## Components

| Page | What is in it | Key components |
| --- | --- | --- |
| [🔖 Audio](Components/Audio.md) | Playing sounds from UI events, and the manager behind it | Play Audio On Click, Hover, Select, Submit, Audio Manager |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Audio Container](Scriptable%20Objects/Audio%20Container.md) | One sound as the game sees it: clips, volume, pitch, looping | `AUC_` | `Base > Audio > New Audio Container` |

All create paths start with `Create > Scriptable Objects >`.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🪛 Unused Audio Clips](Tools/Unused%20Audio%20Clips.md) | Finds audio clips nothing references and lets you select or delete them | `Tools > Base Packages > Assets > Audio > Unused Audio Clips` |

For import settings across all audio files, see [Audio Rules](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tools/Documentation~/Tools/Audio%20Rules.md) in the Tools package.

## Where to start

1. Create an [Audio Container](Scriptable%20Objects/Audio%20Container.md) and drop your clips in.
2. Add a Play Audio On... component from [Audio](Components/Audio.md) to the button or object.
3. Assign the container. Done.

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
