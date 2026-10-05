# Select Slot Button

🔖 **Component** · for everyone

**Tags:** #designer #saving #ui

Marks one slot as the active slot. The Save, Load and Delete buttons all act on whatever this button last selected.

Requires a Unity `Button` on the same GameObject.

## Inspector settings

| Field | Meaning |
| --- | --- |
| **Fixed Slot Index** | Which numbered slot this button selects. Only used with the **Fixed** slot model. Leave at `-1` if a menu fills the slot in at runtime. |

## How to use it

**Fixed slot model:** build three rows in your menu and set Fixed Slot Index to 0, 1 and 2. Done, no code needed.

**Appending or Named model:** the slot list is only known at runtime, so a programmer spawns a row per save and calls `SetSlotId(...)` on the button. You just author the row prefab.

## Notes

- Selecting does not load anything. It only highlights which slot is the target.
- If the button has no slot id it logs a warning and does nothing.
