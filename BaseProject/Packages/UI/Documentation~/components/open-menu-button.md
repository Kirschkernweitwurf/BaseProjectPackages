# Open Menu Button

Opens a menu when the button is clicked.

## Where to put it

On a GameObject that has a **Button** component.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Menu To Open | Yes | The menu identifier asset of the menu you want to open. |
| Parent Menu Identifier | No  | The menu that stays registered as the owner of the opened menu. Use this when the new menu opens _on top of_ the current one and should return to it. |

## Good to know

- If the menu is already open, clicking does nothing.
- Leave **Parent Menu Identifier** empty for normal menus. Only fill it in for sub-menus, like Settings opened from the Pause menu.
