---
type: Scriptable Object
package: Tools
audience: Everyone
tags:
  - artist
  - lighting
  - scenes
summary: Stores the render settings of a scene in an asset, so they can be applied anywhere without making that scene the active one.
---
# Lighting Profile

💌 **Scriptable Object** · for everyone

**Tags:** #artist #lighting #scenes

**Create:** `Assets > Create > Scriptable Objects > Base > Lighting Profile > New Profile`

**Default file name:** `LP_LightingProfile`

Stores the render settings of a scene in an asset, so they can be applied anywhere without making that scene the active one.

## Typical workflow

You normally never create or capture a profile by hand:

1. Add a [🔖 Lighting Profile Applier](../Components/Lighting%20Profile%20Applier.md) to the scene you are lighting.
2. Light the scene in the Lighting window as usual, bake, and save.

On every save and every bake the applier copies the scene's lighting into its profile. If it has no profile yet, one is made for you next to the scene's lighting data, as `<Scene>/LP_<Scene>.asset`.

To capture by hand, open the scene, select the profile and press **Capture From Open Scene**. **Preview In Open Scene** shows a profile without entering Play mode.

## Settings

### Skybox and ambient

| Field | Meaning |
| --- | --- |
| Skybox | The skybox material. |
| Ambient Mode | Skybox, Flat or Trilight. |
| Ambient Intensity | Strength of skybox ambient light. Shown for Skybox mode. |
| Ambient Sky Color | Shown for Flat and Trilight. |
| Ambient Equator Color | Shown for Trilight. |
| Ambient Ground Color | Shown for Trilight. |
| Subtractive Shadow Color | Shadow color in subtractive lighting mode. |

### Fog

| Field | Meaning |
| --- | --- |
| Fog | Turns fog on. Everything below only appears when this is on. |
| Fog Mode | Linear, Exponential or Exponential Squared. |
| Fog Color | The fog color. |
| Fog Density | Shown for the two exponential modes. |
| Fog Start Distance | Shown for Linear. |
| Fog End Distance | Shown for Linear. |

### Reflections

| Field | Meaning |
| --- | --- |
| Reflection Mode | Skybox or Custom. |
| Reflection Resolution | Shown for Skybox mode. |
| Custom Reflection | The cubemap. Shown for Custom mode. |
| Reflection Intensity | Strength of the reflections. |
| Reflection Bounces | How many times reflections bounce. |

### Baked environment

| Field | Meaning |
| --- | --- |
| Environment Reflection | The default reflection the scene showed, usually the baked skybox reflection. Empty when none was baked. |

The profile also stores the baked ambient light and the scene it was captured from. Both are hidden. The scene's name is shown as **Captured From** at the top of the Inspector.

### Halo and flare

| Field | Meaning |
| --- | --- |
| Halo Strength | Size of light halos. |
| Flare Strength | Brightness of lens flares. |
| Flare Fade Speed | How fast flares fade out. |

## Notes

- Fields hide themselves when they do not apply, so the Inspector only shows what matters for the current mode.
- Applying a profile updates the global render settings. The scene file itself is not touched.
- The baked ambient light and default reflection are applied exactly as captured. They live in the scene's lighting data, which Unity only uses for the active scene, so without them a scene loaded next to another one looks different from what you lit.
- If the Inspector warns that reflections are not baked, bake the lighting once and save. Until then the game shows no skybox reflection.
- A profile belongs to the scene it was captured from. If you copy a scene, the copy gets its own profile on its first save instead of overwriting the original's.
- Profiles captured before 3.3.0 have no baked environment yet and recalculate it from the skybox like before. Saving their scene once fixes that.