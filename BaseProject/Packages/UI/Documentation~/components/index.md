# Components

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
| [🪛 Open Menu Button](open-menu-button.md) | Opens a menu |
| [🪛 Close Menu Button](close-menu-button.md) | Closes a menu |
| [🪛 Pause Menu Button](pause-menu-button.md) | Toggles pause and swaps its own icon |
| [🪛 Load Scene Button](load-scene-button.md) | Loads a scene |
| [🪛 Open Link On Click](open-link-on-click.md) | Opens a website |

## Confirmation

For actions that should ask "Are you sure?" first.

| Component | What it does |
| --- | --- |
| [🔧 Confirmation Menu](confirmation-menu.md) | The popup itself |
| [🔧 Confirmation Service](confirmation-service.md) | One-time scene setup |
| [🪛 Confirmed Load Scene Button](confirmed-load-scene-button.md) | Loads a scene after confirming |
| [🪛 Confirmed Quit Button](confirmed-quit-button.md) | Quits the game after confirming |

## Utility

Small helpers for world-space UI, debugging and builds.

| Component | What it does |
| --- | --- |
| [🪛 Billboard](billboard.md) | Makes an object face the camera |
| [🪛 Editor Billboard](editor-billboard.md) | Same, but also in the Scene view |
| [🔧 World Canvas Wrapper](world-canvas-wrapper.md) | Fixes clicks on world-space canvases |
| [🔧 FPS Counter](fps-counter.md) | Shows the current framerate |
| [🔧 Build Version](build-version.md) | Shows the version number |
