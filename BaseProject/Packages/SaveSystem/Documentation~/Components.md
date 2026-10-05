# Components

🗳️ **Components** · things you add to a GameObject

All components you add to a GameObject through **Add Component**.

## Required

| Component | What it does |
| --- | --- |
| [SaveManager](Components/Save%20Manager.md) | The brain. One per game, in your first scene. Holds all settings. |

## Save menu UI

Each button needs a Unity `Button` on the same GameObject. They all find the SaveManager by themselves, so no drag and drop wiring is needed.

| Component | What it does |
| --- | --- |
| [SelectSlotButton](Components/Select%20Slot%20Button.md) | Marks one slot as the active one. |
| [SaveGameButton](Components/Save%20Game%20Button.md) | Writes the game into the active slot, or into a new one. |
| [LoadGameButton](Components/Load%20Game%20Button.md) | Loads the active slot. |
| [DeleteGameButton](Components/Delete%20Game%20Button.md) | Deletes the active slot. |

## Optional extras

| Component | What it does |
| --- | --- |
| [ScreenCapturer](Components/Screen%20Capturer.md) | Adds a screenshot thumbnail to every save. |
| [PlaytimeTracker](Components/Playtime%20Tracker.md) | Counts play time and stores it in the save. |

## Example

| Component | What it does |
| --- | --- |
| PlayerSaveHandler | Sample of how a programmer makes an object savable. Reference only. |

## Typical scene setup

```text
Bootstrap (GameObject)
  - SaveManager
  - ScreenCapturer      (optional)
  - PlaytimeTracker     (optional)

SaveMenu Canvas
  SlotRow_0
    - Button + SelectSlotButton
  SlotRow_1
    - Button + SelectSlotButton
  - Button + SaveGameButton
  - Button + LoadGameButton
  - Button + DeleteGameButton
```

## Good to know

- Buttons disable themselves while they work, so double clicks are safe.
- If nothing is selected, Load and Delete do nothing and log a warning.
- The SaveManager must exist before the buttons are clicked. Put it in your first scene.
