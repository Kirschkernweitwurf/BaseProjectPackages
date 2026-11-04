---
type: Component
package: Tools
audience: Programmers
tags:
  - programmer
  - assets
summary: Sits on the root of a built zoo, so the builder can find it and the Scene view knows which labels to draw.
---
# Zoo Root Marker

🗞️ **Component** · programmers only

**Tags:** #programmer #assets

Sits on the root of a built zoo, so the builder can find it and the Scene view knows which labels to draw.

## Why it exists

The builder needs to find the zoo it created earlier so it can clear or rebuild it. Looking it up by component type is safe. Looking it up by name would break the moment somebody renames the object, or names their own object the same thing. The label settings live here so they come back when the scene is opened again, with or without the builder window.

## What you need to know

- You never add this yourself. It is hidden from the Add Component menu.
- It does nothing at runtime.
- Do not remove it from a built zoo, or the builder will lose track of it and leave the old zoo behind.
- Only one per GameObject.