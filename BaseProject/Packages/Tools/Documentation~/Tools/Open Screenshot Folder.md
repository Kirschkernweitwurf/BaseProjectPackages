---
type: Tool
package: Tools
audience: Everyone
tags:
  - artist
  - designer
  - workflow
summary: "Opens the folder your system saves screenshots to."
menu: Tools > Base Packages > Editor > Open Screenshot Folder
---
# Open Screenshot Folder

🪛 **Tool** · for everyone

**Tags:** #artist #designer #workflow

**Menu:** `Tools > Base Packages > Editor > Open Screenshot Folder`

Opens the folder your system saves screenshots to in the file explorer, so a fresh screenshot is one click away.

## How to use it

Take a screenshot with your system's shortcut, then run **Open Screenshot Folder**.

| System | Folder |
| --- | --- |
| Windows | `Pictures\Screenshots` |
| macOS | The location set in the Screenshot app, the Desktop by default |
| Linux | `Screenshots` in your Pictures folder (XDG user dirs), where GNOME and KDE save them |

## Notes

- If the folder doesn't exist yet, a warning in the Console names the path instead.
- Screenshots from Unity's own capture tools land elsewhere; this only covers the system's screenshots.