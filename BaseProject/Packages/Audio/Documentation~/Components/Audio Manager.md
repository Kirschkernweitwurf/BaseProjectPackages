---
type: component
package: Audio
audience: programmer
tags:
  - programmer
  - sound
  - audio
summary: The service that plays every sound and piece of music.
---
# Audio Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #sound #audio

The service that plays every sound and piece of music. Code and components ask it to play an Audio Container; it picks a pooled Audio Source, sets it up and releases it again when the clip is done.

**Add it:** Inspector › **Add Component** › search `Audio Manager`.

## How to use it

1. Put it on the persistent manager prefab, once.
2. Drag in the **Audio Pool Manager**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Minimum Delay** | Number | 0.1 | Extra seconds an Audio Source stays reserved after its clip ends, before it goes back to the pool. |
| **Min Pitch Inclusive** | Number | 0.95 | Lowest pitch when an Audio Container has Randomize Pitch on. |
| **Max Pitch Inclusive** | Number | 1.05 | Highest pitch when an Audio Container has Randomize Pitch on. |
| **Audio Pool Manager** | [Audio Pool Manager](Audio%20Pool%20Manager.md) |  | **Required.** The pool that hands out Audio Sources. |

## Related

- [🗞️ Audio Pool Manager](Audio%20Pool%20Manager.md)
- [Audio Container](../Scriptable%20Objects/Audio%20Container.md)
- [📖 Audio guide](Audio.md)