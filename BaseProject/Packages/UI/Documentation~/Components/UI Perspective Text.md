---
type: Component
package: UI
audience: Programmers
tags:
  - programmer
summary: "Bends a TextMesh Pro text into the tilt of the UIPerspective above it and hides it while the front faces away."
---
# UI Perspective Text

🗞️ **Component** · programmers only

**Tags:** #programmer

Bends a TextMesh Pro text into the tilt of the UIPerspective above it and hides it while the front faces away. TMP builds its mesh itself and skips mesh effects, so this keeps the flat vertices of the last build and projects a copy of them whenever the tilt changes, without laying the text out again. The perspective adds it to every text on its own.

**Add it:** Inspector › **Add Component** › search `UI Perspective Text`. Needs **TMP_Text** on the same GameObject. Only one per GameObject.

## How to use it

1. Nothing to do: the **UI Perspective** above adds it to every text at runtime.

## Fields

None. Add it and it works.

## Good to know

- A text that gets a new string while it is tilted, like the name on a pooled card, shows the new one: it copies only as many vertices as the text's mesh holds, even when TMP reports more.

## Related

- [UI Perspective](UI%20Perspective.md)
- [UI Perspective Graphic](UI%20Perspective%20Graphic.md)