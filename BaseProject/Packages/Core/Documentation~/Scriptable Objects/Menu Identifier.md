# Menu Identifier

💌 **Scriptable Object** · for everyone

**Tags:** #designer #menus

**Menu path:** `Create > Scriptable Objects > Base > Menus > New Menu Identifier`

**Default file name:** `MID_MenuIdentifier`

## What it does

A name tag for a menu. Every menu in the game gets one, and everything that needs to talk about that menu points at this asset instead of using a text string.

The asset itself is empty. Its name and its identity are the whole point.

## How to use it

1. Create one asset per menu, for example `MID_PauseMenu`, `MID_Settings`, `MID_Inventory`.
2. Drag it into the **Menu Identifier** field on the Menu component.
3. Use it anywhere a menu needs to be referenced:
   - the **Blocking Menus** list on a Menu, to stop a menu from opening while another is up
   - the **Default Back Menu** field on the Menu Manager, usually your pause menu

## Rules

- **Names must be unique across the whole project.** Two identifiers with the same name break code generation. You will see an error in the console if this happens.
- Renaming an asset is safe. Scene and prefab references follow the asset, not the name. Code that used the old generated name will need updating, so tell a programmer when you rename one.
- The `MID_` prefix is the convention. Keep it.

## What happens behind the scenes

Creating, renaming, or deleting one of these automatically triggers Regenerate Menu Identifiers, which rebuilds the registry the game uses at runtime. You do not need to do anything.
