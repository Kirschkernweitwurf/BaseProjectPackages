---
type: Component
package: UI
audience: Everyone
tags:
  - designer
  - artist
  - ui
summary: "Sparkles for rewarding screens: twinkling stars over an area plus bursts on demand."
---
# UI Sparkles

🔖 **Component** · for everyone

**Tags:** #designer #artist #ui

Sparkles for rewarding screens: a field of stars that twinkle all over an area while enabled, plus bursts that spray outward from a point on demand. Runs on unscaled time, because the screens using it pause the date. The sparkles are pooled images animated in one loop instead of one tween each, so dozens of them cost little and never fill up the tween pool.

**Add it:** Inspector › **Add Component** › search `UI Sparkles`. Only one per GameObject.

## How to use it

1. Put it on a full-screen rect above the content and link that rect as **Area**.
2. Add an inactive child image with the sparkle sprite and **Raycast Target** off and link it as **Sparkle Prefab**.
3. Pick the tint **Colors**. Set **Twinkle Count** to 0 for a burst-only field, like hearts on a win screen.
4. The twinkles run while the object is active, so the screen should only enable it while it is shown.

## Fields

### References

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Area** | Rect Transform |  | **Required.** The area the twinkles appear in. Bursts spawn here too, so it should cover the whole screen. |
| **Sparkle Prefab** | Image |  | **Required.** Image cloned for every sparkle. Its sprite and material set the look; the color is rolled. |
| **Colors** | List of Color | White | Colors the sparkles are tinted in, rolled per sparkle. |

### Twinkle

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Twinkle Count** | Whole number | 28 | How many sparkles twinkle at the same time while enabled. |
| **Twinkle Size** | Vector2 | (14, 42) | Smallest and largest size of a twinkle in pixels. |
| **Twinkle Duration** | Number | 1.1 | Seconds one twinkle takes to grow and fade again. |

### Burst

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Burst Count** | Whole number | 18 | Sparkles sprayed by one burst. |
| **Burst Size** | Vector2 | (20, 56) | Smallest and largest size of a burst sparkle in pixels. |
| **Burst Distance** | Vector2 | (120, 360) | Smallest and largest distance a burst sparkle flies in pixels. |
| **Burst Duration** | Number | 0.8 | Seconds a burst sparkle flies before it has faded out. |