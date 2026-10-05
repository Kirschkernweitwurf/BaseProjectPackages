# Pause Menu Button

Opens and closes the pause menu with one button, and swaps the button icon so it always shows the right symbol.

## Where to put it

On a GameObject that has a **Button** component with an **Image** on it. The icon is set on that Image, so the button needs one.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Pause Menu Identifier | Yes | The menu identifier asset of the pause menu. |
| Pause Icon | Yes | Sprite shown while the game is paused. |
| Play Icon | Yes | Sprite shown while the game is running. |

## Good to know

- The icon also updates when the game is paused some other way, for example by a gamepad button, so it never gets out of sync.
- The correct icon is set on scene start, so you do not have to pick one by hand in the prefab.
