# Delete Game Button

🔖 **Component** · for everyone

**Tags:** #designer #saving #ui

Deletes the selected slot, including its data, its screenshot and its metadata.

Requires a Unity `Button` on the same GameObject.

## Inspector settings

None. It always uses the currently selected slot.

## Behaviour

- With no slot selected it logs a warning and does nothing.
- After deleting, the selection is cleared, so nothing is targeted until the player picks another slot.
- Deletion is immediate and permanent.

## Notes

There is no built in confirmation dialog. If you want an "Are you sure?" popup, put your own popup in front and let its confirm button trigger this one.
