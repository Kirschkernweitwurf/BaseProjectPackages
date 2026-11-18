---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Gives a row in a layout group, like a settings row, the height its text needs: never less than designed, more when a wider font, wider spacing or a bigger text size make the text take more lines."
---
# Grow With Text

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Gives a row in a layout group, like a settings row, the height its text needs: never less than designed, more when a wider font, wider spacing or a bigger text size make the text take more lines. A panel grows this way instead of letting text run into the next row. It moves nothing inside the row; it only tells the layout how tall the row wants to be and passes text changes on so the panel measures again.

**Add it:** Inspector › **Add Component** › search `Grow With Text`. Needs **Rect Transform** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a row inside a layout group, like each row of a settings page. Set **Content** to the part that holds the text, with a Vertical Layout Group that controls its children's heights.
2. **Designed Height** is the row's height as designed, **Padding** the room the design leaves around the text: the designed height minus the text's designed height. So a row only grows once its text needs more than the design gave it and it keeps its designed margins, including the gap above a slider.
3. With the designed font, the row keeps its designed height. When a wider font, wider spacing or a bigger text size give the text more lines, the row grows and the page scrolls further.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Content** | Rect Transform |  | **Required.** The part that holds the text, with a layout group that knows its preferred height, like Texts. |
| **Designed Height** | Number | 124 | The height of the row as designed. It never gets smaller than this. |
| **Padding** | Number | 20 | Room above and below the text together, so the text keeps its margin when the row grows. |

## Good to know

- It moves nothing inside the row. Pickers, switches and the focus plate keep their anchors, so they stay centered as the row grows.
- Panels grow, frames shrink: see the **Role** of a text style on [🔖 Localized Text Box](Localized%20Text%20Box.md).

## Related

- [🔖 Localized Text Box](Localized%20Text%20Box.md)
- [🔖 Text Size Group](Text%20Size%20Group.md)
- [🔖 Keep Row In Place On Text Change](Keep%20Row%20In%20Place%20On%20Text%20Change.md)