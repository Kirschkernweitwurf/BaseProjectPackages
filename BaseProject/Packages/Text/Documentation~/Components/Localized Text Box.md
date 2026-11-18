---
type: Component
package: Text
audience: Everyone
tags:
  - artist
  - designer
  - ui
  - localization
summary: "Gives a text box its style for the current language: font, size and line spacing."
---
# Localized Text Box

🔖 **Component** · for everyone

**Tags:** #artist #designer #ui #localization

Gives a text box the style of its kind for the current language: font, material, size and line spacing in one step, so no frame shows the new font at the old size. Auto sizing is off unless the style sets an **Auto Size Min**, which lets a long text shrink down to it. Every box of a kind has the same size and the [🪛 Text Fit](../Tools/Text%20Fit.md) check makes sure translations fit. A style with more than one line evens out its lines, like CSS `text-wrap: balance`, so no word sits alone on the last line. The styles live in an asset table named **Text Styles**, one Text Style asset per kind of box and language. A readable font and wider spacing the player picked go on top of the style.

**Add it:** Inspector › **Add Component** › search `Localized Text Box`. Needs **Text (TMP)** on the same GameObject. Only one per GameObject.

## How to use it

1. Put it on a TextMeshPro text.
2. Pick the box's style from the **Text Styles** table. Each entry of the table is one kind of box, like a button label, with a Text Style asset per language. A new one is made with `Assets > Create > Scriptable Objects > Base > Text > New Text Style`.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Text** | Text (TMP) |  | Filled in automatically. The text on the same object. |
| **Style** | Localized Text Style |  | The style of this box, one per language, from the Text Styles table. |
| **Can Cut Long Text** | Toggle | Off | This box limits its lines on purpose, so a longer text ends in ... without a warning, whatever the style says. For a short look at a text that can be read in full elsewhere. |
| **Line Fit** | Line Fit |  | How a text of several lines uses the box. Balance evens out the lines, for short texts like cards. Pretty keeps them full and only fixes a short last line, for paragraphs in a box of fixed width. |
| **Is Hyphenated** | Toggle | Off | Lets long words break at the end of a line, with a hyphen, in languages that have patterns for it. |
| **Balancer** | Line Balancer |  | Hidden. Runtime state of the line balancing, nothing to set. |
| Balancer › **Cut** | Vector2 |  | How far the margins are moved in right now, left and right. Runtime state, zero in saved assets. |
| **Has Designed Layout** | Toggle | Off |  |
| **Designed Character Spacing** | Number |  |  |
| **Designed Word Spacing** | Number |  |  |
| **Designed Paragraph Spacing** | Number |  |  |
| **Designed Wrapping** | Text Wrapping Modes |  |  |

## Good to know

- **Panels and frames:** a style's **Role** says how its boxes make room when the player's font, spacing or text size make text bigger. A panel (rows, tooltips, dialogs) keeps the size and its layout grows. A frame (cards, buttons, badges, table labels) keeps its box, so the text shrinks back into it, down to **Min Readable Size**. With the designed font, normal spacing and 100 % text size, every style looks exactly as designed.
- **Large view:** a box that is always shown bigger on hover or focus, like a card in a hand, gets that factor from the code that zooms it. As a frame it may then shrink below **Min Readable Size** by that factor, since hovering brings it back to readable size.
- **One-line frames:** a frame style with **Max Lines** 1 never wraps, so a long word shrinks instead of breaking onto a second line. Other boxes keep the wrapping they were designed with.
- **Shared sizes:** under a [🔖 Text Size Group](Text%20Size%20Group.md), boxes with the same style share the size the longest text needs.
- **Reading settings:** the package has no settings menu of its own. The project's settings pass the player's text size, readable font and spacing to the package. Until they do, every box keeps its designed look.
- **Text size:** the player's text size adds the same amount to every text of a style: its **Min Readable Size** times the extra text size. At the largest text size (150 %) the smallest text is 1.5 times as big while titles grow less. A frame tries the bigger size and shrinks back into its box.
- **Overflow warning:** in the editor and in development builds, the box writes a warning to the Console when its text runs out of its box. It reads where the letters really sit, so a one-line text that runs too wide is reported too. A text faded out by a Canvas Group above it (alpha 0) is skipped, since nobody sees it, like a header that grows in faded out while a list opens. A box with **Can Cut Long Text** never warns.
- **Readable fonts:** when the player picks a readable font, the box swaps the style's font for it, scaled to the same lowercase height and with the designed line height. Only fonts the readable font has a swap for change and only in languages it has letters for. With wider spacing on, the extra room goes on top of the box's own letter, word and paragraph spacing, which it keeps from the first time it applies a style. Only paragraphs get it: panel styles with **Max Lines** above 2. Short texts and frames keep their designed spacing. Both apply again at once when the player changes them.
- **Even lines:** styles with **Max Lines** above 1 get the narrowest width that keeps their line count and size and breaks no word. A box with **Can Cut Long Text** skips this, so it fits as many words as it can. The box and its layout stay the same; only the text margins move in on the side the text does not align to. **Line Fit** picks how: **Balance** for short texts like cards, **Pretty** for paragraphs in a box of fixed width, like tooltips. Pretty keeps the lines full and only narrows them as far as a short last line needs (at least two words and a third of the width), so it follows CSS `text-wrap: pretty`. Justified text was ruled out, because narrow boxes like cards open big gaps between words.
- **Hyphens:** **Is Hyphenated** marks long words with soft hyphens right before TextMeshPro reads the text, using the Knuth-Liang patterns in a `Resources/Hyphenation` folder of the project. The package ships no patterns; a programmer adds them. Words under 7 letters stay whole and 3 letters stay on each side of a break. Styled words, like a keyword in a rich text tag, break like any other word, since a long term kept whole leaves big gaps in a narrow box. An icon right after a word stays glued to its end. Languages without patterns, like Japanese and Chinese, pass through. Ship the license notices of the patterns with the game.

## Related

- [🔖 Text Size Group](Text%20Size%20Group.md)
- [🪛 Text Fit](../Tools/Text%20Fit.md)