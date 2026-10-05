# Unique ID Validation

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Assets > Identifier > Enable Unique ID Validation`

A single on and off switch for the automatic ID checks. A checkmark next to the menu item means it is on.

## What it controls

When on, IDs are checked:

- when the editor loads
- when assets are imported
- before a build

When off, none of that runs. Nothing else about the package changes, and you can still assign IDs by hand with [🔧 Generate Unique Ids](Generate%20Unique%20Ids.md).

## Notes

- On by default.
- The setting is saved in `ProjectSettings`, so it is shared with the whole team through version control.
- Turn it off only if the checks get in your way. Missing IDs will break save data.
