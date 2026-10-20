---
type: Component
package: Tools
audience: Everyone
tags:
  - artist
  - lighting
  - scenes
summary: Applies a scene's lighting as soon as that scene is loaded, even when another scene is the active one.
---
# Lighting Profile Applier

🔖 **Component** · for everyone

**Tags:** #artist #lighting #scenes

**Add Component:** Lighting Profile Applier

Applies a scene's lighting as soon as that scene is loaded, even when another scene is the active one. It keeps its [💌 Lighting Profile](../Scriptable%20Objects/Lighting%20Profile.md) in sync with the scene, so the game shows exactly what you lit.

This is the fix for a common problem: when you load several scenes at once, only the active scene's lighting is used. With a profile you decide which lighting wins, no matter which scene is active.

## Fields

| Field | What it is for |
| --- | --- |
| **Profile** | The Lighting Profile asset to apply. Required. Created for you on the first save. |
| **Sun** | The directional light used as the sun. Filled from the Lighting window's Sun Source while synced. |
| **Sync With Scene** | On by default. Copies this scene's lighting into the profile on every save and bake. Turn off to apply a profile captured in another scene. |

## How to use it

1. Add this component to any GameObject in the scene you are lighting.
2. Light the scene in the Lighting window as usual, bake, and save.

That's it. Every save and bake updates the profile. To use a profile from another scene instead, turn **Sync With Scene** off and drop that profile in.

## Notes

- The profile is applied in `Awake`, so it happens before anything is drawn.
- Only the active scene is synced, because the Lighting window always shows the active scene. With several scenes open, make the lit scene active before saving.
- It also refreshes the light probes, which Unity skips for scenes loaded additively.
- **Profile** is marked as required. If you forget it, you get an error in the Console instead of a silent failure.
- One applier per scene is enough. Two of them will fight over the settings.