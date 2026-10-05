# Unused Scripts Overview

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Unused > Unused Scripts Overview`

Lists script files that nothing in the project seems to reference. Same idea as the [🪛 Unused Assets Overview](<Unused Assets Overview.md>), but for code.

## How to use it

1. Press **Scan**.
2. Click a row to ping the script in the Project window.
3. **Dismiss** false positives. They are remembered per project and can be restored from the foldout at the top.
4. **Delete** the rest.

## Please read before deleting

A script can be used in ways the scan cannot see: reflection, generic base classes, editor-only usage, or a name used in a string. Open the file and check before you delete it.
