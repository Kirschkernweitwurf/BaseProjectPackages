# Unused Assets Overview

🪛 **Tool** · for everyone

**Tags:** #artist #designer #sound #assets #project-health

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Unused > Unused Assets Overview`

Lists assets that nothing in the project seems to reference.

## What counts as used

An asset is treated as used when it can be reached from:

- a scene that is enabled in Build Settings
- any Resources folder
- the render pipeline settings
- the preloaded assets list
- anything referenced from ProjectSettings
- an Addressables entry

Everything else under `Assets`, minus code and editor files, shows up in the list.

## How to use it

1. Press **Scan**.
2. Go through the results. Click a row to ping the asset in the Project window.
3. **Dismiss** anything that is a false positive. Dismissed assets are remembered for this project and drop out of the count.
4. **Delete** what you really do not need.

Dismissed assets live in a foldout at the top. You can drag its bottom edge to resize it and restore anything from there.

## Please read before deleting

This is a best guess, not proof. Assets that are only loaded from code or by string path, cannot be detected and will show up as unused. Always check a file before deleting it.
