---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Zooms a whole screen canvas, like a HUD, as far as the player's text size grows text, up to the room the screen has."
---
# Scale Canvas With Text Size

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Zooms a whole screen canvas, like a HUD, as far as the player's text size grows text, up to the room the screen has. It shrinks the canvas's reference resolution, so every part grows together: parts pinned to a corner keep their corner and what hangs under another part, like a list under a bar, keeps its gap. Texts on the canvas keep their designed size within it.

**Add it:** Inspector › **Add Component** › search `Scale Canvas With Text Size`. Needs **Canvas Scaler** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it next to the Canvas Scaler of a screen canvas, like a HUD. Set **Max Zoom** to the room the screen has: at the largest text size, parts in opposite corners must still not meet. At the default 1.25 every part shows 1.25 times as big.
2. Texts on the canvas keep their designed size within it and only grow by what the zoom does not cover, so at 150 % with a zoom of 1.25 the smallest texts grow another 1.2 times. A frame may shrink as far as the zoom grows it back, so labels still fit their boxes.
3. It changes the scaler's reference resolution, so check the screen at the largest text size after you add a new part.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Scaler** | Canvas Scaler |  | **Required.** Filled in automatically. The scaler whose reference resolution shrinks. Auto-assigned from the same GameObject. |
| **Max Zoom** | Number | 1.25 | The largest zoom, the room the screen has. Parts in opposite corners must still not meet at it. |

## Good to know

- The Canvas Scaler needs **Scale With Screen Size**, since only that mode reads the reference resolution.
- It reads the designed reference resolution once, when the object wakes up.

## Related

- [🔖 Scale Grid With Text Size](Scale%20Grid%20With%20Text%20Size.md)
- [🔖 Scale With Text Size](Scale%20With%20Text%20Size.md)
- [🔖 Localized Text Box](Localized%20Text%20Box.md)