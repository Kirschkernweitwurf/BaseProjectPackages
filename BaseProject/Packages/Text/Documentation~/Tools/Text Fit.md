---
type: Tool
package: Text
audience: Everyone
tags:
  - artist
  - designer
  - localization
  - ui
summary: An editor window that renders every localized text in every language and lists the ones that do not fit, with one click to fix the box, open it or open the text.
menu: Tools > Base Packages > Assets > Localization > Text Fit
---
# Text Fit

🪛 **Tool** · for everyone

**Tags:** #artist #designer #localization #ui

**Menu:** `Tools > Base Packages > Assets > Localization > Text Fit`

Renders every localized text in every language in its real box: every label with a Localize String Event, every string code marks with `[ShownIn]` and the texts the project's own sources add. It lists each text that does not fit, one card per text with a line per language and fixes the box, opens it or opens the text with one click.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

## What the window does

| Control | What it does |
| --- | --- |
| **Check Texts** (F5) | Runs the check now. While the window is open it also runs on its own when it opens, after a script reload and after every change to a text, style, prefab or scene. |
| **Suggest Sizes** (F6) | Tries sizes for every text style in every language and opens **Sizes**. |
| **Recheck on save** | On: the list checks itself again a moment after you save a text, style, prefab or scene. Off: it only checks on F5. Only for you. |
| **Worst case** | On: checks with the widest readable font, wider spacing and the largest text size (150 %), the hardest case a player can pick. Text in a part that zooms with the text size may shrink as far as the zoom grows it back. Off: the designed look. Only for you. |
| Language chips | One per language with problems. Click one to see only that language, **All** to see every language again. |
| Search | Filters every page by text, key or box name. |

### Pages

| Page | What it lists |
| --- | --- |
| **Problems** | Texts of a real language that do not fit. They never stop a build; fix them when a screen or a language is finished. |
| **Tight** | Texts that fit today, but not in the pseudo-language, so they have little room left for a longer translation. One card per box with a row per text, since many texts share one box, like a name label that shows every item name. A warning, not a must-fix. |
| **Unchecked** | Boxes with words in them that the check never renders. See below. |
| **Sizes** | The size of each text style per language and the largest that still fits ("or more" when even the largest size tried fits). Green: every text fits. Red: one does not. For a red style, **Use** shrinks it to fit in one language and **Use … Everywhere** to fit in all of them, so a kind of box looks the same in every language. Growing a style is a design choice, so there is no button for it. Undo works as usual. |

Each problem card shows what the text is, where its box lives and why it does not fit in each language: too wide or too tall (by how many percent, at the size the text shrank to), cut with "…", too many lines, a word wider than the box, a missing glyph or a box its layout gives no room. A style can fail too: when it is missing from the **Text Styles** table in a language or its size is below its readable minimum. A long path keeps the asset and the box name and shortens the middle to "…"; hover it to see all of it.

- **Auto Size** turns on auto size for a box without a text style, from its size down to two thirds of it, so a long translation shrinks instead of spilling out. It opens the box so you see the change and undo works.
- **Show Box** opens the prefab (or the scene) and selects the box, ready to resize.
- **Edit Text** opens the string table at the entry, ready to shorten.
- **Ping Source** shows the asset a text built in code comes from, like the data asset of an item.

## How to fix a text that does not fit

1. Let the box shrink the text (**Auto Size**), for a box without a style or
2. Shorten the text in the string table (**Edit Text**) or
3. Make the box larger (**Show Box**) or
4. Pick a smaller size for its style in **Sizes**. The sizes live in the **Text Styles** table, one per language.
5. A box without a style keeps the font and size set on it. Give it a [🔖 Localized Text Box](../Components/Localized%20Text%20Box.md) to style it per language.

## Unchecked boxes

Nobody knows whether a translation fits a box the check never renders. Every box with words in it shows up here until it is checked or ignored:

- A fixed text: give it a **Localize String Event**.
- A text that code writes from a `LocalizedString` field: mark the field with `[ShownIn(nameof(label))]`, naming the text field of the same component. Code that swaps the string of a Localize String Event names that field instead; the check follows it to its label. The check then renders the string in that box.
- A text that code builds, like a sentence put together from several values: a programmer adds an `ITextSampleSource` that knows how the code builds it. The package README explains how.
- A debug label, a clock or a placeholder no player sees: **Ignore** it. The list is saved in `ProjectSettings/TextFitIgnoreList.asset` for the whole team. **Check Again** brings it back.

## Good to know

- **Worst case:** run it before a screen ships. There, frames (cards, tabs, carousel values, table labels) shrink to their readable minimum and panels grow, so a panel may take more lines than its style allows; what still does not fit needs a shorter text or more room. The text size grows every text of a style by the same amount, its Min Readable Size times the extra text size. So the smallest text grows to 150 % while titles grow less. The widest font is the **Widest Font** of the project's Readable Font Set asset. Without such an asset the worst case checks the designed fonts and says so in the Console.
- The check only runs when you start it here, never on a build: while screens and content still change, a text that does not fit is a task, not a reason to stop a build. A build pipeline that wants it runs `TextFitBatch` from the command line: `-batchmode -quit -executeMethod Base.TextPackage.Editor.TextFitBatch.Run`. It exits with 1 when a text of a real language does not fit.
- A text without a translation yet is measured in English, since the check treats English as the fallback of every language. A text English lacks too shows nothing in the game: it fails as "No English text".
- The pseudo-language is the project's Pseudo-Locale. Give it the **Word Expander** method from this package: it makes a text longer with extra words after a space, like a real translation. Unity's own Expander glues random letters to the last word, which makes a word no translation has and fails narrow boxes for the wrong reason. Set its rules by text length to how much longer your longest translations get. Measure again when a new language comes in.
- The check measures where the letters really sit, from their outlines, at the size the text settled on, so a one-line label that runs too wide counts too (TextMeshPro's own overflow flag misses it). The overflow warning of [🔖 Localized Text Box](../Components/Localized%20Text%20Box.md) uses the same measurement (`TextOverflow`).
- A box gets its style from the **Text Styles** entry its Localized Text Box points at, by name or by id.
- Each language is measured as a player of it sees it: its readable font swaps, its hyphenation in boxes with **Is Hyphenated** and real bold for `<b>`. A string list that code shows one at a time, like carousel options, is checked once marked with `[ShownIn]`. A list entry that gets its size from where it is placed is measured there: nested in its screen or under the parent its prefab field names with `[SpawnedIn]`, like the rows of a list that code fills.
- The check renders texts in their real boxes, laid out at 1920 × 1080, so it needs no play mode. A box under a hidden parent, like the Back prompt of a page that opens later, is laid out with that parent shown, from the layout group that sizes the page, so a scroll view inside it gets its real width. A box that is hidden itself keeps its saved size, since the code that shows it also arranges its neighbors.
- Dynamic fonts are measured with an in-memory copy, so the check never adds glyphs to a font asset of the project.
- The console line lists only problems of real languages. Pseudo-language problems show as a count there; read them on the **Tight** tab.
- An overshoot below a tenth of the font size counts as fitting: it is the line's padding, not a visible letter.
- A prefab and every scene that holds it bring the same box; it is checked and listed once, at the prefab.

## Related

- [🔖 Localized Text Box](../Components/Localized%20Text%20Box.md)