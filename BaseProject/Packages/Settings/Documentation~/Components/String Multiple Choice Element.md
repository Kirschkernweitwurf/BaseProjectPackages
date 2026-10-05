# String Multiple Choice Element

🔖 **Component** · for everyone

The same left arrow / label / right arrow picker as [🔖 Int Multiple Choice Element](<Int Multiple Choice Element.md>), with one difference: it saves the **chosen word itself** instead of its position in the list.

That makes it safe to reorder or add options later without breaking saved values.

## Inspector fields

Identical to [🔖 Int Multiple Choice Element](<Int Multiple Choice Element.md>). See that page for the full table.

In short: Setting Key, Title, Description, Left Button, Right Button, Value Text, Selection Indicator Prefab, Selection Indicator Parent, Options.

## When to use this

- Any setting that stores text rather than a number.
- Anywhere you expect the option list to grow over time.

For resolutions specifically, use [🔖 Resolution Choice Element](<Resolution Choice Element.md>) instead. It is this component with the option list filled in for you.

## Good to know

- The label the player sees **is** the saved value. Do not translate these labels, or saved values will stop matching when the language changes.
- If a saved word is not in the list any more, the picker falls back to the first option.
- The list wraps around, and the dots are clickable, the same as the int version.

## Setup checklist

- Options are exactly the text values the setting expects.
- Options are not localized.
- All five references assigned.
- Setting Key matches.
