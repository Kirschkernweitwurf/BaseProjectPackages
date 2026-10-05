# Load Game Button

Loads the selected slot and hands the data back to every object that owns save data.

Requires a Unity `Button` on the same GameObject.

## Inspector settings

None. It always uses the currently selected slot.

## Before it works

A slot must be selected first, usually by a [🪛 Select Slot Button](select-slot-button.md). With no selection it logs a warning and does nothing.

## Possible outcomes

The result is written to the console:

| Result | Meaning |
| --- | --- |
| **Success** | Loaded and applied. |
| **NotFound** | The slot is empty. |
| **Corrupt** | The file exists but cannot be read. |
| **VersionTooNew** | The save comes from a newer build of the game. |

If you want the UI to react to these (an error popup, for example), ask a programmer to hook it up.

## Notes

- The button greys out while loading.
- Loading does not reload the scene. Only registered objects restore their state.
