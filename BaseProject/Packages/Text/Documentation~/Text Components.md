---
type: Overview
package: Text
tags:
  - ui
summary: Ready-made components from the Base Text Package.
---
# Text Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #ui

Ready-made components from the **Base Text Package**. Drop them on a GameObject, fill in the fields in the Inspector, done. No code needed.

## How to use

1. Select the GameObject in your scene or prefab.
2. Click **Add Component** and search for the component name.
3. Fill in the fields marked as **required**. Fields marked required show a red warning in the Inspector until they are filled.

All of them follow the player's reading settings: text size, readable font and wider spacing. The project's settings pass those to the package. Until they do, every component keeps the designed look.

## Text styles

The base of the package. Every text box with a style looks the same in every language and grows with the player's reading settings.

| Component | What it does |
| --- | --- |
| [🔖 Localized Text Box](Components/Localized%20Text%20Box.md) | Gives a text box its style for the current language |
| [🔖 Text Size Group](Components/Text%20Size%20Group.md) | Makes boxes of the same style share one size |

## Zoom

Parts of the screen that are shown bigger as a whole at a bigger text size, so nothing inside has to shrink or wrap.

| Component | What it does |
| --- | --- |
| [🔖 Scale With Text Size](Components/Scale%20With%20Text%20Size.md) | Zooms one part, like a button bar |
| [🔖 Scale Canvas With Text Size](Components/Scale%20Canvas%20With%20Text%20Size.md) | Zooms a whole screen canvas, like a HUD |
| [🔖 Scale Grid With Text Size](Components/Scale%20Grid%20With%20Text%20Size.md) | Zooms a grid and shows fewer columns |
| [🔖 Scale With Text](Components/Scale%20With%20Text.md) | Grows icons as far as the text beside them grows |

## Layout

Boxes and rows that make room for bigger text instead of letting it shrink or spill out.

| Component | What it does |
| --- | --- |
| [🔖 Grow With Label](Components/Grow%20With%20Label.md) | Widens a button when its label needs more room |
| [🔖 Grow With Text](Components/Grow%20With%20Text.md) | Makes a row taller when its text takes more lines |
| [🔖 Widen With Text Size](Components/Widen%20With%20Text%20Size.md) | Widens a box of fixed width, like a tooltip, with its text |
| [🔖 Keep Row In Place On Text Change](Components/Keep%20Row%20In%20Place%20On%20Text%20Change.md) | Keeps the player's row in view when the reading settings change |

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Text styles

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Localized Text Box](Components/Localized%20Text%20Box.md) | Gives a text box its style for the current language: font, size and line spacing. | Everyone |
| [🔖 Text Size Group](Components/Text%20Size%20Group.md) | Makes text boxes of the same kind below it share one size, so frames that sit side by side look even: when one text has to shrink to fit, every box with the same style shrinks with it. | Everyone |

### Zoom

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Scale With Text Size](Components/Scale%20With%20Text%20Size.md) | Zooms a whole part, like a button bar, as far as the player's text size grows text, up to the room the part has. | Everyone |
| [🔖 Scale Canvas With Text Size](Components/Scale%20Canvas%20With%20Text%20Size.md) | Zooms a whole screen canvas, like a HUD, as far as the player's text size grows text, up to the room the screen has. | Everyone |
| [🔖 Scale Grid With Text Size](Components/Scale%20Grid%20With%20Text%20Size.md) | Zooms a grid of entries, like a grid of cards, as far as the player's text size grows text, up to a limit. | Everyone |
| [🔖 Scale With Text](Components/Scale%20With%20Text.md) | Grows icons that sit beside a text, like status icons next to a clock, as far as that text grows with the player's text size, so they keep their size to it. | Everyone |

### Layout

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Grow With Label](Components/Grow%20With%20Label.md) | Gives a button or chip the width its label needs: never narrower than designed, wider when a bigger font, wider spacing or a bigger text size make the label wider, so the label keeps the size the player picked instead of shrinking. | Everyone |
| [🔖 Grow With Text](Components/Grow%20With%20Text.md) | Gives a row in a layout group, like a settings row, the height its text needs: never less than designed, more when a wider font, wider spacing or a bigger text size make the text take more lines. | Everyone |
| [🔖 Widen With Text Size](Components/Widen%20With%20Text%20Size.md) | Widens a box of fixed width, like a tooltip panel, as far as the player's text size, readable font and wider spacing widen its text, so its lines keep about as many words as designed and a long word still fits. | Everyone |
| [🔖 Keep Row In Place On Text Change](Components/Keep%20Row%20In%20Place%20On%20Text%20Change.md) | Keeps the row the player is on at its spot in a scroll view when the reading settings change. | Everyone |
<!-- all-components:end -->