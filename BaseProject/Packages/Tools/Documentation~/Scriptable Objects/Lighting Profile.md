# Lighting Profile

💌 **Scriptable Object** · for everyone

**Tags:** #artist #lighting #scenes

**Create:** `Assets > Create > Scriptable Objects > Base > Lighting Profile > New Profile`

**Default file name:** `LP_LightingProfile`

Stores the render settings of a scene in an asset, so they can be applied anywhere without making that scene the active one.

## Typical workflow

1. Open a scene and set up its lighting the way you want it.
2. Create a profile asset.
3. Press **Capture** in the Inspector. Everything is copied into the asset.
4. Add a [🔖 Lighting Profile Applier](../Components/Lighting%20Profile%20Applier.md) to any scene that should use it and drop the profile in.

The Inspector also has **Apply** and preview buttons so you can check a profile without entering Play mode.

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

### Halo and flare

| Field | Meaning |
| --- | --- |
| Halo Strength | Size of light halos. |
| Flare Strength | Brightness of lens flares. |
| Flare Fade Speed | How fast flares fade out. |

## Notes

- Fields hide themselves when they do not apply, so the Inspector only shows what matters for the current mode.
- Applying a profile updates the global render settings and refreshes the environment lighting. The scene file itself is not touched.
