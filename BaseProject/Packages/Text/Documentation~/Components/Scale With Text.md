---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Grows icons that sit beside a text, like status icons next to a clock, as far as that text grows with the player's text size, so they keep their size to it."
---
# Scale With Text

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Grows icons that sit beside a text, like the signal and battery icons next to a clock, as far as that text really grows on screen with the player's text size, so they keep their size to it. It follows the size the text shows after it fitted into its box, not the size it asked for. So a text in a fixed box that can't grow keeps its icons at their size too. The Xbox Accessibility Guidelines ask icons to scale with text. It scales around its pivot.

**Add it:** Inspector › **Add Component** › search `Scale With Text`. Only one per GameObject.

## How to use it

1. Add it to the object that holds the icons, like the status icons next to a clock. Set **Text** to that text.
2. When the text grows with the text size, the icons grow by the factor the text really shows after fitting into its box, so they keep their size to it. Set the pivot to the side that stays put: status icons right of a clock keep their right edge.
3. Use it for small icons next to one text. For a whole part with many texts, like a button bar, use [🔖 Scale With Text Size](Scale%20With%20Text%20Size.md).

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Text** | [Localized Text Box](Localized%20Text%20Box.md) |  | **Required.** The text the icons sit beside, like the clock. |

## Related

- [🔖 Localized Text Box](Localized%20Text%20Box.md)
- [🔖 Scale With Text Size](Scale%20With%20Text%20Size.md)