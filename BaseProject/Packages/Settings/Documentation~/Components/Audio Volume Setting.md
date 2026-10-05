# Audio Volume Setting

🔖 **Component** · for everyone

Saves the volume of one audio channel and pushes it into your AudioMixer.

**Use one per channel.** Master, Music, SFX, Voice and so on each get their own GameObject with their own AudioVolumeSetting.

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Audio Mixer** | Your AudioMixer asset. Required. |
| **Mixer Parameter** | The name of the exposed parameter on that mixer, for example `MusicVolume`. Required. A dropdown lets you pick from the mixer's exposed parameters. |
| **Default Volume** | Starting volume, 0 to 1. Default is `0.7`. |

## The key

There is no separate key field. **The key is the mixer parameter name.** So if your parameter is `MusicVolume`, type `MusicVolume` into the Setting Key of the slider that controls it.

## Before this works

The parameter has to be exposed on the mixer first:

1. Open your AudioMixer.
2. Select the group you want (for example Music).
3. In the Inspector, right click the **Volume** label and choose **Expose to script**.
4. Open the **Exposed Parameters** dropdown at the top right of the mixer window and rename it to something readable, for example `MusicVolume`.

If you skip this, the dropdown in the component will be empty.

## Good to know

- The saved value is a plain 0 to 1 number. The component converts it to decibels for you, because volume does not sound linear to human ears.
- A value of 0 is true silence, not just very quiet.
- Pair it with a [🔖 Setting Slider](<Setting Slider.md>).

## Setup checklist

- Parameter exposed on the mixer and given a clean name.
- AudioMixer dragged into the component.
- Mixer Parameter picked from the dropdown.
- A SettingSlider somewhere with the same name in its Setting Key.
