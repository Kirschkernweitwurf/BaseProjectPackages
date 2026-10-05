# Save Manager

🗞️ **Component** · programmers only

The main component of the save system. Add exactly one to a GameObject in your first scene.

On start it builds the whole save system from the settings below and registers itself so every button and savable object can find it. On quit it waits for any running save to finish before shutting down.

## Inspector settings

### Slot Model

How saves are organized. Pick one:

| Value | Behaviour | Good for |
| --- | --- | --- |
| **Fixed** | A set number of numbered slots (`slot_0`, `slot_1`, ...). Empty slots still show up in a menu so the player can pick them. Saving overwrites in place. | Classic console style "3 save slots". |
| **Appending** | Every save creates a new entry. Old ones can be pruned automatically. | Autosaves, run history, roguelike runs. |
| **Named** | Unlimited slots with your own names. Saving overwrites the chosen one. | PC style "name your save". Default. |

| Field | Shown when | Meaning |
| --- | --- | --- |
| **Fixed Slot Count** | Slot Model is Fixed | How many slots exist. Default 3. |
| **Max Appending Saves** | Slot Model is Appending | Oldest saves are deleted above this number. Set 0 for unlimited. Default 20. |

You can switch the model later without changing any UI. The buttons adapt.

### Encryption

| Field | Meaning |
| --- | --- |
| **Encryption** | `Auto` = readable JSON in the editor, encrypted in the build. `On` = always encrypted. `Off` = never. Default Auto. |
| **Encryption Passphrase** | The secret used for encryption. Change it once for your project. |
| **Salt** | Extra input for the encryption key. Change it once for your project. |

Loading always works for both plain and encrypted saves, so switching this setting does not break existing saves.

> Do not change the passphrase or salt after you have shipped. Old saves will no longer load.

### Serialization

| Field | Meaning |
| --- | --- |
| **Pretty Print** | Writes JSON with indentation so you can read it while developing. Turn off for smaller files. |
| **Save Version** | The version number of your save layout. A programmer bumps this when the data changes and adds a migration. Leave it alone otherwise. |

## What happens if it is missing

The save, load, delete and select buttons log a warning and do nothing. Nothing crashes.

## Notes

- Only one SaveManager should exist at a time.
- Settings are read once at startup. Changing them during play mode has no effect.
