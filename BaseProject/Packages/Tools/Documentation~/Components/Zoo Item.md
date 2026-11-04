---
type: Component
package: Tools
audience: Programmers
tags:
  - programmer
  - assets
summary: One place in a built zoo and what was measured there.
---
# Zoo Item

🗞️ **Component** · programmers only

**Tags:** #programmer #assets

One place in a built zoo and what was measured there: the size, triangles, materials and anything to look at.

## Why it exists

Keeping the measurement on the object means the zoo can be arranged again and its labels drawn without loading or measuring a single prefab, also after the scene was closed. That is what makes the sliders in the builder fast enough to follow the mouse.

## What you need to know

- You never add this yourself. It is hidden from the Add Component menu.
- It does nothing at runtime.
- The child object is the prefab instance. This object is where it stands in the layout.