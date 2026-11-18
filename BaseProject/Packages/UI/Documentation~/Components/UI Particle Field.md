---
type: Component
package: UI
audience: Programmers
tags:
  - programmer
summary: "Hundreds of small sprites in one mesh, for UI effects too big for UISparkles, like fire over the whole screen."
---
# UI Particle Field

🗞️ **Component** · programmers only

**Tags:** #programmer

Hundreds of small sprites in one mesh, for UI effects too big for UISparkles, like fire over the whole screen. One graphic instead of one Image each, so it costs a single draw call. Runs on scaled time, so it freezes with the date when a menu pauses it.

**Add it:** Inspector › **Add Component** › search `UI Particle Field`. Only one per GameObject.

## How to use it

1. Add it to a stretched UI object and give it a **Sprite**. Use an additive material for fire and light.
2. Call `Emit` from code, in the object's local units. The field moves and removes the particles itself.
3. Set **Stretch** above 1 to make soft dots point where they fly, so they read as flames.

## Fields

### Look

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Sprite** | Sprite |  | **Required.** The sprite every particle draws. |
| **Color Over Life** | Gradient |  | Color and alpha of a particle from birth (left) to death (right). Multiplied by its tint. |

### Motion

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Gravity** | Number |  | Units per second squared a particle falls. 0 leaves it on its course. |
| **Drag** | Number |  | How fast a particle loses its speed. |
| **Sway** | Number |  | Units per second a particle wanders from side to side, so fire flickers instead of rising straight. |
| **Sway Speed** | Number | 1 | How many times a second it wanders from side to side. |
| **Stretch** | Number | 1 | Length of a particle along its motion, relative to its width. Above 1 it points where it flies, so soft dots read as flames. 1 keeps it round and it turns by its spin. |
| **Capacity** | Whole number | 800 | Most particles alive at once. New ones are dropped above it. |

## Good to know

- Use it when an effect needs hundreds of particles. For a few dozen sparkles on a reward screen, [UI Sparkles](UI%20Sparkles.md) is enough.
- It runs on scaled time, so it freezes when a menu pauses the game.