---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Makes text boxes of the same kind below it share one size, so frames that sit side by side look even: when one text has to shrink to fit, every box with the same style shrinks with it."
---
# Text Size Group

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Makes text boxes of the same kind below it share one size, so frames that sit side by side look even: when one text has to shrink to fit, every box with the same style shrinks with it. Used where frames form a set, like a row of signs or the cards in a hand.

**Add it:** Inspector › **Add Component** › search `Text Size Group`. Only one per GameObject.

## How to use it

1. Put it on the object that holds the set, like the parent of a row of signs or the root of a hand of cards.
2. Every [🔖 Localized Text Box](Localized%20Text%20Box.md) below it joins. Boxes with the same text style share one size: each one fits alone first, then all take the smallest.
3. It compares again whenever a box gets a new text or style, like a new entry in the set, a language switch or a new font choice.

## Fields

None. Add it and it works.

## Good to know

- Only boxes with the same style match each other, so titles and body texts each get their own shared size.
- Inactive boxes don't count, so a hidden entry can't make the set smaller.

## Related

- [🔖 Localized Text Box](Localized%20Text%20Box.md)