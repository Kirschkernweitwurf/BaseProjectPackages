---
type: Component
package: Audio
audience: Programmers
tags:
  - programmer
  - sound
  - audio
summary: Keeps reusable Audio Sources ready, one pool per audio type.
---
# Audio Pool Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #sound #audio

Keeps a pool of reusable Audio Sources for each audio type (2D, 3D, music, UI), so playing a sound never creates a GameObject at runtime.

**Add it:** Inspector › **Add Component** › search `Audio Pool Manager`.

## How to use it

1. Put it next to the Audio Manager on the persistent manager prefab.
2. Assign one Audio Source prefab per type. **Each prefab sets the mixer group**, which is what connects a sound to its volume slider.

## Fields

### Setup

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Pool Parent** | Transform |  | Parent for all pooled audio sources. Falls back to this transform when left empty. |
| **Prewarm Count** | Whole number | 4 | How many audio sources each pool creates up front, so the first sounds do not cause a hitch. |
| **Is Clearing Pool After Scene Load** | Toggle | Off | If true, clears all pools when a new scene is loaded. |

### Prefabs

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Audio Source 2D Prefab** | Audio Source prefab |  | **Required.** Audio Source prefab for the `Sfx2D` type. |
| **Audio Source 3D Prefab** | Audio Source prefab |  | **Required.** Audio Source prefab for the `Sfx3D` type. |
| **Audio Source Music Prefab** | Audio Source prefab |  | **Required.** Audio Source prefab for the `Music` type. |
| **Audio Source Ui Prefab** | Audio Source prefab |  | **Required.** Audio Source prefab for the `UI` type. |

## Good to know

- If a category of sound ignores its volume slider, check the mixer group on that type's prefab first.

## Related

- [🗞️ Audio Manager](Audio%20Manager.md)
- [🔖 Audio Volume Setting](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Settings/Documentation~/Components/Audio%20Volume%20Setting.md)
- [Audio Container](../Scriptable%20Objects/Audio%20Container.md)