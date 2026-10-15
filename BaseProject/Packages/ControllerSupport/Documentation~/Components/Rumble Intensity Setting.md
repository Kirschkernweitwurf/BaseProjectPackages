---
type: Component
package: Controller Support
audience: Everyone
tags:
  - designer
  - settings
  - gamepad
summary: Stores how strong gamepad rumble is and passes it to the Rumble Service, where it scales every rumble..
---
# Rumble Intensity Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #gamepad

Stores how strong gamepad rumble is and passes it to the Rumble Service, where it scales every rumble.

**Add it:** Inspector › **Add Component** › search `Rumble Intensity Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Slider** to the menu and type `RumbleIntensity` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Rumble Config |  | **Required.** The Rumble Config asset with the defaults. Use the same asset as the Rumble Service. |

## Good to know

- The key is always `RumbleIntensity`. You cannot change it, so the UI cannot get out of sync.
- Pair it with Rumble Enabled Setting for a toggle next to a slider.

## Related

- [🗞️ Settings Context](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Settings%20Context.md)
- [🗞️ Rumble Service](Rumble%20Service.md)
- [🔖 Rumble Enabled Setting](Rumble%20Enabled%20Setting.md)
- [🔖 Setting Slider](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Setting%20Slider.md)
- [🗳️ Settings components overview](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Settings%20Components.md)