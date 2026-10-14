---
type: component
package: Save System
audience: everyone
tags:
  - designer
  - settings
  - saving
summary: Stores how often a timed autosave happens and passes it to the Autosave Service.
---
# Autosave Interval Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #saving

Stores how often a timed autosave happens and passes it to the Autosave Service. The value is in seconds; a menu that shows minutes converts it for display.

**Add it:** Inspector › **Add Component** › search `Autosave Interval Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Slider** to the menu and type `AutosaveInterval` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Autosave Config |  | **Required.** The Autosave Config asset with the defaults. Use the same asset as the Autosave Service. |

## Good to know

- The key is always `AutosaveInterval`. You cannot change it, so the UI cannot get out of sync.
- Pair it with Autosave Enabled Setting.

## Related

- [🗞️ Settings Context](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Settings%20Context.md)
- [🗞️ Autosave Service](Autosave%20Service.md)
- [🔖 Autosave Enabled Setting](Autosave%20Enabled%20Setting.md)
- [🔖 Setting Slider](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Setting%20Slider.md)
- [🗳️ Settings components overview](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components.md)