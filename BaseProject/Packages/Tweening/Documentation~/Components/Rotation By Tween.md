---
type: Component
package: Tweening
audience: Everyone
tags:
  - artist
  - designer
  - animation
summary: Animates the rotation of the GameObject by an offset.
---
# Rotation By Tween

🔖 **Component** · for everyone

**Tags:** #artist #designer #animation

Animates the rotation of the GameObject, by an offset in Euler degrees. It adds your value on top of the rotation the object has when the tween starts, so the same tween can be played again and again to keep turning.

**Add it:** Inspector › **Add Component** › search `Rotation By Tween`.

## How to use it

1. Add **Rotation By Tween** and set the values. Leave **Use Profile** off for a one-off animation.
2. Add a **Tween Group** (on this object or a parent) and drag this tween into its **Show** list.
3. Play the group: a **Menu** does it on open and close, a **UI Event Trigger** on hover or click, or tick **Play On Start** on the group.

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
| **Profile** | [Vector 3 Tween Profile](../Scriptable%20Objects/Vector3%20Tween%20Profile.md) |  | The profile asset. Its **target** value is used as the offset; its start value is ignored. |
| **Delta Euler Angles** | Vector3 |  | Rotation delta in Euler degrees. |
| **Use Local Rotation** | Toggle | On | If true, tween the local rotation; otherwise, tween the global rotation. |

## Good to know

- A tween on its own does nothing. Something has to play it, almost always a Tween Group.
- Timing comes from, in order: the profile, the shared settings asset, the fields on this component.
- Tweens run on unscaled time, so they also play while a menu has paused the game.
- Euler angles above 360 are fine. `(0, 0, 720)` spins twice.

## Related

- [🔖 Tween Group](Tween%20Group.md)
- [🔖 UI Event Trigger](UI%20Event%20Trigger.md)
- [🔖 Rotation Tween](Rotation%20Tween.md)
- [📖 Tweening guide](Tweening.md)