# V-Sync Setting

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

Saves the VSync setting. VSync locks the frame rate to the monitor refresh rate to remove screen tearing, at the cost of some input latency.

Key: `VSync`

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Default VSync Count** | 0 to 4. Default is `1`. |

What the numbers mean:

| Value | Result |
| --- | --- |
| 0   | Off |
| 1   | On, one frame per monitor refresh |
| 2   | On, one frame per two refreshes (roughly half the frame rate) |
| 3, 4 | Rarely useful, but supported |

Most games only expose 0 and 1.

## Order matters

Put this **above** [🔖 Quality Level Setting](Quality%20Level%20Setting.md) in the Hierarchy.

## Which UI to use

- **On / off only:** a [🔖 Setting Toggle](Setting%20Toggle.md) will not work directly, because this setting stores a number, not a true/false. Use an [🔖 Int Multiple Choice Element](Int%20Multiple%20Choice%20Element.md) with two options, Off and On.
- **More than two options:** an IntMultipleChoiceElement or a [🔖 Setting Dropdown](Setting%20Dropdown.md).

Set the key to `VSync`.

## Setup checklist

- Sits above QualityLevelSetting in the Hierarchy.
- UI element key is `VSync`.
- Option order matches the numbers above (first option = 0).
