# Save Game Button

🔖 **Component** · for everyone

Writes the current game state into a save slot.

Requires a Unity `Button` on the same GameObject.

## Inspector settings

| Field | Meaning |
| --- | --- |
| **Force New Slot** | Ignore the selected slot and always create a fresh save. This turns the button into a "New Save" button. |
| **Fixed Slot Index** | Save straight into this numbered slot. Only used with the **Fixed** slot model. `-1` means "use the selected slot". Disabled while Force New Slot is on. |

## Common setups

| You want | Settings |
| --- | --- |
| Overwrite the slot the player picked | Force New Slot off, Fixed Slot Index `-1` |
| A "New Save" button | Force New Slot on |
| A button bolted to slot 2 | Slot Model Fixed, Fixed Slot Index `2` |

## What gets saved with it

- All registered game objects that own save data.
- A screenshot thumbnail, if a [🗞️ Screen Capturer](<Screen Capturer.md>) is in the scene.
- Total play time, if a [🗞️ Playtime Tracker](<Playtime Tracker.md>) is in the scene.
- Time stamp and app version, always.

After saving, the slot it wrote to becomes the selected slot.

## Notes

- The button greys out while saving and comes back when it is done.
- With the Appending model, old saves above the cap are pruned right after the save.
- With the Fixed model and nothing selected, the save is skipped and a warning is logged. Fixed slots cannot be invented on the fly.
