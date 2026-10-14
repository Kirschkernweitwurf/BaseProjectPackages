---
type: Component
package: Save System
audience: Everyone
tags:
  - designer
  - settings
  - saving
summary: Stores whether autosaving is on and passes it to the Autosave Service.
---
# Autosave Enabled Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #saving

Stores whether autosaving is on and passes it to the Autosave Service. The default comes from the Autosave Config.

**Add it:** Inspector › **Add Component** › search `Autosave Enabled Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Toggle** to the menu and type `AutosaveEnabled` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Autosave Config |  | **Required.** The Autosave Config asset with the defaults. Use the same asset as the Autosave Service. |

## Good to know

- The key is always `AutosaveEnabled`. You cannot change it, so the UI cannot get out of sync.
- Pair it with Autosave Interval Setting for a toggle next to a slider.

## Related

- [🗞️ Settings Context](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Settings%20Context.md)
- [🗞️ Autosave Service](Autosave%20Service.md)
- [🔖 Autosave Interval Setting](Autosave%20Interval%20Setting.md)
- [🔖 Setting Toggle](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Setting%20Toggle.md)
- [🗳️ Settings components overview](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components.md)