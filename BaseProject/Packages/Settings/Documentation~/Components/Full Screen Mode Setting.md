# Full Screen Mode Setting

🔖 **Component** · for everyone

Saves whether the game runs windowed, borderless or exclusive fullscreen.

Key: `FullScreen`

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Available Modes** | The list of modes you want to offer, in the order they appear in the menu. Cannot be empty. |
| **Default Index** | Which entry in that list is used on first run. Counting starts at 0. Default is `1`. |

The default list is:

| Index | Mode | What players call it |
| --- | --- | --- |
| 0   | Exclusive Full Screen | Fullscreen |
| 1   | Full Screen Window | Borderless |
| 2   | Windowed | Windowed |

Remove any you do not want to support. On Mac and Linux, Exclusive Full Screen behaves the same as Full Screen Window, so many projects drop it.

## What gets saved

The **position in your list**, not the mode itself. If you reorder the list after players have saved a value, their choice will point at a different mode. Decide the order early and leave it alone.

## Order matters

Put this **before** [🔖 Resolution Setting](Resolution%20Setting.md) in the Hierarchy. The mode has to be set before the resolution is applied or the resolution can come out wrong on startup.

## Which UI to use

An [🔖 Int Multiple Choice Element](Int%20Multiple%20Choice%20Element.md) or a [🔖 Setting Dropdown](Setting%20Dropdown.md), with Setting Key set to `FullScreen`.

Type the option labels yourself and make sure they are in the **same order** as the Available Modes list.

## Setup checklist

- Modes list matches the labels in your UI element, in the same order.
- Sits above ResolutionSetting in the Hierarchy.
- UI element key is `FullScreen`.
