---
type: Component
package: Services
audience: Programmers
tags:
  - programmer
summary: Lets systems clean up in a fixed order when the game quits.
---
# Shutdown Manager

🗞️ **Component** · programmers only

**Tags:** #programmer

Gives every system a chance to clean up when the game quits, before any GameObject is destroyed. Systems implement `IShutdownHandler` and register themselves.

**Add it:** Inspector › **Add Component** › search `Shutdown Manager`.

## How to use it

1. Put it on the persistent manager prefab, once.

## Fields

None. Add it and it works.

## Related

- [🔖 Bootstrapper](Bootstrapper.md)