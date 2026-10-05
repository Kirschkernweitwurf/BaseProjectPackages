# Missing Scripts Overview

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Unity Editor > Project Health > Unused > Missing Scripts Overview`

Finds every "Missing (Mono Script)" in the project and lets you jump straight to it.

## How to use it

1. Open the window.
2. Pick what to search: **Scenes**, **Prefabs**, **Scriptable Objects**.
3. Press **Scan**.
4. Results are grouped by asset. Click a row to select the GameObject or asset that holds the broken reference.
5. Use the search field to narrow down the list.

When nothing is found you get a green confirmation instead of an empty list.

## Notes

- Scanning scenes opens them in the background, so a full scan can take a moment on a large project.
- The window only reports. Fixing is up to you: either reassign the script or remove the component.
