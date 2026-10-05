# Tweening Package

📦 **Package** · reusable, works in any of our projects

Animations without code. Put a tween component on an object, set the values, and let a Tween Group play it.

## Components

| Page | What is in it | Key components |
| --- | --- | --- |
| [🔖 Tweening](Components/Tweening.md) | Every tween component, Tween Group and UI Event Trigger | Tween Group, UI Event Trigger, 20 tween components |

## Scriptable Objects

| Page | What it is | Prefix | Create path |
| --- | --- | --- | --- |
| [💌 Tween Settings](Scriptable%20Objects/Tween%20Settings.md) | Shared timing: duration, delay, easing, looping | `TS_` | `Base > Tweening > New TweenSettings` |
| [💌 Float Tween Profile](Scriptable%20Objects/Float%20Tween%20Profile.md) | Values plus timing for fades, alpha and fill amounts | `TPF_` | `Base > Tweening > Profiles > New FloatTweenProfile` |
| [💌 Color Tween Profile](Scriptable%20Objects/Color%20Tween%20Profile.md) | Values plus timing for tints and text colors | `TPC_` | `Base > Tweening > Profiles > New ColorTweenProfile` |
| [💌 Vector3 Tween Profile](Scriptable%20Objects/Vector3%20Tween%20Profile.md) | Values plus timing for position, rotation and scale | `TPV_` | `Base > Tweening > Profiles > New Vector3TweenProfile` |

All create paths start with `Create > Scriptable Objects >`.

### Settings or profile?

- A **profile** says _what_ to animate and _how fast_. Pick the one matching the value type.
- A **settings** asset only says _how fast_. Many profiles and tweens can share one.

Use a profile when the same animation repeats across objects. Use a settings asset when the timing should match but the values differ.

## Where to start

| You want to | Go to |
| --- | --- |
| Animate something | [Tweening](Components/Tweening.md). A tween does nothing until a Tween Group plays it. |
| Add hover or click feedback | [UI Event Trigger](Components/Tweening.md) |
| Reuse an animation across objects | A [tween profile](Scriptable%20Objects/Float%20Tween%20Profile.md) or a shared [settings asset](Scriptable%20Objects/Tween%20Settings.md) |

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
