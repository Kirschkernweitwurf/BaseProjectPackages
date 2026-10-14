---
type: component
package: Save System
audience: everyone
tags:
  - designer
  - settings
  - saving
summary: Stores the shortest time allowed between two autosaves and passes it to the Autosave Service.
---
# Autosave Cooldown Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #saving

Stores the shortest time allowed between two autosaves and passes it to the Autosave Service. Optional: most games keep the cooldown a developer decision and only show the interval.

**Add it:** Inspector › **Add Component** › search `Autosave Cooldown Setting`.

## How to use it

1. Put it on its own GameObject in the settings scene, next to the other setting components.
2. Add a **Setting Slider** to the menu and type `AutosaveCooldown` into its **Setting Key**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Config** | Autosave Config |  | **Required.** The Autosave Config asset with the defaults. Use the same asset as the Autosave Service. |

## Good to know

- The key is always `AutosaveCooldown`. You cannot change it, so the UI cannot get out of sync.

## Related

- [🗞️ Settings Context](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Settings%20Context.md)
- [🗞️ Autosave Service](Autosave%20Service.md)
- [🔖 Autosave Interval Setting](Autosave%20Interval%20Setting.md)
- [🗳️ Settings components overview](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components.md)