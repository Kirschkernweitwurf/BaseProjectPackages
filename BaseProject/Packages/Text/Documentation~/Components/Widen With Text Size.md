---
type: Component
package: Text
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Widens a box of fixed width, like a tooltip panel, as far as the player's text size, readable font and wider spacing widen its text, so its lines keep about as many words as designed and a long word still fits."
---
# Widen With Text Size

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Widens a box of fixed width, like a tooltip panel, as far as the player's text size, readable font and wider spacing widen its text, so its lines keep about as many words as designed and a long word still fits. The width depends on the reading settings only, never on the text, so every box of the kind keeps one width and none jumps from one text to the next. A text that only wraps once it reaches a width, like a tooltip text, keeps its own width below that and wraps at the grown one. A chat bubble also ends at its longest line once it wraps. It only tells the layout how wide the box wants to be, above a Layout Element, which keeps the designed width for the editor.

**Add it:** Inspector › **Add Component** › search `Widen With Text Size`. Needs **Rect Transform** on the same GameObject. Only one per GameObject.

## How to use it

1. Add it to a box of fixed width that holds running text, like a tooltip panel. The texts inside that only wrap at a width, like the tooltip's title and body, can get one too, with **Short Text** set to the text itself.
2. **Text** is the box whose reading settings the width follows, like the tooltip body. Its line width grows with the text size, the readable font and wider spacing together, so the lines keep about as many words as designed, up to 1.5 times the designed width. Past that a tooltip grows taller instead of covering half the screen.
3. The width depends on the settings only, never on the text, so every tooltip keeps one width.
4. **Max Width** caps the growth where the box would run into something, like a chat bubble that must stay clear of the other side of the chat.
5. **Is Fit To Longest Line** makes a chat bubble: a wrapped text fills each line before the next starts and the box ends at its longest line, like in a messaging app. Give its [Localized Text Box](Localized%20Text%20Box.md) the **Fill** line fit, since even lines would leave room at the side again.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Text** | [Localized Text Box](Localized%20Text%20Box.md) |  | **Required.** The text whose size the width follows, like the body of a tooltip. |
| **Designed Width** | Number |  | The width in the designed look. It grows by as much as a line of the text does. |
| **Max Width** | Number |  | Optional: the widest the box gets, however much its text grows, like the room a chat bubble has before it reaches the other side. 0 lets it grow as far as the text does. |
| **Short Text** | Text (TMP) |  | Optional: a text that keeps its own width while it is shorter and wraps at the grown width, like a tooltip text. Empty gives the box the grown width always. |
| **Is Fit To Longest Line** | Toggle | Off | Ends the box at the short text's longest line once it wraps, like a chat bubble. Off keeps a wrapped text at the grown width, like a tooltip. Pair it with the Fill line fit, since even lines leave room. |

## Related

- [🔖 Localized Text Box](Localized%20Text%20Box.md)
- [🔖 Grow With Text](Grow%20With%20Text.md)
- [🔖 Grow With Label](Grow%20With%20Label.md)