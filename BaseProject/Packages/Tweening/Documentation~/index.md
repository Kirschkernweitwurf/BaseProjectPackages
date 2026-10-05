# Tweening Package

Animations without code. Put a tween component on an object, set the values, and let a Tween Group play it.

## Components

| Page | What is in it | Key components |
| --- | --- | --- |
| [🔖 Tweening](components/tweening.md) | Every tween component, Tween Group and UI Event Trigger | Tween Group, UI Event Trigger, 20 tween components |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Tween Settings](scriptable-objects/tween-settings.md) | Shared timing: duration, delay, easing, looping | `TS_` | `Base > Tweening > New TweenSettings` |
| [💌 Float Tween Profile](scriptable-objects/float-tween-profile.md) | Values plus timing for fades, alpha and fill amounts | `TPF_` | `Base > Tweening > Profiles > New FloatTweenProfile` |
| [💌 Color Tween Profile](scriptable-objects/color-tween-profile.md) | Values plus timing for tints and text colors | `TPC_` | `Base > Tweening > Profiles > New ColorTweenProfile` |
| [💌 Vector3 Tween Profile](scriptable-objects/vector3-tween-profile.md) | Values plus timing for position, rotation and scale | `TPV_` | `Base > Tweening > Profiles > New Vector3TweenProfile` |

All create paths start with `Create > Scriptable Objects >`.

### Settings or profile?

- A **profile** says _what_ to animate and _how fast_. Pick the one matching the value type.
- A **settings** asset only says _how fast_. Many profiles and tweens can share one.

Use a profile when the same animation repeats across objects. Use a settings asset when the timing should match but the values differ.

## Where to start

| You want to | Go to |
| --- | --- |
| Animate something | [Tweening](components/tweening.md). A tween does nothing until a Tween Group plays it. |
| Add hover or click feedback | [UI Event Trigger](components/tweening.md) |
| Reuse an animation across objects | A [tween profile](scriptable-objects/float-tween-profile.md) or a shared [settings asset](scriptable-objects/tween-settings.md) |

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
