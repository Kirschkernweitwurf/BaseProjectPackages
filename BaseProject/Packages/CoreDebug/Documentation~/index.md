# Core Debug Package

📦 **Package** · reusable, works in any of our projects

The in-game debug tools: a debug menu with a cheat console and a log console, and debug shapes that also show up in a build. Leave this package out of the release project and none of it ships.

## Debug menu

Press **F1** in play mode (the default binding) to open the debug menu. It remembers whether you last had the cheat console or the log console open.

| Part | What it does |
| --- | --- |
| **Cheat console** | Type a command and press Enter. `help` lists every command the game has. Programmers add commands with `[CheatCommand]`. |
| **Log console** | Shows the same log as Unity's Console, in the running game. It starts recording before the first scene, so nothing is missed while the menu is closed. |

The prefabs for the menu and both consoles are in the Content package.

## Debug drawing

`DebugDraw` draws lines, arrows, boxes, spheres and text labels that are visible in the Game view and in development builds, not only in the Scene view. Programmers call it from code; everyone else sees the result.

From the cheat console, `debugdraw_enabled` switches it on and off and `debugdraw_clear` removes everything currently drawn.

## Good to know

All `DebugDraw` calls are removed from release builds automatically. Programmers can keep them by defining `BASE_DEBUG_DRAW`.

For the code side, see the package [README](../README.md).

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
