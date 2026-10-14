---
type: Guide
package: Audio
tags:
  - sound
  - designer
  - audio
summary: How sounds are played in our projects, and which Play Audio component to pick.
---
# Audio

📖 **Guide** · for everyone

**Tags:** #sound #designer #audio

How sounds get from a clip to the speakers, and which component to use. Every component has its own page with all fields.

## The rule

Everything plays an [Audio Container](../Scriptable%20Objects/Audio%20Container.md), never a raw clip. If you want to drag a `.wav` somewhere, make a container for it first. The container holds the clips, the volume, the random pitch and the **Audio Type**, which decides the volume slider it listens to.

## Sounds on UI

| You want | Add |
| --- | --- |
| Mouse hover feedback | [🔖 Play Audio On Hover](Play%20Audio%20On%20Hover.md) |
| Keyboard and gamepad navigation feedback | [🔖 Play Audio On Select](Play%20Audio%20On%20Select.md) |
| A confirm sound for every input device | [🔖 Play Audio On Submit](Play%20Audio%20On%20Submit.md) |
| A mouse click only | [🔖 Play Audio On Click](Play%20Audio%20On%20Click.md) |

The usual button: **On Hover** plus **On Select** for the highlight, **On Submit** for the confirm. Do not add On Click and On Submit together, or a mouse click plays twice.

All of them need an Event System in the scene and a Graphic with **Raycast Target** on.

## Behind the scenes

| Component | Job |
| --- | --- |
| [🗞️ Audio Manager](Audio%20Manager.md) | Plays every sound. Code and components ask it. |
| [🗞️ Audio Pool Manager](Audio%20Pool%20Manager.md) | Keeps reusable Audio Sources, one pool per Audio Type. Each pool's prefab sets the mixer group. |

Both sit on the persistent manager prefab. You do not add them to a scene.

## When a sound is wrong

| Problem | Check |
| --- | --- |
| Silent | Is the container field filled in? The Console warns when it is empty. |
| Ignores its volume slider | The mixer group on the Audio Source prefab for that Audio Type, in the Audio Pool Manager. |
| Plays twice on click | On Click and On Submit on the same button. Remove On Click. |
| Clips nobody uses | Run the [Unused Audio Clips](../Tools/Unused%20Audio%20Clips.md) tool. |