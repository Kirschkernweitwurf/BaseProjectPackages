---
type: Guide
package: Tweening
tags:
  - artist
  - designer
  - animation
summary: How tweens, Tween Groups and UI Event Triggers fit together, and which tween to pick.
---
# Tweening

📖 **Guide** · for everyone

**Tags:** #artist #designer #animation

How the tweening components fit together. Every component has its own page with all fields; this page helps you pick the right one.

## The three parts

| Part | Job | Page |
| --- | --- | --- |
| **A tween** | Animates one value, for example alpha or scale | One page per tween, see the table below |
| **A Tween Group** | Plays several tweens as one Show or Hide animation | [🔖 Tween Group](Tween%20Group.md) |
| **Something that plays the group** | A Menu, a UI Event Trigger, or Play On Start | [🔖 UI Event Trigger](UI%20Event%20Trigger.md) |

A tween on its own does nothing. **Always put it in a Tween Group.**

## The naming rule

Every tween comes in two flavors. The suffix tells you where the animation starts.

| Suffix | Starts from | Use it when |
| --- | --- | --- |
| **Tween** | A start value you set | The animation should look the same every time |
| **ToTween** | The value the object has when the scene loads | The start depends on how the object was placed |
| **ByTween** | The current value, plus your offset | You want a relative nudge, like "turn 90 degrees more" |

Example: [Fade Tween](Fade%20Tween.md) always goes from `0` to `1`. [Fade To Tween](Fade%20To%20Tween.md) goes from the Canvas Group's alpha in the scene to `1`.

## Which tween animates what

### Transform

| Animates | Fixed start | From current |
| --- | --- | --- |
| Position | [🔖 Position Tween](Position%20Tween.md) | [🔖 Position To Tween](Position%20To%20Tween.md) |
| Rotation | [🔖 Rotation Tween](Rotation%20Tween.md) | [🔖 Rotation By Tween](Rotation%20By%20Tween.md) (offset) |
| Scale | [🔖 Scale Tween](Scale%20Tween.md) | [🔖 Scale To Tween](Scale%20To%20Tween.md) |

### UI

| Animates | Needs | Fixed start | From current |
| --- | --- | --- | --- |
| Fade a whole UI branch | Canvas Group | [🔖 Fade Tween](Fade%20Tween.md) | [🔖 Fade To Tween](Fade%20To%20Tween.md) |
| Image tint | Image | [🔖 Image Color Tween](Image%20Color%20Tween.md) | [🔖 Image Color To Tween](Image%20Color%20To%20Tween.md) |
| Image fill (bars, timers) | Image | [🔖 Image Fill Amount Tween](Image%20Fill%20Amount%20Tween.md) | [🔖 Image Fill Amount To Tween](Image%20Fill%20Amount%20To%20Tween.md) |
| Any UI Graphic tint | Graphic | [🔖 Graphic Color Tween](Graphic%20Color%20Tween.md) | [🔖 Graphic Color To Tween](Graphic%20Color%20To%20Tween.md) |
| TextMeshPro color | Text (TMP) | [🔖 TMP Color Tween](TMP%20Color%20Tween.md) | [🔖 TMP Color To Tween](TMP%20Color%20To%20Tween.md) |
| TextMeshPro alpha | Text (TMP) | [🔖 TMP Alpha Tween](TMP%20Alpha%20Tween.md) | [🔖 TMP Alpha To Tween](TMP%20Alpha%20To%20Tween.md) |

### Renderer

| Animates | Needs | Fixed start | From current |
| --- | --- | --- | --- |
| Sprite tint | Sprite Renderer | [🔖 Sprite Renderer Color Tween](Sprite%20Renderer%20Color%20Tween.md) | [🔖 Sprite Renderer Color To Tween](Sprite%20Renderer%20Color%20To%20Tween.md) |

The needed component and its reference are filled in for you when you add the tween.

## Fields every tween shares

| Field | What it does |
| --- | --- |
| **Use Profile** | Take values **and** timing from a profile asset |
| **Use Settings Asset** | Take only the timing from a shared [Tween Settings](../Scriptable%20Objects/Tween%20Settings.md) asset |
| **Tween Settings** | Duration, delay and easing when no asset drives them |
| **Loop Settings** | Loop count and loop type when no asset drives them |

**Priority:** profile beats settings asset beats the fields on the component. The Inspector hides whatever is not in use.

## Recipes

| You want | Do this |
| --- | --- |
| A menu that fades in | Canvas Group + [Fade To Tween](Fade%20To%20Tween.md) + [Tween Group](Tween%20Group.md), the group as the Menu's Content Root |
| A button that grows on hover | [Scale To Tween](Scale%20To%20Tween.md) + Tween Group + two [UI Event Triggers](UI%20Event%20Trigger.md) (`OnHover` and `OnSelect`) |
| A pulsing icon | [Scale Tween](Scale%20Tween.md) with Loop Count `-1` and Loop Type `Yoyo`, group on Play On Start |
| The same animation on many objects | A [Vector3](../Scriptable%20Objects/Vector3%20Tween%20Profile.md), [Float](../Scriptable%20Objects/Float%20Tween%20Profile.md) or [Color](../Scriptable%20Objects/Color%20Tween%20Profile.md) tween profile |
| A staggered reveal | Tween Group with Sequence Mode `Sequential` |

## Also in this package

- [🗞️ Tween Runner](Tween%20Runner.md): the service that moves every tween. It sits on the manager prefab.
- [📖 Tweening in Code](Tweening%20in%20Code.md): tweens, timelines and effects from code, as a replacement for DOTween.
