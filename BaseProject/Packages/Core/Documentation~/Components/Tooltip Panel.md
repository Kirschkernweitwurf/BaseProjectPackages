---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: Draws one tooltip panel with icon, title and text.
---
# Tooltip Panel

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Draws one panel of a tooltip: an optional header with icon and title, and the text below it. Parts a tooltip leaves empty are hidden. Texts grow with their content up to **Max Text Width** and wrap after that, so short tooltips stay small. The Tooltip View copies this panel for every further panel a tooltip needs.

**Add it:** Inspector › **Add Component** › search `Tooltip Panel`. Only one per GameObject.

## How to use it

1. Build the panel inside the Tooltip View's content: a background Image with a Vertical Layout Group.
2. Add a header row with a Horizontal Layout Group, holding an Image for the icon and a TextMeshPro text for the title.
3. Add a TextMeshPro text for the body below the header.
4. Add **Tooltip Panel** to the panel and drag in the texts, the icon and the header row.
5. For icons inside the text, give the texts a TextMeshPro sprite asset.

## Fields

### Settings

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Max Text Width** | Number | 320 | Widest a text gets before it wraps, in canvas units. |

### References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Body** | Text (TMP) |  | **Required.** Shows the entry's text. Give it a sprite asset to allow icons in the text. |
| **Title** | Text (TMP) |  | Shows the entry's title. Optional. |
| **Icon** | Image |  | Shows the entry's icon in front of the title. Optional. |
| **Header** | GameObject |  | Row that holds icon and title, hidden when the entry has neither. Optional. |

## Good to know

- The layout groups need **Control Child Size** on and **Child Force Expand** off, so the panel shrinks to its texts.
- The panel adds a Layout Element to its texts to cap their width.
- The icon is tinted per entry, so one white icon can serve several categories.

## Related

- [🔖 Tooltip View](Tooltip%20View.md)
- [🔖 Tooltip Trigger](Tooltip%20Trigger.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)