---
type: Component
package: UI
audience: Programmers
tags:
  - programmer
summary: "Bends the mesh of an image or other graphic into the tilt of the UIPerspective above it and hides it while its side faces away."
---
# UI Perspective Graphic

🗞️ **Component** · programmers only

**Tags:** #programmer

Bends the mesh of an image or other graphic into the tilt of the UIPerspective above it and hides it while its side faces away. The perspective adds it to every graphic on its own; add it by hand only to mark a back side, like the back of a card.

**Add it:** Inspector › **Add Component** › search `UI Perspective Graphic`. Only one per GameObject.

## How to use it

1. Nothing to do for normal graphics: the **UI Perspective** above adds it at runtime.
2. Add it by hand only to a back side image and tick **Is Back Side**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Is Back Side** | Toggle | Off | Whether this graphic is on the back, so it draws only while the UI is turned around. |

## Related

- [UI Perspective](UI%20Perspective.md)
- [UI Perspective Text](UI%20Perspective%20Text.md)