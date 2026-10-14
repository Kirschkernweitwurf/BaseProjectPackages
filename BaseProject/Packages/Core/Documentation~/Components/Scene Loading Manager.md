---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
  - scenes
summary: Loads and unloads scenes and keeps the persistent scene loaded.
---
# Scene Loading Manager

🗞️ **Component** · programmers only

**Tags:** #programmer #scenes

Manages scene loading and unloading operations, including a persistent scene that remains loaded. Provides asynchronous methods to load scenes with progress reporting via `SceneLoadEvents`. Uses Unity's Awaitable for play-mode-safe, allocation-free async operations.

**Add it:** Inspector › **Add Component** › search `Scene Loading Manager`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.
2. Set **Persistent Scene Name** to the scene that holds the managers.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Persistent Scene Name** | Text |  | **Required.** The scene that stays loaded the whole time, usually the one with the managers. |

## Related

- [🔖 Loading Screen](Loading%20Screen.md)
- [🔖 Load Scene Button](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/UI/Documentation~/Components/Load%20Scene%20Button.md)