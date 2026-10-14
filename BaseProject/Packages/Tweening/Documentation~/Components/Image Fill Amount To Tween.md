---
type: component
package: Tweening
audience: everyone
tags:
  - artist
  - designer
  - animation
  - ui
summary: Animates the fill amount of a UI Image from its current value to a target.
requires:
  - Image
---
# Image Fill Amount To Tween

🔖 **Component** · for everyone

**Tags:** #artist #designer #animation #ui

Animates the fill amount of a UI Image, for bars and radial timers. It starts from whatever value the object has when the scene loads (captured in Awake) and moves to the target you set. Use it when the start depends on how the object was placed in the scene.

**Add it:** Inspector › **Add Component** › search `Image Fill Amount To Tween`. Needs **Image** on the same GameObject.

## How to use it

1. Make sure the GameObject has a **Image**. The tween fills in its reference itself.
2. Add **Image Fill Amount To Tween** and set the values. Leave **Use Profile** off for a one-off animation.
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
| **Profile** | [Float Tween Profile](../Scriptable%20Objects/Float%20Tween%20Profile.md) |  | The profile asset with values and timing. Shown while **Use Profile** is on. |
| **Target Fill Amount** | Number | 1 | The target fill amount to tween to. |
| **Image** | Image |  | Filled in automatically. The Image that is animated. |

## Good to know

- A tween on its own does nothing. Something has to play it, almost always a Tween Group.
- Timing comes from, in order: the profile, the shared settings asset, the fields on this component.
- Tweens run on unscaled time, so they also play while a menu has paused the game.
- The start value is read once in Awake. Moving the object later in play mode does not change where the tween starts.
- The Image must have **Image Type** set to **Filled**, otherwise the fill amount does nothing.

## Related

- [🔖 Tween Group](Tween%20Group.md)
- [🔖 UI Event Trigger](UI%20Event%20Trigger.md)
- [🔖 Image Fill Amount Tween](Image%20Fill%20Amount%20Tween.md)
- [📖 Tweening guide](Tweening.md)