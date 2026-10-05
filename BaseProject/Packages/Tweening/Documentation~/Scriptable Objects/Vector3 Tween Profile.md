# Vector3 Tween Profile

💌 **Scriptable Object** · for everyone

**Menu path:** `Create > Scriptable Objects > Base > Tweening > Profiles > New Vector3TweenProfile`

**Default file name:** `TPV_Vector3TweenProfile`

## What it does

A complete, reusable animation recipe for anything with three axes: position, rotation, and scale. It stores the start value, the target value, the timing, and the loop behavior in one asset.

Use it when several objects should animate exactly the same way, for example a shared "pop in" or "slide from the left".

## Fields

| Field | What it does |
| --- | --- |
| **Start Value** | The value the animation begins at |
| **Target Value** | The value the animation moves to |
| **Use Settings Asset** | Take the timing from a shared [Tween Settings](<Tween Settings.md>) asset instead of the fields below |
| **Settings Asset** | The shared timing asset, shown when the toggle above is on |
| **Tween Settings** | Duration, delay, and easing for this profile |
| **Loop Settings** | Loop count and loop type for this profile |

> **Start Value** is ignored by the "To" components (`PositionToTween`, `ScaleToTween`). Those
> always start from whatever value the object had when the scene loaded.
>
> **Rotation By Tween** is a special case: it reads **Target Value** as a rotation _offset_ in
> degrees, and ignores **Start Value** entirely.

## Which components accept it

- Position Tween / Position To Tween
- Rotation Tween / Rotation By Tween
- Scale Tween / Scale To Tween

## How to use it

1. Create the asset and set the values and timing.
2. On the tween component, tick **Use Profile**.
3. Drop the asset into the **Profile** field.
4. The component's own value and timing fields hide themselves, because the profile drives them.

## Tips

- Position profiles are risky to share between objects that sit in different places, since the values are absolute. Prefer a "To" component, or Rotation By Tween, when you want something relative.
- For scale, remember that `(1, 1, 1)` is normal size. `(0, 0, 0)` is invisible, which makes a handy "pop in" start value.
