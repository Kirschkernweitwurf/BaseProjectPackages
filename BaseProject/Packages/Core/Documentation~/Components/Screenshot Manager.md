---
type: component
package: Core
audience: programmer
tags:
  - programmer
  - debugging
summary: Saves screenshots on F6 (normal size) and F7 (four times larger).
---
# Screenshot Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

Saves a screenshot when the player presses **F6** (screen size) or **F7** (four times the screen size, for marketing shots). Files go to a `Screenshots` folder in the game's save data folder.

**Add it:** Inspector › **Add Component** › search `Screenshot Manager`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.

## Fields

None. Add it and it works.

## Good to know

- The file name holds the game title, the resolution and the time.
- The keys live in the Permanent input map, so they work in every menu.

## Related

- [🗞️ Input Manager](Input%20Manager.md)