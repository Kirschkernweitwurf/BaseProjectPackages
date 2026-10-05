# Quality Level Setting

🔖 **Component** · for everyone

Saves the graphics quality level (Low, Medium, High, and so on).

Key: `Quality`

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Default Quality Level** | Which quality level to use on first run. Leave at `-1` to use whatever Unity already has selected. |

The levels themselves are defined in **Project Settings > Quality**, not here. Counting starts at 0 with the topmost level in that window.

## What gets saved

The position in Unity's quality level list. If you reorder levels in Project Settings after release, saved values will point at a different level.

## Order matters

Put this **after** [🔖 V Sync Setting](<V-Sync Setting.md>) in the Hierarchy. Changing quality level in Unity can silently overwrite VSync. This component restores VSync afterwards, but it needs VSync to have been loaded first.

## Which UI to use

An [🔖 Int Multiple Choice Element](<Int Multiple Choice Element.md>) or a[🔖 Setting Dropdown](<Setting Dropdown.md>) with key `Quality`.

Your option labels must be in the **same order** as the levels in Project Settings > Quality. There is no automatic sync, so if a programmer adds a quality level, tell them to update the menu too.

## Setup checklist

- Option labels match Project Settings > Quality, in the same order.
- Sits below VSyncSetting in the Hierarchy.
- UI element key is `Quality`.
