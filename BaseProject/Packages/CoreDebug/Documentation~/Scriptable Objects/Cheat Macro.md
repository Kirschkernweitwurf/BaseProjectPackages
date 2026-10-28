---
type: Scriptable Object
package: Core Debug
audience: Everyone
tags:
  - debugging
  - designer
  - programmer
summary: "Your own cheat command that runs several cheats in a row."
---
# Cheat Macro

💌 **Scriptable Object** · for everyone

**Tags:** #debugging #designer #programmer

**Menu path:** `Create > Scriptable Objects > Base > Debug > New Cheat Macro`

**Default file name:** `CM_CheatMacro`

## What it does

Your own cheat command. Type its name in the cheat console and it runs its lines one after another, as if you had typed them. Use it to get into your usual test setup in one go, for example a command named after you that freezes affection and makes the drink infinite.

No code needed. It shows up in `help` and in Tab completion like any other command.

## Fields

| Field | What it does |
| --- | --- |
| **Command** | What you type in the console. Any name, for example your own. Empty uses the asset name. |
| **Description** | Shown next to the command in `help`. |
| **Lines** | Cheat console lines, run top to bottom. Each one is a command with its arguments, exactly as you would type it. |

## How to use it

1. Create a folder named `CheatMacros` inside any `Resources` folder, for example `Assets/Resources/CheatMacros`.
2. Create the asset there and set **Command**.
3. Add one line per cheat to **Lines**, for example `date.affection.freeze`.
4. In play mode, open the cheat console and type your command.

## Tips

- Only assets inside a `Resources/CheatMacros` folder are picked up. Anywhere else, the command does not exist.
- A line can call another macro. A macro that calls itself is skipped instead of looping forever.
- The console answers with one line per cheat, so you can see which ones worked.
- Lines a macro runs don't go into the console history, so the arrow keys still bring back what you typed.

## Run it automatically

Type `autorun.add` and your command, for example `autorun.add Jonny`. From then on it runs on its own as soon as the console finds every cheat its lines need, so you no longer type it at the start of every date. It runs again for each new date, but not twice for the same one.

- The list is yours alone: it lives in this computer's settings, not in version control, so teammates keep their own.
- `autorun.list` shows your list, `autorun.remove Jonny` takes a command off it.
- Any command without arguments works, not only macros.
- The Unity console shows a line each time something ran automatically.