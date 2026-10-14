---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - menus
  - ui
  - scenes
summary: A Menu that shows itself while scenes load, with progress bar and spinner.
---
# Loading Screen

🔖 **Component** · for everyone

**Tags:** #designer #menus #ui #scenes

A **Menu** that shows itself automatically while a scene loads and hides again afterwards. Nothing has to open or close it: it listens to the scene loading events.

**Add it:** Inspector › **Add Component** › search `Loading Screen`.

## How to use it

1. Build the loading screen UI and set it up like a [Menu](Menu.md).
2. Optional: add a filled **Image** for progress and a spinning icon.
3. Turn on **Has Minimum Show Time** so fast loads do not flash the screen.
4. Fill **Scenes To Show For** only if some loads should not show it.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Menu Identifier** | [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | **Required.** The Menu Identifier asset that names this menu. Buttons and code open the menu through it. |
| **Content Root** | [Tween Group](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tween%20Group.md) |  | **Required.** The Tween Group that plays the open and close animation. |
| **Priority** | Priority |  | `Low`, `Medium`, `High` or `Critical`. The open menu with the highest priority gets the Back press. |
| **Open On Start** | Toggle | Off | Opens the menu, with animation, when the scene starts. |
| **Listen To On Back Action** | Toggle | On | Lets the menu close itself when the player presses Back (Escape, gamepad B). |
| **Blocking Menus** | List of [Menu Identifier](../Scriptable%20Objects/Menu%20Identifier.md) |  | While one of these menus is open, this menu cannot open. |

### Loading Screen References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Progress Image** | Image |  | **Required.** An Image whose fill amount shows the load progress. Set its Image Type to **Filled**. |
| **Spinner** | Rect Transform |  | **Required.** A RectTransform that rotates while loading. |

### Animation Settings

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Spinner Rotation Speed** | Number | 180 | Rotation speed for the spinner, in degrees per second. |
| **Fill Smooth Speed** | Number | 5 | How fast the progress bar catches up. Lower is smoother but lags more. |

### Minimum Show Time

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Has Minimum Show Time** | Toggle | On | If enabled, the loading screen will stay visible for at least this duration. |
| **Minimum Show Time** | Number | 1 | Minimum time (in seconds) the loading screen must remain visible. |

### Scene Filtering

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Scenes To Show For** | List of Text |  | Only show the loading screen for these scenes. Empty shows it for every scene. |

## Good to know

- The spinner uses unscaled time, so it keeps turning even while the game is paused.
- One second minimum show time is a good start.

## Related

- [🔖 Menu](Menu.md)
- [🗞️ Scene Loading Manager](Scene%20Loading%20Manager.md)
- [🔖 Load Scene Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Load%20Scene%20Button.md)
- [📖 Menus guide](Menus.md)