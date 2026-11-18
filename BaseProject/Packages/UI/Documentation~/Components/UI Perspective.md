---
type: Component
package: UI
audience: Programmers
tags:
  - programmer
summary: "Tilts a piece of UI in 3D with real perspective, like a card turned in the hand."
---
# UI Perspective

🗞️ **Component** · programmers only

**Tags:** #programmer

Tilts a piece of UI in 3D with real perspective, like a card turned in the hand. An overlay canvas draws everything flat, so a rotation around x or y only squashes the UI; this bends the meshes of every graphic and text below it instead. Each graphic draws only while its side faces the eye, so a card can spin and show a back (a child with a UIPerspectiveGraphic marked as back side). The roll stays on the transform. A child's Pos Z sets its depth: a positive z sits behind the UI and shifts against it while it tilts, for a parallax layer and still shows at its laid out size while flat. Graphics spawned later are picked up the next time the tilt leaves rest.

**Add it:** Inspector › **Add Component** › search `UI Perspective`. Needs **Rect Transform** on the same GameObject. Only one per GameObject.

## How to use it

1. Put it on the root of the UI that should tilt, like a card face.
2. For a back side, add a child image (Raycast Target off) with a **UI Perspective Graphic** and tick **Is Back Side**.
3. From code, set `Tilt` (pitch, yaw in degrees). Zero is flat, a yaw of 180 shows the back. `Twist` turns the UI around its own upright axis before the tilt, for a card lying tipped back on a table that turns over or lifts one long edge off it: with a yaw that would spin around the screen's upright axis instead. A twist of 180 also shows the back. The [New Cards Reveal View](New%20Cards%20Reveal%20View.md) lays its riffle on a table this way.
4. Every other graphic and text below gets its hook added at runtime, nothing else to set up.
5. For a parallax layer, give a child a **Pos Z**: positive sits behind the UI, negative in front. It keeps its laid out size while flat and slides against the rest once the UI tilts. Put it under a **Mask** to keep it inside its window, like the still rays behind an aura's tile ([Aura Tile View](Aura%20Tile%20View.md)).

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Distance** | Number | 3 | How far the eye sits in front of the UI, in heights of this rect. Smaller is a stronger perspective; around 3 looks like a card held in front of you. |

## Related

- [UI Perspective Graphic](UI%20Perspective%20Graphic.md)
- [UI Perspective Text](UI%20Perspective%20Text.md)