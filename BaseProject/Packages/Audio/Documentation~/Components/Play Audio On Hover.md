---
type: Component
package: Audio
audience: Everyone
tags:
  - sound
  - designer
  - audio
  - ui
summary: Plays an Audio Container when the mouse pointer enters the object.
---
# Play Audio On Hover

🔖 **Component** · for everyone

**Tags:** #sound #designer #audio #ui

Plays an Audio Container when the mouse pointer enters the object. Mouse only. Add Play Audio On Select next to it for keyboard and gamepad navigation.

**Add it:** Inspector › **Add Component** › search `Play Audio On Hover`.

## How to use it

1. Create an **Audio Container** for the sound (never drag a raw clip anywhere).
2. Add this component to the UI element, usually a button.
3. Drag the container into **Hover Sound**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Hover Sound** | [Audio Container](../Scriptable%20Objects/Audio%20Container.md) |  | **Required.** The Audio Container to play. |

## Good to know

- The Audio Manager is looked up on the first sound, so an element that gets its event in the frame it turns on, like the first row of a page that is selected right away, plays without an error.
- Needs an Event System in the scene and a Graphic with **Raycast Target** on.
- An empty sound field logs a warning and stays silent.
- Usual button setup: **On Hover** + **On Select** for the highlight, **On Submit** for the confirm.

## Related

- [🔖 Play Audio On Click](Play%20Audio%20On%20Click.md)
- [🔖 Play Audio On Select](Play%20Audio%20On%20Select.md)
- [🔖 Play Audio On Submit](Play%20Audio%20On%20Submit.md)
- [Audio Container](../Scriptable%20Objects/Audio%20Container.md)
- [📖 Audio guide](Audio.md)