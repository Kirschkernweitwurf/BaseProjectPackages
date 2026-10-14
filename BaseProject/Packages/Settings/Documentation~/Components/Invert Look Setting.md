---
type: Component
package: Settings
audience: Everyone
tags:
  - designer
  - settings
  - input
summary: Stores whether the horizontal or vertical look axis is flipped.
---
# Invert Look Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #input

Stores whether one look axis is flipped and passes it to Control Settings, where gameplay code reads it. Use one per axis.

**Add it:** Inspector › **Add Component** › search `Invert Look Setting`.

## How to use it

1. Add one for the vertical axis (most games only need this one) and, if wanted, one for the horizontal axis.
2. Add a **Setting Toggle** per axis. Its key is `InvertLookVertical` or `InvertLookHorizontal`.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Axis** | Look Axis | Vertical | Which axis this one flips. Each axis has its own key and its own toggle. |
| **Default Inverted** | Toggle | Off | Whether the axis is flipped before the player has changed anything. |

## Good to know

- The key follows the axis: `InvertLookVertical` or `InvertLookHorizontal`.

## Related

- [🗞️ Settings Context](Settings%20Context.md)
- [🗞️ Control Settings](Control%20Settings.md)
- [🔖 Look Sensitivity Setting](Look%20Sensitivity%20Setting.md)
- [🔖 Setting Toggle](Setting%20Toggle.md)
- [🗳️ Settings components overview](../Components.md)