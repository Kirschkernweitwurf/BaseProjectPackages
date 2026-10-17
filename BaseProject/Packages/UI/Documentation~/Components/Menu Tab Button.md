---
type: Component
package: UI
audience: Everyone
tags:
  - designer
  - ui
  - menus
summary: Selects a tab of a Menu Group Module and highlights itself while that tab is open.
---
# Menu Tab Button

🔖 **Component** · for everyone

**Tags:** #designer #ui #menus

Selects one tab of a [Menu Group Module](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu%20Group%20Module.md) when the button is clicked. An optional indicator shows which tab is selected.

## Where to put it

On a GameObject that has a **Button** component.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Group | Yes | The Menu Group Module on the menu that holds the tabs. |
| Tab | Yes | The menu identifier of the tab this button selects. |
| Selected Indicator | No  | A GameObject that is on while this tab is selected, for example a highlight or an underline. |

## Good to know

- If the menu that holds the tabs is closed, clicking opens it right on this tab.
- The button does not need to sit inside the tab. It usually sits in a tab bar of the container.

## Related

- [🔖 Menu Group Module](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu%20Group%20Module.md)
- [🔖 Open Menu Button](Open%20Menu%20Button.md)