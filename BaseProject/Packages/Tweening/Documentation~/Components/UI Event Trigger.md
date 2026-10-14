---
type: Component
package: Tweening
audience: Everyone
tags:
  - artist
  - designer
  - animation
  - ui
summary: Plays a Tween Group on hover, click, select or submit, without code.
requires:
  - Rect Transform
---
# UI Event Trigger

🔖 **Component** · for everyone

**Tags:** #artist #designer #animation #ui

Plays a Tween Group in response to a UI event. The quickest way to add hover and click feedback to buttons without writing code.

**Add it:** Inspector › **Add Component** › search `UI Event Trigger`. Needs **Rect Transform** on the same GameObject.

## How to use it

1. Put it on the UI element that should react, for example a button.
2. Pick the **Event Type**.
3. Drag in the **Tween Group** to play.
4. For mouse **and** gamepad support, add a second trigger: one with `OnHover`, one with `OnSelect`, both pointing at the same group.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Event Type** | EUI Event Type |  | `OnHover` (mouse), `OnSelect` (keyboard and gamepad), `OnClick` or `OnSubmit`. |
| **Tween Group** | [Tween Group](Tween%20Group.md) |  | The group to play. |

## Good to know

- Needs an Event System in the scene and a Graphic with **Raycast Target** on to receive pointer events.
- Click and submit only ever play Show. Use a group whose show tweens return to the start, for example a Yoyo scale punch.

## What each event does

| Event Type | Show is played when | Hide is played when |
| --- | --- | --- |
| **On Hover** | the pointer enters | the pointer leaves |
| **On Select** | the element gets selected | the element gets deselected |
| **On Click** | the pointer clicks | never |
| **On Submit** | the element is submitted (Enter, gamepad A) | never |

## Related

- [🔖 Tween Group](Tween%20Group.md)
- [🔖 Play Audio On Hover](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/Components/Play%20Audio%20On%20Hover.md)
- [🔖 Play Audio On Select](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/Components/Play%20Audio%20On%20Select.md)
- [📖 Tweening guide](Tweening.md)