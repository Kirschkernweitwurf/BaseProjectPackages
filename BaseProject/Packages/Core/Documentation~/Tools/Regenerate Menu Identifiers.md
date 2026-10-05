# Regenerate Menu Identifiers

🪛 **Tool** · for everyone

**Tags:** #designer #menus

**Menu path:** `Tools > Base Packages > Assets > Menu > Regenerate Menu Identifiers`

## What it does

Scans the project for every Menu Identifier asset and rebuilds two things from them:

1. `MenuIdentifiers.cs` at `Assets/Generated/MenuIdentifiers/MenuIdentifiers.cs`. A generated class that lets programmers write `MenuIdentifiers.PauseMenu` instead of dragging an asset into a field.
2. **The registry asset** (`MIR_Registry.asset`). This is what lets the game find identifiers at runtime in a build.

## How to use it

You normally never need to. Adding, renaming, or deleting a Menu Identifier triggers the regeneration automatically on the next editor tick.

Run it manually if something looks out of sync, for example after a merge, after pulling changes from someone else, or if a menu suddenly cannot be found.

## What it prints

| Message | Meaning |
| --- | --- |
| `Generated MenuIdentifiers.cs with N entries` | Everything worked |
| `Duplicate MenuIdentifier name '...'` | Two identifier assets share a name. Rename one and run again. Nothing is generated until this is fixed. |
| `Created MenuIdentifierRegistry at '...'` | No registry existed, so a new one was made |
| `Deleting duplicate MenuIdentifierRegistry at '...'` | More than one registry existed. Only one is kept. |
| `... is not under a Resources folder` | The registry will not ship in a build. Move it into a Resources folder. |

## Rules

- Exactly **one** registry may exist per project. Extra ones are deleted automatically.
- The registry has to live somewhere under a `Resources` folder. You can move it wherever you like as long as that stays true.
- Identifier asset names must be unique, since they become C# property names.
- The tool only writes files when something actually changed, so it will not spam your version control with pointless edits.
