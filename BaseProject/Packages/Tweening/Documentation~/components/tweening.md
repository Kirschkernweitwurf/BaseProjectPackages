# Tweening

Components that animate a single property over time. You add them to a GameObject, set a start and target value, and something else triggers them.

## The naming rule

Every tween comes in two or three flavors. The suffix tells you where the animation starts.

| Suffix | Behavior | Use it when |
| --- | --- | --- |
| **Tween** | Fixed start to fixed target. Both values are authored by you. | You want the exact same animation every time |
| **ToTween** | Starts from whatever the object had at scene load, moves to your target. | You want to animate from wherever the object currently sits |
| **ByTween** | Applies your value as an _offset_ from the current value. | You want a relative nudge, like "rotate 90 degrees more" |

Example: `FadeTween` always goes from `0` to `1`. `FadeToTween` goes from the Canvas Group's current alpha to `1`.

## Shared fields

Every tween component has these.

| Field | What it does |
| --- | --- |
| **Use Profile** | Take values _and_ timing from a profile asset |
| **Profile** | The profile asset, shown when the toggle above is on |
| **Use Settings Asset** | Take only the timing from a shared Tween Settings asset |
| **Settings Asset** | The shared timing asset, shown when the toggle above is on |
| **Tween Settings** | Duration, delay, and easing, when no asset drives them |
| **Loop Settings** | Loop count and loop type, when no asset drives them |

The inspector only shows fields that are actually in use. Ticking **Use Profile** hides the value and timing fields, because the profile now owns them.

**Priority order:** profile beats settings asset beats the fields on the component.

## The component list

### Transform

| Component | Animates | Extra fields |
| --- | --- | --- |
| **Position Tween** | Position, fixed start to fixed target | Initial Position, Target Position, Use Local Position |
| **Position To Tween** | Position, current to target | Target Position, Use Local Position |
| **Rotation Tween** | Rotation in Euler degrees, fixed to fixed | Start Euler Angles, Target Euler Angles, Use Local Rotation |
| **Rotation By Tween** | Rotation by an offset from wherever it is now | Delta Euler Angles, Use Local Rotation |
| **Scale Tween** | Local scale, fixed to fixed | Start Scale, Target Scale |
| **Scale To Tween** | Local scale, current to target | Target Scale |

### UI

| Component | Animates | Requires | Extra fields |
| --- | --- | --- | --- |
| **Fade Tween** | Canvas Group alpha, fixed to fixed | Canvas Group | Start Alpha, Target Alpha |
| **Fade To Tween** | Canvas Group alpha, current to target | Canvas Group | Target Alpha |
| **Image Color Tween** | Image tint, fixed to fixed | Image | Start Color, Target Color |
| **Image Color To Tween** | Image tint, current to target | Image | Target Color |
| **Image Fill Amount Tween** | Image fill, fixed to fixed | Image | Start Fill Amount, Target Fill Amount |
| **Image Fill Amount To Tween** | Image fill, current to target | Image | Target Fill Amount |
| **Graphic Color Tween** | Any UI Graphic tint, fixed to fixed | Graphic | Start Color, Target Color |
| **Graphic Color To Tween** | Any UI Graphic tint, current to target | Graphic | Target Color |
| **TMP Color Tween** | TextMeshPro color, fixed to fixed | TMP Text | Start Color, Target Color |
| **TMP Color To Tween** | TextMeshPro color, current to target | TMP Text | Target Color |
| **TMP Alpha Tween** | TextMeshPro alpha, fixed to fixed | TMP Text | Start Alpha, Target Alpha |
| **TMP Alpha To Tween** | TextMeshPro alpha, current to target | TMP Text | Target Alpha |

### Renderer

| Component | Animates | Requires | Extra fields |
| --- | --- | --- | --- |
| **Sprite Renderer Color Tween** | Sprite tint, fixed to fixed | Sprite Renderer | Start Color, Target Color |
| **Sprite Renderer Color To Tween** | Sprite tint, current to target | Sprite Renderer | Target Color |

The required component and its reference field are filled in for you when you add the tween.

---

## Tween Group

**This is the component you will use most.** A tween on its own does nothing until something plays it. The Tween Group is what plays it.

It bundles several tweens together and gives you a **Show** and a **Hide**.

### Fields

| Field | What it does |
| --- | --- |
| **Show Tween Behaviours** | The tweens played when the object shows |
| **Hide Tween Behaviours** | The tweens played when the object hides |
| **Play On Start** | Automatically play the show tweens when the object starts |
| **Sequence Mode** | `Parallel` plays everything at once, `Sequential` plays them one after another |

### How to use it

1. Add your tween components to the GameObject, for example a Fade To Tween and a Scale To Tween.
2. Add a Tween Group.
3. Drag both tweens into **Show Tween Behaviours**.
4. Something calls **Show()** or **Hide()**, usually a Menu or a UI Event Trigger.

### Good to know

- If **Hide Tween Behaviours** is empty, hiding plays the show tweens **backwards**. This is usually what you want, and it saves you setting up a second list. Only fill in the hide list when the exit animation is genuinely different.
- The group sets the GameObject active before it plays, so you can leave objects disabled in the scene and let the group turn them on.
- `Parallel` is right for most UI. `Sequential` is right for staggered reveals.

---

## UI Event Trigger

Plays a Tween Group in response to a UI event. This is the quickest way to add hover and click feedback without writing any code.

### Fields

| Field | What it does |
| --- | --- |
| **Event Type** | `OnHover`, `OnClick`, `OnSelect`, or `OnSubmit` |
| **Tween Group** | The group to play |

### Behavior per event type

| Event Type | Show is called on | Hide is called on |
| --- | --- | --- |
| **OnHover** | Pointer enters | Pointer exits |
| **OnClick** | Pointer clicks | never |
| **OnSelect** | Element gets selected | Element gets deselected |
| **OnSubmit** | Element is submitted (Enter or gamepad A) | never |

### Tips

- `OnHover` is for mouse. `OnSelect` is for keyboard and gamepad navigation. Add **both** if your game supports both, each pointing at the same group.
- Needs an Event System in the scene and a Graphic with **Raycast Target** on to receive events.
