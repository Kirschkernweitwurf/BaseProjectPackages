---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - settings
  - gamepad
summary: Stores whether gamepad rumble is on and passes it to the Rumble Service..
---
# Rumble Enabled Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #gamepad

Stores whether gamepad rumble is on and passes it to the Rumble Service.

**Add it:** Inspector › **Add Component** › search `Rumble Enabled Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Toggle** to the menu and type `RumbleEnabled` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Rumble Config |  | **Required.** The Rumble Config asset with the defaults. Use the same asset as the Rumble Service. |

## Good to know

- The key is always `RumbleEnabled`. You cannot change it, so the UI cannot get out of sync.

## Related

- [🗞️ Settings Context](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Settings%20Context.md)
- [🗞️ Rumble Service](Rumble%20Service.md)
- [🔖 Rumble Intensity Setting](Rumble%20Intensity%20Setting.md)
- [🔖 Setting Toggle](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Setting%20Toggle.md)
- [🗳️ Settings components overview](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Settings%20Components.md)