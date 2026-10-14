---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
summary: Lets systems send each other events without knowing each other.
---
# Event Bus

🗞️ **Component** · programmers only

**Tags:** #programmer

Default `IEventBus` implementation backed by multicast delegates.

Registers itself with the service locator through `GameServiceBehaviour` and drops every handler on destroy, so no subscription survives a scene unload.

**Add it:** Inspector › **Add Component** › search `Event Bus`.

## How to use it

1. Put it on one of the manager prefabs, once. You rarely add it to a scene yourself.
2. Inspect live subscriptions with the Event Bus Window.

## Fields

None. Add it and it works.

## Related

- [Event Bus Window](../Tools/Event%20Bus%20Window.md)