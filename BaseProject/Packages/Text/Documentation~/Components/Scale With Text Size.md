---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Zooms a whole part, like a button bar, as far as the player's text size grows text, up to the room the part has."
---
# Scale With Text Size

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Zooms a whole part, like a button bar at the bottom of a menu, as far as the player's text size grows text, up to the room the part has. The part keeps its designed layout and just gets bigger, so nothing inside has to shrink, wrap or cut. It scales around its pivot, so a part pinned to an edge keeps that edge. For a part on a canvas of its own, [🔖 Scale Canvas With Text Size](Scale%20Canvas%20With%20Text%20Size.md) zooms the whole canvas instead.

**Add it:** Inspector › **Add Component** › search `Scale With Text Size`. Only one per GameObject.

## How to use it

1. Add it to a part that should grow as a whole, like a button bar at the bottom of a menu. Put its anchor and pivot at the bottom center, so it grows up and to both sides.
2. At a larger text size the whole part is shown bigger: icons, labels and background alike. Texts inside grow only by what the zoom does not cover, so they keep their designed size to the part.
3. Put the pivot where the part should stay put: an edge with something next to it, like a title above.
4. Set **Max Zoom** to the room the part has on screen. 1.5 follows the largest text size. A part that would run off the screen gets less and its texts grow the rest of the way.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Max Zoom** | Number | Largest Scale | The largest zoom, the room the part has on screen. At 1.5 it follows the largest text size. |

## Related

- [🔖 Scale Canvas With Text Size](Scale%20Canvas%20With%20Text%20Size.md)
- [🔖 Scale Grid With Text Size](Scale%20Grid%20With%20Text%20Size.md)
- [🔖 Scale With Text](Scale%20With%20Text.md)