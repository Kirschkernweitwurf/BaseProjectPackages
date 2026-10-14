---
type: component
package: Settings
audience: everyone
tags:
  - designer
  - programmer
  - settings
  - input
summary: Saves every key and button the player has rebound, for one input actions asset.
---
# Rebind Setting

🔖 **Component** · for everyone

**Tags:** #designer #programmer #settings #input

Saves every rebinding of one Input Action asset in a single setting. One setting covers the whole asset, so adding another rebind row later needs no extra work.

**Add it:** Inspector › **Add Component** › search `Rebind Setting`.

## How to use it

1. Put it in the settings scene and assign the **Action Asset** the game plays with.
2. Add a **Rebind Button** per action the player may change. They all use the key `Rebinds`.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Action Asset** | Input Action Asset |  | The Input Action asset the rebinds apply to. |

## Good to know

- If the project plays with a generated input wrapper, the wrapper uses its own copy of the asset. A programmer then has to subclass this component and hand it that copy, otherwise rebinds do nothing in game.
- The key is always `Rebinds`.

## Related

- [🗞️ Settings Context](Settings%20Context.md)
- [🔖 Rebind Button](Rebind%20Button.md)
- [🗞️ Control Settings](Control%20Settings.md)
- [🗳️ Settings components overview](../Components.md)