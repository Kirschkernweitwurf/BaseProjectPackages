---
type: Tool
package: Tools
audience: Everyone
tags:
  - artist
  - designer
  - workflow
summary: "Shows every canvas in the order Unity draws it and changes its sorting in place."
menu: Tools > Base Packages > Editor > Canvas Render Order
---
# Canvas Render Order

🪛 **Tool** · for everyone

**Tags:** #artist #designer #workflow

**Menu:** `Tools > Base Packages > Editor > Canvas Render Order`

Shows every canvas in the order Unity draws it and changes its sorting in place. Use it to find out why one screen covers another, and to fix the layering of all screens from one list.

## How to use it

1. Open the window. It lists the canvases of the open scenes, and of the prefab open in Prefab Mode.
2. Tick **Prefabs** to also list the canvases of every prefab under `Assets`.
3. Read each list from top to bottom: the top row is drawn first, the bottom row ends up in front.
4. Change **Sorting Layer** or **Order** right in the row. Click a name to select the canvas or ping its prefab.
5. Use the search field to filter by canvas, scene or prefab name.

## What is listed

Only canvases that sort on their own: root canvases, and nested canvases with **Override Sorting**. A nested canvas without it is drawn as part of its parent.

| Section | Holds | Sorted by |
| --- | --- | --- |
| Camera and World Space | Screen Space Camera and World Space canvases | Sorting layer, then order |
| Screen Space Overlay | Overlay canvases, drawn on top of everything else | Display, then order |

Overlay canvases ignore their sorting layer, so their rows only offer the order.

A ⚠ on a row means another canvas in the same section has the same layer and order. Unity then decides by camera distance or hierarchy order, which can flip between screens. A prefab instance and its own prefab do not count as a tie.

## Notes

- Scene changes are undoable and become instance overrides on prefab instances.
- Prefab changes are saved into the prefab right away and are not undoable. Variants and nested prefabs keep the change as their own override.
- A canvas from a nested prefab or a variant base is only listed in the prefab where its sorting changes.
- In Play mode the list includes `DontDestroyOnLoad`. Scene changes are lost when you stop, and prefabs are locked.
- Between canvases on different cameras the camera order comes first. The list does not show that.
- **Refresh** rescans everything, also after you add sorting layers.