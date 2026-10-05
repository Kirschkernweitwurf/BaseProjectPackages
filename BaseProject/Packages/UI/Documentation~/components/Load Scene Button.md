# Load Scene Button

🔖 **Component** · for everyone

Loads a scene when the button is clicked. Everything currently loaded is unloaded first.

## Where to put it

On a GameObject that has a **Button** component.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Scene Name To Load | Yes | The scene to load. Pick it from the dropdown. |

## Good to know

- The dropdown only lists scenes that are in the Build Settings. If your scene is missing, add it there first.
- Loading happens in the background, so the game does not freeze.
- For a scene change that should ask the player first, use[🔖 Confirmed Load Scene Button](<Confirmed Load Scene Button.md>).
