---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
  - settings
summary: "Keeps the row the player is on at its spot in a scroll view when the reading settings change."
---
# Keep Row In Place On Text Change

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui #settings

Keeps the row the player is on at its spot in a scroll view when the reading settings change. A new text size, font or spacing changes the height of every row above it, while the scroll view keeps its offset. Without this the row slides away, often out of view, right while the player changes the setting in it. The row it keeps is the selected one; a hover selects a row too. A mouse press drops the selection, so then it is the row selected last. Without either it is the top row in view.

**Add it:** Inspector › **Add Component** › search `Keep Row In Place On Text Change`. Needs **Scroll Rect** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it next to the Scroll Rect of every page of a settings menu.
2. When the text size, the font or the spacing changes, it notes where the selected row sits in the view (a hover selects a row too, the gamepad focus does anyway; a mouse press drops the selection, so it falls back to the row selected last), lets the page lay out with the new sizes and scrolls so that row is back at the same spot. Without a selected row it holds the top row in view.
3. Add it to any other scroll view in which the reading settings can change while it is open.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Scroll Rect** | Scroll Rect |  | **Required.** Filled in automatically. The scroll view whose row stays in place. Auto-assigned from the same GameObject. |

## Good to know

- A row is a direct child of the scroll view's content. A selected button deeper inside a row counts for its whole row.

## Related

- [🔖 Grow With Text](Grow%20With%20Text.md)
- [🔖 Localized Text Box](Localized%20Text%20Box.md)