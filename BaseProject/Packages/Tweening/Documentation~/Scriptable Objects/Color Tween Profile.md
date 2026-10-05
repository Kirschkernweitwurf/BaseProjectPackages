# Color Tween Profile

💌 **Scriptable Object** · for everyone

**Menu path:** `Create > Scriptable Objects > Base > Tweening > Profiles > New ColorTweenProfile`

**Default file name:** `TPC_ColorTweenProfile`

## What it does

A complete, reusable animation recipe for color changes: image tints, text colors, and sprite renderer tints. It stores the start color, the target color, the timing, and the loop behavior in one asset.

Use it when several objects should animate exactly the same way, for example a shared "button hover highlight" or "damage flash".

## Fields

| Field | What it does |
| --- | --- |
| **Start Value** | The color the animation begins at |
| **Target Value** | The color the animation moves to |
| **Use Settings Asset** | Take the timing from a shared [Tween Settings](Tween%20Settings.md) asset instead of the fields below |
| **Settings Asset** | The shared timing asset, shown when the toggle above is on |
| **Tween Settings** | Duration, delay, and easing for this profile |
| **Loop Settings** | Loop count and loop type for this profile |

> **Start Value** is ignored by the "To" components (`ImageColorToTween`, `TmpColorToTween`,
> `GraphicColorToTween`, `SpriteRendererColorToTween`). Those always start from whatever color the
> object had when the scene loaded.

## Which components accept it

- Image Color Tween / Image Color To Tween
- Graphic Color Tween / Graphic Color To Tween
- TMP Color Tween / TMP Color To Tween
- Sprite Renderer Color Tween / Sprite Renderer Color To Tween

## How to use it

1. Create the asset and set the colors and timing.
2. On the tween component, tick **Use Profile**.
3. Drop the asset into the **Profile** field.
4. The component's own value and timing fields hide themselves, because the profile drives them.

## Tips

- The alpha channel of the color is animated too. Watch out for accidentally fading things out when you only meant to change the tint.
- For a pure fade, use a Float profile with a Fade Tween instead. That animates a Canvas Group and covers the whole subtree at once.
