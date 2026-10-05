# Confirmed Quit Button

Asks the player to confirm, then closes the game.

## Where to put it

On a GameObject that has a **Button** component. The scene also needs a[🔧 Confirmation Service](confirmation-service.md) and a [🔧 Confirmation Menu](confirmation-menu.md).

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Warning Text | Yes | The question shown in the popup. |
| Confirm Text | No  | Label of the confirm button. Empty uses the popup default. |
| Cancel Text | No  | Label of the cancel button. Empty uses the popup default. |

## Good to know

- In the Unity Editor this stops Play mode instead of closing anything, so you can test it safely.
