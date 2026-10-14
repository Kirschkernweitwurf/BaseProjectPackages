---
type: component
package: Tweening
audience: everyone
tags:
  - artist
  - designer
  - animation
  - ui
summary: Plays several tweens as one Show or Hide animation, at once or one after another.
---
# Tween Group

🔖 **Component** · for everyone

**Tags:** #artist #designer #animation #ui

**The component you use most.** A tween on its own does nothing until something plays it, and the Tween Group is what plays it. It bundles tweens into a **Show** and a **Hide** animation and plays them all at once or one after another.

**Add it:** Inspector › **Add Component** › search `Tween Group`.

## How to use it

1. Add the tweens you need, for example a Fade To Tween and a Scale To Tween.
2. Add a **Tween Group** and drag the tweens into **Show Tween Behaviors**.
3. Leave **Hide Tween Behaviors** empty: hiding then plays the show tweens backwards.
4. Let something call Show and Hide: a **Menu** (its Content Root), a **UI Event Trigger**, or **Play On Start**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Show Tween Behaviors** | List of Tween Behaviour Base |  | Tweens played when the object shows. Pick them with the component picker. |
| **Hide Tween Behaviors** | List of Tween Behaviour Base |  | Tweens played when the object hides. Leave empty to play the show list backwards. |
| **Play On Start** | Toggle | Off | Plays the show tweens as soon as the object starts. |
| **Sequence Mode** | Sequence Mode |  | `Parallel` plays everything at once, `Sequential` one after another. |

## Good to know

- Only fill in the hide list when the exit animation is really different from the entry played backwards.
- The group turns its GameObject on before it plays, so objects can start disabled in the scene.
- `Parallel` suits most UI. `Sequential` is for staggered reveals.
- Tween Groups can be nested: put a group into another group's list to build longer sequences.

## Related

- [🔖 UI Event Trigger](UI%20Event%20Trigger.md)
- [🔖 Menu](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Core/Documentation~/Components/Menu.md)
- [🔖 Fade To Tween](Fade%20To%20Tween.md)
- [🔖 Scale To Tween](Scale%20To%20Tween.md)
- [📖 Tweening guide](Tweening.md)