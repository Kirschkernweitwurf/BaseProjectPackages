---
type: component
package: Settings
audience: everyone
tags:
  - designer
  - settings
  - ui
summary: "The same left arrow / label / right arrow picker as Int Multiple Choice Element, with one difference: it saves the chosen word itself instead of its position in the list."
---
# String Multiple Choice Element

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

The same left arrow / label / right arrow picker as [🔖 Int Multiple Choice Element](Int%20Multiple%20Choice%20Element.md), with one difference: it saves the **chosen word itself** instead of its position in the list.

That makes it safe to reorder or add options later without breaking saved values.

## Inspector fields

Identical to [🔖 Int Multiple Choice Element](Int%20Multiple%20Choice%20Element.md). See that page for the full table.

In short: Setting Key, Title, Description, Left Button, Right Button, Value Text, Selection Indicator Prefab, Selection Indicator Parent, Options.

## When to use this

- Any setting that stores text rather than a number.
- Anywhere you expect the option list to grow over time.

For resolutions specifically, use [🔖 Resolution Choice Element](Resolution%20Choice%20Element.md) instead. It is this component with the option list filled in for you.

## Fields

### Setting Element

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Setting Key** | Text |  | **Required.** Key of the String setting this picker stores its choice in. |
| **Title** | Localized text |  | Shown in the Setting Flavor Text while this row is focused. |
| **Description** | Localized text |  | Shown in the Setting Flavor Text while this row is focused. |

### Multiple Choice

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Left Button** | Button |  | **Required.** Button that picks the previous option. |
| **Right Button** | Button |  | **Required.** Button that picks the next option. |
| **Value Text** | Text (TMP) |  | **Required.** Shows the chosen option. |
| **Selection Indicator Prefab** | [Selection Indicator Button](Selection%20Indicator%20Button.md) |  | **Required.** Prefab for one dot in the row under the picker. |
| **Selection Indicator Parent** | Transform |  | **Required.** Where the dots are spawned. |
| **Options** | List of Text |  | The words the player can pick from, in order. The chosen word is what gets saved. |

## Good to know

- The label the player sees **is** the saved value. Do not translate these labels, or saved values will stop matching when the language changes.
- If a saved word is not in the list any more, the picker falls back to the first option.
- The list wraps around, and the dots are clickable, the same as the int version.

## Setup checklist

- Options are exactly the text values the setting expects.
- Options are not localized.
- All five references assigned.
- Setting Key matches.