# Utility

Small helpers that solve one problem each.

## Activate After Time

Turns a GameObject on after a delay. The target is switched **off** immediately when the scene starts, then switched on when the timer runs out.

| Field | What it does |
| --- | --- |
| **Delay** | Seconds to wait |
| **Use Unscaled Time** | Ignore the game speed, so the timer still runs while the game is paused |
| **Target** | The GameObject to turn on |

Typical use: a tutorial hint that appears a few seconds into a scene, or staggering when parts of a UI show up.

Turn on **Use Unscaled Time** for anything inside a menu that pauses the game, otherwise the timer never finishes.

## Activate After Frames

Same idea, but waits a number of frames instead of seconds.

| Field | What it does |
| --- | --- |
| **Frames** | Number of frames to wait |
| **Target** | The GameObject to turn on |

Typical use: working around a first-frame layout problem. If a UI element appears in the wrong place for one frame before Unity's layout system catches up, delaying it by one or two frames hides the pop.

Both components put the target on a **separate GameObject** from themselves. Putting the component on the object it disables would stop the component too.

---

## Tooltip Trigger

Shows a tooltip when the pointer hovers this object, and hides it when the pointer leaves.

| Field | What it does |
| --- | --- |
| **Tooltip Text** | The text to show. Multi-line is supported. |
| **Priority** | `Low`, `Medium`, `High`, or `Critical`. When several tooltips overlap, the highest one wins. |

### Requirements and notes

- Needs a Graphic with **Raycast Target** on, and an Event System in the scene.
- Needs a **Tooltip View** somewhere in the UI. Without one, nothing appears.
- Only one Tooltip Trigger is allowed per GameObject.
- An empty **Tooltip Text** logs a warning and shows nothing.
- The tooltip hides itself if the object gets disabled while hovered, so it will not get stuck on screen.
- Text can also be changed from code, for example to show a live value.

---

The Bootstrapper that used to be listed here is now part of the Services package: [🔖 Bootstrapper](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/components/bootstrapper.md).
