---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Zooms a grid of entries, like a grid of cards, as far as the player's text size grows text, up to a limit."
---
# Scale Grid With Text Size

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Zooms a grid of entries, like a grid of cards, as far as the player's text size grows text, up to a limit. The whole grid is shown bigger, so every entry keeps its designed layout and its own scale animations, like a card's hover swell. A grid with a fixed number of columns shows fewer of them, so a row keeps about its designed width and the grid scrolls further down instead.

**Add it:** Inspector › **Add Component** › search `Scale Grid With Text Size`. Needs **Grid Layout Group** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a Grid Layout Group with a fixed number of columns, like a grid of cards in a menu. The default **Max Zoom** of 1.5 follows the largest text size.
2. The whole grid is shown bigger and the number of columns drops (8 columns become 5 at 1.5), so a row keeps about its designed width and the grid scrolls further down. Entries keep their own scale animations, like a deal-in or a hover swell, since only the grid scales.
3. Texts in the grid keep their designed size within the zoom, so they barely have to shrink.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Grid** | Grid Layout Group |  | **Required.** Filled in automatically. The grid that zooms. Auto-assigned from the same GameObject. |
| **Max Zoom** | Number | Largest Scale | The largest zoom. At 1.5 it follows the largest text size. |

## Good to know

- A grid with a fixed row count shows fewer rows the same way. A flexible grid keeps its layout and only zooms.

## Related

- [🔖 Scale Canvas With Text Size](Scale%20Canvas%20With%20Text%20Size.md)
- [🔖 Scale With Text Size](Scale%20With%20Text%20Size.md)