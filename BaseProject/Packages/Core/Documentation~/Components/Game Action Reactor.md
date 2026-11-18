---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
summary: Base for components that react to one game action picked in the Inspector.
---
# Game Action Reactor

🗞️ **Component** · programmers only

**Tags:** #programmer

The base of every component that reacts to one game action, like a camera shake when a card is played. Pick the action and the moment in the Inspector; the subclass decides what the reaction does. It subscribes to the [Game Action System](Game%20Action%20System.md) while it is enabled.

**Add it:** derive a component from `GameActionReactor` and override `React`. Add that component, not this base.

## How to use it

1. Derive from `GameActionReactor` and override `React(GameAction)`.
2. Add your component to a scene object and pick the **Action** and the **Timing**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Action** | Game Action Reference |  | The game action this component reacts to. |
| **Timing** | Reaction Timing | Post | When it reacts: Pre before the action and its rules, On as it runs, Post once it is done. |

## Related

- [🗞️ Game Action System](Game%20Action%20System.md)