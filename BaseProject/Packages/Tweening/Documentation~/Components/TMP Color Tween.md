---
type: component
package: Tweening
audience: everyone
tags:
  - artist
  - designer
  - animation
  - ui
summary: Animates the color of a TextMeshPro text from a fixed start to a fixed target.
requires:
  - Text (TMP)
---
# TMP Color Tween

🔖 **Component** · for everyone

**Tags:** #artist #designer #animation #ui

Animates the color of a TextMeshPro text. It always plays from the start value you set to the target value you set, so it looks the same every time.

**Add it:** Inspector › **Add Component** › search `TMP Color Tween`. Needs **Text (TMP)** on the same GameObject.

## How to use it

1. Make sure the GameObject has a **Text (TMP)**. The tween fills in its reference itself.
2. Add **TMP Color Tween** and set the values. Leave **Use Profile** off for a one-off animation.
3. Add a **Tween Group** (on this object or a parent) and drag this tween into its **Show** list.
4. Play the group: a **Menu** does it on open and close, a **UI Event Trigger** on hover or click, or tick **Play On Start** on the group.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Use Profile** | Toggle | Off | Take the values **and** the timing from a profile asset. Hides the value and timing fields below. |
| **Use Settings Asset** | Toggle | Off | Take only the timing (duration, delay, easing, looping) from a shared Tween Settings asset. |
| **Settings Asset** | [Tween Settings](../Scriptable%20Objects/Tween%20Settings.md) |  | The shared timing asset. Shown while **Use Settings Asset** is on. |
| **Tween Settings** | Tween Settings |  | Duration, delay and easing when no asset drives them. |
| Tween Settings › **Duration** | Number | 0.5 | How long one play takes, in seconds. |
| Tween Settings › **Delay** | Number | 0 | Seconds to wait before the tween starts. |
| Tween Settings › **Easing** | Easing Type |  | The curve of the motion, for example Out Cubic for a soft stop. |
| **Loop Settings** | Loop Settings |  | How often and how the tween repeats when no asset drives it. |
| Loop Settings › **Loop Count** | Whole number | 0 | Extra plays after the first one. `-1` loops forever. |
| Loop Settings › **Loop Type** | Loop Type |  | `Restart` jumps back to the start, `Yoyo` plays forwards and backwards. |
| **Profile** | [Color Tween Profile](../Scriptable%20Objects/Color%20Tween%20Profile.md) |  | The profile asset with values and timing. Shown while **Use Profile** is on. |
| **Start Color** | Color | White | The starting color to tween from. |
| **Target Color** | Color | White | The target color to tween to. |
| **Text** | Text (TMP) |  | Filled in automatically. The TextMeshPro text that is animated. |

## Good to know

- A tween on its own does nothing. Something has to play it, almost always a Tween Group.
- Timing comes from, in order: the profile, the shared settings asset, the fields on this component.
- Tweens run on unscaled time, so they also play while a menu has paused the game.

## Related

- [🔖 Tween Group](Tween%20Group.md)
- [🔖 UI Event Trigger](UI%20Event%20Trigger.md)
- [🔖 TMP Color To Tween](TMP%20Color%20To%20Tween.md)
- [📖 Tweening guide](Tweening.md)