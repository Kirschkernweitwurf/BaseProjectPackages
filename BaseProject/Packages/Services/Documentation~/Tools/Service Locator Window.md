# Service Locator Window

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Runtime > Service Locator`

**For:** Programmers, and anyone debugging a scene that "does nothing"

A live list of every service registered while the game runs: its type, the object behind it, where that object lives, and whether it is still usable.

## How to use it

1. Open the window and enter Play mode.
2. The table refreshes on its own.
3. Click a column header to sort. Drag the edges to resize.

## What to look for

- **A service is missing.** Usually the scene has no [Bootstrapper](../Components/Bootstrapper.md), or the manager prefab does not contain that service.
- **A service is marked as destroyed.** It was registered and its object is gone. Code that still asks for it gets nothing. This is the case the window exists for.

## Good to know

The window only reads. It never registers or removes anything.
