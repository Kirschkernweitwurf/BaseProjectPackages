---
type: overview
package: Save System
tags:
  - saving
  - ui
summary: All components you add to a GameObject through Add Component.
---
# Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #saving #ui

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

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Buttons

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Select Slot Button](Components/Select%20Slot%20Button.md) | Marks one slot as the active slot. | Everyone |
| [🔖 Save Game Button](Components/Save%20Game%20Button.md) | Writes the current game state into a save slot. | Everyone |
| [🔖 Load Game Button](Components/Load%20Game%20Button.md) | Loads the selected slot and hands the data back to every object that owns save data. | Everyone |
| [🔖 Delete Game Button](Components/Delete%20Game%20Button.md) | Deletes the selected slot, including its data, its screenshot and its metadata. | Everyone |

### Helpers

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Playtime Tracker](Components/Playtime%20Tracker.md) | Optional. | Everyone |
| [🔖 Screen Capturer](Components/Screen%20Capturer.md) | Optional. | Everyone |

### Autosave settings

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Autosave Enabled Setting](Components/Autosave%20Enabled%20Setting.md) | Stores whether autosaving is on and passes it to the Autosave Service. | Everyone |
| [🔖 Autosave Interval Setting](Components/Autosave%20Interval%20Setting.md) | Stores how often a timed autosave happens and passes it to the Autosave Service. | Everyone |
| [🔖 Autosave Cooldown Setting](Components/Autosave%20Cooldown%20Setting.md) | Stores the shortest time allowed between two autosaves and passes it to the Autosave Service. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Autosave Service](Components/Autosave%20Service.md) | Saves on a timer and at checkpoints, never more often than its cooldown allows. | Programmers |
| [🗞️ Save Manager](Components/Save%20Manager.md) | The main component of the save system. | Programmers |
<!-- all-components:end -->