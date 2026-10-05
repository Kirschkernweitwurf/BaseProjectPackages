# Tween Settings

**Menu path:** `Create > Scriptable Objects > Base > Tweening > New TweenSettings`

**Default file name:** `TS_TweenSettings`

## What it does

A shared timing asset. It stores _how long_ and _how_ an animation plays, but not _what_ it animates. Assign it to many tweens at once so you can retune all of them from one place.

Good for house styles like `TS_UISnappy`, `TS_CardFlip`, or `TS_MenuFade`.

## Fields

### Settings

| Field | What it does |
| --- | --- |
| **Duration** | How long the animation takes, in seconds |
| **Delay** | How long to wait before it starts, in seconds |
| **Easing** | The speed curve. See [easings.net](http://easings.net) for what each one looks like. |

### Loop

| Field | What it does |
| --- | --- |
| **Loop Count** | How many _extra_ plays after the first one. `0` plays once. `-1` loops forever. |
| **Loop Type** | `None`, `Restart`, `PingPong`, or `Continue`. See below. |

**Loop types**

- **None** plays once and stops.
- **Restart** snaps back to the start value and plays again.
- **PingPong** alternates forward and backward. Each direction counts as one loop, so
  `Loop Count = 1` gives one forward and one backward play.
- **Continue** plays again from the original start without snapping back visibly.

## How to use it

1. Create the asset and set your timing.
2. On any tween component, tick **Use Settings Asset** and drop this asset into the field.
3. The tween's own duration, delay, easing, and loop fields disappear from the inspector, because the asset drives them now.

## Tips

- Use these for anything that should feel consistent. If every menu fade in the game shares one asset, changing the feel of the whole game is a one-field edit.
- A tween profile can also point at a settings asset, so profiles can share timing too.
- Changing this asset while the game is playing does not update running tweens on its own. A programmer needs to refresh them.
