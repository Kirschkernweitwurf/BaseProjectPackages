# Float Tween Profile

💌 **Scriptable Object** · for everyone

**Tags:** #artist #designer #animation

**Menu path:** `Create > Scriptable Objects > Base > Tweening > Profiles > New FloatTweenProfile`

**Default file name:** `TPF_FloatTweenProfile`

## What it does

A complete, reusable animation recipe for anything driven by a single number: fades, alpha values, and image fill amounts. It stores the start value, the target value, the timing, and the loop behavior in one asset.

Use it when several objects should animate exactly the same way.

## Fields

| Field | What it does |
| --- | --- |
| **Start Value** | The number the animation begins at |
| **Target Value** | The number the animation moves to |
| **Use Settings Asset** | Take the timing from a shared [Tween Settings](Tween%20Settings.md) asset instead of the fields below |
| **Settings Asset** | The shared timing asset, shown when the toggle above is on |
| **Tween Settings** | Duration, delay, and easing for this profile |
| **Loop Settings** | Loop count and loop type for this profile |

> **Start Value** is ignored by the "To" components (`FadeToTween`, `ImageFillAmountToTween`,
> `TmpAlphaToTween`). Those always start from whatever value the object had when the scene loaded.

## Which components accept it

- Fade Tween / Fade To Tween
- Image Fill Amount Tween / Image Fill Amount To Tween
- TMP Alpha Tween / TMP Alpha To Tween

## How to use it

1. Create the asset and set the values and timing.
2. On the tween component, tick **Use Profile**.
3. Drop the asset into the **Profile** field.
4. The component's own value and timing fields hide themselves, because the profile drives them.

## Tips

- Reach for a profile when the same animation appears on many objects. For a one-off, just fill in the fields on the component.
- Combine a profile with a shared settings asset to separate "what it animates" from "how fast it animates".
