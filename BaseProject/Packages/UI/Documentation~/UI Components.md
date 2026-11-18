---
type: Overview
package: UI
tags:
  - ui
summary: Ready-made components from the Base UI Package.
---
# UI Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #ui

Ready-made components from the **Base UI Package**. Drop them on a GameObject, fill in the fields in the Inspector, done. No code needed.

## How to use

1. Select the GameObject in your scene or prefab.
2. Click **Add Component** and search for the component name.
3. Fill in the fields marked as **required**. Fields marked required show a red warning in the Inspector until they are filled.

Two prefabs are included as a starting point:
`Prefabs/Buttons/BasicTextButton` and `Prefabs/Buttons/BasicImageButton`.

## Buttons

All of these go on a GameObject that already has a Unity **Button** component. They hook themselves up automatically, so you do **not** need to add anything to the button's `OnClick ()` list.

| Component | What it does |
| --- | --- |
| [🔖 Open Menu Button](Components/Open%20Menu%20Button.md) | Opens a menu |
| [🔖 Close Menu Button](Components/Close%20Menu%20Button.md) | Closes a menu |
| [🔖 Menu Tab Button](Components/Menu%20Tab%20Button.md) | Switches between the tabs of a menu |
| [🔖 Pause Menu Button](Components/Pause%20Menu%20Button.md) | Toggles pause and swaps its own icon |
| [🔖 Load Scene Button](Components/Load%20Scene%20Button.md) | Loads a scene |
| [🔖 Open Link On Click](Components/Open%20Link%20On%20Click.md) | Opens a website |

## Confirmation

For actions that should ask "Are you sure?" first.

| Component | What it does |
| --- | --- |
| [🗞️ Confirmation Menu](Components/Confirmation%20Menu.md) | The popup itself |
| [🗞️ Confirmation Service](Components/Confirmation%20Service.md) | One-time scene setup |
| [🔖 Confirmed Load Scene Button](Components/Confirmed%20Load%20Scene%20Button.md) | Loads a scene after confirming |
| [🔖 Confirmed Quit Button](Components/Confirmed%20Quit%20Button.md) | Quits the game after confirming |

## Utility

Small helpers for world-space UI, debugging and builds.

| Component | What it does |
| --- | --- |
| [🔖 Billboard](Components/Billboard.md) | Makes an object face the camera |
| [🔖 Editor Billboard](Components/Editor%20Billboard.md) | Same, but also in the Scene view |
| [🗞️ World Canvas Wrapper](Components/World%20Canvas%20Wrapper.md) | Fixes clicks on world-space canvases |
| [🗞️ FPS Counter](Components/FPS%20Counter.md) | Shows the current framerate |
| [🗞️ Build Version](Components/Build%20Version.md) | Shows the version number |

## Effects

Particles and 3D tilts for screen-space UI, where Unity's own particles and rotations don't reach.

| Component | What it does |
| --- | --- |
| [🔖 UI Sparkles](Components/UI%20Sparkles.md) | Twinkling stars over an area plus bursts on demand |
| [🗞️ UI Particle Field](Components/UI%20Particle%20Field.md) | Hundreds of sprites in one mesh, like fire over the screen |
| [🗞️ UI Perspective](Components/UI%20Perspective.md) | Tilts UI in 3D with real perspective |
| [🗞️ UI Perspective Graphic](Components/UI%20Perspective%20Graphic.md) | Bends one graphic under a UI Perspective |
| [🗞️ UI Perspective Text](Components/UI%20Perspective%20Text.md) | Bends one text under a UI Perspective |

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Buttons

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Open Menu Button](Components/Open%20Menu%20Button.md) | Opens a menu when the button is clicked. | Everyone |
| [🔖 Close Menu Button](Components/Close%20Menu%20Button.md) | Closes a menu when the button is clicked. | Everyone |
| [🔖 Menu Tab Button](Components/Menu%20Tab%20Button.md) | Selects a tab of a Menu Group Module and highlights itself while that tab is open. | Everyone |
| [🔖 Pause Menu Button](Components/Pause%20Menu%20Button.md) | Opens and closes the pause menu with one button, and swaps the button icon so it always shows the right symbol. | Everyone |
| [🔖 Load Scene Button](Components/Load%20Scene%20Button.md) | Loads a scene when the button is clicked. | Everyone |
| [🔖 Open Link On Click](Components/Open%20Link%20On%20Click.md) | Opens a website in the player's browser when the button is clicked. | Everyone |

### Confirmation

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Confirmed Quit Button](Components/Confirmed%20Quit%20Button.md) | Asks the player to confirm, then closes the game. | Everyone |
| [🔖 Confirmed Load Scene Button](Components/Confirmed%20Load%20Scene%20Button.md) | Asks the player to confirm, then loads a scene. | Everyone |
| [🔖 Confirmation Menu](Components/Confirmation%20Menu.md) | The "Are you sure?" popup. | Everyone |

### Helpers

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Billboard](Components/Billboard.md) | Turns an object so it always faces the camera. | Everyone |
| [🔖 Editor Billboard](Components/Editor%20Billboard.md) | Same idea as Billboard, but it also turns while you are working in the Scene view. | Everyone |
| [🔖 World Canvas Wrapper](Components/World%20Canvas%20Wrapper.md) | Hooks a world-space canvas up to the main camera. | Everyone |
| [🔖 Fps Counter](Components/FPS%20Counter.md) | Shows the current framerate on screen. | Everyone |
| [🔖 Build Version](Components/Build%20Version.md) | Shows the version and build number, for example 1.2.3 [47]. | Everyone |

### Effects

| Component | What it does | For |
| --- | --- | --- |
| [🔖 UI Sparkles](Components/UI%20Sparkles.md) | Sparkles for rewarding screens: twinkling stars over an area plus bursts on demand. | Everyone |
| [🗞️ UI Particle Field](Components/UI%20Particle%20Field.md) | Hundreds of small sprites in one mesh, for UI effects too big for UI Sparkles. | Programmers |
| [🗞️ UI Perspective](Components/UI%20Perspective.md) | Tilts a piece of UI in 3D with real perspective. | Programmers |
| [🗞️ UI Perspective Graphic](Components/UI%20Perspective%20Graphic.md) | Bends one graphic into the perspective of its UI Perspective. | Programmers |
| [🗞️ UI Perspective Text](Components/UI%20Perspective%20Text.md) | Bends one text into the perspective of its UI Perspective. | Programmers |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Confirmation Service](Components/Confirmation%20Service.md) | The connection between the confirm buttons and the popup. | Programmers |
<!-- all-components:end -->