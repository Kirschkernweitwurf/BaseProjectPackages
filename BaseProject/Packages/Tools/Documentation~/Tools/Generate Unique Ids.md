# Generate Unique Ids

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Assets > Identifier > Generate Unique IDs`

Gives every scriptable object that needs one a stable ID. Save files and other persisted data point at these IDs, so they must never change once assigned.

## How to use it

Run the menu command. It goes through all scriptable objects in the project and assigns an ID to anything that does not have a valid one yet.

The Console tells you how many were updated.

## What it touches

Only scriptable objects that implement `IUniquelyIdentifiable`, such as the [✉️ Unique Id Scriptable Object](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Utility/Documentation~/Scriptable%20Objects/Unique%20Id%20Scriptable%20Object.md).

Assets that already have a valid ID are left alone. Running the command twice is safe and changes nothing.

## Notes

- IDs are GUIDs and are hidden in the Inspector on purpose. They are not meant to be edited.
- Run it after creating new assets, or let the automatic validation handle it. See [🔧 Unique ID Validation](Unique%20ID%20Validation.md).
