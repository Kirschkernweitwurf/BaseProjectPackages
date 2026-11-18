---
type: Component
package: Core
audience: Programmers
tags:
  - programmer
summary: Runs game actions one by one and lets other systems react before and after each.
---
# Game Action System

🗞️ **Component** · programmers only

**Tags:** #programmer

Runs every game action (draw a card, take damage, ...) one after another. Before an action, reactions may change or cancel it; then the action runs and plays its animation; after it, reactions may start follow-up actions. The next action waits until the current one, animation included, is done.

The rules live in the plain class `GameActionResolver`, which this component hosts and registers as the `IGameActionSystem` service, so a headless simulation can run the same rules without a scene.

**Add it:** Inspector › **Add Component** › search `Game Action System`.

## How to use it

1. Put it once in the gameplay scene, on the managers.
2. Derive each action from `GameAction` and perform it with `IGameActionSystem.Perform`.
3. React with `SubscribePre`, `SubscribeOn` and `SubscribePost`, or derive a `GameActionReactor` for a component that reacts to an action picked in the Inspector.
4. Cheats call `CheatActionRunner`, so rules and feedback react to them like to any other action.

## Fields

None. Add it and it works.

## Good to know

- Follow-ups run right after the action that caused them, before anything queued earlier. So a turn can queue "play card, end the turn, draw" up front and the card's own consequences still land first.
- A follow-up a reaction queued still runs when the action itself throws, so one broken action never stalls the game.