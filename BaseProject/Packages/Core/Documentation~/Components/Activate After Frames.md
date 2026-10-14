---
type: component
package: Core
audience: everyone
tags:
  - designer
  - ui
summary: Turns a GameObject on after a number of frames.
---
# Activate After Frames

🔖 **Component** · for everyone

**Tags:** #designer #ui

Turns a GameObject on after a number of frames. Useful to hide a one-frame layout glitch: if a UI element shows up in the wrong place for a frame, delaying it by one or two frames hides the pop.

**Add it:** Inspector › **Add Component** › search `Activate After Frames`.

## How to use it

1. Add it to a GameObject that stays active, not to the target itself.
2. Drag in the **Target** and set **Frames**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Frames** | Whole number | 1 | Frames to wait. |
| **Target** | GameObject |  | The GameObject to turn on. |

## Related

- [🔖 Activate After Time](Activate%20After%20Time.md)
- [📖 Core utility guide](Utility.md)