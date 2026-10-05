# Component Clipboard

🪛 **Tool** · for everyone

**Tags:** #artist #designer #workflow

**Menu:** `Tools > Base Packages > Unity Editor > Component Clipboard`

Unity's own component clipboard only holds one component at a time. This window holds as many as you want.

## How to use it

1. Select a GameObject. The window lists its components with a checkbox each.
2. Tick the components you want. Shift click selects a range.
3. Press **Copy**.
4. Select another GameObject, or several.
5. Press **Paste**.

## Badges

Each row shows what a paste would do:

| Badge | Meaning |
| --- | --- |
| Add | The target does not have this component yet, so it gets added. |
| Overwrite | The target already has it, so the values are replaced. |
| Missing | The script behind this snapshot no longer exists. |

## Other actions

Right click a row for a context menu with copy, paste, delete and reorder.

## Notes

- Transforms are skipped. Unity does not allow them to be added or removed.
- The clipboard survives editor restarts. It is stored in `UserSettings`, which means it is personal and is not committed to version control.
- Pasting onto several selected GameObjects at once works.
