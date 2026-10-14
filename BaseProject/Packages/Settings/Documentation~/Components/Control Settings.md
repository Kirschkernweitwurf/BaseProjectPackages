---
type: component
package: Settings
audience: programmer
tags:
  - programmer
  - settings
  - input
summary: Holds the control choices (sensitivity, invert) so gameplay code reads them from one place.
---
# Control Settings

🗞️ **Component** · programmers only

**Tags:** #programmer #settings #input

Holds the control values the player picked, like look sensitivity and inverted axes, so gameplay code reads them from one place. It does not save anything itself; the setting components push their values in.

**Add it:** Inspector › **Add Component** › search `Control Settings`.

## How to use it

1. Put it on a manager prefab, once.
2. Read it from gameplay code through the service locator.

## Fields

None. Add it and it works.

## Good to know

- Works in scenes without a settings menu too: then it simply keeps the defaults.

## Related

- [🔖 Look Sensitivity Setting](Look%20Sensitivity%20Setting.md)
- [🔖 Invert Look Setting](Invert%20Look%20Setting.md)
- [🔖 Rebind Setting](Rebind%20Setting.md)