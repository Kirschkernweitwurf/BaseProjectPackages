---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
summary: Shows several menus inside this menu as tabs, one at a time.
---
# Menu Group Module

🔖 **Component** · for everyone

**Tags:** #designer #menus

Turns a menu into a container of **tabs**. The tabs are normal menus, and only one of them is open at a time: opening one closes the others. The container opens its start tab by itself and closes the tabs again when it closes. Think of the apps on a phone, or the pages of a settings screen.

**Add it:** Inspector › **Add Component** › search `Menu Group Module`.

## How to use it

1. Build every tab as a normal [Menu](Menu.md) with its own identifier. Give each tab the **same Priority** as the container.
2. Put the module on the **same GameObject** as the container Menu. It finds the Menu itself.
3. Drop the tab identifiers into **Tabs**. The order is the order of the next and previous tab actions.
4. Add a [Menu Tab Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Menu%20Tab%20Button.md) for every tab.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Owner Menu** | [Menu](Menu.md) |  | Filled in automatically. The menu that holds the tabs. Filled in from the same GameObject when empty. |
| **Tabs** | List of [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | The menus shown as tabs. The order is the order of Next and Previous. |
| **Default Tab** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | The tab that opens together with the container. Empty uses the first tab. |
| **Remember Last Tab** | Toggle | Off | Reopens on the tab that was open last instead of the default tab. |
| **Back Behavior** | Choice | Close Container | What happens when the open tab is closed, for example by Back. See below. |
| **Wrap Around** | Toggle | On | Next on the last tab goes to the first, Previous on the first goes to the last. |
| **Next Tab Action** | Input action |  | Optional. Selects the next tab, for example a shoulder button. |
| **Previous Tab Action** | Input action |  | Optional. Selects the previous tab. |

### Back Behavior

| Option | Back on a tab |
| --- | --- |
| **Close Container** | Closes the container together with the tab. |
| **Return To Default Tab** | Goes back to the default tab first, like the home screen of a phone. Back on the default tab closes the container. |

## Groups inside groups

A tab can have a Menu Group Module of its own. A Settings tab inside the phone can hold Audio, Video and Controls tabs:

```
Pause Menu - Phone   Menu + Menu Group Module
├─ Quick Access      default tab
├─ Deck
└─ Settings          Menu + Menu Group Module
   ├─ Audio          default tab
   └─ Video
```

- Closing the phone closes Settings, and Settings closes its open tab.
- Back always reaches the deepest open tab first.
- Both groups may use the same next and previous actions. Only the innermost open group reacts, and only while its screen is on top, so a dialog over the tabs keeps them still.

## Good to know

- The old tab finishes its close animation before the new one opens, so two tabs never show at once.
- Clicking fast is safe. The group always ends on the last tab that was picked.
- Opening a tab from anywhere (a Menu Tab Button, an Open Menu Button, code) also opens the container when it is closed.
- A Close Menu Button on a tab acts like Back.
- A tab with a lower Priority than the container never gets Back. The console warns about it.
- The container opens its tabs *embedded*, also a tab opened from outside, so the container and its open tab count as one screen for the [Menu Manager](Menu%20Manager.md). Shortcuts and prompt bars on the container keep working while a tab is open.

## Related

- [🔖 Menu](Menu.md)
- [🔖 Menu Tab Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Menu%20Tab%20Button.md)
- [🔖 Menu Reset Module](Menu%20Reset%20Module.md)
- [📖 Menus guide](Menus.md)