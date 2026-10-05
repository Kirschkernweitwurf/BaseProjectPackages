# Components

🗳️ **Components** · things you add to a GameObject

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
| [🔖 Open Menu Button](<Components/Open Menu Button.md>) | Opens a menu |
| [🔖 Close Menu Button](<Components/Close Menu Button.md>) | Closes a menu |
| [🔖 Pause Menu Button](<Components/Pause Menu Button.md>) | Toggles pause and swaps its own icon |
| [🔖 Load Scene Button](<Components/Load Scene Button.md>) | Loads a scene |
| [🔖 Open Link On Click](<Components/Open Link On Click.md>) | Opens a website |

## Confirmation

For actions that should ask "Are you sure?" first.

| Component | What it does |
| --- | --- |
| [🗞️ Confirmation Menu](<Components/Confirmation Menu.md>) | The popup itself |
| [🗞️ Confirmation Service](<Components/Confirmation Service.md>) | One-time scene setup |
| [🔖 Confirmed Load Scene Button](<Components/Confirmed Load Scene Button.md>) | Loads a scene after confirming |
| [🔖 Confirmed Quit Button](<Components/Confirmed Quit Button.md>) | Quits the game after confirming |

## Utility

Small helpers for world-space UI, debugging and builds.

| Component | What it does |
| --- | --- |
| [🔖 Billboard](Components/Billboard.md) | Makes an object face the camera |
| [🔖 Editor Billboard](<Components/Editor Billboard.md>) | Same, but also in the Scene view |
| [🗞️ World Canvas Wrapper](<Components/World Canvas Wrapper.md>) | Fixes clicks on world-space canvases |
| [🗞️ FPS Counter](<Components/FPS Counter.md>) | Shows the current framerate |
| [🗞️ Build Version](<Components/Build Version.md>) | Shows the version number |
