---
type: Start Page
package: Tweening
tags:
  - animation
summary: Animations without code.
---
# Tweening Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #animation

Animations without code. Put a tween component on an object, set the values, and let a Tween Group play it.

## Components

Every component has its own page. [🗳️ Components](Components.md) lists them all, the [📖 guide](Components/Tweening.md) explains how they fit together.

### Playing tweens

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Tween Group](Components/Tween%20Group.md) | Plays several tweens as one Show or Hide animation, at once or one after another. | Everyone |
| [🔖 UI Event Trigger](Components/UI%20Event%20Trigger.md) | Plays a Tween Group on hover, click, select or submit, without code. | Everyone |

### Transform tweens

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Position Tween](Components/Position%20Tween.md) | Animates the position of the GameObject from a fixed start to a fixed target. | Everyone |
| [🔖 Position To Tween](Components/Position%20To%20Tween.md) | Animates the position of the GameObject from its current value to a target. | Everyone |
| [🔖 Rotation Tween](Components/Rotation%20Tween.md) | Animates the rotation of the GameObject from a fixed start to a fixed target. | Everyone |
| [🔖 Rotation By Tween](Components/Rotation%20By%20Tween.md) | Animates the rotation of the GameObject by an offset. | Everyone |
| [🔖 Scale Tween](Components/Scale%20Tween.md) | Animates the local scale of the GameObject from a fixed start to a fixed target. | Everyone |
| [🔖 Scale To Tween](Components/Scale%20To%20Tween.md) | Animates the local scale of the GameObject from its current value to a target. | Everyone |

### UI tweens

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Fade Tween](Components/Fade%20Tween.md) | Animates the alpha of a Canvas Group from a fixed start to a fixed target. | Everyone |
| [🔖 Fade To Tween](Components/Fade%20To%20Tween.md) | Animates the alpha of a Canvas Group from its current value to a target. | Everyone |
| [🔖 Image Color Tween](Components/Image%20Color%20Tween.md) | Animates the tint of a UI Image from a fixed start to a fixed target. | Everyone |
| [🔖 Image Color To Tween](Components/Image%20Color%20To%20Tween.md) | Animates the tint of a UI Image from its current value to a target. | Everyone |
| [🔖 Image Fill Amount Tween](Components/Image%20Fill%20Amount%20Tween.md) | Animates the fill amount of a UI Image from a fixed start to a fixed target. | Everyone |
| [🔖 Image Fill Amount To Tween](Components/Image%20Fill%20Amount%20To%20Tween.md) | Animates the fill amount of a UI Image from its current value to a target. | Everyone |
| [🔖 Graphic Color Tween](Components/Graphic%20Color%20Tween.md) | Animates the color of any UI Graphic from a fixed start to a fixed target. | Everyone |
| [🔖 Graphic Color To Tween](Components/Graphic%20Color%20To%20Tween.md) | Animates the color of any UI Graphic from its current value to a target. | Everyone |
| [🔖 TMP Color Tween](Components/TMP%20Color%20Tween.md) | Animates the color of a TextMeshPro text from a fixed start to a fixed target. | Everyone |
| [🔖 TMP Color To Tween](Components/TMP%20Color%20To%20Tween.md) | Animates the color of a TextMeshPro text from its current value to a target. | Everyone |
| [🔖 TMP Alpha Tween](Components/TMP%20Alpha%20Tween.md) | Animates the alpha of a TextMeshPro text from a fixed start to a fixed target. | Everyone |
| [🔖 TMP Alpha To Tween](Components/TMP%20Alpha%20To%20Tween.md) | Animates the alpha of a TextMeshPro text from its current value to a target. | Everyone |

### Renderer tweens

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Sprite Renderer Color Tween](Components/Sprite%20Renderer%20Color%20Tween.md) | Animates the tint of a Sprite Renderer from a fixed start to a fixed target. | Everyone |
| [🔖 Sprite Renderer Color To Tween](Components/Sprite%20Renderer%20Color%20To%20Tween.md) | Animates the tint of a Sprite Renderer from its current value to a target. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Tween Runner](Components/Tween%20Runner.md) | The service that moves every running tween forward each frame. | Programmers |

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
| Animate something | The [📖 Tweening guide](Components/Tweening.md). A tween does nothing until a [Tween Group](Components/Tween%20Group.md) plays it. |
| Add hover or click feedback | [UI Event Trigger](Components/UI%20Event%20Trigger.md) |
| Reuse an animation across objects | A [tween profile](Scriptable%20Objects/Float%20Tween%20Profile.md) or a shared [settings asset](Scriptable%20Objects/Tween%20Settings.md) |

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.