---
type: Tool
package: Tools
audience: Everyone
tags:
  - artist
  - designer
  - workflow
summary: "Opens the folder the game saves its screenshots to."
menu: Tools > Base Packages > Editor > Open Screenshot Folder
---
# Open Screenshot Folder

🪛 **Tool** · for everyone

**Tags:** #artist #designer #workflow

**Menu:** `Tools > Base Packages > Editor > Open Screenshot Folder`

Opens the folder the [🗞️ Screenshot Manager](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Screenshot%20Manager.md) saves its screenshots to in the file explorer, so a fresh screenshot is one click away.

## How to use it

Take a screenshot with the Screenshot Manager's keys in Play Mode, then run **Open Screenshot Folder**.

The folder is `Screenshots` in the game's save data folder (`Application.persistentDataPath`). The editor and a build of the same game share it.

## Notes

- If the folder doesn't exist yet, a warning in the Console names the path instead.
- Screenshots taken with the system's own shortcut land elsewhere; this only covers the game's screenshots.