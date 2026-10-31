---
type: Component
package: Core Debug
audience: Programmers
tags:
  - programmer
  - debugging
summary: "Runs cheat commands: Enter runs, Tab completes, arrow keys walk the history, Ctrl+W and Ctrl+U delete."
requires:
  - Cheat Console View
---
# Cheat Console Controller

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

Runs the cheat console. **Enter** runs the typed command, **Tab** completes it word by word, like a shell (`date.a` becomes `date.affection`), **Up** and **Down** walk through earlier commands. Like in a shell, **Ctrl+W** deletes the word before the cursor (a dot counts as a word of its own) and **Ctrl+U** everything before it. In the editor the Game view ignores main menu shortcuts while the console has focus, so Ctrl+U does not open Undo History.

Commands on scene objects are found when the console opens, so scene loads stay fast while it is closed. If you set up commands with `autorun.add`, it also looks on scene loads until each of them ran once. A scene that loads while the console is open is picked up at once. The history of typed commands is kept on this computer (PlayerPrefs, not version control), so the arrow keys still reach them after leaving play mode or restarting the editor. It keeps the last 50 commands, skips a command typed twice in a row, and `history.clear` forgets it.

**Add it:** Inspector › **Add Component** › search `Cheat Console Controller`. Needs **Cheat Console View** on the same GameObject.

## How to use it

1. Put it on the same GameObject as the **Cheat Console View**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **View** | [Cheat Console View](Cheat%20Console%20View.md) |  | Filled in automatically. The cheat console view on the same GameObject. |

## Related

- [🗞️ Cheat Console View](Cheat%20Console%20View.md)
- [🗞️ Debug Menu Controller](Debug%20Menu%20Controller.md)