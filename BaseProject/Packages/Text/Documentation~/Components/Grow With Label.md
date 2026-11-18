---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Gives a button or chip the width its label needs: never narrower than designed, wider when a bigger font, wider spacing or a bigger text size make the label wider, so the label keeps the size the player picked instead of shrinking."
---
# Grow With Label

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Gives a button or chip the width its label needs: never narrower than designed, wider when a bigger font, wider spacing or a bigger text size make the label wider, so the label keeps the size the player picked instead of shrinking. The button's own layout group measures label and padding together; this only lets that width win over the button's Layout Element, which keeps the designed width.

**Add it:** Inspector › **Add Component** › search `Grow With Label`. Needs **Horizontal Or Vertical Layout Group** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a button that has its own Horizontal Layout Group (padding around the label) and a Layout Element that fixes its width.
2. **Designed Width** is the Layout Element's width. In the designed look the button keeps it; a label that needs more, at a bigger text size or in a wider font, widens the button instead of shrinking.
3. A row of tabs can use it too: a tab bar widens with its tabs, with a Content Size Fitter that takes the width.
4. Not for keys of an on-screen keyboard or other buttons in a fixed grid: they keep their box and their labels are written short.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Content** | Horizontal Or Vertical Layout Group |  | **Required.** Filled in automatically. The button's own layout group, which knows how wide label and padding are together. Auto-assigned from the same GameObject. |
| **Designed Width** | Number |  | The width as designed. The button never gets narrower than this. |

## Related

- [🔖 Grow With Text](Grow%20With%20Text.md), the height counterpart for rows
- [🔖 Widen With Text Size](Widen%20With%20Text%20Size.md)