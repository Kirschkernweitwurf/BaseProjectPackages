# Setting Toggle

🔖 **Component** · for everyone

**Tags:** #designer #settings #ui

An on / off switch with a label that reads "On" or "Off".

Needs a Unity **Toggle** on the same GameObject. Unity adds one automatically when you add this component.

Works with settings that store a **true/false** value.

## Inspector fields

Shared by every UI element:

| Field | What to put in it |
| --- | --- |
| **Setting Key** | The key of the setting this controls. Must match exactly. Required. |
| **Title** | Localized name shown in the [🔖 Setting Flavor Text](Setting%20Flavor%20Text.md) panel while this element is focused. |
| **Description** | Localized explanation shown in the same panel. |

Specific to this element:

| Field | What to put in it |
| --- | --- |
| **State Text** | The text object that shows On or Off. Required. |
| **On Label** | Localized text for the on state. |
| **Off Label** | Localized text for the off state. |

## Good to know

- Flipping the toggle updates the setting immediately. There is no Apply button.
- If the setting is changed somewhere else, for example by a Reset Defaults button, the toggle updates itself.
- None of the built-in setting components store true/false, so this one is for your own settings, for example Subtitles or Screen Shake.

## Setup checklist

- Setting Key matches the setting component.
- State Text assigned.
- On Label and Off Label point at real localization entries.
