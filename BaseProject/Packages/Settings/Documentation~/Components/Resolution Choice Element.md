# Resolution Choice Element

🔖 **Component** · for everyone

The arrow picker for screen resolution. It fills its own option list with the resolutions the player's monitor actually supports, so you never type them in.

Pairs with [🔖 Resolution Setting](Resolution%20Setting.md). Set the Setting Key to `Resolution`.

## Inspector fields

The same as [🔖 String Multiple Choice Element](String%20Multiple%20Choice%20Element.md), which it is built on:

| Field | What to put in it |
| --- | --- |
| **Setting Key** | `Resolution` |
| **Title** | Localized name shown in the flavor text panel. |
| **Description** | Localized explanation. |
| **Left Button** | Required. |
| **Right Button** | Required. |
| **Value Text** | Required. |
| **Selection Indicator Prefab** | Required. |
| **Selection Indicator Parent** | Required. |
| **Options** | **Leave empty.** It is overwritten at runtime. |

## Good to know

- Options look like `1920x1080`, highest first.
- Duplicates are removed, so a monitor that reports the same resolution at several refresh rates only shows up once.
- The dot row can get long. A monitor can easily report 15 or more resolutions. Consider hiding the indicator row for this one element by making the indicator prefab invisible, or give the parent a layout group that can handle the width.
- Test in a build. Editor resolution lists are not what players will see.

## Setup checklist

- Setting Key is `Resolution`.
- Options list is empty.
- Indicator row has room for many dots, or is hidden.
- Checked in a real build.
