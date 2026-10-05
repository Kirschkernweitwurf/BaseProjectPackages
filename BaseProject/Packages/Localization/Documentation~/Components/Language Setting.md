# Language Setting

🔖 **Component** · for everyone

Saves the game language and switches it through Unity's Localization package.

Key: `Language`

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Available Locales** | The languages you want to offer, in the order they appear in the menu. Cannot be empty. |
| **Default Index** | Which entry in that list is used on first run. Counting starts at 0. Default is `0`. |

Drag Locale assets in from your project. They live under **Assets > Localization > Locales** if you used the default Localization setup, and you can also find them in Project Settings > Localization.

## What gets saved

The **position in your list**, not the language itself. Reordering the list after release changes what saved values point at. Add new languages to the end.

## Order matters

Put this **near the top** of the Hierarchy, above anything that reads translated text while starting up. Otherwise the first frame of your menu can appear in the wrong language.

## Which UI to use

An [🔖 Int Multiple Choice Element](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Int%20Multiple%20Choice%20Element.md) or a[🔖 Setting Dropdown](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Setting%20Dropdown.md) with key `Language`.

Write each language in its own language (Deutsch, English, Francais), not translated. That is the standard and it is what players expect.

## Setup checklist

- Locales dragged in, in the order you want them shown.
- UI labels are in the same order as the locales.
- Sits high in the Hierarchy.
- UI element key is `Language`.
