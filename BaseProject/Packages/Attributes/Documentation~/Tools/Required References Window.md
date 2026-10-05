# Required References Window

🪛 **Tool** · for everyone

**Menu:** `Tools > Base Packages > Unity Editor > References > Required References`

## What it does

Lists every field in the project that is marked as required but is still empty.

The window checks two places:

- All objects in the **currently open scenes** (including inactive ones)
- All **ScriptableObject assets** in the project

Fields inside nested classes and lists are checked too, so a missing reference deep inside a data structure still shows up.

## What counts as a problem

Anything flagged by a validation attribute, mainly:

| Attribute | Fails when |
| --- | --- |
| `[Required]` | The object reference is empty (None) |
| `[NotNullOrEmpty]` | The text is empty, or the list has no entries |

## How to use it

1. Open the window from the menu.
2. The bar at the top shows how many problems were found.
3. Every entry is grouped by the GameObject or asset that owns it.
4. **Click an entry** to select and ping the object, so you can fix it right away.
5. Use the **search field** on the top right to filter by name.
6. Press **Refresh** if you want to force a full rescan.

When everything is filled in, the window shows a success message instead of a list.

## Live updates

The window keeps itself up to date on its own:

- Scene changes are picked up almost instantly
- Asset changes are picked up when the project changes
- Entering or leaving play mode triggers a rescan

You can leave the window docked next to the inspector while you work.

## Related

Missing references are also logged as errors in the console when you enter play mode. Those log messages point back to this window.
