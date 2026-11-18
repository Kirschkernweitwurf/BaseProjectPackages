---
type: Start Page
package: Text
tags:
  - ui
  - localization
summary: "Localized text styles for TextMeshPro, readable fonts and text size options for accessibility and a check that every translation fits."
---
# Text Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #ui #localization

Localized text styles for TextMeshPro, readable fonts and text size options for accessibility and a check that every translation fits its box.

The goal is simple: every kind of text box looks the same in every language and every player can read it. A designer picks a style per box once. Font, size and line spacing then follow the language and the player's reading settings on their own.

## What's inside

| Section | What it covers | Pages |
| --- | --- | --- |
| Text styles | One style per kind of text box and language: font, size, line spacing and how the box makes room for bigger text. Even lines and hyphens for longer texts. | Localized Text Box, Text Size Group |
| Zoom | Parts of the screen that zoom as a whole with the player's text size. | Scale With Text Size, Scale Canvas With Text Size, Scale Grid With Text Size, Scale With Text |
| Layout | Boxes and rows that grow with their text and scroll views that keep the player's row in place. | Grow With Label, Grow With Text, Widen With Text Size, Keep Row In Place On Text Change |
| Text Fit | An editor window that renders every localized text in every language and lists what does not fit. | Text Fit |

## Sections

| Section | What is inside |
| --- | --- |
| [🗳️ Components](Text%20Components.md) | Scripts you drop on a GameObject in a scene or prefab. |
| [🧰 Tools](Text%20Tools.md) | The Text Fit window. |

## What your project provides

The package is code only. It ships no fonts, styles or hyphenation patterns.

| What | Why |
| --- | --- |
| An asset table named **Text Styles** | Each entry is one kind of text box, like a button label, with a Text Style asset per language. Every Localized Text Box picks its entry there. The Text Fit window finds the table by this exact name. |
| A **Readable Font Set** asset | The readable font choices and the wider spacing. The Text Fit worst case reads its widest font. |
| Reading settings | The package has no settings menu. The project's settings code passes the player's text size, readable font and spacing to the package. Until it does, every text keeps its designed look. A programmer finds the details in the package README. |
| Hyphenation patterns | Only for boxes with **Is Hyphenated**. They go in a `Resources/Hyphenation` folder, see the package README. |
| A Pseudo-Locale | Optional. It makes every text longer, so Text Fit can list the texts with little room left. |

Text Style and Readable Font Set assets are created with `Assets > Create > Scriptable Objects > Base > Text`.

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Text%20Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.