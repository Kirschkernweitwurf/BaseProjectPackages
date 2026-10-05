# Menu Item Manager

**Menu:** `Tools > Base Packages > Menu Management > Menu Item Manager`

Rearranges editor menu items by drag and drop, without touching any code.

This works for every tool marked with `[DynamicMenuItem]`. Instead of a fixed path and priority baked into the script, the layout lives in data and you edit it here.

## The two lists

| List | What it is |
| --- | --- |
| **Shipped** | The layout that comes with the package. Read only when the package is installed as a package. |
| **Overlay** | Your project's own layout. Always editable. Anything you change lands here. |

The shipped section can be collapsed if it is in your way.

## How to use it

1. Open the window.
2. Drag entries and groups around to build the menu you want.
3. Create groups to make submenus. Rename them inline.
4. Click the thin divider between two rows to add or remove a separator line there.
5. Press **Reload** if the real menu bar still looks stale. This forces a script reload so Unity rebuilds the menu.

## Buttons

| Button | What it does |
| --- | --- |
| Auto Group | Rebuilds groups from each entry's default path and shortens the entry to its last part. |
| Sort A-Z | Sorts groups and entries by name at every level. |
| Clean Missing | Removes entries whose code no longer exists. |
| Open | Opens the script that defines the selected entry. |

## Priorities

Priority is calculated automatically from the order in the list. You can switch a single entry to a manual value with the **M** button, and go back to automatic with **A**.

A gap bigger than 10 between two priorities is what makes Unity draw a separator line, which is why the separator toggle exists.

## Notes

- The overlay is saved in `ProjectSettings`, so the team shares it.
- Undo works inside the window, up to 100 steps.
