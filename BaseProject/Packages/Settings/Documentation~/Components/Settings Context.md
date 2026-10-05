# Settings Context

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

The brain of the settings system. Everything else finds it automatically.

Put **one** on a GameObject in your settings scene. Without it, no setting saves and no UI element binds.

## Inspector fields

None. It works out of the box.

## What it does

- Creates the storage (PlayerPrefs by default) and the list of all settings.
- Lets setting components register themselves when they wake.
- **Saves everything automatically when the scene is destroyed.** You do not need a save button.

## Buttons you can wire up

You can call these from a UI Button's OnClick, or from code.

| Method | What it does |
| --- | --- |
| `Save()` | Writes every setting to disk now. |
| `Revert()` | Throws away unsaved changes and goes back to the last saved values. Good for a Cancel button. |
| `ResetToDefaults()` | Puts every setting back to its default. Good for a "Restore defaults" button. |
| `Reload()` | Reads every setting from disk again and reapplies it. |

## Important: load saved values on start

The context does not read saved values by itself. Call `Reload()` **once**, after all setting components have woken up, so the player's saved choices are actually applied.

The easy way: put a small script or a UnityEvent that calls `Reload()` in `Start` on a GameObject that runs after the setting components.

## Setup checklist

- One SettingsContext in the scene.
- It wakes before your setting components (it uses execution order `-98`, so this is usually already true, but do not give your own components an earlier order).
- Something calls `Reload()` once at startup.
- A Cancel button calls `Revert()`, if your menu has one.
