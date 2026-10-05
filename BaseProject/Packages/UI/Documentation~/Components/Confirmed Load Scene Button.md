# Confirmed Load Scene Button

🔖 **Component** · for everyone

**Tags:** #designer #ui #scenes

Asks the player to confirm, then loads a scene. Use it for "Back to main menu" so nobody loses progress by accident.

## Where to put it

On a GameObject that has a **Button** component. The scene also needs a [🗞️ Confirmation Service](Confirmation%20Service.md) and a [🗞️ Confirmation Menu](Confirmation%20Menu.md).

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Warning Text | Yes | The question shown in the popup. |
| Confirm Text | No  | Label of the confirm button. Empty uses the popup default. |
| Cancel Text | No  | Label of the cancel button. Empty uses the popup default. |
| Scene Name To Load | Yes | The scene to load. Pick it from the dropdown. |

## Good to know

- Custom labels read better than a plain "Confirm". For example: _"Return to_ _menu?"_ with **Leave** and **Stay**.
- If nothing happens on click, check that the scene has a Confirmation Service.
