# Utility Package

📦 **Package** · reusable, works in any of our projects

Small building blocks every other package uses: serializable collections, logging, platform flags and helpers. Almost all of it is for programmers.

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [✉️ Unique Id Scriptable Object](<Scriptable Objects/Unique Id Scriptable Object.md>) | An asset carrying a stable ID | `UID_` | `Base > UniqueId > New ScriptableObject` |

All create paths start with `Create > Scriptable Objects >`. IDs are generated and checked by the [Generate Unique Ids](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tools/Documentation~/Tools/Generate%20Unique%20Ids.md) tool in the Tools package.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🔧 Custom Log Handler](<Tools/Custom Log Handler.md>) | Adds the class name to every `Debug.Log` in the console | `Tools > Base Packages > Unity Editor > Logging > Enable Custom Log Handler` |

For the code side (collections, logging API, helpers), see the package [README](../README.md).

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
