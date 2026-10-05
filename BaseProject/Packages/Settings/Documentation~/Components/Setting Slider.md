# Setting Slider

🔖 **Component** · for everyone

A slider, with optional minus and plus buttons and a number readout.

Needs a Unity **Slider** on the same GameObject.

Works with settings that store a **number between 0 and 1**, which in practice means [🔖 Audio Volume Setting](<Audio Volume Setting.md>).

## Inspector fields

Shared by every UI element:

| Field | What to put in it |
| --- | --- |
| **Setting Key** | The key of the setting this controls. Must match exactly. Required. |
| **Title** | Localized name shown in the [🔖 Setting Flavor Text](<Setting Flavor Text.md>) panel. |
| **Description** | Localized explanation shown in the same panel. |

Specific to this element:

| Field | What to put in it |
| --- | --- |
| **Percentage Text** | Optional. Shows the current number, rounded. Leave empty to hide it. |
| **Decrease Button** | Optional minus button. |
| **Increase Button** | Optional plus button. |
| **Button Step** | How much one press of minus or plus moves the slider. Default is `1`. |

## Set up the Slider itself

On the Slider component:

- **Min Value:** `0`
- **Max Value:** `100` if you want the readout to show 0 to 100, or `10` for 0 to 10.
- **Whole Numbers:** on, if you want clean steps.

The saved value is always the slider value divided by Max Value, so the range you pick is purely for display. A Max Value of 100 with the slider at 70 saves `0.7`.

## Good to know

- The readout only shows the raw number, with no `%` sign. Put a separate text object next to it if you want one.
- Do not try to make volume "sound right" by curving the slider. Audio conversion is already handled inside AudioVolumeSetting.

## Setup checklist

- Slider Min Value is 0.
- Max Value matches the number you want players to see.
- Setting Key matches the setting component (for audio, the mixer parameter name).
- Button Step makes sense for your Max Value.
