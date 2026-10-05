# Unique Id Scriptable Object

✉️ **Scriptable Object** · programmers only

**Tags:** #programmer #workflow

**Create:** `Assets > Create > Scriptable Objects > Base > UniqueId > New ScriptableObject`

**Default file name:** `UID_UniqueIdScriptableObject`

A minimal asset whose only job is to carry a stable, unique ID. Use one wherever you need something to point at that keeps working after renames and moves.

## Fields

The ID is hidden in the Inspector on purpose. It is generated for you and must not be edited by hand, because saved data points at it.

## How to fill it

Run [🔧 Generate Unique Ids](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tools/Documentation~/Tools/Generate%20Unique%20Ids.md). Every asset without a valid ID gets one. Assets that already have one are left untouched.

If the automatic checks are on, this mostly happens by itself. See [🔧 Unique ID Validation](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tools/Documentation~/Tools/Unique%20ID%20Validation.md).

## Notes

- The ID is a GUID, generated once and then kept forever.
- Changing an ID breaks any save file or reference that used the old one.
- Any scriptable object in your own code can join this system by implementing `IUniquelyIdentifiable`.
