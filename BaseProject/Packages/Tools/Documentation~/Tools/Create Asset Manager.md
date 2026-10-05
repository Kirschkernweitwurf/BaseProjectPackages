# Create Asset Manager

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Menu Management > Create Asset Manager`

The same window as the [🔧 Menu Item Manager](<Menu Item Manager.md>), but for the **Assets > Create** menu.

It manages every scriptable object marked with `[DynamicCreateAssetMenu]`. Use it to keep the Create menu tidy as the project grows.

## Extra column

Next to the path, each entry has a **File Name** column. That is the default name given to a new asset, without the extension. For example `LP_LightingProfile` creates `LP_LightingProfile.asset`.

## How to use it

1. Open the window.
2. Drag entries and groups into the structure you want.
3. Rename groups to build submenus.
4. Click the divider between two rows to toggle a separator line.
5. Press **Reload** if the Create menu still looks stale.

## Buttons

| Button | What it does |
| --- | --- |
| Auto Group | Rebuilds groups from each entry's default path. |
| Sort A-Z | Sorts everything by name. |
| Clean Missing | Removes entries whose class no longer exists. |
| Open | Opens the script behind the entry. |

## Notes

- Shipped layout is read only. Your changes go into the project overlay, saved in `ProjectSettings`.
- Priorities are automatic by default and can be overridden per entry.
